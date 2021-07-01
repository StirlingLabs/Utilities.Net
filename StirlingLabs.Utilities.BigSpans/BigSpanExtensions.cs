using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InlineIL;
using static InlineIL.IL;
using static InlineIL.IL.Emit;

namespace StirlingLabs.Utilities
{
    public static class BigSpanExtensions
    {
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
        
        public static unsafe void CopyTo<T>(this T[] srcArray, BigSpan<T> dst)
            => (new BigSpan<T>(srcArray,false)).CopyTo(dst);

        public static unsafe void CopyTo<T>(this Span<T> src, BigSpan<T> dst)
        {
            var srcLen = (nuint)src.Length;
            var dstLen = dst.Length;
            if (srcLen > dstLen)
                throw new ArgumentException("Too short.", nameof(dst));

            var length = srcLen < dstLen ? srcLen : dstLen;

            DeclareLocals(
                new LocalVar("rDst", TypeRef.Type<T>().MakeByRefType())
                    .Pinned(),
                new LocalVar("rSrc", TypeRef.Type<T>().MakeByRefType())
                    .Pinned()
            );

            Push(ref dst.GetPinnableReference());
            Stloc("rDst");
            Push(ref src.GetPinnableReference());
            Stloc("rSrc");
            Ldloc("rDst");
            Pop(out var pDst);
            Ldloc("rSrc");
            Pop(out var pSrc);
            if (pDst == default) throw new ArgumentNullException(nameof(dst));
            if (pSrc == default) throw new ArgumentNullException(nameof(src));
            var sizeOfT = (nuint)Unsafe.SizeOf<T>();
            var srcOffset = sizeOfT * srcLen;
            var copyLength = length - srcOffset;
            if (copyLength > 0)
                BigSpanHelpers.Copy((byte*)pDst + srcOffset, pSrc, copyLength);
        }

        public static unsafe void CopyTo<T>(this ReadOnlySpan<T> src, BigSpan<T> dst)
        {
            var srcLen = (nuint)src.Length;
            var dstLen = dst.Length;
            if (srcLen > dstLen)
                throw new ArgumentException("Too short.", nameof(dst));

            var length = srcLen < dstLen ? srcLen : dstLen;

            DeclareLocals(
                new LocalVar("rDst", TypeRef.Type<T>().MakeByRefType())
                    .Pinned(),
                new LocalVar("rSrc", TypeRef.Type<T>().MakeByRefType())
                    .Pinned()
            );

            Push(ref dst.GetPinnableReference());
            Stloc("rDst");
            Push(ref Unsafe.AsRef(src.GetPinnableReference()));
            Stloc("rSrc");
            Ldloc("rDst");
            Pop(out var pDst);
            Ldloc("rSrc");
            Pop(out var pSrc);
            if (pDst == default) throw new ArgumentNullException(nameof(dst));
            if (pSrc == default) throw new ArgumentNullException(nameof(src));
            var sizeOfT = (nuint)Unsafe.SizeOf<T>();
            var srcOffset = sizeOfT * srcLen;
            var copyLength = length - srcOffset;
            if (copyLength > 0)
                BigSpanHelpers.Copy((byte*)pDst + srcOffset, pSrc, copyLength);
        }

        public static unsafe bool SequenceEqual<T>(this BigSpan<T> a, BigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b) == 0;

        public static unsafe bool SequenceEqual<T>(this ReadOnlyBigSpan<T> a, BigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b) == 0;

        public static unsafe bool SequenceEqual<T>(this BigSpan<T> a, ReadOnlyBigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b) == 0;

        public static unsafe bool SequenceEqual<T>(this ReadOnlyBigSpan<T> a, ReadOnlyBigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b) == 0;

        public static unsafe bool SequenceEqual<T>(this Span<T> a, BigSpan<T> b)
            where T : unmanaged
            => b.CompareMemory(a) == 0;

        public static unsafe bool SequenceEqual<T>(this ReadOnlySpan<T> a, BigSpan<T> b)
            where T : unmanaged
            => b.CompareMemory(a) == 0;

        public static unsafe bool SequenceEqual<T>(this BigSpan<T> a, ReadOnlySpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b) == 0;

        public static unsafe bool SequenceEqual<T>(this ReadOnlyBigSpan<T> a, ReadOnlySpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b) == 0;

        public static unsafe int SequenceCompare<T>(this BigSpan<T> a, BigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b);

        public static unsafe int SequenceCompare<T>(this ReadOnlyBigSpan<T> a, BigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b);

        public static unsafe int SequenceCompare<T>(this BigSpan<T> a, ReadOnlyBigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b);

        public static unsafe int SequenceCompare<T>(this ReadOnlyBigSpan<T> a, ReadOnlyBigSpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b);

        public static unsafe int SequenceCompare<T>(this Span<T> a, BigSpan<T> b)
            where T : unmanaged
            => -b.CompareMemory(a);

        public static unsafe int SequenceCompare<T>(this ReadOnlySpan<T> a, BigSpan<T> b)
            where T : unmanaged
            => -b.CompareMemory(a);

        public static unsafe int SequenceCompare<T>(this BigSpan<T> a, ReadOnlySpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b);

        public static unsafe int SequenceCompare<T>(this ReadOnlyBigSpan<T> a, ReadOnlySpan<T> b)
            where T : unmanaged
            => a.CompareMemory(b);
    }
}
