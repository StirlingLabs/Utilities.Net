using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
#if NET6_0_OR_GREATER
using System.Numerics;
#endif
#if !NETSTANDARD
using System.Runtime.Intrinsics.Arm;
#endif

namespace StirlingLabs.Utilities;

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("Design", "CA1060", Justification = "Unnecessary")]
[SuppressMessage("Interoperability", "SYSLIB1054", Justification = "Unnecessary")]
public static class Posix
{
    // https://github.com/dotnet/runtime/blob/e297470559fb95344ab52c8dad561885a1430e7e/src/native/libs/System.Native/pal_dynamicload.c#L25-L36
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const string libc = "libc";

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const string pthread = "pthread";

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(pthread)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern nint pthread_self();

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(pthread)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int pthread_setaffinity_np(nint thread, nint cpusetsize, void* cpuset);

    public static unsafe void SetCurrentThreadProcessorAffinityMask(nuint procBits)
    {
#if !NET6_0_OR_GREATER
        static bool IsPow2(nuint value) => (value & (value - 1)) == 0 && value != 0;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !IsPow2(procBits))
            throw new PlatformNotSupportedException(
                "MacOS only supports setting thread affinity by tags, use SetCurrentThreadProcessorAffinity.");
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !BitOperations.IsPow2(procBits))
            throw new PlatformNotSupportedException(
                "MacOS only supports setting thread affinity by tags, use SetCurrentThreadProcessorAffinity.");
#endif

        var procCount = Environment.ProcessorCount;
        var procCountBytes = (procCount + 7) / 8;
        var procMask = (nuint)((1 << procCount) - 1);
        procBits &= procMask;
        if (procBits == 0)
            throw new InvalidOperationException("No valid processors are set in the affinity mask.");

        var result = pthread_setaffinity_np(pthread_self(), procCountBytes, &procBits);
        if (result != 0)
            throw new($"Posix error status {result}");
    }

    public static void SetCurrentThreadProcessorAffinity(int procIndex)
    {
        var procCount = Environment.ProcessorCount;
        if (procIndex > procCount)
            throw new($"Processor index {procIndex} is out of range for {procCount} processors");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Mac.SetCurrentThreadProcessorAffinity(procIndex);
            return;
        }

        SetCurrentThreadProcessorAffinityMask((nuint)(1 << procIndex));
    }

    public static unsafe void ClearCurrentThreadProcessorAffinity()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Mac.ClearCurrentThreadProcessorAffinity();
            return;
        }

        var procCount = Environment.ProcessorCount;
        var procCountBytes = (procCount + 7) / 8;
        var procMask = (nuint)((1 << procCount) - 1);
        var result = pthread_setaffinity_np(pthread_self(), procCountBytes, &procMask);
        if (result != 0)
            throw new($"Posix error status {result}");
    }

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe void* mmap(void* addr, nuint length, int prot, int flags, int fd, nint offset);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int munmap(void* addr, nuint length);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int mprotect(void* addr, nuint length, int prot);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int open(byte* path, O_FLAGS flags);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int read(int fd, byte* dest, nuint count);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int write(int fd, byte* src, nuint count);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern nint lseek(int fd, nint offset, SEEK_FLAGS whence);

#if NET6_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int errno() => Marshal.GetLastPInvokeError();
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int errno() => Marshal.GetLastWin32Error();
#endif

    internal static readonly ConcurrentDictionary<nint, nuint> Allocations = new();

    internal static readonly unsafe void* MMapFail = (void*)-1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static unsafe bool Allocate(out void* addr, nuint size, bool executable = false)
    {
        addr = mmap(null, size, executable ? 0x3 : 0x7, 0x22, -1, 0);

        if (addr == null || addr == MMapFail)
            return false;

        Allocations[(nint)addr] = size;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool Free(void* addr)
        => Allocations.TryRemove((nint)addr, out var size)
            && munmap(addr, size) == 0;

    public static unsafe bool Protect(void* addr, bool read, bool write, bool exec)
        => Allocations.TryGetValue((nint)addr, out var size)
            && mprotect(addr, size, (read ? 1 : 0) | (write ? 2 : 0) | (exec ? 4 : 0)) == 0;

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(pthread)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern unsafe int pthread_getattr_np(nint thread, void** attr);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(pthread)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static extern unsafe int pthread_attr_getstack(void* attr, out void* stackaddr, out nuint stacksize);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(pthread)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static extern unsafe int pthread_attr_getstackaddr(void* attr, out void* stackaddr);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(pthread)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static extern unsafe int pthread_attr_getstacksize(void* attr, out nuint stacksize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static unsafe bool GetStackSize(out nuint stackSize)
    {
        void* attr = null;
        if (pthread_getattr_np(pthread_self(), &attr) == 0
            && pthread_attr_getstacksize(attr, out stackSize) == 0)
            return true;

        stackSize = 0;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static unsafe bool GetStackBase(out void* pStackBase)
    {
        void* attr = null;
        if (pthread_getattr_np(pthread_self(), &attr) == 0
            && pthread_attr_getstackaddr(attr, out pStackBase) == 0)
            return true;

        pStackBase = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static unsafe bool GetStackInfo(out void* pStackBase, out nuint stackSize)
    {
        void* attr = null;
        if (pthread_getattr_np(pthread_self(), &attr) == 0
            && pthread_attr_getstack(attr, out pStackBase, out stackSize) == 0)
            return true;

        pStackBase = null;
        stackSize = 0;
        return false;
    }

    [Flags]
    internal enum O_FLAGS
    {
        O_RDONLY = 0x0000,

        O_WRONLY = 0x0001,

        O_RDWR = 0x0002,

        O_CREAT = 0x0040,

        O_EXCL = 0x0080,

        O_TRUNC = 0x0200,

        O_APPEND = 0x0400,

        O_NONBLOCK = 0x0800,

        O_SYNC = 0x1000,

        O_ASYNC = 0x2000,

        O_DIRECT = 0x4000,

        O_LARGEFILE = 0x8000,

        O_DIRECTORY = 0x10000,

        O_NOFOLLOW = 0x20000,

        O_NOATIME = 0x40000,

        O_CLOEXEC = 0x80000,

        O_PATH = 0x100000,

        O_DSYNC = 0x200000,

        O_TMPFILE = 0x400000,

        O_NDELAY = O_NONBLOCK,

        O_RSYNC = O_SYNC,

        O_ACCMODE = 0x0003
    }

    internal enum SEEK_FLAGS
    {
        SEEK_SET = 0,

        SEEK_CUR = 1,

        SEEK_END = 2
    }

    internal static readonly object[] _fdTmpLocks = new object[Environment.ProcessorCount];

    internal static readonly int[] _fdTmpBytes = new int[Environment.ProcessorCount];

    static unsafe Posix()
    {
        var szTmp = "/tmp\0"u8;
        fixed (byte* pszTmp = szTmp)
        {
            for (var i = 0; i < _fdTmpBytes.Length; i++)
            {
                _fdTmpBytes[i] = open(pszTmp, O_FLAGS.O_RDWR | O_FLAGS.O_EXCL | O_FLAGS.O_TMPFILE | O_FLAGS.O_CLOEXEC);
                _fdTmpLocks[i] = new();
            }
        }
    }

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(libc)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    public static extern int sched_getcpu();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetCurrentProcessorIndex()
        => sched_getcpu();

    private static int _counter;

    public static unsafe bool IsMemoryWritable(byte* p)
    {
        // this abuses the fact that the kernel will not allow us to write to a read-only page
        var index = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            ? Thread.GetCurrentProcessorId()
#else
            ? GetCurrentProcessorIndex()
#endif
#if NETSTANDARD
            : (Interlocked.Increment(ref _counter) - 1) % Environment.ProcessorCount;
#else
            : ArmBase.IsSupported
                ? (Interlocked.Increment(ref _counter) - 1) % Environment.ProcessorCount
                : Mac.GetCurrentProcessorIndex();
#endif

        lock (_fdTmpLocks[index])
        {
            var fd = _fdTmpBytes[index];
            var ws = write(fd, p, 1);
            lseek(fd, 0, SEEK_FLAGS.SEEK_SET);
            if (ws == -1) return false; // not readable

            var rs = read(fd, p, 1);
            lseek(fd, 0, SEEK_FLAGS.SEEK_SET);
            return rs != -1; // not writable, probably errno 14 (bad address)
        }
    }
}
