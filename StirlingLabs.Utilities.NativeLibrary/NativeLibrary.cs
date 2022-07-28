using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

#if !NETSTANDARD
using SysNativeLibrary = System.Runtime.InteropServices.NativeLibrary;
using DllImportResolver = System.Runtime.InteropServices.DllImportResolver;
#else
public delegate IntPtr DllImportResolver(
    string libraryName,
    Assembly assembly,
    DllImportSearchPath? searchPath
);
#endif

[PublicAPI]
public abstract partial class NativeLibrary
{
#if !NETSTANDARD
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void SetDllImportResolver(Assembly assembly, DllImportResolver resolver)
    {
#if NETSTANDARD
        lock (Resolvers)
            Resolvers.GetOrCreateValue(assembly).AddLast(resolver);
#else
        SysNativeLibrary.SetDllImportResolver(assembly, resolver);
#endif
    }

#if !NETSTANDARD
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IntPtr GetExport(IntPtr handle, string name)
    {
#if NETSTANDARD
        if (handle == default)
            throw new ArgumentNullException(nameof(handle));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        var export = Loader.GetExport(handle, name);
        if (export == default)
            throw new TypeLoadException($"Entry point not found: {name}");

        return export;
#else
        return System.Runtime.InteropServices.NativeLibrary.GetExport(handle, name);
#endif
    }

#if !NETSTANDARD
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IntPtr Load(string libraryName, Assembly assembly, DllImportSearchPath? searchPath = null)
    {
#if NETSTANDARD
        if (string.IsNullOrEmpty(libraryName))
            throw new ArgumentNullException(nameof(libraryName));
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        lock (Resolvers)
        {
            if (!Resolvers.TryGetValue(assembly, out var resolvers))
                return Load(libraryName);

            foreach (var resolver in resolvers)
            {
                var result = resolver(libraryName, assembly, searchPath);
                if (result != default)
                    return result;
            }

            var loaded = Load(libraryName);
            if (loaded == default)
                throw new DllNotFoundException(libraryName);

            return loaded;
        }
#else
        return SysNativeLibrary.Load(libraryName, assembly, searchPath);
#endif
    }

#if !NETSTANDARD
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static IntPtr Load(string libraryPath)
    {
#if NETSTANDARD
        if (string.IsNullOrEmpty(libraryPath))
            throw new ArgumentNullException(nameof(libraryPath));

        var loaded = Loader.Load(libraryPath);
        if (loaded == default)
            throw new DllNotFoundException(libraryPath);

        return loaded;
#else
        return SysNativeLibrary.Load(libraryPath);
#endif
    }
}


#if NETSTANDARD
public abstract partial class NativeLibrary
{
    private static readonly INativeLibraryLoader Loader
        = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? WindowsLinkage.Instance
            : PosixLinkage.Instance;

    private static readonly ConditionalWeakTable<Assembly, LinkedList<DllImportResolver>> Resolvers = new();

    private interface INativeLibraryLoader
    {
        void Init();

        IntPtr Load(string libraryPath);

        IntPtr GetExport(IntPtr handle, string name);

        string? GetLastError();
    }

    private static class PosixLinkage
    {
        static PosixLinkage()
        {
            //Trace.TraceInformation($"LibDl initializing.");
            INativeLibraryLoader loader;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    loader = new LibSystemB();
                    loader.Init();
                    loader.GetLastError();
                }
                catch
                {
                    try
                    {
                        loader = new LibSystem();
                        loader.Init();
                        loader.GetLastError();
                    }
                    catch
                    {
                        try
                        {
                            loader = new LibDl1();
                            loader.Init();
                            loader.GetLastError();
                        }
                        catch
                        {
                            throw new DllNotFoundException("Can't find dynamic loader library!");
                        }
                    }
                }
            }
            else
            {
                // not delayed linkage
                try
                {
                    loader = new LibDl2();
                    loader.Init();
                }
                catch
                {
                    try
                    {
                        loader = new LibDl1();
                        loader.Init();
                    }
                    catch
                    {
                        throw new DllNotFoundException("Can't find dynamic loader library!");
                    }
                }
            }

            Instance = loader;
        }

        internal static readonly INativeLibraryLoader Instance;
    }

    private sealed class LibSystemB : INativeLibraryLoader
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private const string LibName = "System.B";

        [DllImport(LibName, EntryPoint = "dlopen")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr Load(string fileName, int flags);

        [DllImport(LibName, EntryPoint = "dlsym")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr GetExport(IntPtr handle, string symbol);

        [DllImport(LibName, EntryPoint = "dlerror")]
        private static extern unsafe sbyte* GetLastError();

        unsafe string? INativeLibraryLoader.GetLastError()
        {
            var err = GetLastError();
            return err == default ? null : new(err);
        }

        unsafe IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            var lib = Load(libraryPath, 0x0002 /*RTLD_NOW*/);
            if (lib != default)
                return lib;

            var err = GetLastError();
            if (err == default)
                return default;

            var errStr = new string(err);
            throw new InvalidOperationException(errStr);
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
            => GetExport(handle, name);

        public void Init() { }
    }

    private sealed class LibSystem : INativeLibraryLoader
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private const string LibName = "System";

        [DllImport(LibName, EntryPoint = "dlopen")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr Load(string fileName, int flags);

        [DllImport(LibName, EntryPoint = "dlsym")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr GetExport(IntPtr handle, string symbol);

        [DllImport(LibName, EntryPoint = "dlerror")]
        private static extern unsafe sbyte* GetLastError();

        unsafe string? INativeLibraryLoader.GetLastError()
        {
            var err = GetLastError();
            return err == default ? null : new(err);
        }
        unsafe IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            var lib = Load(libraryPath, 0x0002 /*RTLD_NOW*/);
            if (lib != default)
                return lib;

            var err = GetLastError();
            if (err == default)
                return default;

            var errStr = new string(err);
            throw new InvalidOperationException(errStr);
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
            => GetExport(handle, name);

        public void Init() { }
    }

    private sealed class LibDl1 : INativeLibraryLoader
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private const string LibName = "dl"; // can be libdl.so or libdl.dylib

        [DllImport(LibName, EntryPoint = "dlopen")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr Load(string fileName, int flags);

        [DllImport(LibName, EntryPoint = "dlsym")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr GetExport(IntPtr handle, string symbol);

        [DllImport(LibName, EntryPoint = "dlerror")]
        private static extern unsafe sbyte* GetLastError();

        unsafe string? INativeLibraryLoader.GetLastError()
        {
            var err = GetLastError();
            return err == default ? null : new(err);
        }

        unsafe IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            var lib = Load(libraryPath, 0x0002 /*RTLD_NOW*/);
            if (lib != default)
                return lib;

            var err = GetLastError();
            if (err == default)
                return default;

            var errStr = new string(err);
            throw new InvalidOperationException(errStr);
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
            => GetExport(handle, name);

        public void Init() { }
    }

    private sealed class LibDl2 : INativeLibraryLoader
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private const string LibName = "dl.so.2";

        [DllImport(LibName, EntryPoint = "dlopen")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr Load(string fileName, int flags);

        [DllImport(LibName, EntryPoint = "dlsym")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr GetExport(IntPtr handle, string symbol);

        [DllImport(LibName, EntryPoint = "dlerror")]
        private static extern unsafe sbyte* GetLastError();

        unsafe string? INativeLibraryLoader.GetLastError()
        {
            var err = GetLastError();
            return err == default ? null : new(err);
        }

        unsafe IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            var lib = Load(libraryPath, 0x0002 /*RTLD_NOW*/);
            if (lib != default)
                return lib;

            var err = GetLastError();
            if (err == default)
                return default;

            var errStr = new string(err);
            throw new InvalidOperationException(errStr);
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
            => GetExport(handle, name);

        public void Init() { }
    }

    private sealed class WindowsLinkage : INativeLibraryLoader
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private const string LibName = "kernel32";

        private WindowsLinkage() { }

        internal static readonly INativeLibraryLoader Instance = new WindowsLinkage();

        [DllImport(LibName, EntryPoint = "LoadLibrary", SetLastError = true)]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr Load(string lpFileName);

        [DllImport(LibName, EntryPoint = "GetProcAddress")]
        // ReSharper disable once MemberHidesStaticFromOuterClass
        private static extern IntPtr GetExport(IntPtr handle, string procedureName);

        string? INativeLibraryLoader.GetLastError()
        {
            var err = Marshal.GetLastWin32Error();
            return err == default ? null : new Win32Exception(err).Message;
        }

        IntPtr INativeLibraryLoader.Load(string libraryPath)
        {
            var lib = Load(libraryPath);
            if (lib != default)
                return lib;

            var err = Marshal.GetLastWin32Error();
            if (err == default)
                return default;

            if (err == 126)
                throw new DllNotFoundException(new Win32Exception(err).Message);
            throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
        }

        IntPtr INativeLibraryLoader.GetExport(IntPtr handle, string name)
            => GetExport(handle, name);

        public void Init() { }
    }
}


#endif
