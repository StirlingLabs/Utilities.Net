using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests
{
    public static partial class BigSpanTests
    {
        [Test]
        public static void BigSpanSize()
        {
            if (BigSpanHelpers.Is64Bit)
            {
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfByReference<byte>());

                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfBigSpan<byte>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<byte>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfBigSpan<int>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<int>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfBigSpan<double>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<double>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfBigSpan<object>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<object>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfBigSpan<string>());
                Assert.AreEqual(16, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<string>());
            }
            else
            {
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfByReference<byte>());

                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfBigSpan<byte>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<byte>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfBigSpan<int>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<int>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfBigSpan<double>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<double>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfBigSpan<object>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<object>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfBigSpan<string>());
                Assert.AreEqual(8, BigSpanHelpers.GetSizeOfReadOnlyBigSpan<string>());
            }
        }

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


        [Test]
        public static void HoldsObjectReference()
        {
            //var wrCtrl = CreateWeakRefObject();
            var bsObj = CreateObjectRefs(out var wrObj, out var sObj);

            var garbageCollected = 0uL;

            GarbageCollectedNotifier.GarbageCollected += () => {
                ++garbageCollected;
            };

            var collected = false;

            for (var i = 0; i < 10000; ++i)
            {
                Unsafe.AreSame(ref sObj[0], ref bsObj[0u]);

                if (wrObj.TryGetTarget(out var expected))
                {
                    // not yet collected
                }
                else
                {
                    collected = true;
                }
                GC.Collect(0, GCCollectionMode.Forced, true, true);

                if (collected) return;
            }

            Assert.IsFalse(collected);
        }


        [Test]
        public static void ReleasesObjectReference()
        {
            //var wrCtrl = CreateWeakRefObject();
            var bsObj = CreateObjectRefs(out var wrObj, out var sObj);
            //var sObj = CreateObjectRefs(out var wrObj);

            var garbageCollected = 0uL;

            GarbageCollectedNotifier.GarbageCollected += () => {
                ++garbageCollected;
            };

            var collected = false;

            for (var i = 0; i < 10000; ++i)
            {
                Unsafe.AreSame(ref sObj[0], ref bsObj[0u]);

                if (wrObj.TryGetTarget(out var expected))
                {
                    // not yet collected
                }
                else
                {
                    collected = true;
                }
                GC.Collect(0, GCCollectionMode.Forced, true, true);

                if (collected) return;
            }

            Assert.IsFalse(collected);

            sObj = null;
            bsObj = default;

            for (var i = 0; i < 10000; ++i)
            {
                if (!wrObj.TryGetTarget(out var expected))
                    collected = true;

                GC.Collect(0, GCCollectionMode.Forced, true, true);

                if (collected) return;
            }

            Assert.IsTrue(collected);
        }


        [Test]
        public static unsafe void ImplicitSpanUpgradeTest2G()
        {
            var notTwoGigs = new Span<byte>((void*)0, int.MaxValue);
            BigSpan<byte> twoGigs = notTwoGigs;
            Assert.AreEqual(twoGigs.Length, (nuint)int.MaxValue);
        }

        [Test]
        public static void ImplicitSpanUpgradeTest4G()
        {
            var notFourGigs = MemoryMarshal.CreateSpan(ref Unsafe.NullRef<byte>(), unchecked((int)uint.MaxValue));
            BigSpan<byte> fourGigs = notFourGigs;
            Assert.AreEqual(fourGigs.Length, (nuint)uint.MaxValue);
        }

        private static BigSpan<object> CreateObjectRefs(out WeakReference<object> wr, out Span<object> sp)
        {
            var o = new object();
            wr = new(o);
            sp = MemoryMarshal.CreateSpan(ref o, 1)!;
            return new(ref o!, 1);
        }
        private static Span<object> CreateObjectRefs(out WeakReference<object> wr)
        {
            var o = new object();
            wr = new(o);
            return MemoryMarshal.CreateSpan(ref o, 1)!;
        }

        private static WeakReference<object> CreateWeakRefObject()
            => new(new());


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void DebugProbe<T>(ref T a, ref T b)
            => Debugger.Break();
    }
}
