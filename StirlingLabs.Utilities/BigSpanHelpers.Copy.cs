using System.Runtime.CompilerServices;

namespace StirlingLabs.Utilities
{
    internal static partial class BigSpanHelpers
    {
        internal static unsafe void Copy(void* destinationPointer, void* sourcePointer, nuint length)
        {
            var l = length;
            while (l >= uint.MaxValue)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(destinationPointer), ref Unsafe.AsRef<byte>(sourcePointer), uint.MaxValue);
                l -= uint.MaxValue;
            }
            if (l > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(destinationPointer), ref Unsafe.AsRef<byte>(sourcePointer), (uint)l);
        }
        internal static unsafe void Copy<T>(T* destinationPointer, T* sourcePointer, nuint count) where T : unmanaged
        {
            var l = checked((nuint)((ulong)sizeof(T) * count));
            while (l >= uint.MaxValue)
            {
                Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(destinationPointer), ref Unsafe.AsRef<byte>(sourcePointer), uint.MaxValue);
                l -= uint.MaxValue;
            }
            if (l > 0)
                Unsafe.CopyBlockUnaligned(ref Unsafe.AsRef<byte>(destinationPointer), ref Unsafe.AsRef<byte>(sourcePointer), (uint)l);
        }
    }
}
