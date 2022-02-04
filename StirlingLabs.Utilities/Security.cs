using System;
using System.Diagnostics;
using System.Security.Cryptography;
using JetBrains.Annotations;


namespace StirlingLabs.Utilities;

[PublicAPI]
public static class Security
{
        
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public static unsafe void FillWithRandomData(this BigSpan<byte> span)
    {
        nuint index = 0;
        var bufferRemaining = span.Length;
        var maxBuffer = (uint)int.MaxValue;

        while (bufferRemaining > maxBuffer)
        {
            RandomNumberGenerator.Fill(span.Slice(index, (int)maxBuffer));
            index += maxBuffer;
            bufferRemaining -= maxBuffer;
        }
        Debug.Assert(bufferRemaining < maxBuffer);
        Debug.Assert(bufferRemaining < int.MaxValue);
        if (bufferRemaining > 0)
            RandomNumberGenerator.Fill(span.Slice(index, (int)bufferRemaining));
    }

    public static unsafe void FillWithRandomData(this BigSpan<byte> span, RandomNumberGenerator rng)
    {
        nuint index = 0;
        var bufferRemaining = span.Length;
        var maxBuffer = (uint)int.MaxValue;

        while (bufferRemaining > maxBuffer)
        {
            rng.GetBytes(span.Slice(index, (int)maxBuffer));
            index += maxBuffer;
            bufferRemaining -= maxBuffer;
        }
        Debug.Assert(bufferRemaining < maxBuffer);
        Debug.Assert(bufferRemaining < int.MaxValue);
        if (bufferRemaining > 0)
            rng.GetBytes(span.Slice(index, (int)bufferRemaining));
    }
    public static unsafe void FillWithNonZeroRandomData(this BigSpan<byte> span, RandomNumberGenerator? rng = null)
    {
        rng ??= RandomNumberGenerator.Create();
        nuint index = 0;
        var bufferRemaining = span.Length;
        var maxBuffer = (uint)int.MaxValue;

        while (bufferRemaining > maxBuffer)
        {
            rng.GetNonZeroBytes(span.Slice(index, (int)maxBuffer));
            index += maxBuffer;
            bufferRemaining -= maxBuffer;
        }
        Debug.Assert(bufferRemaining < maxBuffer);
        Debug.Assert(bufferRemaining < int.MaxValue);
        if (bufferRemaining > 0)
            rng.GetNonZeroBytes(span.Slice(index, (int)bufferRemaining));
    }
#endif
        
    // TODO: have XKCP.NET support .NET Standard
#if !NETSTANDARD

    public static byte[] GetHash(byte[] data)
        => Xkcp.Sha3_256(data);

    public static byte[] GetHash(ReadOnlySpan<byte> data)
        => Xkcp.Sha3_256(data);

    public static unsafe byte[] GetHash(ReadOnlyBigSpan<byte> data)
    {
        var numArray = new byte[32];
        fixed (byte* output = numArray)
        fixed (byte* input = data)
        {
            if (Xkcp.Sha3_256(output, input, data.Length) != 0)
                throw new NotImplementedException("Hashing failed.");
            return numArray;
        }
    }
#endif
}