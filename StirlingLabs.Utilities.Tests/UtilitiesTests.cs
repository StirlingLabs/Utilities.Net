using System.Runtime.CompilerServices;
using FluentAssertions;
using NUnit.Framework;
using StirlingLabs.Utilities;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.All)]
public class UtilitiesTests
{
    [Test]
    public void TypeEqualsTest()
    {
        var o1 = new object();
        var o2 = new object();
        var o3 = new object();
        // these compare the same type
        o1.TypeIs(o2).Should().BeTrue();
        o1.TypeIs(o3).Should().BeTrue();
        o2.TypeIs(o3).Should().BeTrue();

        {
            var a = new StrongBox<short>(1);
            var b = new StrongBox<int>(1);
            var c = new StrongBox<int>(1);
            var d = new StrongBox<ulong>(1);

            // these compare generics of the same type
            a.TypeIs(b).Should().BeFalse();
            a.TypeIs(b).Should().BeFalse();
            b.TypeIs(c).Should().BeTrue();
            a.TypeIs(d).Should().BeFalse();
            b.TypeIs(d).Should().BeFalse();

            // these compare different types
            a.TypeIs(o1).Should().BeFalse();
            b.TypeIs(o1).Should().BeFalse();
            c.TypeIs(o1).Should().BeFalse();
            d.TypeIs(o1).Should().BeFalse();
        }

    }
    [Test]
    public unsafe void TypeTokenTest()
    {
        var o = new object();
        var info = Type<object>.Info;
        var x = o.GetRuntimeTypeHandle();
        info.Should().NotBeNull();
        var mt = Type<object>.RuntimeTypeHandle;
        var p = (*(nint**)Unsafe.AsPointer(ref info))[3];
        mt.Should().Be(p);
        x.Should().Be(mt);
    }

    [Test]
    public void InitClassTest()
    {
        var called = false;
        var x = Common.Init(new Nothing(), item => {
            Assert.NotNull(item);
            called = true;
        });
        Assert.True(called);
        Assert.NotNull(x);
    }

    [Test]
    public unsafe void NewInitTest()
    {

        var called = false;
        var pX = NativeMemory.New<NothingStruct>(pItem => {
            Assert.NotZero((nint)pItem);
            called = true;
        });
        Assert.True(called);
        Assert.NotZero((nint)pX);
        NativeMemory.Free(pX);
    }

    [Test]
    public unsafe void InitNewTest()
    {

        var called = false;
        var pX = Common.Init(NativeMemory.New<NothingStruct>(), pItem => {
            Assert.NotZero((nint)pItem);
            called = true;
        });
        Assert.True(called);
        Assert.NotZero((nint)pX);
        NativeMemory.Free(pX);
    }
}
