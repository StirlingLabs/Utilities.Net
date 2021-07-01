using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class Sha3
    {
        public const int KeccakFRounds = 24;

        private static readonly ulong[] Rndc = new ulong[KeccakFRounds]
        {
            0x0000000000000001, 0x0000000000008082, 0x800000000000808a,
            0x8000000080008000, 0x000000000000808b, 0x0000000080000001,
            0x8000000080008081, 0x8000000000008009, 0x000000000000008a,
            0x0000000000000088, 0x0000000080008009, 0x000000008000000a,
            0x000000008000808b, 0x800000000000008b, 0x8000000000008089,
            0x8000000000008003, 0x8000000000008002, 0x8000000000000080,
            0x000000000000800a, 0x800000008000000a, 0x8000000080008081,
            0x8000000000008080, 0x0000000080000001, 0x8000000080008008
        };

        internal struct Context
        {
            public State st;

            public int pt, rSiz, mdLen;
        }

        [StructLayout(LayoutKind.Sequential, Size = 25 * 8)]
        internal struct State
        {
            public Span<ulong> q
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => MemoryMarshal.Cast<State, ulong>(MemoryMarshal.CreateSpan(ref this, 1));
            }

            public Span<byte> b
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref this, 1));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong RotateLeft(ulong x, int y)
            => System.Numerics.BitOperations.RotateLeft(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref T p<T>(ref this State st, int i)
            => ref Unsafe.Add(ref Unsafe.As<State, T>(ref Unsafe.AsRef(st)), i);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref ulong q(ref this State st, int i)
            => ref st.p<ulong>(i);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref byte b(ref this State st, int i)
            => ref st.p<byte>(i);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void KeccakF(ref this Context ctx)
        {
            // variables

            ref var st = ref ctx.st;

            FlipEndianIfNeeded(st);

            // actual iteration

            for (var r = 0; r < KeccakFRounds; r++)
            {

                // Theta
                ulong bc0, bc1, bc2, bc3, bc4;
                {
                    bc0 = st.q(0) ^ st.q(5) ^ st.q(10) ^ st.q(15) ^ st.q(20);
                    bc1 = st.q(1) ^ st.q(6) ^ st.q(11) ^ st.q(16) ^ st.q(21);
                    bc2 = st.q(2) ^ st.q(7) ^ st.q(12) ^ st.q(17) ^ st.q(22);
                    bc3 = st.q(3) ^ st.q(8) ^ st.q(13) ^ st.q(18) ^ st.q(23);
                    bc4 = st.q(4) ^ st.q(9) ^ st.q(14) ^ st.q(19) ^ st.q(24);
                }

                var ta = bc4 ^ RotateLeft(bc1, 1);
                var tb = bc0 ^ RotateLeft(bc2, 1);
                var tc = bc1 ^ RotateLeft(bc3, 1);
                var td = bc2 ^ RotateLeft(bc4, 1);
                var te = bc3 ^ RotateLeft(bc0, 1);

                st.q(0) ^= ta;
                st.q(1) ^= tb;
                st.q(2) ^= tc;
                st.q(3) ^= td;
                st.q(4) ^= te;

                st.q(5) ^= ta;
                st.q(6) ^= tb;
                st.q(7) ^= tc;
                st.q(8) ^= td;
                st.q(9) ^= te;

                st.q(10) ^= ta;
                st.q(11) ^= tb;
                st.q(12) ^= tc;
                st.q(13) ^= td;
                st.q(14) ^= te;

                st.q(15) ^= ta;
                st.q(16) ^= tb;
                st.q(17) ^= tc;
                st.q(18) ^= td;
                st.q(19) ^= te;

                st.q(20) ^= ta;
                st.q(21) ^= tb;
                st.q(22) ^= tc;
                st.q(23) ^= td;
                st.q(24) ^= te;

                // Rho Pi
                KeccakFRhoPi(ref st);

                //  Chi
                {
                    KeccakFChi(ref st, 0);
                    KeccakFChi(ref st, 5);
                    KeccakFChi(ref st, 10);
                    KeccakFChi(ref st, 15);
                    KeccakFChi(ref st, 20);
                }

                //  Iota
                st.q(0) ^= Rndc[r];
            }

            FlipEndianIfNeeded(st);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void KeccakFRhoPi(ref State st)
        {
            var t = st.q(1);
            {
                ref var rq = ref st.q(10);
                var x = rq;
                rq = RotateLeft(t, 1);
                t = x;
            }
            {
                ref var rq = ref st.q(7);
                var x = rq;
                rq = RotateLeft(t, 3);
                t = x;
            }
            {
                ref var rq = ref st.q(11);
                var x = rq;
                rq = RotateLeft(t, 6);
                t = x;
            }
            {
                ref var rq = ref st.q(17);
                var x = rq;
                rq = RotateLeft(t, 10);
                t = x;
            }
            {
                ref var rq = ref st.q(18);
                var x = rq;
                rq = RotateLeft(t, 15);
                t = x;
            }
            {
                ref var rq = ref st.q(3);
                var x = rq;
                rq = RotateLeft(t, 21);
                t = x;
            }
            {
                ref var rq = ref st.q(5);
                var x = rq;
                rq = RotateLeft(t, 28);
                t = x;
            }
            {
                ref var rq = ref st.q(16);
                var x = rq;
                rq = RotateLeft(t, 36);
                t = x;
            }
            {
                ref var rq = ref st.q(8);
                var x = rq;
                rq = RotateLeft(t, 45);
                t = x;
            }
            {
                ref var rq = ref st.q(21);
                var x = rq;
                rq = RotateLeft(t, 55);
                t = x;
            }
            {
                ref var rq = ref st.q(24);
                var x = rq;
                rq = RotateLeft(t, 2);
                t = x;
            }
            {
                ref var rq = ref st.q(4);
                var x = rq;
                rq = RotateLeft(t, 14);
                t = x;
            }
            {
                ref var rq = ref st.q(15);
                var x = rq;
                rq = RotateLeft(t, 27);
                t = x;
            }
            {
                ref var rq = ref st.q(23);
                var x = rq;
                rq = RotateLeft(t, 41);
                t = x;
            }
            {
                ref var rq = ref st.q(19);
                var x = rq;
                rq = RotateLeft(t, 56);
                t = x;
            }
            {
                ref var rq = ref st.q(13);
                var x = rq;
                rq = RotateLeft(t, 8);
                t = x;
            }
            {
                ref var rq = ref st.q(12);
                var x = rq;
                rq = RotateLeft(t, 25);
                t = x;
            }
            {
                ref var rq = ref st.q(2);
                var x = rq;
                rq = RotateLeft(t, 43);
                t = x;
            }
            {
                ref var rq = ref st.q(20);
                var x = rq;
                rq = RotateLeft(t, 62);
                t = x;
            }
            {
                ref var rq = ref st.q(14);
                var x = rq;
                rq = RotateLeft(t, 18);
                t = x;
            }
            {
                ref var rq = ref st.q(22);
                var x = rq;
                rq = RotateLeft(t, 39);
                t = x;
            }
            {
                ref var rq = ref st.q(9);
                var x = rq;
                rq = RotateLeft(t, 61);
                t = x;
            }
            {
                ref var rq = ref st.q(6);
                var x = rq;
                rq = RotateLeft(t, 20);
                t = x;
            }
            {
                ref var rq = ref st.q(1);
                var x = rq;
                rq = RotateLeft(t, 44);
                t = x;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void KeccakFChi(ref State st, int i)
        {
            var bc0 = st.q(i + 0);
            var bc1 = st.q(i + 1);
            var bc2 = st.q(i + 2);
            var bc3 = st.q(i + 3);
            var bc4 = st.q(i + 4);
            st.q(i + 0) ^= ~bc1 & bc2;
            st.q(i + 1) ^= ~bc2 & bc3;
            st.q(i + 2) ^= ~bc3 & bc4;
            st.q(i + 3) ^= ~bc4 & bc0;
            st.q(i + 4) ^= ~bc0 & bc1;
        }

        internal static void FlipEndianIfNeeded(State st)
        {
            if (BitConverter.IsLittleEndian)
                return;

            for (var i = 0; i < 25; i++)
                st.q(i) = BinaryPrimitives.ReverseEndianness(st.q(i));
        }

        // Initialize the context for SHA3
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Init(ref this Context c, int mdLen)
        {
            int i;

            ref var st = ref c.st;
            for (i = 0; i < 25; i++)
                st.q(i) = 0;

            c.mdLen = mdLen;
            c.rSiz = 200 - 2 * mdLen;
            c.pt = 0;

            return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int InitShake(ref this Context c, int bits)
            => c.Init(bits / 8);

        // update state with more data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Update(ref this Context c, ReadOnlySpan<byte> data)
        {
            int i;
            var len = data.Length;

            var j = c.pt;
            ref var st = ref c.st;
            for (i = 0; i < len; i++)
            {
                st.b(j++) ^= data[i];
                if (j < c.rSiz)
                    continue;

                c.KeccakF();
                j = 0;
            }

            c.pt = j;

            return 1;
        }

        // update state with more data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Update(ref this Context c, ReadOnlyBigSpan<byte> data)
        {
            nuint i;
            var len = data.Length;

            var j = c.pt;
            ref var st = ref c.st;
            for (i = 0; i < len; i++)
            {
                st.b(j++) ^= data[i];
                if (j < c.rSiz)
                    continue;

                c.KeccakF();
                j = 0;
            }

            c.pt = j;

            return 1;
        }

        // finalize and output a hash
        internal static int Final(ref this Context c, Span<byte> md)
        {
            int i;

            ref var st = ref c.st;

            st.b(c.pt) ^= 0x06;
            st.b(c.rSiz - 1) ^= 0x80;
            c.KeccakF();

            for (i = 0; i < c.mdLen; i++)
            {
                md[i] = st.b(i);
            }

            return 1;
        }

        // SHAKE128 and SHAKE256 extensible-output functionality
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ShakeExtensibleOutputFormatMode(ref this Context c)
        {
            ref var st = ref c.st;

            st.b(c.pt) ^= 0x1F;
            st.b(c.rSiz - 1) ^= 0x80;
            c.KeccakF();
            c.pt = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ShakeOut(ref this Context c, Span<byte> output)
        {
            int i;
            var len = output.Length;

            ref var st = ref c.st;

            var j = c.pt;
            for (i = 0; i < len; i++)
            {
                if (j >= c.rSiz)
                {
                    c.KeccakF();
                    j = 0;
                }

                output[i] = st.b(j++);
            }

            c.pt = j;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ShakeOut(ref this Context c, BigSpan<byte> output)
        {
            nuint i;
            var len = output.Length;

            ref var st = ref c.st;

            var j = c.pt;
            for (i = 0; i < len; i++)
            {
                if (j >= c.rSiz)
                {
                    c.KeccakF();
                    j = 0;
                }

                output[i] = st.b(j++);
            }

            c.pt = j;
        }

        public static void Hash(ReadOnlySpan<byte> input, Span<byte> digest)
        {
            Context sha3 = default;

            sha3.Init(digest.Length);
            sha3.Update(input);
            sha3.Final(digest);
        }

        public static byte[] Hash224(ReadOnlySpan<byte> input)
        {
            var digest = new byte[28];
            Hash(input, digest);
            return digest;
        }

        public static byte[] Hash256(ReadOnlySpan<byte> input)
        {
            var digest = new byte[32];
            Hash(input, digest);
            return digest;
        }

        public static byte[] Hash384(ReadOnlySpan<byte> input)
        {
            var digest = new byte[48];
            Hash(input, digest);
            return digest;
        }

        public static byte[] Hash512(ReadOnlySpan<byte> input)
        {
            var digest = new byte[64];
            Hash(input, digest);
            return digest;
        }

        public static void Hash(ReadOnlyBigSpan<byte> input, Span<byte> digest)
        {
            Context sha3 = default;

            sha3.Init(digest.Length);
            sha3.Update(input);
            sha3.Final(digest);
        }

        public static byte[] Hash224(ReadOnlyBigSpan<byte> input)
        {
            var digest = new byte[28];
            Hash(input, digest);
            return digest;
        }

        public static byte[] Hash256(ReadOnlyBigSpan<byte> input)
        {
            var digest = new byte[32];
            Hash(input, digest);
            return digest;
        }

        public static byte[] Hash384(ReadOnlyBigSpan<byte> input)
        {
            var digest = new byte[48];
            Hash(input, digest);
            return digest;
        }

        public static byte[] Hash512(ReadOnlyBigSpan<byte> input)
        {
            var digest = new byte[64];
            Hash(input, digest);
            return digest;
        }

        public static void Shake(int bits, ReadOnlySpan<byte> input, Span<byte> digest)
        {
            Context sha3 = default;

            sha3.Init(bits / 8);
            sha3.Update(input);
            sha3.ShakeExtensibleOutputFormatMode();
            sha3.ShakeOut(digest);
        }

        public static void Shake(int bits, ReadOnlyBigSpan<byte> input, Span<byte> digest)
        {
            Context sha3 = default;

            sha3.Init(bits / 8);
            sha3.Update(input);
            sha3.ShakeExtensibleOutputFormatMode();
            sha3.ShakeOut(digest);
        }
    }
}
