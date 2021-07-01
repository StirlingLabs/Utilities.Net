using System;
using System.Buffers;
using System.Security.Cryptography;
using JetBrains.Annotations;
using Org.BouncyCastle.Crypto.Digests;

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
        {
            var hash = new byte[32];
            var sha3 = new Sha3Digest(256);
            sha3.BlockUpdate(data, 0, data.Length);
            sha3.DoFinal(hash, 0);
            return hash;
        }

        public static byte[] GetHash(Span<byte> data, int bufferSize = 32768)
        {
            var hash = new byte[32];
            var sha3 = new Sha3Digest(256);
            var length = data.Length;
            var i = 0;
            if (data.Length > bufferSize)
            {
                var pool = ArrayPool<byte>.Shared;
                var copyBuf = pool.Rent(bufferSize);
                do
                {
                    data.Slice(i, bufferSize).CopyTo(copyBuf);
                    sha3.BlockUpdate(copyBuf, 0, bufferSize);
                    i += bufferSize;
                    length -= bufferSize;
                } while (data.Length > bufferSize);
                if (data.Length > 0)
                {
                    data.Slice(i, length).CopyTo(copyBuf);
                    sha3.BlockUpdate(copyBuf, 0, length);
                }
                pool.Return(copyBuf);
            }
            sha3.DoFinal(hash, 0);
            return hash;
        }

        public static byte[] GetHash(BigSpan<byte> data, uint bufferSize = 32768)
        {
            var bufferSizeInt = checked((int)bufferSize);
            var hash = new byte[32];
            var sha3 = new Sha3Digest(256);
            var length = data.Length;
            nuint i = 0;
            if (data.Length > bufferSize)
            {
                var pool = ArrayPool<byte>.Shared;
                var copyBuf = pool.Rent((int)bufferSize);
                do
                {
                    data.Slice(i, bufferSizeInt).CopyTo(copyBuf);
                    sha3.BlockUpdate(copyBuf, 0, bufferSizeInt);
                    i += bufferSize;
                    length -= bufferSize;
                } while (data.Length > bufferSize);
                if (data.Length > 0)
                {
                    data.Slice(i, length).CopyTo(copyBuf);
                    sha3.BlockUpdate(copyBuf, 0, (int)length);
                }
                pool.Return(copyBuf);
            }
            sha3.DoFinal(hash, 0);
            return hash;
        }
    }
}