using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

        [Theory]
        [Explicit]
        public static void CopyToLargeSizeTest([Values(256, 256+(long)int.MaxValue)]long bufferSize)
        {
            // If this test is run in a 32-bit process, the large allocation will fail.
            if (Unsafe.SizeOf<IntPtr>() != sizeof(long))
            {
                return;
            }

            var GuidCount = (nuint)(bufferSize / Unsafe.SizeOf<Guid>());
            var allocatedFirst = false;
            var allocatedSecond = false;
            var memBlockFirst = IntPtr.Zero;
            var memBlockSecond = IntPtr.Zero;

            unsafe
            {
                try
                {
                    
                    allocatedFirst = AllocateNativeMemory((nint)bufferSize, out memBlockFirst);
                    allocatedSecond = AllocateNativeMemory((nint)bufferSize, out memBlockSecond);

                    if (allocatedFirst && allocatedSecond)
                    {
                        ref var memoryFirst = ref Unsafe.AsRef<Guid>(memBlockFirst.ToPointer());
                        var spanFirst = new BigSpan<Guid>(memBlockFirst.ToPointer(), GuidCount);

                        ref var memorySecond = ref Unsafe.AsRef<Guid>(memBlockSecond.ToPointer());
                        var spanSecond = new BigSpan<Guid>(memBlockSecond.ToPointer(), GuidCount);

                        var theGuid = Guid.Parse("900DBAD9-00DB-AD90-00DB-AD900DBADBAD");
                        for (nuint count = 0; count < GuidCount; ++count)
                        {
                            Unsafe.Add(ref memoryFirst, (nint)count) = theGuid;
                        }

                        spanFirst.CopyTo(spanSecond);

                        for (nuint count = 0; count < GuidCount; ++count)
                        {
                            var guidfirst = Unsafe.Add(ref memoryFirst, (nint)count);
                            var guidSecond = Unsafe.Add(ref memorySecond, (nint)count);
                            Assert.AreEqual(guidfirst, guidSecond);
                        }
                    }
                }
                finally
                {
                    if (allocatedFirst)
                        FreeNativeMemory(ref memBlockFirst);
                    if (allocatedSecond)
                        FreeNativeMemory(ref memBlockSecond);
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