using System.Runtime.CompilerServices;

namespace StirlingLabs.Utilities
{
    internal static partial class BigSpanHelpers
    {
        internal static unsafe void Copy(void* destinationPointer, void* sourcePointer, nuint length)
        {
            ref var dstRef = ref Unsafe.AsRef<byte>(destinationPointer);
            ref var srcRef = ref Unsafe.AsRef<byte>(sourcePointer);
            var l = length;
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
        internal static unsafe void Copy<T>(T* destinationPointer, T* sourcePointer, nuint count) where T : unmanaged
        {
            ref var dstRef = ref Unsafe.AsRef<byte>(destinationPointer);
            ref var srcRef = ref Unsafe.AsRef<byte>(sourcePointer);
            var l = checked((nuint)((ulong)sizeof(T) * count));
            while (l >= uint.MaxValue)
            {
                Unsafe.CopyBlockUnaligned(ref dstRef, ref srcRef, uint.MaxValue);
                dstRef = ref Unsafe.Add(ref dstRef, (nint)uint.MaxValue);
                srcRef = ref Unsafe.Add(ref srcRef, (nint)uint.MaxValue);
                l -= uint.MaxValue;
            }
            if (l > 0)
                Unsafe.CopyBlockUnaligned(ref dstRef, ref srcRef, (uint)l);
        }
    }
}
