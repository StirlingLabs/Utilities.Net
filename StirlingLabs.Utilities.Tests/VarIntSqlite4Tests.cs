using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using NUnit.Framework;
using StirlingLabs.Utilities.Assertions;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.All)]
public class VarIntSqlite4Tests
{
    [DatapointSource]
    [UsedImplicitly]
    public static VarIntSqlite4TestCase[] WellKnownTestCases =
    {
        new(0, new byte[] { 0 }),
        new(1, new byte[] { 1 }),
        new(240, new byte[] { 0xF0 }),
        new(241, new byte[] { 0xF1, 0x01 }),
        new(2287, new byte[] { 0xF8, 0xFF }),
        new(2288, new byte[] { 0xF9, 0x00, 0x00 }),
        new(0x108EF, new byte[] { 0xF9, 0xFF, 0xFF }),
        new(0x108F0, new byte[] { 0xFA, 0x01, 0x08, 0xF0 }),
        new(0xffffffuL, new byte[] { 0xFA, 0xFF, 0xFF, 0xFF }),
        new(0x1000000uL, new byte[] { 0xFB, 0x01, 0x00, 0x00, 0x00 }),
        new(0xffffffffuL, new byte[] { 0xFB, 0xFF, 0xFF, 0xFF, 0xFF }),
        new(0x100000000uL, new byte[] { 0xFC, 0x01, 0x00, 0x00, 0x00, 0x00 }),
        new(0xffffffffffuL, new byte[] { 0xFC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }),
        new(0x10000000000uL, new byte[] { 0xFD, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00 }),
        new(0xffffffffffffuL, new byte[] { 0xFD, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }),
        new(0x1000000000000uL, new byte[] { 0xFE, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),
        new(0xffffffffffffffuL, new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }),
        new(0x100000000000000uL, new byte[] { 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }),
        new(0xffffffffffffffffuL, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }),
    };

    [DatapointSource]
    [UsedImplicitly]
    public static IEnumerable<ulong> SomeFuzz
    {
        // ReSharper disable once CognitiveComplexity
        get {
            yield return 0;

            for (var i = 0; i < 64; ++i)
                yield return ulong.MaxValue >> i;

            for (var i = 0; i < 64; ++i)
            {
                yield return 1uL << i;
                for (var j = 0; j < 64; ++j)
                {
                    if (i == j)
                        continue;

                    yield return (1uL << i) | (1uL << j);
                }
            }
        }
    }

    [Theory]
    public void GetDecodedLengthTest(VarIntSqlite4TestCase testCase)
        => Assert.AreEqual(testCase.Encoded.Length, VarIntSqlite4.GetDecodedLength(testCase.Encoded[0]));

    [Theory]
    public void GetEncodedLengthTest(VarIntSqlite4TestCase testCase)
        => Assert.AreEqual(testCase.Encoded.Length, VarIntSqlite4.GetEncodedLength(testCase.Decoded));

    [Theory]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void RoundTripTest(ulong value)
    {
        Span<byte> result = stackalloc byte[9];

        var length = VarIntSqlite4.Encode(value, result);

        ReadOnlySpan<byte> encoded = result.Slice(0, length);

        var decoded = VarIntSqlite4.Decode(encoded);

        Assert.AreEqual(value, decoded);
    }
    
    [Test]
    public void RoundTripPerfTest()
    {
        RoundTripTest(0);

        for (var i = 0; i < 64; ++i)
            RoundTripTest(ulong.MaxValue >> i);

        for (var i = 0; i < 64; ++i)
        {
            RoundTripTest( 1uL << i );
            for (var j = 0; j < 64; ++j)
            {
                if (i == j)
                    continue;

                RoundTripTest( (1uL << i) | (1uL << j));
            }
        }
    }

    [Theory]
    public unsafe void EncodeTest(VarIntSqlite4TestCase testCase)
    {
        var expectedLength = testCase.Encoded.Length;
        Span<byte> result = stackalloc byte[9];
        var actualLength = VarIntSqlite4.Encode(testCase.Decoded, result);

        Assert.AreEqual(expectedLength, actualLength);

        StringAssert.AreEqualIgnoringCase(
            testCase.Encoded.ToHexString(),
            result.Slice(0, actualLength).ToHexString());
    }


    [Theory]
    public void DecodeTest(VarIntSqlite4TestCase testCase)
    {
        var result = VarIntSqlite4.Decode(testCase.Encoded);

        Assert.AreEqual(testCase.Decoded, result);
    }
}
