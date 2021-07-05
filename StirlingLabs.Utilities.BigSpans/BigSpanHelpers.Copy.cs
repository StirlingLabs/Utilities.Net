using System.Runtime.CompilerServices;

namespace StirlingLabs.Utilities
{
    internal static partial class BigSpanHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy(void* destinationPointer, void* sourcePointer, nuint length)
        {
            var l = length;
            ref var dstRef = ref Unsafe.AsRef<byte>(destinationPointer);
            ref var srcRef = ref Unsafe.AsRef<byte>(sourcePointer);
            while (l >= uint.MaxValue)
            {
                Unsafe.CopyBlockUnaligned(ref dstRef, ref srcRef, uint.MaxValue);
                dstRef = ref Unsafe.AddByteOffset(ref dstRef, (nint)uint.MaxValue);
                srcRef = ref Unsafe.AddByteOffset(ref srcRef, (nint)uint.MaxValue);
                l -= uint.MaxValue;
            }
            if (l > 0)
                Unsafe.CopyBlockUnaligned(ref dstRef, ref srcRef, (uint)l);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void Copy<T>(T* destinationPointer, T* sourcePointer, nuint count) where T : unmanaged
        {
            var l = checked((nuint)((ulong)sizeof(T) * count));
            Copy((void*)destinationPointer, sourcePointer, l);
        }
    }
}
