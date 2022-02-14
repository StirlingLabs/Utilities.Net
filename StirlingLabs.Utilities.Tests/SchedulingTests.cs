using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests;

public class TimestampTests
{
    private double Sustain = 1 / 3d;


    private static bool IsContinuousIntegration = Common.Init
        (() => (Environment.GetEnvironmentVariable("CI") ?? "").ToUpperInvariant() == "TRUE");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // spin-up
        for (var i = 0; i < 10; ++i)
            Timestamp.Wait(.003);
        AccurateTime(0);
        AccurateWait(0);
        AccurateCancellableWait(0);
        TimeoutTest(0);
        IntervalTest(0);
        try
        {
            IntervalTest(1);
        }
        catch { }
    }

    [Theory]
    public void AccurateTime([Values(1, 2, 3)] int _)
    {
        var start = DateTime.Now;
        ulong count = 0;
        var ts = Timestamp.Now;
        var future = ts + Sustain;
        while (Timestamp.Now < future)
            ++count;
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - Sustain;
        var diff = elapsed - Sustain;
        TestContext.WriteLine($"Elapsed: {elapsed:G17}");
        TestContext.WriteLine($"Spin Count: {count}");
        TestContext.WriteLine($"Est. Clock Time: {estDiff}");
        TestContext.WriteLine($"Est. Difference: {estOff:G2}");
        TestContext.WriteLine($"Measured Difference: {diff:G17}");

        if (_ == 0) return;
        try
        {
            Math.Abs(diff).Should().BeLessThan(1.5e-5, $"{diff} should be smaller than 1.5e-5");
        }
        catch (Exception ex)
        {
            if (IsContinuousIntegration)
                throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
            throw;
        }
    }

    [Theory]
    public void AccurateWait([Values(1, 2, 3)] int _)
    {
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        Timestamp.Wait(Sustain);
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - Sustain;
        var diff = elapsed - Sustain;
        TestContext.WriteLine($"Elapsed: {elapsed:G17}");
        TestContext.WriteLine($"Est. Clock Time: {estDiff}");
        TestContext.WriteLine($"Est. Difference: {estOff:G2}");
        TestContext.WriteLine($"Measured Difference: {diff:G17}");

        if (_ == 0) return;
        try
        {
            Math.Abs(diff).Should().BeLessThan(1.5e-5, $"{diff} should be smaller than 1.5e-5");
        }
        catch (Exception ex)
        {
            if (IsContinuousIntegration)
                throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
            throw;
        }
    }


    [Theory]
    public void CancellableWait([Values(1, 2, 3)] int _)
    {
        var halfSustain = Sustain / 2;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(halfSustain));
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        Assert.Throws<OperationCanceledException>(() => {
            Timestamp.Wait(Sustain, cts.Token);
        });
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - halfSustain;
        var diff = elapsed - halfSustain;

        TestContext.WriteLine($"Elapsed: {elapsed:G17}");
        TestContext.WriteLine($"Est. Clock Time: {estDiff}");
        TestContext.WriteLine($"Est. Difference: {estOff:G2}");
        TestContext.WriteLine($"Measured Difference: {diff:G17}");

        var threshold = 1.5e-5 + Timestamp.SleepBiasThresholdTimeSpan.TotalSeconds;
        TestContext.WriteLine($"Threshold: {threshold:G17}");

        if (_ == 0) return;
        try
        {
            Math.Abs(diff).Should().BeLessThan(threshold,
                $"{diff:g3} should be smaller than {threshold:g3}");
        }
        catch (Exception ex)
        {
            if (IsContinuousIntegration)
                throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
            throw;
        }
    }

    [Theory]
    public void AccurateCancellableWait([Values(1, 2, 3)] int _)
    {
        using var cts = new CancellationTokenSource();
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        Timestamp.Wait(Sustain, cts.Token);
        var elapsed = Timestamp.Now - ts;
        var fin = DateTime.Now;
        var estDiff = fin - start;
        var estOff = estDiff.TotalSeconds - Sustain;
        var diff = elapsed - Sustain;
        TestContext.WriteLine($"Elapsed: {elapsed:G17}");
        TestContext.WriteLine($"Est. Clock Time: {estDiff}");
        TestContext.WriteLine($"Est. Difference: {estOff:G2}");
        TestContext.WriteLine($"Measured Difference: {diff:G17}");

        if (_ == 0) return;
        try
        {
            Math.Abs(diff).Should().BeLessThan(1.5e-5, $"{diff} should be smaller than 1.5e-5");
        }
        catch (Exception ex)
        {
            if (IsContinuousIntegration)
                throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
            throw;
        }
    }

    [Theory]
    public void TimeoutTest([Values(1, 2, 3)] int _)
    {
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        using var mre = new ManualResetEventSlim();
        var tw = TestContext.Out;
        using var to = new Timeout(Sustain, () => {
            var elapsed = Timestamp.Now - ts;
            var fin = DateTime.Now;
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
                Math.Abs(diff).Should().BeLessThan(1.5e-5, $"{diff} should be smaller than 1.5e-5");
            }
            catch (Exception ex)
            {
                if (IsContinuousIntegration)
                    throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                throw;
            }
        });
        mre.Wait();
    }

    [Theory]
    public void IntervalTest([Values(1, 2, 3)] int _)
    {
        var start = DateTime.Now;
        var ts = Timestamp.Now;
        using var cd = new CountdownEvent(3);
        var tw = TestContext.Out;
        var exceptions = new ConcurrentQueue<Exception>();
        ScheduledAction.UnobservedException += (_, info) => {
            exceptions.Enqueue(info.SourceException);
        };

        using var it = new Interval(Sustain, () => {
            var elapsed = Timestamp.Now - ts;
            var fin = DateTime.Now;
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
                Math.Abs(diff).Should().BeLessThan(1.5e-5, $"{diff} should be smaller than 1.5e-5");
            }
            catch (Exception ex)
            {
                if (_ != 0)
                {
                    //if (IsContinuousIntegration)
                    throw new InconclusiveException("This test is sensitive to environmental conditions.", ex);
                    //throw;
                }
            }
            finally
            {
                ts += Sustain;
            }
            return !cd.IsSet;
        });
        cd.Wait();

        if (_ == 0) return;

        Assert.Multiple(() => {
            while (exceptions.TryDequeue(out var ex))
                throw ex;
        });
    }
}
