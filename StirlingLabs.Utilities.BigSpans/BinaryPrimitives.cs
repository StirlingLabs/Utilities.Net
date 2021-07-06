using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// @formatter:off
#if !NETSTANDARD
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif
// @formatter:on

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class BinaryPrimitives
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReverseEndianness(byte value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReverseEndianness(sbyte value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReverseEndianness(short value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReverseEndianness(ushort value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReverseEndianness(int value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReverseEndianness(uint value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReverseEndianness(long value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReverseEndianness(ulong value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantUnsafeContext
        public static unsafe int SingleToInt32Bits(float value)
        {
#if !NETSTANDARD
            // Workaround for https://github.com/dotnet/runtime/issues/11413
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Sse2.IsSupported)
                return Sse2.ConvertToInt32(Vector128.CreateScalarUnsafe(value).AsInt32());
#endif
            return Unsafe.As<float, int>(ref value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once RedundantUnsafeContext
        public static unsafe long DoubleToInt64Bits(double value)
        {
#if !NETSTANDARD
            // Workaround for https://github.com/dotnet/runtime/issues/11413
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (Sse2.X64.IsSupported)
                return Sse2.X64.ConvertToInt64(Vector128.CreateScalarUnsafe(value).AsInt64());
#endif
            return Unsafe.As<double, long>(ref value);
        }

        /// <summary>
        /// Writes a <see cref="double" /> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="destination" /> is too small to contain a <see cref="double" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDoubleLittleEndian(BigSpan<byte> destination, double value)
        {
            if (!BitConverter.IsLittleEndian)
                destination.Write(ReverseEndianness(DoubleToInt64Bits(value)));
            else
                destination.Write(value);
        }

        /// <summary>
        /// Writes an Int16 into a span of bytes as little endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt16LittleEndian(BigSpan<byte> destination, short value)
            => destination.Write(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Writes an Int32 into a span of bytes as little endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt32LittleEndian(BigSpan<byte> destination, int value)
            => destination.Write(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Writes an Int64 into a span of bytes as little endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteInt64LittleEndian(BigSpan<byte> destination, long value)
            => destination.Write(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Writes a <see cref="float" /> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="destination" /> is too small to contain a <see cref="float" />.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteSingleLittleEndian(BigSpan<byte> destination, float value)
        {
            if (!BitConverter.IsLittleEndian)
                destination.Write(ReverseEndianness(SingleToInt32Bits(value)));
            else
                destination.Write(value);
        }

        /// <summary>
        /// Write a UInt16 into a span of bytes as little endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt16LittleEndian(BigSpan<byte> destination, ushort value)
            => destination.Write(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Write a UInt32 into a span of bytes as little endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt32LittleEndian(BigSpan<byte> destination, uint value)
            => destination.Write(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Write a UInt64 into a span of bytes as little endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUInt64LittleEndian(BigSpan<byte> destination, ulong value)
            => destination.Write(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Writes a <see cref="double" /> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns>
        /// <see langword="true" /> if the span is large enough to contain a <see cref="double" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteDoubleLittleEndian(BigSpan<byte> destination, double value)
            => !BitConverter.IsLittleEndian
                ? destination.TryWrite(ReverseEndianness(DoubleToInt64Bits(value)))
                : destination.TryWrite(value);

        /// <summary>
        /// Writes an Int16 into a span of bytes as little endian.
        /// </summary>
        /// <returns>If the span is too small to contain the value, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt16LittleEndian(BigSpan<byte> destination, short value)
            => destination.TryWrite(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Writes an Int32 into a span of bytes as little endian.
        /// </summary>
        /// <returns>If the span is too small to contain the value, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt32LittleEndian(BigSpan<byte> destination, int value)
            => destination.TryWrite(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Writes an Int64 into a span of bytes as little endian.
        /// </summary>
        /// <returns>If the span is too small to contain the value, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteInt64LittleEndian(BigSpan<byte> destination, long value)
            => destination.TryWrite(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Writes a <see cref="float" /> into a span of bytes, as little endian.
        /// </summary>
        /// <param name="destination">The span of bytes where the value is to be written, as little endian.</param>
        /// <param name="value">The value to write into the span of bytes.</param>
        /// <returns>
        /// <see langword="true" /> if the span is large enough to contain a <see cref="float" />; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteSingleLittleEndian(BigSpan<byte> destination, float value)
            => BitConverter.IsLittleEndian
                ? destination.TryWrite(value)
                : destination.TryWrite(ReverseEndianness(SingleToInt32Bits(value)));

        /// <summary>
        /// Write a UInt16 into a span of bytes as little endian.
        /// </summary>
        /// <returns>If the span is too small to contain the value, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt16LittleEndian(BigSpan<byte> destination, ushort value)
            => destination.TryWrite(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Write a UInt32 into a span of bytes as little endian.
        /// </summary>
        /// <returns>If the span is too small to contain the value, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt32LittleEndian(BigSpan<byte> destination, uint value)
            => destination.TryWrite(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));

        /// <summary>
        /// Write a UInt64 into a span of bytes as little endian.
        /// </summary>
        /// <returns>If the span is too small to contain the value, return false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWriteUInt64LittleEndian(BigSpan<byte> destination, ulong value)
            => destination.TryWrite(BitConverter.IsLittleEndian ? value : ReverseEndianness(value));
    }
}
