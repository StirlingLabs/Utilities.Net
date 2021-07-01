using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using NUnit.Framework;
using StirlingLabs.Utilities;
using static StirlingLabs.Utilities.Common;

namespace StirlingLabs.Utilities.Tests
{
    public static partial class BigSpanTests
    {
        [Test]
        public static void IndexAccess()
        {
            var expected = new object();
            var span = new BigSpan<object>(new[] { expected });

            var actual = span[0u];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public static void OutOfBoundsIndexAccess()
        {
            var span = new BigSpan<object>();

            BigSpanAssert<object>
                .Throws<IndexOutOfRangeException>(span, s => s[0u]);
        }

        [Test]
        public static void IndexAccessByIndex()
        {
            var expected = new object();
            var span = new BigSpan<object>(new[] { expected });

            var actual = span[^1];

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public static void RangeAccess()
        {
            var expected = new object();
            var span = new BigSpan<object>(new[] { expected });

            var actual = span[..^1][0u];

            Assert.AreEqual(expected, actual);
        }
    }
}
