using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using JetBrains.Annotations;


namespace StirlingLabs.Utilities;

[PublicAPI]
public static class Security
{
#if !(NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
    private static readonly Lazy<RandomNumberGenerator> LazyRng = new(RandomNumberGenerator.Create);
    private static RandomNumberGenerator Rng => LazyRng.Value;
#endif
    public static void FillWithRandomData(this BigSpan<byte> span)
    {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
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
#else
        FillWithRandomData(span, Rng);
#endif
    }

    public static void FillWithRandomData(this BigSpan<byte> span, RandomNumberGenerator rng)
    {
        if (rng is null) throw new ArgumentNullException(nameof(rng));

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        span.AsSmallSlices(rng.GetBytes);
#else
        var remaining = span;

        var requestedSize = (nuint)Math.Min(1024, span.Length);

        var rented = ArrayPool<byte>.Shared.Rent((int)requestedSize);

        requestedSize = (nuint)Math.Min(span.Length, (ulong)rented.LongLength);

        var rentedSpan = new Span<byte>(rented, 0, (int)requestedSize);

        try
        {
            while (remaining.Length >= requestedSize)
            {
                rng.GetBytes(rented, 0, (int)requestedSize);
                rentedSpan.CopyTo(remaining);
                remaining.Advance(requestedSize);
            }

            requestedSize = remaining.Length;

            if (requestedSize <= 0) return;

            rentedSpan = new(rented, 0, (int)requestedSize);

            rng.GetBytes(rented, 0, (int)requestedSize);
            rentedSpan.CopyTo(remaining);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
#endif
    }
    public static void FillWithNonZeroRandomData(this BigSpan<byte> span)
    {
        using var rng = RandomNumberGenerator.Create();

        FillWithNonZeroRandomData(span, rng);
    }

#if !(NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
    private const int NonZeroRngBufferSmallSize = 16;
    private const int NonZeroRngBufferMediumSize = 1024;
    private const int NonZeroRngBufferLargeSize = 65536;
    private static ThreadLocal<byte[]> _nonZeroRngBufferSmall = new(() => new byte[NonZeroRngBufferSmallSize]);
    private static ThreadLocal<byte[]> _nonZeroRngBufferMedium = new(() => new byte[NonZeroRngBufferMediumSize]);
    private static ThreadLocal<byte[]> _nonZeroRngBufferLarge = new(() => new byte[NonZeroRngBufferLargeSize]);
#endif

    public static void FillWithNonZeroRandomData(this BigSpan<byte> span, RandomNumberGenerator rng)
    {
        if (rng is null) throw new ArgumentNullException(nameof(rng));

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        span.AsSmallSlices(rng.GetNonZeroBytes);
#else
        var remaining = span;

        if (remaining.Length >= NonZeroRngBufferLargeSize)
        {
            var buffer = _nonZeroRngBufferLarge.Value;
            do
            {
                rng.GetNonZeroBytes(buffer);
                buffer.CopyTo(remaining);
                remaining.Advance(NonZeroRngBufferLargeSize);
            } while (remaining.Length >= NonZeroRngBufferLargeSize);
        }
            
        if (remaining.Length >= NonZeroRngBufferMediumSize)
        {
            var buffer = _nonZeroRngBufferMedium.Value;
            do
            {
                rng.GetNonZeroBytes(buffer);
                buffer.CopyTo(remaining);
                remaining.Advance(NonZeroRngBufferMediumSize);
            } while (remaining.Length >= NonZeroRngBufferMediumSize);
        }
            
        if (remaining.Length >= NonZeroRngBufferSmallSize)
        {
            var buffer = _nonZeroRngBufferSmall.Value;
            do
            {
                rng.GetNonZeroBytes(buffer);
                buffer.CopyTo(remaining);
                remaining.Advance(NonZeroRngBufferSmallSize);
            } while (remaining.Length >= NonZeroRngBufferSmallSize);
        }
            
        // ReSharper disable once InvertIf
        if (remaining.Length > 0)
        {
            var buffer = _nonZeroRngBufferSmall.Value;
            rng.GetNonZeroBytes(buffer);
            new Span<byte>(buffer,0,(int)remaining.Length).CopyTo(remaining);
        }
#endif

    }

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
