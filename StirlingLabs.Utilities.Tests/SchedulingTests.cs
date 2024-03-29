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

[NonParallelizable]
public class SchedulingTests
{
    private const int NoGcRegionSize = 16 * 1024 * 1024;

    private double Sustain = 1 / 3d;

    private static readonly bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");

    private static readonly double AcceptableErrorThreshold = IsContinuousIntegration ? 1e-4 : 2e-5;

    private static bool _inSetUp;

    private static void ForceStartNoGcRegion()
    {
        do
        {
            while (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
                try { GC.EndNoGCRegion(); }
                catch
                {
                    /* discard */
                }

            GC.Collect(2, GCCollectionMode.Forced, true, true);

            try
            {
                while (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion)
                    GC.TryStartNoGCRegion(NoGcRegionSize, true);
            }
            catch
            {
                /* discard */
            }
        } while (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion);
    }

    [OneTimeSetUp]
    [SuppressMessage("Design", "CA1031", Justification = "Test Setup")]
    public void OneTimeSetUp()
    {
        MultimediaTimerPeriod.BeginPeriod(1);

        _inSetUp = true;

        var tw = TestContext.Progress;

        tw.WriteLine("=== BEGIN ONE TIME SETUP ===");

        tw.WriteLine($"Stopwatch.IsHighResolution={Stopwatch.IsHighResolution}");

        tw.WriteLine($"Stopwatch.Frequency={Stopwatch.Frequency}");

        if (GCSettings.IsServerGC)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

#if NET6_0_OR_GREATER
        if (IsContinuousIntegration)
            Trace.Listeners.Add(new ConsoleTraceListener());
#endif

        // spin-up
        for (var i = 0; i < 24; ++i)
            Timestamp.Wait(.003);

        Common.Try(() => AccurateTime(1));
        TearDown();
        Common.Try(() => AccurateTime(1));
        TearDown();

        Common.Try(() => AccurateCancellableWait(1));
        TearDown();
        Common.Try(() => AccurateCancellableWait(1));
        TearDown();

        for (var j = 0; j < 3; ++j)
        {

            Common.Try(() => AccurateWait(j));
            TearDown();
            Common.Try(() => AccurateWait(j));
            TearDown();

            Common.Try(() => TimeoutTest(j));
            TearDown();
            Common.Try(() => TimeoutTest(j));
            TearDown();

            for (var i = 1; i <= 3; ++i)
            {
                Common.Try(() => IntervalTest(j));
                TearDown();
            }
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
    {
        Trace.Flush();

        MultimediaTimerPeriod.EndPeriod(1);
    }

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
    [NonParallelizable]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    public void AccurateTime([Values(1, 2, 3)] int _)
    {
        ForceStartNoGcRegion();
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

        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
    }

    [Order(2)]
    [Theory]
    [NonParallelizable]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    public void AccurateWait([Values(1, 2, 3)] int _)
    {
        ForceStartNoGcRegion();
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

        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
    }

    [Order(3)]
    [Theory]
    [NonParallelizable]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    public void CancellableWait1([Values(1, 2, 3)] int _)
    {
        ForceStartNoGcRegion();
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

        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
    }

    [Order(4)]
    [Theory]
    [NonParallelizable]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    public void CancellableWait2([Values(1, 2, 3)] int _)
    {
        ForceStartNoGcRegion();
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

        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
    }

    [Order(5)]
    [Theory]
    [NonParallelizable]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    public void AccurateCancellableWait([Values(1, 2, 3)] int _)
    {
        using var cts = new CancellationTokenSource();
        ForceStartNoGcRegion();
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

        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
    }

    [Order(6)]
    [Theory]
    [NonParallelizable]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    public void TimeoutTest([Values(1, 2, 3)] int _)
    {
        ForceStartNoGcRegion();
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

        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();
    }

    [Order(7)]
    [Theory]
    [NonParallelizable]
    public void IntervalTest([Values(1, 2, 3)] int _)
    {
        ForceStartNoGcRegion();
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
                ForceStartNoGcRegion();
            }
            return true;
        });

        cd.Wait(TimeSpan.FromSeconds(Sustain * 4));

        if (GCSettings.LatencyMode == GCLatencyMode.NoGCRegion)
            GC.EndNoGCRegion();

        if (_inSetUp) return;

        using (new AssertionScope())
        {
            while (exceptions.TryDequeue(out var ex))
            {
                if (ex is not InconclusiveException)
                    throw ex;
            }
        }

        cd.CurrentCount.Should().Be(0, $"Signalled {cd.InitialCount - cd.CurrentCount}/{cd.InitialCount} times");
    }
}
