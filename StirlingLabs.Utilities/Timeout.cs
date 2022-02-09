using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public sealed class Timeout : ScheduledAction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Timeout CreateFromSeconds(double seconds, Action callback)
        => new(seconds, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Timeout CreateFromTicks(long ticks, Action callback)
        => new(ticks, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Timeout Create(TimeSpan timeSpan, Action callback)
        => new(timeSpan.Ticks, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Timeout(double seconds, Action callback)
        : this((long)(seconds * Stopwatch.Frequency), callback) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Timeout(long ticks, Action callback)
        : base(ticks, callback) { }
}
