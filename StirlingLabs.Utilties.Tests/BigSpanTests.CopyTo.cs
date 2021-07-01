using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using NUnit.Framework;
using StirlingLabs.Utilities;
using static StirlingLabs.Utilities.Common;

namespace StirlingLabs.Utilties.Tests
{
    public static partial class BigSpanTests
    {
        [Test]
        public static void TryCopyTo()
        {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100, 101 };

            var srcSpan = new BigSpan<int>(src);
            var success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            Assert.AreEqual(src, dst);
        }

        [Test]
        public static void TryCopyToSingle()
        {
            int[] src = { 1 };
            int[] dst = { 99 };

            var srcSpan = new BigSpan<int>(src);
            var success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            Assert.AreEqual(src, dst);
        }

        [Test]
        public static void TryCopyToArraySegmentImplicit()
        {
            int[] src = { 1, 2, 3 };
            int[] dst = { 5, 99, 100, 101, 10 };
            var segment = new ArraySegment<int>(dst, 1, 3);

            var srcSpan = new BigSpan<int>(src);
            var success = srcSpan.TryCopyTo(segment);
            Assert.True(success);
            Assert.AreEqual(src, segment);
        }

        [Test]
        public static void TryCopyToEmpty()
        {
            int[] src = { };
            int[] dst = { 99, 100, 101 };

            var srcSpan = new BigSpan<int>(src);
            var success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            int[] expected = { 99, 100, 101 };
            Assert.AreEqual(expected, dst);
        }

        [Test]
        public static void TryCopyToLonger()
        {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100, 101, 102 };

            var srcSpan = new BigSpan<int>(src);
            var success = srcSpan.TryCopyTo(dst);
            Assert.True(success);
            int[] expected = { 1, 2, 3, 102 };
            Assert.AreEqual(expected, dst);
        }

        [Test]
        public static void TryCopyToShorter()
        {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100 };

            var srcSpan = new BigSpan<int>(src);
            var success = srcSpan.TryCopyTo(dst);
            Assert.False(success);
            int[] expected = { 99, 100 };
            Assert.AreEqual(expected, dst); // TryCopyTo() checks for sufficient space before doing any copying.
        }

        [Test]
        public static void CopyToShorter()
        {
            int[] src = { 1, 2, 3 };
            int[] dst = { 99, 100 };

            var srcSpan = new BigSpan<int>(src);
            BigSpanAssertHelpers.Throws<ArgumentException, int>(srcSpan, _srcSpan => _srcSpan.CopyTo(dst));
            int[] expected = { 99, 100 };
            Assert.AreEqual(expected, dst); // CopyTo() checks for sufficient space before doing any copying.
        }

        [Test]
        public static void Overlapping1()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97 };

            var src = new BigSpan<int>(a, 1, 6);
            var dst = new BigSpan<int>(a, 2, 6);
            src.CopyTo(dst);

            int[] expected = { 90, 91, 91, 92, 93, 94, 95, 96 };
            Assert.AreEqual(expected, a);
        }

        [Test]
        public static void Overlapping2()
        {
            int[] a = { 90, 91, 92, 93, 94, 95, 96, 97 };

            var src = new BigSpan<int>(a, 2, 6);
            var dst = new BigSpan<int>(a, 1, 6);
            src.CopyTo(dst);

            int[] expected = { 90, 92, 93, 94, 95, 96, 97, 97 };
            Assert.AreEqual(expected, a);
        }

        [Test]
        public static void CopyToArray()
        {
            int[] src = { 1, 2, 3 };
            Span<int> dst = new int[3] { 99, 100, 101 };

            src.CopyTo(dst);
            Assert.AreEqual(src, dst.ToArray());
        }

        [Test]
        public static void CopyToSingleArray()
        {
            int[] src = { 1 };
            Span<int> dst = new int[1] { 99 };

            src.CopyTo(dst);
            Assert.AreEqual(src, dst.ToArray());
        }

        [Test]
        public static void CopyToEmptyArray()
        {
            int[] src = { };
            Span<int> dst = new int[3] { 99, 100, 101 };

            src.CopyTo(dst);
            int[] expected = { 99, 100, 101 };
            Assert.AreEqual(expected, dst.ToArray());

            Span<int> dstEmpty = new int[0] { };

            src.CopyTo(dstEmpty);
            int[] expectedEmpty = { };
            Assert.AreEqual(expectedEmpty, dstEmpty.ToArray());
        }

        [Test]
        public static void CopyToLongerArray()
        {
            int[] src = { 1, 2, 3 };
            Span<int> dst = new int[4] { 99, 100, 101, 102 };

            src.CopyTo(dst);
            int[] expected = { 1, 2, 3, 102 };
            Assert.AreEqual(expected, dst.ToArray());
        }

        [Test]
        public static void CopyToShorterArray()
        {
            int[] src = { 1, 2, 3 };
            var dst = new int[2] { 99, 100 };

            BigSpanAssertHelpers.Throws<ArgumentException, int>(src, _src => _src.CopyTo(dst));
            int[] expected = { 99, 100 };
            Assert.AreEqual(expected, dst); // CopyTo() checks for sufficient space before doing any copying.
        }

        [Test]
        public static void CopyToCovariantArray()
        {
            var src = new[] { "Hello" };
            Span<object> dst = new object[] { "world" };

            src.CopyTo(dst);
            Assert.AreEqual("Hello", dst[0]);
        }

        [Test]
        public static void FillWithRandomDataCoverage()
        {
            var k = new Span<byte>();
            
            nuint bufferSize = 256;

            var allocated = false;
            IntPtr memBlock = default;

            unsafe
            {
                try
                {

                    Assert.True(allocated = UnmanagedMemory.Allocate(bufferSize, out memBlock));

                    var memoryFirst = (byte*)memBlock.ToPointer();
                    var spanFirst = new BigSpan<byte>(memoryFirst, bufferSize);

                    spanFirst.FillWithRandomData();

                    // take a sample and ensure all of the sampled bytes are non-zero
                    var nonZeroBytes = 0;

                    for (nuint i = 0; i < bufferSize; ++i)
                        if (spanFirst[i] != 0)
                            nonZeroBytes++;

                    Assert.NotZero(nonZeroBytes);
                }
                finally
                {
                    if (allocated)
                        UnmanagedMemory.Free(ref memBlock);
                }
            }
        }

        [Test]
        public static void FillWithRandomNonZeroData()
        {
            nuint bufferSize = 256;

            var allocated = false;
            IntPtr memBlock = default;

            unsafe
            {
                try
                {

                    Assert.True(allocated = UnmanagedMemory.Allocate(bufferSize, out memBlock));

                    var memoryFirst = (byte*)memBlock.ToPointer();
                    var spanFirst = new BigSpan<byte>(memoryFirst, bufferSize);

                    spanFirst.FillWithNonZeroRandomData();

                    for (nuint i = 0; i < bufferSize; ++i)
                        Assert.NotZero(spanFirst[i]);
                }
                finally
                {
                    if (allocated)
                        UnmanagedMemory.Free(ref memBlock);
                }
            }
        }

        [Theory]
        public static void CopyToSmallSizeTest([Values(1uL, 64uL, 384uL, 1024uL)] ulong longBufferSize)
        {
            var bufferSize = (nuint)longBufferSize;

            var allocatedFirst = false;
            var allocatedSecond = false;
            IntPtr memBlockFirst = default;
            IntPtr memBlockSecond = default;

            unsafe
            {
                try
                {

                    Assert.True(allocatedFirst = UnmanagedMemory.Allocate(bufferSize, out memBlockFirst));
                    Assert.True(allocatedSecond = UnmanagedMemory.Allocate(bufferSize, out memBlockSecond));

                    var memoryFirst = (byte*)memBlockFirst.ToPointer();
                    var spanFirst = new BigSpan<byte>(memoryFirst, bufferSize);

                    var memorySecond = (byte*)memBlockSecond.ToPointer();
                    var spanSecond = new BigSpan<byte>(memorySecond, bufferSize);

                    spanFirst.FillWithNonZeroRandomData();

                    // take a sample and ensure all of the sampled bytes are non-zero
                    for (nuint i = 0; i < Math.Min(bufferSize - 1,63); ++i)
                        Assert.NotZero(spanFirst[i]);
                    Assert.NotZero(spanFirst[^1]);

                    spanFirst.CopyTo(spanSecond);

                    Assert.Zero(spanFirst.CompareMemory(spanSecond));
                }
                finally
                {
                    if (allocatedFirst)
                        UnmanagedMemory.Free(ref memBlockFirst);
                    if (allocatedSecond)
                        UnmanagedMemory.Free(ref memBlockSecond);
                }
            }
        }

        [Theory]
        [Explicit]
        public static void CopyToLargeSizeTest([Values(256uL + int.MaxValue, 256uL + uint.MaxValue)] ulong longBufferSize)
        {
            var bufferSize = (nuint)longBufferSize;

            var allocatedFirst = false;
            var allocatedSecond = false;
            IntPtr memBlockFirst = default;
            IntPtr memBlockSecond = default;

            unsafe
            {
                try
                {

                    Assert.True(allocatedFirst = UnmanagedMemory.Allocate(bufferSize, out memBlockFirst));
                    Assert.True(allocatedSecond = UnmanagedMemory.Allocate(bufferSize, out memBlockSecond));

                    var memoryFirst = (byte*)memBlockFirst.ToPointer();
                    var spanFirst = new BigSpan<byte>(memoryFirst, bufferSize);

                    var memorySecond = (byte*)memBlockSecond.ToPointer();
                    var spanSecond = new BigSpan<byte>(memorySecond, bufferSize);

                    spanFirst.FillWithNonZeroRandomData();

                    spanFirst.CopyTo(spanSecond);

                    Assert.Zero(spanFirst.CompareMemory(spanSecond));
                }
                finally
                {
                    if (allocatedFirst)
                        UnmanagedMemory.Free(ref memBlockFirst);
                    if (allocatedSecond)
                        UnmanagedMemory.Free(ref memBlockSecond);
                }
            }
        }

        [Test]
        [SuppressMessage("Security", "CA5394", Justification = "Test case with no security requirement.")]
        public static void CopyToVaryingSizes()
        {
            const int MaxLength = 2048;

            var rng = new Random();
            var inputArray = new byte[MaxLength];
            Span<byte> inputSpan = inputArray;
            Span<byte> outputSpan = new byte[MaxLength];
            Span<byte> allZerosSpan = new byte[MaxLength];

            // Test all inputs from size 0 .. MaxLength (inclusive) to make sure we don't have
            // gaps in our Memmove logic.
            for (var i = 0; i <= MaxLength; i++)
            {
                // Arrange

                rng.NextBytes(inputArray);
                outputSpan.Clear();

                // Act

                inputSpan.Slice(0, i).CopyTo(outputSpan);

                // Assert

                Assert.True(inputSpan.Slice(0, i).SequenceEqual(outputSpan.Slice(0, i))); // src successfully copied to dst
                Assert.True(outputSpan.Slice(i).SequenceEqual(allZerosSpan.Slice(i))); // no other part of dst was overwritten
            }
        }
    }
}
