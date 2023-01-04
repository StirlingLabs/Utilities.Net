using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public static class HexEncoding
{
    public static bool IsHexDigit(this char c)
        => c is >= '0' and <= '9'
            or >= 'A' and <= 'F'
            or >= 'a' and <= 'f';

    public static bool IsHexDigitAscii(this byte c)
        => c is >= (byte)'0' and <= (byte)'9'
            or >= (byte)'A' and <= (byte)'F'
            or >= (byte)'a' and <= (byte)'f';

    public static Span<byte> ToBytes(ReadOnlySpan<byte> hexString, Span<byte> buffer)
    {
        if (hexString.IsEmpty) return buffer;

        if (buffer.Length < hexString.Length / 2)
            throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, "Buffer too small");

        for (var i = 0; i + 1 < hexString.Length; i += 2)
        {
            var chHi = hexString[i];
            var chLo = hexString[i + 1];
            if (!chLo.IsHexDigitAscii() || !chHi.IsHexDigitAscii())
                throw new ArgumentOutOfRangeException(nameof(hexString), "Invalid hex string");
            buffer[i / 2] = (byte)(
                (((chHi & 0xF) << 4) + ((chHi & 0x40) >> 2) * 9)
                | ((chLo & 0xF) + ((chLo & 0x40) >> 6) * 9)
            );
        }

        return buffer;
    }

    public static byte[] ToBytes(ReadOnlySpan<byte> hexString)
    {
        if (hexString.IsEmpty) return Array.Empty<byte>();

        var buf = new byte[hexString.Length / 2];
        ToBytes(hexString, buf);
        return buf;
    }

    public static Span<byte> ToBytes(ReadOnlySpan<char> hexString, Span<byte> buffer)
    {
        if (hexString.IsEmpty) return buffer;

        if (buffer.Length < hexString.Length / 2)
            throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, "Buffer too small");

        for (var i = 0; i + 1 < hexString.Length; i += 2)
        {
            var chHi = hexString[i];
            var chLo = hexString[i + 1];
            if (!chLo.IsHexDigit() || !chHi.IsHexDigit())
                throw new ArgumentOutOfRangeException(nameof(hexString), "Invalid hex string");
            buffer[i / 2] = (byte)(
                (((chHi & 0xF) << 4) + ((chHi & 0x40) >> 2) * 9)
                | ((chLo & 0xF) + ((chLo & 0x40) >> 6) * 9)
            );
        }

        return buffer;
    }

    public static byte[] ToBytes(ReadOnlySpan<char> hexString)
    {
        if (hexString.IsEmpty) return Array.Empty<byte>();

        var buf = new byte[hexString.Length / 2];
        ToBytes(hexString, buf);
        return buf;
    }

    public static Span<byte> ToBytes(string hexString, Span<byte> buffer)
    {
#if NETSTANDARD2_0

        if (hexString is null) throw new ArgumentNullException(hexString);

        if (buffer.Length < hexString.Length / 2)
            throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, "Buffer too small");

        using var hexStrEnum = hexString.GetEnumerator();
        for (var i = 0; i + 1 < hexString.Length; i += 2)
        {
            hexStrEnum.MoveNext();
            var chHi = hexStrEnum.Current;
            hexStrEnum.MoveNext();
            var chLo = hexStrEnum.Current;
            if (!chLo.IsHexDigit() || !chHi.IsHexDigit())
                throw new ArgumentOutOfRangeException(nameof(hexString), "Invalid hex string");
            buffer[i / 2] = (byte)(
                (((chHi & 0xF) << 4) + ((chHi & 0x40) >> 2) * 9)
                | ((chLo & 0xF) + ((chLo & 0x40) >> 6) * 9)
            );
        }

        return buffer;
#else
        return ToBytes((ReadOnlySpan<char>)hexString, buffer);
#endif
    }

    public static byte[] ToBytes(string hexString)
    {
        if (hexString is null) throw new ArgumentNullException(hexString);

        var buf = new byte[hexString.Length / 2];
        ToBytes(hexString, buf);
        return buf;
    }

    public static unsafe string ToHexString(this ReadOnlySpan<byte> data)
    {

        var args = (
            p: (IntPtr)Unsafe.AsPointer(ref Unsafe.AsRef(data.GetPinnableReference())),
            l: data.Length
        );
#if NETSTANDARD2_0
        var strLen = data.Length * 2;
        var s = new string('\0', strLen);
        fixed (char* pStr = s)
            CreateUpperCaseString(new(pStr, strLen), args);
        return s;
#else
        return string.Create(data.Length * 2, args, CreateUpperCaseString);
#endif
    }


    public static unsafe string ToHexString(this ReadOnlySpan<byte> data, bool lowerCase)
    {

        var args = (
            p: (IntPtr)Unsafe.AsPointer(ref Unsafe.AsRef(data.GetPinnableReference())),
            l: data.Length
        );
#if NETSTANDARD2_0
        var strLen = data.Length * 2;
        var s = new string('\0', strLen);
        fixed (char* pStr = s)
            if (lowerCase)
                CreateLowerCaseString(new(pStr, strLen), args);
            else
                CreateUpperCaseString(new(pStr, strLen), args);
        return s;
#else
        return string.Create(data.Length * 2, args, lowerCase ? CreateLowerCaseString : CreateUpperCaseString);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void CreateUpperCaseString(Span<char> c, (IntPtr p, int l) args)
    {
        var (p, l) = args;
        var s = new ReadOnlySpan<byte>((void*)p, l);
        var u = MemoryMarshal.Cast<char, int>(c);
        for (var i = 0; i < l; ++i)
        {
            var b = s[i];
            var nibLo = b >> 4;
            var isDigLo = (nibLo - 10) >> 31;
            var chLo = 0x37 + nibLo + (isDigLo & -0x7);
            var nibHi = b & 0xF;
            var isDigHi = (nibHi - 10) >> 31;
            var chHi = 0x37 + nibHi + (isDigHi & -0x7);
            u[i] = (chHi << 16) | chLo;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void CreateLowerCaseString(Span<char> c, (IntPtr p, int l) args)
    {
        var (p, l) = args;
        var s = new ReadOnlySpan<byte>((void*)p, l);
        var u = MemoryMarshal.Cast<char, int>(c);
        for (var i = 0; i < l; ++i)
        {
            var b = s[i];
            var nibLo = b >> 4;
            var isDigLo = (nibLo - 10) >> 31;
            var chLo = 0x57 + nibLo + (isDigLo & -0x27);
            var nibHi = b & 0xF;
            var isDigHi = (nibHi - 10) >> 31;
            var chHi = 0x57 + nibHi + (isDigHi & -0x27);
            u[i] = (chHi << 16) | chLo;
        }
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this Span<byte> bytes) => ToHexString((ReadOnlySpan<byte>)bytes);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this byte[] bytes) => ToHexString((ReadOnlySpan<byte>)bytes);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this Memory<byte> bytes) => ToHexString(bytes.Span);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this Span<byte> bytes, bool lowerCase) => ToHexString((ReadOnlySpan<byte>)bytes, lowerCase);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this byte[] bytes, bool lowerCase) => ToHexString((ReadOnlySpan<byte>)bytes, lowerCase);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexString(this Memory<byte> bytes, bool lowerCase) => ToHexString(bytes.Span, lowerCase);
}
