using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using static System.BitConverter;

namespace StirlingLabs.Utilities;

using static BinaryPrimitives;

// ReSharper disable CommentTypo
/// <summary>
/// SQLite 4 style variable length integer codec.
/// </summary>
/// <remarks>
/// (From <a href="https://www.sqlite.org/src4/doc/trunk/www/varint.wiki">SQLite 4 documentation on varints</a>)
/// 
/// A variable length integer is an encoding of 64-bit unsigned integers into between 1 and 9 bytes.
/// 
/// The encoding has the following properties:
/// 1. Smaller (and more common) values use fewer bytes and take up less space than larger (and less common) values.
/// 2. The length of any varint can be determined by looking at just the first byte of the encoding.
/// 3. Lexicographical and numeric ordering for varints are the same. Hence if a group of varints are
///    order lexicographically (that is to say, if they are order by memcmp() with shorter varints coming first)
///    then those varints will also be in numeric order. This property means that varints can be used as keys in
///    the key/value backend storage and the records will occur in numerical order of the keys.
///
/// The encoding is described by algorithms to decode (convert from varint to 8-byte unsigned integer)
/// and to encode (convert from 8-byte unsigned integer to varint).
/// Treat each byte of the encoding as an unsigned integer between 0 and 255.
/// Let the bytes of the encoding be called A0, A1, A2, ..., A8.
/// </remarks>
// ReSharper restore CommentTypo
[PublicAPI]
public static class VarIntSqlite4
{
    /// <summary>
    /// Uses the first byte of an encoded value to determine the length in bytes.
    /// </summary>
    /// <param name="firstByte">The first byte of the value.</param>
    /// <returns>Byte count of the encoded value including this byte, can be 1 to 9 inclusively.</returns>
    [ValueRange(1, 9)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetDecodedLength(byte firstByte)
        => firstByte switch
        {
            < 241 => 1,
            < 249 => 2,
            _ => firstByte - 246
        };

    /// <summary>
    /// Determines the encoded byte length of a value.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>Byte count of the decoded value, can be 1 to 9 inclusively.</returns>
    [ValueRange(1, 9)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetEncodedLength(ulong value)
        => value switch
        {
            <= 240u => 1,
            <= 2287u => 2,
            <= 67823u => 3,
            <= 16777215u => 4,
            <= 4294967295u => 5,
            <= 1099511627775u => 6,
            <= 281474976710655u => 7,
            <= 72057594037927935u => 8,
            _ => 9
        };

    /// <remarks>Please use the safe <see cref="Decode"/> method instead.</remarks>
    /// <seealso cref="Decode"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1045", Justification = "Unsafe variant")]
    public static ulong DecodeUnsafe(ref byte r, int l)
    {
        var a0 = Read<byte>(ref r);

        switch (l)
        {
            case 1: return a0;
            case 2: {
                ulong a1 = Read<byte>(ref r, 1);
                return 240uL + 256uL * (a0 - 241uL) + a1;
            }
            case 3: {
                ulong a1 = Read<byte>(ref r, 1);
                ulong a2 = Read<byte>(ref r, 1 + 1);
                return 2288uL + 256uL * a1 + a2;
            }
            case 4: {
                ulong a = Read<uint>(ref r);
                if (IsLittleEndian)
                    a = ReverseEndianness(a >> 8) >> (8 + 32);
                else
                    a &= 0x00FFFFFFuL;
                return a;
            }
            case 5: {
                return ReadBigEndian<uint>(ref r, 1);
            }
            case 6: {
                ulong a1To4 = ReadBigEndian<uint>(ref r, 1);
                ulong a5 = Read<byte>(ref r, 1 + 4);
                return (a1To4 << 8) | a5;
            }
            case 7: {
                ulong a1To4 = ReadBigEndian<uint>(ref r, 1);
                ulong a5A6 = ReadBigEndian<ushort>(ref r, 1 + 4);
                return (a1To4 << 16) | a5A6;
            }
            case 8: {
                var a = Read<ulong>(ref r); // a0 to a8
                if (IsLittleEndian)
                    a = ReverseEndianness(a >> 8) >> 8;
                else
                    a &= 0x00FFFFFFFFFFFFFFuL;
                return a;
            }
            case 9: {
                return ReadBigEndian<ulong>(ref r, 1);
            }
            // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
            default: return 0; // impossible
        }
    }

    /// <remarks>Please use the safe <see cref="Encode"/> method instead.</remarks>
    /// <seealso cref="Encode"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1045", Justification = "Unsafe variant")]
    [SuppressMessage("ReSharper", "CognitiveComplexity", Justification = "Performance")]
    public static void EncodeUnsafe(ulong value, int encodedLength, ref byte result)
    {
        switch (encodedLength)
        {
            case 1: {
                result = (byte)value; // a0
                break;
            }
            case 2: {
                ushort a = (byte)((value - 240) / 256 + 241); // a0
                a |= (ushort)(((value - 240u) % 256u) << 8); // a1
                Write(ref result, a);
                break;
            }
            case 3: {
                ushort a = 249; // a0
                a |= (ushort)(((value - 2288) / 256) << 8); // a1
                var a2 = (byte)((value - 2288) % 256); // a2
                Write(ref result, a);
                Write(ref result, 2, a2);
                break;
            }
            case 4: {
                var a = value;
                if (IsLittleEndian)
                    a = ReverseEndianness(value);
                a >>= (8 - (3 + 1)) * 8;
                Write(ref result, (uint)a | 250); // a0 to a3
                break;
            }
            case 5: {
                result = 251; // a0
                var a = value;
                if (IsLittleEndian)
                    a = ReverseEndianness(value);
                a >>= (8 - 4) * 8;
                Write(ref result, 1, (uint)a); // a1 to a4
                break;
            }
            case 6: {
                // TODO: optimize to uint + ushort write
                //result = 252; // a0
                var a = value;
                if (IsLittleEndian)
                    a = ReverseEndianness(value);
                a >>= (8 - (5 + 1)) * 8;
                Write(ref result, (uint)a | 252);
                Write(ref result, 4, (ushort)(a >> 32));
                break;
            }
            case 7: {
                result = 253; // a0
                var a = value;
                if (IsLittleEndian)
                    a = ReverseEndianness(value);
                a >>= (8 - 6) * 8;
                Write(ref result, 1, (uint)a);
                Write(ref result, 5, (ushort)(a >> 32));
                break;
            }
            case 8: {
                var a = value;
                if (IsLittleEndian)
                    a = ReverseEndianness(value);
                Write(ref result, a | 254); // a0 to a8
                break;
            }
            case 9: {
                result = 255; // a0
                var a = value;
                if (IsLittleEndian)
                    a = ReverseEndianness(value);
                Write(ref result, 1, a);
                break;
            }
            // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
            default: return; // impossible
        }
    }

    /// <summary>
    /// Decodes a value from a byte span.
    /// </summary>
    /// <param name="data">The byte span to read from.</param>
    /// <returns>A decoded value.</returns>
    /// <exception cref="ArgumentException">The span does not contain sufficient bytes to decode the value.</exception>
    public static ulong Decode(ReadOnlySpan<byte> data)
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static ArgumentException DataSpanTooShortException(int missing)
            => new($"Missing {missing} byte(s).", nameof(data));

        ref var r = ref MemoryMarshal.GetReference(data);

        var a0 = Read<byte>(ref r);
        var l = GetDecodedLength(a0);

        var ext = data.Length - l;
        if (ext < 0) throw DataSpanTooShortException(-ext);

        return DecodeUnsafe(ref r, l);
    }

    /// <summary>
    /// Encodes a value into a byte span.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <param name="result">The byte span to be written to.</param>
    /// <returns>The length in bytes of the encoded value.</returns>
    /// <exception cref="ArgumentException">The span is too short to contain the encoded value.</exception>
    public static int Encode(ulong value, Span<byte> result)
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static ArgumentException ResultSpanTooShortException(int missing)
            => new($"Missing {missing} byte(s).", nameof(result));

        var l = GetEncodedLength(value);

        var ext = result.Length - l;
        if (ext < 0) throw ResultSpanTooShortException(-ext);

        EncodeUnsafe(value, l, ref MemoryMarshal.GetReference(result));

        return l;
    }
}
