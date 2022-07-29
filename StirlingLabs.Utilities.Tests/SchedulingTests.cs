using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using static FluentAssertions.FluentActions;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.None)]
public class SchedulingTests
{
    private const int NoGcRegionSize = 16 * 1024 * 1024;

    private double Sustain = 1 / 3d;

    private static readonly bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");

    private static readonly double AcceptableErrorThreshold = IsContinuousIntegration ? 1e-4 : 2e-5;

    private static bool _inSetUp;

    [OneTimeSetUp]
    [SuppressMessage("Design", "CA1031", Justification = "Test Setup")]
    public void OneTimeSetUp()
    {
        _inSetUp = true;

        var tw = TestContext.Progress;

        tw.WriteLine("=== BEGIN ONE TIME SETUP ===");

        tw.WriteLine($"Stopwatch.IsHighResolution={Stopwatch.IsHighResolution}");

        tw.WriteLine($"Stopwatch.Frequency={Stopwatch.Frequency}");

#if NET6_0_OR_GREATER
        if (IsContinuousIntegration)
            Trace.Listeners.Add(new ConsoleTraceListener());
#endif

        // spin-up
        for (var i = 0; i < 10; ++i)
            Timestamp.Wait(.003);

        Common.Try(() => AccurateTime(1));
        TearDown();

        Common.Try(() => AccurateCancellableWait(1));
        TearDown();

        for (var j = 0; j < 3; ++j)
        {

            Common.Try(() => AccurateWait(j));
            TearDown();

            Common.Try(() => TimeoutTest(j));
            TearDown();

            for (var i = 1; i <= 3; ++i)
            {
                Common.Try(() => IntervalTest(j));
                TearDown();
            }
        }

        //Common.Try(() => IntervalTest(1));

        Thread.Sleep(TimeSpan.FromSeconds(Sustain));

        _inSetUp = false;

        tw.WriteLine("=== END ONE TIME SETUP ===");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
        => Trace.Flush();

    [SetUp]
    public void SetUp()
        => TestContext.Progress.WriteLine($"=== BEGIN {TestContext.CurrentContext.Test.FullName} ===");

    [TearDown]
    public void TearDown()
    {
        TestContext.Progress.WriteLine($"=== END {TestContext.CurrentContext.Test.FullName} ===");

        if (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion)
            return;

        try
        {
            if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                GC.EndNoGCRegion();
        }
        catch
        {
            if (_inSetUp) return;
            throw;
        }
    }

    [Order(1)]
    [Theory]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AccurateTime([Values(1, 2, 3)] int _)
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.TryStartNoGCRegion(NoGcRegionSize, true)
            .Should().BeTrue();
        ulong count = 0;
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        var future = ts + Sustain;
        while (Timestamp.Now < future)
            ++count;
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - Sustain;
        var diff = elapsed - Sustain;

        var tw = TestContext.Progress;
        tw.WriteLine($"Elapsed: {elapsed:G17}");
        tw.WriteLine($"Spin Count: {count}");
        tw.WriteLine($"Est. Clock Time: {estDiff}");
        tw.WriteLine($"Est. Difference: {estOff:G2}");
        tw.WriteLine($"Measured Difference: {diff:G17}");

        try
        {
            Math.Abs(diff).Should().BeLessThan(AcceptableErrorThreshold, $"{diff} should be smaller than {AcceptableErrorThreshold}");
        }
        catch (Exception ex)
        {
            if (!_inSetUp)
            {
                if (IsContinuousIntegration)
                    throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                throw;
            }
        }
    }

    [Order(2)]
    [Theory]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AccurateWait([Values(1, 2, 3)] int _)
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.TryStartNoGCRegion(NoGcRegionSize, true)
            .Should().BeTrue();
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        Timestamp.Wait(Sustain);
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - Sustain;
        var diff = elapsed - Sustain;

        var tw = TestContext.Progress;
        tw.WriteLine($"Elapsed: {elapsed:G17}");
        tw.WriteLine($"Est. Clock Time: {estDiff}");
        tw.WriteLine($"Est. Difference: {estOff:G2}");
        tw.WriteLine($"Measured Difference: {diff:G17}");

        try
        {
            Math.Abs(diff).Should().BeLessThan(AcceptableErrorThreshold, $"{diff} should be smaller than {AcceptableErrorThreshold}");
        }
        catch (Exception ex)
        {
            if (!_inSetUp)
            {
                if (IsContinuousIntegration)
                    throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                throw;
            }
        }
    }

    [Order(3)]
    [Theory]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void CancellableWait1([Values(1, 2, 3)] int _)
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.TryStartNoGCRegion(NoGcRegionSize, true)
            .Should().BeTrue();
        var halfSustain = Sustain / 2;
        using var cts = new CancellationTokenSource();
        // ReSharper disable once AccessToDisposedClosure
        using var cto = new Timeout(halfSustain, () => cts.Cancel());
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        // ReSharper disable once AccessToDisposedClosure
        Invoking(() => Timestamp.Wait(Sustain, cts.Token))
            .Should().Throw<OperationCanceledException>();
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - halfSustain;
        var diff = elapsed - halfSustain;

        var tw = TestContext.Progress;
        tw.WriteLine($"Elapsed: {elapsed:G17}");
        tw.WriteLine($"Est. Clock Time: {estDiff}");
        tw.WriteLine($"Est. Difference: {estOff:G2}");
        tw.WriteLine($"Measured Difference: {diff:G17}");

        var threshold = AcceptableErrorThreshold + Timestamp.SleepBiasThresholdTimeSpan.TotalSeconds;
        tw.WriteLine($"Threshold: {threshold:G17}");

        try
        {
            Math.Abs(diff).Should().BeLessThan(threshold,
                $"{diff:g3} should be smaller than {threshold:g3}");
        }
        catch (Exception ex)
        {
            if (!_inSetUp)
            {
                if (IsContinuousIntegration)
                    throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                throw;
            }
        }
    }

    [Order(4)]
    [Theory]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void CancellableWait2([Values(1, 2, 3)] int _)
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.TryStartNoGCRegion(NoGcRegionSize, true)
            .Should().BeTrue();
        var halfSustain = Sustain / 2;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(halfSustain));
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        // ReSharper disable once AccessToDisposedClosure
        Invoking(() => Timestamp.Wait(Sustain, cts.Token))
            .Should().Throw<OperationCanceledException>();
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - halfSustain;
        var diff = elapsed - halfSustain;

        var tw = TestContext.Progress;
        tw.WriteLine($"Elapsed: {elapsed:G17}");
        tw.WriteLine($"Est. Clock Time: {estDiff}");
        tw.WriteLine($"Est. Difference: {estOff:G2}");
        tw.WriteLine($"Measured Difference: {diff:G17}");

        var threshold = AcceptableErrorThreshold + Timestamp.SleepBiasThresholdTimeSpan.TotalSeconds;
        tw.WriteLine($"Threshold: {threshold:G17}");

        try
        {
            Math.Abs(diff).Should().BeLessThan(threshold,
                $"{diff:g3} should be smaller than {threshold:g3}");
        }
        catch (Exception ex)
        {
            if (!_inSetUp)
            {
                if (IsContinuousIntegration)
                    throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                throw;
            }
        }
    }

    [Order(5)]
    [Theory]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AccurateCancellableWait([Values(1, 2, 3)] int _)
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        using var cts = new CancellationTokenSource();
        GC.TryStartNoGCRegion(NoGcRegionSize, true)
            .Should().BeTrue();
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        Timestamp.Wait(Sustain, cts.Token);
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - Sustain;
        var diff = elapsed - Sustain;
        var tw = TestContext.Progress;
        tw.WriteLine($"Elapsed: {elapsed:G17}");
        tw.WriteLine($"Est. Clock Time: {estDiff}");
        tw.WriteLine($"Est. Difference: {estOff:G2}");
        tw.WriteLine($"Measured Difference: {diff:G17}");

        try
        {
            Math.Abs(diff).Should().BeLessThan(AcceptableErrorThreshold, $"{diff} should be smaller than {AcceptableErrorThreshold}");
        }
        catch (Exception ex)
        {
            if (!_inSetUp)
            {
                if (IsContinuousIntegration)
                    throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                throw;
            }
        }
    }

    [Order(6)]
    [Theory]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void TimeoutTest([Values(1, 2, 3)] int _)
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.TryStartNoGCRegion(NoGcRegionSize, true)
            .Should().BeTrue();
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        using var mre = new ManualResetEventSlim();
        var tw = TestContext.Progress;
        using var to = new Timeout(Sustain, () => {
            var elapsed = Timestamp.Now - ts;
            var fin = DateTime.Now;

            if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                GC.EndNoGCRegion();

            var estDiff = fin - start;
            var estOff = estDiff.TotalSeconds - Sustain;
            var diff = elapsed - Sustain;
            tw.WriteLine($"Elapsed: {elapsed:G17}");
            tw.WriteLine($"Est. Clock Time: {estDiff}");
            tw.WriteLine($"Est. Difference: {estOff:G2}");
            tw.WriteLine($"Measured Difference: {diff:G17}");
            mre.Set();

            try
            {
                Math.Abs(diff).Should().BeLessThan(AcceptableErrorThreshold, $"{diff} should be smaller than {AcceptableErrorThreshold}");
            }
            catch (Exception ex)
            {
                if (!_inSetUp)
                {
                    if (IsContinuousIntegration)
                        throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                    throw;
                }
            }
        });
        mre.Wait(TimeSpan.FromSeconds(Sustain * 2));

        if (!_inSetUp)
            mre.IsSet
                .Should().BeTrue();
    }

    [Order(7)]
    [Theory]
    public void IntervalTest([Values(1, 2, 3)] int _)
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.TryStartNoGCRegion(NoGcRegionSize, true)
            .Should().BeTrue();
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        using var cd = new CountdownEvent(3);
        var tw = TestContext.Progress;
        var exceptions = new ConcurrentQueue<Exception>();
        ScheduledAction.UnobservedException += (_, info) => {
            exceptions.Enqueue(info.SourceException);
        };

        using var it = new Interval(Sustain, () => {
            var elapsed = Timestamp.Now - ts;
            var fin = DateTime.Now;

            if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                GC.EndNoGCRegion();

            var estDiff = fin - start;
            var estOff = estDiff.TotalSeconds - Sustain;
            var diff = elapsed - Sustain;
            tw.WriteLine($"Elapsed: {elapsed:G17}");
            tw.WriteLine($"Est. Clock Time: {estDiff}");
            tw.WriteLine($"Est. Difference: {estOff:G2}");
            tw.WriteLine($"Measured Difference: {diff:G17}");
            cd.Signal();

            try
            {
                Math.Abs(diff).Should().BeLessThan(AcceptableErrorThreshold, $"{diff} should be smaller than {AcceptableErrorThreshold}");
            }
            catch (Exception ex)
            {
                if (!_inSetUp)
                {
                    if (IsContinuousIntegration)
                        throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                    throw;
                }
            }
            finally
            {
                ts += Sustain;
            }

            if (cd.IsSet) return false;
            if (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion)
            {
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.TryStartNoGCRegion(NoGcRegionSize, true)
                    .Should().BeTrue();
            }
            return true;
        });

        cd.Wait(TimeSpan.FromSeconds(Sustain * 4));

        if (_inSetUp) return;

        using (new AssertionScope())
        {
            while (exceptions.TryDequeue(out var ex))
                ex.Should().BeOfType<InconclusiveException>();
        }

        cd.CurrentCount.Should().Be(0, $"Signalled {cd.InitialCount - cd.CurrentCount}/{cd.InitialCount} times");
    }
}
