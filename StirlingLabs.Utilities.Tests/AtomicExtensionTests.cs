using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FluentAssertions;
using NUnit.Framework;
using StirlingLabs.Utilities;
using StirlingLabs.Utilities.Extensions;

namespace StirlingLabs.Utilities.Tests;

[TestFixture]
[Ignore("Not yet implemented.")]
public class AtomicExtensionTests
{
    [Test]
    public void Atomic32IncSignedTest()
    {
        Span<int> span = stackalloc int[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFF;

        var expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);

        actual |= unchecked((int)0x80000000);

        expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32IncUnsignedTest()
    {
        Span<uint> span = stackalloc uint[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFF;

        var expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);

        actual |= 0x80000000;

        expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32DecSignedTest()
    {
        Span<int> span = stackalloc int[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFF;

        var expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);

        actual |= unchecked((int)0x80000000);

        expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32DecUnsignedTest()
    {
        Span<uint> span = stackalloc uint[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFF;

        var expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);

        actual |= 0x80000000;

        expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64IncSignedTest()
    {
        Span<long> span = stackalloc long[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFFFFFFFFFF;

        var expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);

        actual |= unchecked((long)0x8000000000000000);

        expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64IncUnsignedTest()
    {
        Span<ulong> span = stackalloc ulong[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFFFFFFFFFF;

        var expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);

        actual |= 0x8000000000000000;

        expected = actual + 1;

        actual.AtomicIncrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64DecSignedTest()
    {
        Span<long> span = stackalloc long[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFFFFFFFFFF;

        var expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);

        actual |= unchecked((long)0x8000000000000000);

        expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64DecUnsignedTest()
    {
        Span<ulong> span = stackalloc ulong[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFFFFFFFFFF;

        var expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);

        actual |= 0x8000000000000000;

        expected = actual - 1;

        actual.AtomicDecrement();

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32AddSignedTest()
    {
        Span<int> span = stackalloc int[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFF;

        var expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);

        actual |= unchecked((int)0x80000000);

        expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32AddUnsignedTest()
    {
        Span<uint> span = stackalloc uint[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFF;

        var expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);

        actual |= 0x80000000;

        expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64AddSignedTest()
    {
        Span<long> span = stackalloc long[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFFFFFFFFFF;

        var expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);

        actual |= unchecked((long)0x8000000000000000);

        expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64AddUnsignedTest()
    {
        Span<ulong> span = stackalloc ulong[1];

        Security.FillWithNonZeroRandomData(MemoryMarshal.AsBytes(span));

        ref var actual = ref span[0];

        actual &= 0x7FFFFFFFFFFFFFFF;

        var expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);

        actual |= 0x8000000000000000;

        expected = actual + 1;

        actual.AtomicAdd(1);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32AndSignedTest()
    {
        Span<int> span = stackalloc int[1];

        ref var actual = ref span[0];

        actual = unchecked((int)0xFEC84210);

        var expected = actual & 0x55555555;

        actual.AtomicAnd(0x55555555);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32AndUnsignedTest()
    {
        Span<uint> span = stackalloc uint[1];

        ref var actual = ref span[0];

        actual = 0xFEC84210;

        var expected = actual & 0x55555555;

        actual.AtomicAnd(0x55555555);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64AndSignedTest()
    {
        Span<long> span = stackalloc long[1];

        ref var actual = ref span[0];

        actual = unchecked((long)0xFEC84210FEC84210);

        var expected = actual & 0x5555555555555555;

        actual.AtomicAnd(0x5555555555555555);

        actual.Should().Be(expected);
    }

    [Test]
    public unsafe void Atomic64AndUnsignedTest()
    {
        Span<ulong> span = stackalloc ulong[1];

        ref var actual = ref span[0];

        actual = 0xFEC84210FEC84210;

        var expected = actual & 0x5555555555555555;

        var p = (nint)LlvmAtomicOps<ulong>.StoreAnd;
        var s = $"0x{p:X8}";
        Console.WriteLine(s);
        actual.AtomicAnd(0x5555555555555555);

        actual.Should().Be(expected);
    }


    [Test]
    public unsafe void Atomic32CmpXchgSignedTest()
    {
        Span<int> span = stackalloc int[1];

        ref var actual = ref span[0];

        actual = unchecked((int)0xFEC84210);

        var expected = 0;

        var p = (nint)LlvmAtomicOps<int>.CmpXchg;
        var s = $"0x{p:X8}";
        Console.WriteLine(s);

        actual.AtomicCompareExchange(expected, expected);

        actual.Should().Be(actual);

        actual.AtomicCompareExchange(actual, expected);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic32CmpXchgUnsignedTest()
    {
        Span<uint> span = stackalloc uint[1];

        ref var actual = ref span[0];

        actual = 0xFEC84210;

        var expected = 0u;

        actual.AtomicCompareExchange(expected, expected);

        actual.Should().Be(actual);

        actual.AtomicCompareExchange(actual, expected);

        actual.Should().Be(expected);
    }

    [Test]
    public void Atomic64CmpXchgSignedTest()
    {
        Span<long> span = stackalloc long[1];

        ref var actual = ref span[0];

        actual = unchecked((long)0xFEC84210FEC84210);

        var expected = 0L;

        actual.AtomicCompareExchange(expected, expected);

        actual.Should().Be(actual);

        actual.AtomicCompareExchange(actual, expected);

        actual.Should().Be(expected);
    }

    [Test]
    public unsafe void Atomic64CmpXchgUnsignedTest()
    {
        Span<ulong> span = stackalloc ulong[1];

        ref var actual = ref span[0];

        actual = 0xFEC84210FEC84210;

        var expected = 0uL;

        var p = (nint)LlvmAtomicOps<ulong>.CmpXchg;
        var s = $"0x{p:X8}";
        Console.WriteLine(s);

        actual.AtomicCompareExchange(expected, expected);

        actual.Should().Be(actual);

        actual.AtomicCompareExchange(actual, expected);

        actual.Should().Be(expected);
    }
}
