using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

#nullable enable
namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class Common
    {
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
        public static void FreeNativeMemory(ref nint nativePointer)
        {
            Marshal.FreeHGlobal(nativePointer);
            nativePointer = default;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void FreeNativeMemory(ref void* nativePointer)
        {
            Marshal.FreeHGlobal((nint)nativePointer);
            nativePointer = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllocateNativeMemory(nint size, out nint pointer)
        {
            try
            {
                pointer = Marshal.AllocHGlobal(size);
            }
            catch (OutOfMemoryException)
            {
                pointer = default;
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool AllocateNativeMemory<T>(nint count, out T* pointer)
            where T: unmanaged
        {
            var success = AllocateNativeMemory(count * sizeof(T), out var pointerValue);
            pointer = (T*)pointerValue;
            return success;
        }
    }
}
