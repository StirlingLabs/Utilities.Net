using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public static class HexEncoding
{
    public static Span<byte> ToBytes(string hexString, Span<byte> buffer)
    {
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
            buffer[i / 2] = (byte)(
                (((chHi & 0xF) << 4) + ((chHi & 0x40) >> 2) * 9)
                | ((chLo & 0xF) + ((chLo & 0x40) >> 6) * 9)
            );
        }

        return buffer;
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
                CreateString(new(pStr, strLen), args);
            return s;
#else
        return string.Create(data.Length * 2, args, CreateString);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void CreateString(Span<char> c, (IntPtr p, int l) args)
    {
        var (p, l) = args;
        var s = new ReadOnlySpan<byte>((void*)p, l);
        var u = MemoryMarshal.Cast<char, int>(c);
        for (var i = 0; i < l; ++i)
        {
            var b = s[i];
            var nibLo = b >> 4;
            var isDigLo = (nibLo - 10) >> 31;
            var chLo = 55 + nibLo + (isDigLo & -7);
            var nibHi = b & 0xF;
            var isDigHi = (nibHi - 10) >> 31;
            var chHi = 55 + nibHi + (isDigHi & -7);
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
}