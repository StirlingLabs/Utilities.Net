using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

#nullable enable
namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class Common
    {
        private static readonly Assembly ThisAssembly = typeof(Common).Assembly;
        public static T OnDemand<T>(ref WeakReference<T>? cache, Func<T> factory)
            where T : class
        {
            T? d;
            if (cache is null)
                cache = new(d = factory());
            else if (!cache.TryGetTarget(out d))
                cache.SetTarget(d = factory());
            return d;
        }

        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> hasher)
            => new DelegatingEqualityComparer<T>(equals, hasher);
        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals)
            => new DelegatingEqualityComparer<T>(equals);
        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T, int> hasher)
            => new DelegatingEqualityComparer<T>(hasher);

        /// <summary>
        /// Prevents generation of boxing instructions in certain situations.
        /// </summary>
        /// <remarks>
        /// The innocent looking construct:
        /// <code>
        ///    Assert.Throws&lt;E&gt;( () =&gt; new Span() );
        /// </code>
        /// generates a hidden box of the Span as the return value of the lambda. This makes the IL illegal and unloadable on
        /// runtimes that enforce the actual Span rules (never mind that we expect never to reach the box instruction...)
        ///
        /// The workaround is to code it like this:
        /// <code>
        ///    Assert.Throws&lt;E&gt;( () =&gt; new Span().DontBox() );
        /// </code>
        /// which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
        /// </remarks>
        public static void DontBox<T>(this Span<T> span)
        {
            // This space intentionally left blank.
        }


        /// <summary>
        /// Returns a reference to the 0th element of the BigSpan. If the BigSpan is empty, returns a reference to the location where the 0th element
        /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
        /// </summary>
        public static ref T GetReference<T>(in this BigSpan<T> span) where T : unmanaged
            => ref span._pointer.Value;

        /// <summary>
        /// Returns a reference to the 0th element of the ReadOnlyBigSpan. If the ReadOnlyBigSpan is empty, returns a reference to the location where the 0th element
        /// would have been stored. Such a reference may or may not be null. It can be used for pinning but must never be dereferenced.
        /// </summary>
        public static ref T GetReference<T>(in this ReadOnlyBigSpan<T> span) where T : unmanaged
            => ref span._pointer.Value;

        public static nuint GetLength<T>(this T[] array)
            => BigSpanHelpers.Is64Bit ? (nuint)array.LongLength : (nuint)array.Length;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FreeUnmanagedMemory(ref nint nativePointer)
        {
            free((void*)nativePointer);
            nativePointer = default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FreeUnmanagedMemory(ref void* nativePointer)
        {
            free(nativePointer);
            nativePointer = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool AllocateUnmanagedMemory(nuint size, out nint pointer)
        {
            try
            {
                pointer = (nint)malloc(size);
            }
            catch (OutOfMemoryException)
            {
                pointer = default;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool AllocateUnmanagedMemory(nuint size, out void* pointer)
        {
            try
            {
                pointer = malloc(size);
            }
            catch (OutOfMemoryException)
            {
                pointer = default;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool AllocateUnmanagedMemory<T>(nuint count, out T* pointer)
            where T : unmanaged
        {
            var success = AllocateUnmanagedMemory(count * (nuint)sizeof(T), out void* pointerValue);
            pointer = (T*)pointerValue;
            return success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BigSpan<T> AllocateNativeMemoryBigSpan<T>(nuint count)
            where T : unmanaged
        {
            if (AllocateUnmanagedMemory(count * (nuint)sizeof(T), out void* pointerValue))
            {
                return new(pointerValue, count);
            }
            throw new OutOfMemoryException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FreeNativeMemoryBigSpan<T>(ref BigSpan<T> span)
            where T : unmanaged
        {
            free(span.GetUnsafePointer());
            span = default;
        }

        static Common()
        {
            NativeLibrary.SetDllImportResolver(ThisAssembly, (name, assembly, path) => {
                if (name == "c" && assembly == ThisAssembly)
                {
                    return NativeLibrary.Load(
                        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                            ? "msvcrt"
                            : "libc"
                    );
                }
                return default;
            });
        }

        [DllImport("c")]
        internal static extern unsafe void* malloc(nuint size);

        [DllImport("c")]
        internal static extern unsafe void free(void* size);

        [DllImport("c")]
        internal static extern unsafe int memcmp(void* ptr1, void* ptr2, nuint num);
    }
}
