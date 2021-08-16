using System.Runtime.CompilerServices;
using FluentAssertions;
using NUnit.Framework;
using StirlingLabs.Utilities;

namespace StirlingLabs.Utilities.Tests
{
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
    }
}
