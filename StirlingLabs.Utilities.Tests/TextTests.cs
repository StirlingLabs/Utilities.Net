using System;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using  StirlingLabs.Utilities.Text;

namespace StirlingLabs.Utilities.Tests;


[Parallelizable(ParallelScope.All)]
public static class TextTests
{
    [Test]
    public static void CompareStringsBasicInvariantCulture()
    {
        ICU4X.Compare("a", "a", StringComparison.InvariantCulture)
            .Should().Be(0);
        ICU4X.Compare("a", "a", StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("a", "b", StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare("a", "b", StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(-1);
        
        ICU4X.Compare("b", "a", StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare("b", "a", StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(1);
        
        ICU4X.Compare("a", "A", StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare("a", "A", StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("A", "a", StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare("A", "a", StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(0);
        
        var started = Stopwatch.GetTimestamp();
        var c = 10_000;
        for (var i = 0; i < c; ++i)
        {
            ICU4X.Compare("a", "a", StringComparison.InvariantCulture)
                .Should().Be(0);
            ICU4X.Compare("a", "a", StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("a", "b", StringComparison.InvariantCulture)
                .Should().Be(-1);
            ICU4X.Compare("a", "b", StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(-1);
        
            ICU4X.Compare("b", "a", StringComparison.InvariantCulture)
                .Should().Be(1);
            ICU4X.Compare("b", "a", StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(1);
        
            ICU4X.Compare("a", "A", StringComparison.InvariantCulture)
                .Should().Be(-1);
            ICU4X.Compare("a", "A", StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("A", "a", StringComparison.InvariantCulture)
                .Should().Be(1);
            ICU4X.Compare("A", "a", StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(0);
        }
        var elapsed = new TimeSpan( Stopwatch.GetTimestamp() - started );
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed /c).Ticks / 10d}us each)");
    }
    
    [Test]
    public static void CompareStringsBasicCurrentCulture()
    {
        ICU4X.Compare("a", "a", StringComparison.CurrentCulture)
            .Should().Be(0);
        ICU4X.Compare("a", "a", StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("a", "b", StringComparison.CurrentCulture)
            .Should().Be(-1);
        ICU4X.Compare("a", "b", StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(-1);
        
        ICU4X.Compare("b", "a", StringComparison.CurrentCulture)
            .Should().Be(1);
        ICU4X.Compare("b", "a", StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(1);
        
        ICU4X.Compare("a", "A", StringComparison.CurrentCulture)
            .Should().Be(-1);
        ICU4X.Compare("a", "A", StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("A", "a", StringComparison.CurrentCulture)
            .Should().Be(1);
        ICU4X.Compare("A", "a", StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(0);

        var started = Stopwatch.GetTimestamp();
        var c = 10_000;
        for (var i = 0; i < c; ++i)
        {
            ICU4X.Compare("a", "a", StringComparison.CurrentCulture)
                .Should().Be(0);
            ICU4X.Compare("a", "a", StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("a", "b", StringComparison.CurrentCulture)
                .Should().Be(-1);
            ICU4X.Compare("a", "b", StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(-1);
        
            ICU4X.Compare("b", "a", StringComparison.CurrentCulture)
                .Should().Be(1);
            ICU4X.Compare("b", "a", StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(1);
        
            ICU4X.Compare("a", "A", StringComparison.CurrentCulture)
                .Should().Be(-1);
            ICU4X.Compare("a", "A", StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("A", "a", StringComparison.CurrentCulture)
                .Should().Be(1);
            ICU4X.Compare("A", "a", StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(0);
        }
        var elapsed = new TimeSpan( Stopwatch.GetTimestamp() - started );
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed /c).Ticks / 10d}us each)");
    }


    [Test]
    public static void CompareUtf8StringsBasicInvariantCulture()
    {
        ICU4X.Compare("a"u8, "a"u8, StringComparison.InvariantCulture)
            .Should().Be(0);
        ICU4X.Compare("a"u8, "a"u8, StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("a"u8, "b"u8, StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare("a"u8, "b"u8, StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(-1);
        
        ICU4X.Compare("b"u8, "a"u8, StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare("b"u8, "a"u8, StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(1);
        
        ICU4X.Compare("a"u8, "A"u8, StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare("a"u8, "A"u8, StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("A"u8, "a"u8, StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare("A"u8, "a"u8, StringComparison.InvariantCultureIgnoreCase)
            .Should().Be(0);
        
        var started = Stopwatch.GetTimestamp();
        var c = 10_000;
        for (var i = 0; i < c; ++i)
        {
            ICU4X.Compare("a"u8, "a"u8, StringComparison.InvariantCulture)
                .Should().Be(0);
            ICU4X.Compare("a"u8, "a"u8, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("a"u8, "b"u8, StringComparison.InvariantCulture)
                .Should().Be(-1);
            ICU4X.Compare("a"u8, "b"u8, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(-1);
        
            ICU4X.Compare("b"u8, "a"u8, StringComparison.InvariantCulture)
                .Should().Be(1);
            ICU4X.Compare("b"u8, "a"u8, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(1);
        
            ICU4X.Compare("a"u8, "A"u8, StringComparison.InvariantCulture)
                .Should().Be(-1);
            ICU4X.Compare("a"u8, "A"u8, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("A"u8, "a"u8, StringComparison.InvariantCulture)
                .Should().Be(1);
            ICU4X.Compare("A"u8, "a"u8, StringComparison.InvariantCultureIgnoreCase)
                .Should().Be(0);
        }
        var elapsed = new TimeSpan( Stopwatch.GetTimestamp() - started );
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed /c).Ticks / 10d}us each)");
    }
    
    [Test]
    public static void CompareUtf8StringsBasicCurrentCulture()
    {
        ICU4X.Compare("a"u8, "a"u8, StringComparison.CurrentCulture)
            .Should().Be(0);
        ICU4X.Compare("a"u8, "a"u8, StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("a"u8, "b"u8, StringComparison.CurrentCulture)
            .Should().Be(-1);
        ICU4X.Compare("a"u8, "b"u8, StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(-1);
        
        ICU4X.Compare("b"u8, "a"u8, StringComparison.CurrentCulture)
            .Should().Be(1);
        ICU4X.Compare("b"u8, "a"u8, StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(1);
        
        ICU4X.Compare("a"u8, "A"u8, StringComparison.CurrentCulture)
            .Should().Be(-1);
        ICU4X.Compare("a"u8, "A"u8, StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(0);
        
        ICU4X.Compare("A"u8, "a"u8, StringComparison.CurrentCulture)
            .Should().Be(1);
        ICU4X.Compare("A"u8, "a"u8, StringComparison.CurrentCultureIgnoreCase)
            .Should().Be(0);

        var started = Stopwatch.GetTimestamp();
        var c = 10_000;
        for (var i = 0; i < c; ++i)
        {
            ICU4X.Compare("a"u8, "a"u8, StringComparison.CurrentCulture)
                .Should().Be(0);
            ICU4X.Compare("a"u8, "a"u8, StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("a"u8, "b"u8, StringComparison.CurrentCulture)
                .Should().Be(-1);
            ICU4X.Compare("a"u8, "b"u8, StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(-1);
        
            ICU4X.Compare("b"u8, "a"u8, StringComparison.CurrentCulture)
                .Should().Be(1);
            ICU4X.Compare("b"u8, "a"u8, StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(1);
        
            ICU4X.Compare("a"u8, "A"u8, StringComparison.CurrentCulture)
                .Should().Be(-1);
            ICU4X.Compare("a"u8, "A"u8, StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(0);
        
            ICU4X.Compare("A"u8, "a"u8, StringComparison.CurrentCulture)
                .Should().Be(1);
            ICU4X.Compare("A"u8, "a"u8, StringComparison.CurrentCultureIgnoreCase)
                .Should().Be(0);
        }
        var elapsed = new TimeSpan( Stopwatch.GetTimestamp() - started );
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed /c).Ticks / 10d}us each)");
    }
}
