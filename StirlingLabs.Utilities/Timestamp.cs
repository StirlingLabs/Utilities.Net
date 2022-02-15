using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using InlineIL;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public readonly struct Timestamp : IComparable<Timestamp>, IEquatable<Timestamp>, IComparable
{
    private readonly long _ticks;

    internal static readonly long OneSecond = Stopwatch.Frequency;

    internal static readonly double TimeSpanTicksScale = OneSecond / (double)TimeSpan.TicksPerSecond;

    internal static readonly double DoublePrecisionOneSecondReciprocal = 1.0 / OneSecond;

    internal static readonly long PreemptionBiasTicks = (long)(6e-6 * OneSecond);

    internal static readonly long SleepBiasThreshold = (long)(0.1 * OneSecond);

    internal static readonly TimeSpan SleepBiasThresholdTimeSpan = new(SleepBiasThreshold);

    internal static readonly long YieldBiasThreshold = (long)(0.005 * OneSecond);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Wait(double seconds)
        => WaitTicksInternal((long)(TimeSpan.FromSeconds(seconds).Ticks * TimeSpanTicksScale));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Wait(TimeSpan timeSpan)
        => WaitTicksInternal((long)(timeSpan.Ticks * TimeSpanTicksScale));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WaitTicks(long ticks)
        => WaitTicksInternal(ticks);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Wait(double seconds, CancellationToken ct)
        => WaitTicksInternal((long)(TimeSpan.FromSeconds(seconds).Ticks * TimeSpanTicksScale), ct);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Wait(TimeSpan timeSpan, CancellationToken ct)
        => WaitTicksInternal((long)(timeSpan.Ticks * TimeSpanTicksScale), ct);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WaitTicks(long ticks, CancellationToken ct)
        => WaitTicksInternal(ticks, ct);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WaitTicksInternal(long ticks)
    {
        var realFin = Stopwatch.GetTimestamp() + ticks;

        /*
        if (ticks > OneSecond)
            GC.Collect(2, GCCollectionMode.Forced, true, true);
        else if (ticks > OneSecond / 8 && GCSettings.LatencyMode >= GCLatencyMode.Interactive)
            GC.Collect(0, GCCollectionMode.Forced, true, false);
        */

        var fin = realFin - PreemptionBiasTicks;

        var tsTicks = (long)(ticks / TimeSpanTicksScale);

        if (tsTicks > SleepBiasThreshold)
            Thread.Sleep(new TimeSpan(tsTicks - SleepBiasThreshold));

        var beforeFin = fin - YieldBiasThreshold;

        while (Stopwatch.GetTimestamp() < beforeFin)
            Thread.Yield();

        SpinWaitUntil(fin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinWaitUntil(long fin)
    {
        while (Stopwatch.GetTimestamp() < fin)
            IL.Emit.Nop();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WaitTicksInternal(long ticks, CancellationToken ct)
    {
        var realFin = Stopwatch.GetTimestamp() + ticks;

        /*
        if (ticks > OneSecond)
            GC.Collect(2, GCCollectionMode.Forced, true, true);
        else if (ticks > OneSecond / 8 && GCSettings.LatencyMode >= GCLatencyMode.Interactive)
            GC.Collect(0, GCCollectionMode.Forced, true, false);
        */
        ct.ThrowIfCancellationRequested();

        var fin = realFin - PreemptionBiasTicks;

        if (ticks > SleepBiasThreshold)
            do
            {
                Thread.Sleep(SleepBiasThresholdTimeSpan);
                ct.ThrowIfCancellationRequested();
            } while (realFin - SleepBiasThreshold - GetCurrentTicks() > 0);

        var beforeFin = fin - YieldBiasThreshold;

        while (Stopwatch.GetTimestamp() < beforeFin)
        {
            Thread.Yield();
            ct.ThrowIfCancellationRequested();
        }

        SpinWaitUntil(fin, ct);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SpinWaitUntil(long fin, CancellationToken ct)
    {
        while (Stopwatch.GetTimestamp() < fin)
            ct.ThrowIfCancellationRequested();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Timestamp(long ticks)
        => _ticks = ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Timestamp other)
        => _ticks == other._ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => obj is Timestamp other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => _ticks.GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Timestamp left, Timestamp right)
        => left._ticks == right._ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Timestamp left, Timestamp right)
        => left._ticks == right._ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static sbyte One(in bool v)
        => Unsafe.As<bool, sbyte>(ref Unsafe.AsRef(v));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Timestamp other)
    {
        var d = other._ticks - _ticks;
#pragma warning disable 675
        return One(d > 0) | (One(d < 0) * -1);
#pragma warning restore 675
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return 1;

        return obj is Timestamp other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(Timestamp)}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(Timestamp left, Timestamp right)
        => left._ticks < right._ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(Timestamp left, Timestamp right)
        => left._ticks > right._ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(Timestamp left, Timestamp right)
        => left._ticks <= right._ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(Timestamp left, Timestamp right)
        => left._ticks >= right._ticks;

    public static Timestamp Now
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Unsafe.As<long, Timestamp>(ref Unsafe.AsRef(GetCurrentTicks()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetCurrentTicks()
        => Stopwatch.GetTimestamp();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Timestamp operator +(Timestamp ts, double d)
        => new(ts._ticks + (long)(d * OneSecond));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Timestamp operator -(Timestamp ts, double d)
        => new(ts._ticks - (long)(d * OneSecond));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double operator -(Timestamp ts, Timestamp d)
        => (ts._ticks - d._ticks) * DoublePrecisionOneSecondReciprocal;
}
