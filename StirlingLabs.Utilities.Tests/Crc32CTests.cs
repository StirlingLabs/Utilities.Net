using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests
{
    public class Crc32CTests
    {
        
        // CRC RevEng CRC Catalogue CRC-32/ISCSI
        [Test]
        public void WellKnownTest1()
        {
            Span<byte> testVector = stackalloc byte[]
            {
                (byte)'1',
                (byte)'2',
                (byte)'3',
                (byte)'4',
                (byte)'5',
                (byte)'6',
                (byte)'7',
                (byte)'8',
                (byte)'9'
            };

            const uint expected = 0xe3069283;

            var actual = Crc32C.Calculate(testVector);

            Assert.AreEqual(expected, actual);
        }

        // RFC 3720 B.4 CRC Example 1: 32 bytes of zeroes
        [Test]
        public void WellKnownTest2()
        {
            Span<byte> testVector = stackalloc byte[32];

            const uint expected = 0x8a9136aa;

            var actual = Crc32C.Calculate(testVector);

            Assert.AreEqual(expected, actual);
        }

        // RFC 3720 B.4 CRC Example 2: 32 bytes of ones
        [Test]
        public unsafe void WellKnownTest3()
        {
            Span<byte> testVector = stackalloc byte[32];
            fixed (byte* p = testVector)
                Unsafe.InitBlock(p, 0xff, 32);

            const uint expected = 0x62a8ab43;

            var actual = Crc32C.Calculate(testVector);

            Assert.AreEqual(expected, actual);
        }

        // RFC 3720 B.4 CRC Example 3: 32 bytes of incrementing 00..1f
        [Test]
        public unsafe void WellKnownTest4()
        {
            Span<byte> testVector = stackalloc byte[32];
            for (var i = 0; i < 32; ++i)
                testVector[i] = (byte)i;

            const uint expected = 0x46dd794e;

            var actual = Crc32C.Calculate(testVector);

            Assert.AreEqual(expected, actual);
        }


        // RFC 3720 B.4 CRC Example 4: 32 bytes of decrementing 1f..00
        [Test]
        public unsafe void WellKnownTest5()
        {
            Span<byte> testVector = stackalloc byte[32];
            for (var i = 0; i < 32; ++i)
                testVector[i] = (byte)(0x1f - i);

            const uint expected = 0x113fdb5c;

            var actual = Crc32C.Calculate(testVector);

            Assert.AreEqual(expected, actual);
        }


        // RFC 3720 B.4 CRC Example 5: An iSCSI - SCSI Read (10) Command PDU
        [Test]
        public unsafe void WellKnownTest6()
        {
            Span<byte> testVector = stackalloc byte[]
            {
                0x01, 0xc0, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x14, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x04, 0x00,
                0x00, 0x00, 0x00, 0x14,
                0x00, 0x00, 0x00, 0x18,
                0x28, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x02, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00
            };

            const uint expected = 0xd9963a56;

            var actual = Crc32C.Calculate(testVector);

            Assert.AreEqual(expected, actual);
        }
    }
}
