using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static System.BitConverter;
using static System.Runtime.CompilerServices.Unsafe;
using static System.Runtime.InteropServices.MemoryMarshal;

namespace StirlingLabs.Utilities;

[PublicAPI]
[SuppressMessage("Design", "CA1045", Justification = "Low level primitives")]
public static partial class BinaryPrimitives
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(ReadOnlySpan<byte> data) where T : unmanaged
        => Read<T>(ref AsRef(GetReference(data)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(ref byte r) where T : unmanaged
        => As<byte, T>(ref r);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(ReadOnlySpan<byte> data, int offset) where T : unmanaged
        => Read<T>(ref AsRef(GetReference(data)), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(ref byte r, int offset) where T : unmanaged
        => As<byte, T>(ref Add(ref r, offset));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadBigEndian<T>(ReadOnlySpan<byte> data) where T : unmanaged
        => Read<T>(ref AsRef(GetReference(data)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadBigEndian<T>(ref byte r) where T : unmanaged
    {
        var v = As<byte, T>(ref r);
        return IsLittleEndian ? ReverseEndianness(v) : v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadBigEndian<T>(ReadOnlySpan<byte> data, int offset) where T : unmanaged
        => ReadBigEndian<T>(ref AsRef(GetReference(data)), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadBigEndian<T>(ref byte r, int offset) where T : unmanaged
    {
        var v = As<byte, T>(ref Add(ref r, offset));
        return IsLittleEndian ? ReverseEndianness(v) : v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadLittleEndian<T>(ReadOnlySpan<byte> data) where T : unmanaged
        => Read<T>(ref AsRef(GetReference(data)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadLittleEndian<T>(ref byte r) where T : unmanaged
    {
        var v = As<byte, T>(ref r);
        return !IsLittleEndian ? ReverseEndianness(v) : v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadLittleEndian<T>(ReadOnlySpan<byte> data, int offset) where T : unmanaged
        => ReadLittleEndian<T>(ref AsRef(GetReference(data)), offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadLittleEndian<T>(ref byte r, int offset) where T : unmanaged
    {
        var v = As<byte, T>(ref Add(ref r, offset));
        return !IsLittleEndian ? ReverseEndianness(v) : v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(Span<byte> data, T value) where T : unmanaged
        => Write(ref GetReference(data), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(Span<byte> data, int offset, T value) where T : unmanaged
        => Write(ref GetReference(data), offset, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(ref byte r, T value) where T : unmanaged
        => WriteUnaligned(ref r, value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(ref byte r, int offset, T value) where T : unmanaged
        => WriteUnaligned(ref Add(ref r, offset), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBigEndian<T>(Span<byte> data, T value) where T : unmanaged
        => WriteBigEndian(ref GetReference(data), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBigEndian<T>(Span<byte> data, int offset, T value) where T : unmanaged
        => WriteBigEndian(ref GetReference(data), offset, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBigEndian<T>(ref byte r, T value) where T : unmanaged
        => WriteUnaligned(ref r,
            IsLittleEndian ? ReverseEndianness(value) : value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBigEndian<T>(ref byte r, int offset, T value) where T : unmanaged
        => WriteUnaligned(ref Add(ref r, offset),
            IsLittleEndian ? ReverseEndianness(value) : value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLittleEndian<T>(Span<byte> data, T value) where T : unmanaged
        => WriteLittleEndian(ref GetReference(data), value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLittleEndian<T>(Span<byte> data, int offset, T value) where T : unmanaged
        => WriteLittleEndian(ref GetReference(data), offset, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLittleEndian<T>(ref byte r, T value) where T : unmanaged
        => WriteUnaligned(ref r,
            !IsLittleEndian ? ReverseEndianness(value) : value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteLittleEndian<T>(ref byte r, int offset, T value) where T : unmanaged
        => WriteUnaligned(ref Add(ref r, offset),
            !IsLittleEndian ? ReverseEndianness(value) : value);
}
