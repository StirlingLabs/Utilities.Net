using System;
using System.Diagnostics;
using FluentAssertions;
using StirlingLabs.Utilities.Text;

namespace StirlingLabs.Utilities.Tests;

public sealed class TextTests
{
    public void CompareStringsBasicInvariantCulture(TextWriter logger)
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
        var elapsed = new TimeSpan(Stopwatch.GetTimestamp() - started);
        logger.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed / c).Ticks / 10d}us each)");
    }

    public void CompareStringsBasicCurrentCulture(TextWriter logger)
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
        var elapsed = new TimeSpan(Stopwatch.GetTimestamp() - started);
        logger.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed / c).Ticks / 10d}us each)");
    }
    
    public void CompareUtf8StringsBasicInvariantCulture(TextWriter logger)
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
        var elapsed = new TimeSpan(Stopwatch.GetTimestamp() - started);
        logger.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed / c).Ticks / 10d}us each)");
    }

    public void CompareUtf8StringsBasicCurrentCulture(TextWriter logger)
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
        var elapsed = new TimeSpan(Stopwatch.GetTimestamp() - started);
        logger.WriteLine($"Elapsed: {elapsed.TotalMilliseconds}ms ({(elapsed / c).Ticks / 10d}us each)");
    }

    public void CompareUtf8StringsEszettCurrentCulture(TextWriter logger)
    {
        var capitalEszett = "ẞ"u8;
        var lowercaseEszett = "ß"u8;
        // invariant culture / "und" locale
        ICU4X.Compare(capitalEszett, capitalEszett, StringComparison.InvariantCulture)
            .Should().Be(0);
        ICU4X.Compare(lowercaseEszett, lowercaseEszett, StringComparison.InvariantCulture)
            .Should().Be(0);
        ICU4X.Compare(capitalEszett, lowercaseEszett, StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare(lowercaseEszett, capitalEszett, StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare(capitalEszett, "SS"u8, StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare(lowercaseEszett, "SS"u8, StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare(capitalEszett, "ss"u8, StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare(lowercaseEszett, "ss"u8, StringComparison.InvariantCulture)
            .Should().Be(1);
        ICU4X.Compare("SS"u8, capitalEszett, StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare("SS"u8, lowercaseEszett, StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare("ss"u8, capitalEszett, StringComparison.InvariantCulture)
            .Should().Be(-1);
        ICU4X.Compare("ss"u8, lowercaseEszett, StringComparison.InvariantCulture)
            .Should().Be(-1);

        // specific culture

        var locale = "en-u-ks-level1";
        ICU4X.Compare(capitalEszett, capitalEszett, locale)
            .Should().Be(0);
        ICU4X.Compare(lowercaseEszett, lowercaseEszett, locale)
            .Should().Be(0);
        ICU4X.Compare(capitalEszett, lowercaseEszett, locale)
            .Should().Be(1);
        ICU4X.Compare(lowercaseEszett, capitalEszett, locale)
            .Should().Be(-1);
        ICU4X.Compare(capitalEszett, "SS"u8, locale)
            .Should().Be(0);
        ICU4X.Compare(lowercaseEszett, "SS"u8, locale)
            .Should().Be(-1);
        ICU4X.Compare(capitalEszett, "ss"u8, locale)
            .Should().Be(1);
        ICU4X.Compare(lowercaseEszett, "ss"u8, locale)
            .Should().Be(0);
        ICU4X.Compare("SS"u8, capitalEszett, locale)
            .Should().Be(0);
        ICU4X.Compare("SS"u8, lowercaseEszett, locale)
            .Should().Be(1);
        ICU4X.Compare("ss"u8, capitalEszett, locale)
            .Should().Be(-1);
        ICU4X.Compare("ss"u8, lowercaseEszett, locale)
            .Should().Be(0);
    }
}
