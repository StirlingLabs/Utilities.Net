#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace StirlingLabs.Utilities
{
    [SuppressMessage("Security", "CA5394", Justification = "Intentional")]
    public class InsecureRandomNumberGenerator : RandomNumberGenerator
    {
        private static readonly Random R = new(unchecked((int)Stopwatch.GetTimestamp()));

        public override void GetBytes(Span<byte> data)
        {
            R.NextBytes(data);
        }

        public override void GetBytes(byte[] data)
            => GetBytes((Span<byte>)data);


        public override void GetNonZeroBytes(Span<byte> data)
        {
            R.NextBytes(data);
            for (var i = 0; i < data.Length; ++i)
            {
                ref var b = ref data[i];
                if (b == 0)
                    b = (byte)R.Next(1, 255);
            }
        }

        public override void GetNonZeroBytes(byte[] data)
            => GetNonZeroBytes((Span<byte>)data);
    }
}
#endif
