using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace StirlingLabs.Utilities
{
    public class InsecureRandomNumberGenerator : RandomNumberGenerator
    {
        private static readonly Random R = new Random(unchecked((int)Stopwatch.GetTimestamp()));

        public override void GetBytes(Span<byte> data)
            => R.NextBytes(data);

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
