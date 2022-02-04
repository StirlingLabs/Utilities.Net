using System;
using System.Runtime.CompilerServices;
using static System.BitConverter;

namespace StirlingLabs.Utilities;

public static partial class BinaryPrimitives
{
        
    /// <summary>
    /// Writes a <see cref="double" /> into a span of bytes, as big endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
    /// <param name="value">The value to write into the span of bytes.</param>
    /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="destination" /> is too small to contain a <see cref="double" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteDoubleBigEndian(BigSpan<byte> destination, double value)
    {
        if (IsLittleEndian)
            destination.Write(ReverseEndianness(DoubleToInt64Bits(value)));
        else
            destination.Write(value);
    }

    /// <summary>
    /// Writes an Int16 into a span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt16BigEndian(BigSpan<byte> destination, short value)
        => destination.Write(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Writes an Int32 into a span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32BigEndian(BigSpan<byte> destination, int value)
        => destination.Write(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Writes an Int64 into a span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt64BigEndian(BigSpan<byte> destination, long value)
        => destination.Write(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Writes a <see cref="float" /> into a span of bytes, as big endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
    /// <param name="value">The value to write into the span of bytes.</param>
    /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="destination" /> is too small to contain a <see cref="float" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSingleBigEndian(BigSpan<byte> destination, float value)
    {
        if (IsLittleEndian)
            destination.Write(ReverseEndianness(SingleToInt32Bits(value)));
        else
            destination.Write(value);
    }

    /// <summary>
    /// Write a UInt16 into a span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16BigEndian(BigSpan<byte> destination, ushort value)
        => destination.Write(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Write a UInt32 into a span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt32BigEndian(BigSpan<byte> destination, uint value)
        => destination.Write(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Write a UInt64 into a span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt64BigEndian(BigSpan<byte> destination, ulong value)
        => destination.Write(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Writes a <see cref="double" /> into a span of bytes, as big endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
    /// <param name="value">The value to write into the span of bytes.</param>
    /// <returns>
    /// <see langword="true" /> if the span is large enough to contain a <see cref="double" />; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>Writes exactly 8 bytes to the beginning of the span.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteDoubleBigEndian(BigSpan<byte> destination, double value)
        => IsLittleEndian
            ? destination.TryWrite(ReverseEndianness(DoubleToInt64Bits(value)))
            : destination.TryWrite(value);

    /// <summary>
    /// Writes an Int16 into a span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain the value, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteInt16BigEndian(BigSpan<byte> destination, short value)
        => destination.TryWrite(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Writes an Int32 into a span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain the value, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteInt32BigEndian(BigSpan<byte> destination, int value)
        => destination.TryWrite(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Writes an Int64 into a span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain the value, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteInt64BigEndian(BigSpan<byte> destination, long value)
        => destination.TryWrite(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Writes a <see cref="float" /> into a span of bytes, as big endian.
    /// </summary>
    /// <param name="destination">The span of bytes where the value is to be written, as big endian.</param>
    /// <param name="value">The value to write into the span of bytes.</param>
    /// <returns>
    /// <see langword="true" /> if the span is large enough to contain a <see cref="float" />; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>Writes exactly 4 bytes to the beginning of the span.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteSingleBigEndian(BigSpan<byte> destination, float value)
        => IsLittleEndian
            ? destination.TryWrite(value)
            : destination.TryWrite(ReverseEndianness(SingleToInt32Bits(value)));

    /// <summary>
    /// Write a UInt16 into a span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain the value, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteUInt16BigEndian(BigSpan<byte> destination, ushort value)
        => destination.TryWrite(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Write a UInt32 into a span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain the value, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteUInt32BigEndian(BigSpan<byte> destination, uint value)
        => destination.TryWrite(IsLittleEndian ? value : ReverseEndianness(value));

    /// <summary>
    /// Write a UInt64 into a span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain the value, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteUInt64BigEndian(BigSpan<byte> destination, ulong value)
        => destination.TryWrite(IsLittleEndian ? value : ReverseEndianness(value));
}