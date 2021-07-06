using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
