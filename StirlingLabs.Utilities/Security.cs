using System;
using System.Security.Cryptography;
using JetBrains.Annotations;


namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class Security
    {
        public static void FillWithRandomData(this BigSpan<byte> span)
        {
            nuint index = 0;
            var bufferRemaining = span.Length;
            while (bufferRemaining > int.MaxValue)
            {

                RandomNumberGenerator.Fill(span.Slice(index, int.MaxValue));
                index += int.MaxValue;
                bufferRemaining -= int.MaxValue;
            }
            if (bufferRemaining > 0)
                RandomNumberGenerator.Fill(span.Slice(index, (int)bufferRemaining));
        }
        public static void FillWithRandomData(this BigSpan<byte> span, RandomNumberGenerator rng)
        {
            nuint index = 0;
            var bufferRemaining = span.Length;
            while (bufferRemaining > int.MaxValue)
            {
                rng.GetBytes(span.Slice(index, int.MaxValue));
                index += int.MaxValue;
                bufferRemaining -= int.MaxValue;
            }
            if (bufferRemaining > 0)
                rng.GetBytes(span.Slice(index, (int)bufferRemaining));
        }
        public static void FillWithNonZeroRandomData(this BigSpan<byte> span, RandomNumberGenerator? rng = null)
        {
            rng ??= RandomNumberGenerator.Create();
            nuint index = 0;
            var bufferRemaining = span.Length;
            while (bufferRemaining > int.MaxValue)
            {
                rng.GetNonZeroBytes(span.Slice(index, int.MaxValue));
                index += int.MaxValue;
                bufferRemaining -= int.MaxValue;
            }
            if (bufferRemaining > 0)
                rng.GetNonZeroBytes(span.Slice(index, (int)bufferRemaining));
        }

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
    }
}
