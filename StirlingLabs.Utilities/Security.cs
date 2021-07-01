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
    }
}