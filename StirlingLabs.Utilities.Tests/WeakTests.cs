using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests;

[ExcludeFromCodeCoverage]
public static class WeakTests
{
    private static Span<object> CreateObjectRefs(out Weak<object> wr)
    {
        var o = new object();
        wr = new(o);
        return MemoryMarshal.CreateSpan(ref o, 1)!;
    }

#if DEBUG
    [Ignore("Garbage collector will hold a reference to items expected to be collected under Debug mode.")]
#endif
    [Test]
    public static void WeakReleasesObjectReference()
    {

        var garbageCollected = 0uL;

        void OnGc() => ++garbageCollected;

        try
        {
            Weak<object> wrObj;
            bool collected;

            [MethodImpl(MethodImplOptions.NoInlining)]
            Weak<object> ActiveStackScope()
            {
                //var wrCtrl = CreateWeakRefObject();
                var sObj = CreateObjectRefs(out wrObj);
                //var sObj = CreateObjectRefs(out var wrObj);

                GarbageCollectedNotifier.GarbageCollected += OnGc;

                collected = false;

                for (var i = 0; i < 10000; ++i)
                {
                    if (!wrObj.IsAlive)
                        collected = true;
                    // else not yet collected

                    GC.Collect(2, GCCollectionMode.Forced, true, true);

                    if (collected) return wrObj;
                }

                Assert.IsFalse(collected);

                sObj = null;
                sObj = default;
                return wrObj;
            }

            wrObj = ActiveStackScope();

            for (var i = 0; i < 10000; ++i)
            {
                if (!wrObj.IsAlive)
                    collected = true;

                GC.Collect(2, GCCollectionMode.Forced, true, true);

                if (collected) return;
            }

            Assert.IsTrue(collected);
        }
        finally
        {
            GarbageCollectedNotifier.GarbageCollected -= OnGc;
        }
    }
}
