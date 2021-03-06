using System;
using System.Runtime.CompilerServices;
using static System.BitConverter;
using static System.Runtime.CompilerServices.Unsafe;

namespace StirlingLabs.Utilities;

public static partial class BinaryPrimitives
{
    /// <summary>
    /// Reads a <see cref="double" /> from the beginning of a read-only span of bytes, as big endian.
    /// </summary>
    /// <param name="source">The read-only span to read.</param>
    /// <returns>The big endian value.</returns>
    /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="source"/> is too small to contain a <see cref="double" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDoubleBigEndian(ReadOnlyBigSpan<byte> source)
        => IsLittleEndian
            ? Int64BitsToDouble(ReverseEndianness(source.Read<long>()))
            : As<byte, double>(ref source.GetReference());

    /// <summary>
    /// Reads an Int16 out of a read-only span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16BigEndian(ReadOnlyBigSpan<byte> source)
    {
        var result = source.Read<short>();
        return IsLittleEndian ? result : ReverseEndianness(result);
    }

    /// <summary>
    /// Reads an Int32 out of a read-only span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32BigEndian(ReadOnlyBigSpan<byte> source)
    {
        var result = source.Read<int>();
        return IsLittleEndian ? result : ReverseEndianness(result);
    }

    /// <summary>
    /// Reads an Int64 out of a read-only span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64BigEndian(ReadOnlyBigSpan<byte> source)
    {
        var result = source.Read<long>();
        return IsLittleEndian ? result : ReverseEndianness(result);
    }

    /// <summary>
    /// Reads a <see cref="float" /> from the beginning of a read-only span of bytes, as big endian.
    /// </summary>
    /// <param name="source">The read-only span to read.</param>
    /// <returns>The big endian value.</returns>
    /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="source"/> is too small to contain a <see cref="float" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingleBigEndian(ReadOnlyBigSpan<byte> source)
        => IsLittleEndian ? Int32BitsToSingle(ReverseEndianness(source.Read<int>())) : source.Read<float>();

    /// <summary>
    /// Reads a UInt16 out of a read-only span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16BigEndian(ReadOnlyBigSpan<byte> source)
    {
        var result = source.Read<ushort>();
        return IsLittleEndian ? result : ReverseEndianness(result);
    }

    /// <summary>
    /// Reads a UInt32 out of a read-only span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32BigEndian(ReadOnlyBigSpan<byte> source)
    {
        var result = source.Read<uint>();
        return IsLittleEndian ? result : ReverseEndianness(result);
    }

    /// <summary>
    /// Reads a UInt64 out of a read-only span of bytes as big endian.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64BigEndian(ReadOnlyBigSpan<byte> source)
    {
        var result = source.Read<ulong>();
        return IsLittleEndian ? result : ReverseEndianness(result);
    }

    /// <summary>
    /// Reads a <see cref="double" /> from the beginning of a read-only span of bytes, as big endian.
    /// </summary>
    /// <param name="source">The read-only span of bytes to read.</param>
    /// <param name="value">When this method returns, the value read out of the read-only span of bytes, as big endian.</param>
    /// <returns>
    /// <see langword="true" /> if the span is large enough to contain a <see cref="double" />; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>Reads exactly 8 bytes from the beginning of the span.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadDoubleBigEndian(ReadOnlyBigSpan<byte> source, out double value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);

        var success = source.TryRead(out long tmp);
        value = Int64BitsToDouble(ReverseEndianness(tmp));
        return success;
    }

    /// <summary>
    /// Reads an Int16 out of a read-only span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain an Int16, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadInt16BigEndian(ReadOnlyBigSpan<byte> source, out short value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);

        var success = source.TryRead(out short tmp);
        if (success)
            value = ReverseEndianness(tmp);
        else
            SkipInit(out value);
        return success;
    }

    /// <summary>
    /// Reads an Int32 out of a read-only span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain an Int32, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadInt32BigEndian(ReadOnlyBigSpan<byte> source, out int value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);

        var success = source.TryRead(out int tmp);
        if (success)
            value = ReverseEndianness(tmp);
        else
            SkipInit(out value);
        return success;
    }

    /// <summary>
    /// Reads an Int64 out of a read-only span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain an Int64, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadInt64BigEndian(ReadOnlyBigSpan<byte> source, out long value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);

        var success = source.TryRead(out long tmp);
        if (success)
            value = ReverseEndianness(tmp);
        else
            SkipInit(out value);
        return success;
    }

    /// <summary>
    /// Reads a <see cref="float" /> from the beginning of a read-only span of bytes, as big endian.
    /// </summary>
    /// <param name="source">The read-only span of bytes to read.</param>
    /// <param name="value">When this method returns, the value read out of the read-only span of bytes, as big endian.</param>
    /// <returns>
    /// <see langword="true" /> if the span is large enough to contain a <see cref="float" />; otherwise, <see langword="false" />.
    /// </returns>
    /// <remarks>Reads exactly 4 bytes from the beginning of the span.</remarks>
    public static bool TryReadSingleBigEndian(ReadOnlyBigSpan<byte> source, out float value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);
        var success = source.TryRead(out int tmp);
        value = Int32BitsToSingle(ReverseEndianness(tmp));
        return success;
    }

    /// <summary>
    /// Reads a UInt16 out of a read-only span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain a UInt16, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadUInt16BigEndian(ReadOnlyBigSpan<byte> source, out ushort value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);

        var success = source.TryRead(out ushort tmp);
        if (success)
            value = ReverseEndianness(tmp);
        else
            SkipInit(out value);
        return success;
    }

    /// <summary>
    /// Reads a UInt32 out of a read-only span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain a UInt32, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadUInt32BigEndian(ReadOnlyBigSpan<byte> source, out uint value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);

        var success = source.TryRead(out uint tmp);
        if (success)
            value = ReverseEndianness(tmp);
        else
            SkipInit(out value);
        return success;
    }

    /// <summary>
    /// Reads a UInt64 out of a read-only span of bytes as big endian.
    /// </summary>
    /// <returns>If the span is too small to contain a UInt64, return false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryReadUInt64BigEndian(ReadOnlyBigSpan<byte> source, out ulong value)
    {
        if (IsLittleEndian)
            return source.TryRead(out value);

        var success = source.TryRead(out ulong tmp);
        if (success)
            value = ReverseEndianness(tmp);
        else
            SkipInit(out value);
        return success;
    }
}