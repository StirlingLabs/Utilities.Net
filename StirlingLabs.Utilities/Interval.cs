using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public sealed class Interval : ScheduledAction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Interval CreateFromSeconds(double seconds, Func<bool> callback)
        => new(seconds, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Interval CreateFromTicks(long ticks, Func<bool> callback)
        => new(ticks, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Interval Create(TimeSpan timeSpan, Func<bool> callback)
        => new(timeSpan.Ticks, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Interval(double seconds, Func<bool> callback)
        : this((long)(seconds * Stopwatch.Frequency), callback) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Interval(long ticks, Func<bool> callback)
        : base(ticks, callback) { }
}