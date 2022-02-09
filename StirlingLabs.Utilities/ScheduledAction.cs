using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using JetBrains.Annotations;
using Medallion.Collections;

namespace StirlingLabs.Utilities;

[PublicAPI]
[SuppressMessage("Design", "CA1063", Justification = "It's fine")]
public abstract class ScheduledAction : IDisposable
{
    protected static long UniqueCounter;

    protected static readonly Comparer<ScheduledAction> Comparer
        = Comparer<ScheduledAction>.Create((a, b)
            => a.Next.CompareTo(b.Next));

    protected static readonly PriorityQueue<ScheduledAction> Schedule = new(Comparer);

    protected static readonly ManualResetEventSlim NotEmptyEvent = new(false, (1 << 11) - 1);

    private static readonly Thread Thread = new(TimerThread)
    {
        IsBackground = true,
        Name = "Scheduling",
        Priority = ThreadPriority.Highest
    };

    protected static readonly long MaxWaitTicks = (long)(Stopwatch.Frequency * .001);
    
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off, execution critical")]
    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    private static void TimerThread()
    {
        try
        {
            for (;;)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                NotEmptyEvent.Wait(1);

                var nextSchedulerWake = Stopwatch.GetTimestamp() + MaxWaitTicks;
                lock (Schedule)
                {
                    var c = Schedule.Count;
                    if (c <= 0)
                        NotEmptyEvent.Reset();
                    else
                        for (var i = 0; i < c; ++i)
                        {
                            var act = Schedule.Dequeue();
                            if (act is Interval)
                            {
                                var willRepeat = true;
                                long actNext;
                                while (Stopwatch.GetTimestamp() >= (actNext = act.Next - PreemptionBiasTicks))
                                {
                                    try
                                    {
                                        willRepeat = act.Invoke();
                                    }
                                    catch (Exception ex)
                                    {
                                        OnUnobservedException(act, ExceptionDispatchInfo.Capture(ex));
                                        if (ex is ThreadAbortException)
                                            throw;
                                    }

                                    if (!willRepeat)
                                        break;
                                }

                                if (!willRepeat)
                                    continue;

                                if (actNext < nextSchedulerWake)
                                    nextSchedulerWake = actNext;

                                Schedule.Enqueue(act);
                            }
                            else
                            {
                                var actNext = act.Next - PreemptionBiasTicks;
                                if (Stopwatch.GetTimestamp() >= actNext)
                                    try
                                    {
                                        act.Invoke();
                                    }
                                    catch (Exception ex)
                                    {
                                        OnUnobservedException(act, ExceptionDispatchInfo.Capture(ex));
                                        if (ex is ThreadAbortException)
                                            throw;
                                    }
                                else
                                {
                                    Schedule.Enqueue(act);

                                    if (actNext < nextSchedulerWake)
                                        nextSchedulerWake = actNext;
                                }
                            }
                        }

                    Timestamp.WaitTicks(nextSchedulerWake - Stopwatch.GetTimestamp());
                }
            }
        }
        catch (ThreadAbortException)
        {
            // ok
        }
    }

    static ScheduledAction()
        => Thread.Start();

    public readonly long Unique;

    protected long Base;

    private long Next
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Base + Ticks;
    }

    public readonly long Ticks;

    public double Seconds => (double)Ticks / Stopwatch.Frequency;

    protected readonly Delegate Callback;
    private static readonly long PreemptionBiasTicks = (long)(Timestamp.PreemptionBiasTicks * 1.5);

    public bool IsDisposed => Base == -1;

    [SuppressMessage("Design", "CA1063", Justification = "It's fine")]
    public void Dispose()
    {
        lock (Schedule) Schedule.Remove(this);
        Base = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Invoke()
    {
        if (IsDisposed) return false;

        Base += Ticks;
        switch (Callback)
        {
            case Action act:
                act();
                return true;
            case Func<bool> fn:
                return fn();
        }

        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ScheduledAction(long ticks, Action callback)
    {
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        Base = Stopwatch.GetTimestamp();
        Unique = Interlocked.Increment(ref UniqueCounter);
        Ticks = ticks;
        lock (Schedule) Schedule.Add(this);
        NotEmptyEvent.Set();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected ScheduledAction(long ticks, Func<bool> callback)
    {
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
        Base = Stopwatch.GetTimestamp();
        Unique = Interlocked.Increment(ref UniqueCounter);
        Ticks = ticks;
        lock (Schedule) Schedule.Add(this);
        NotEmptyEvent.Set();
    }


    [SuppressMessage("Design", "CA1003", Justification = "Done")]
    public static event EventHandler<ScheduledAction, ExceptionDispatchInfo>? UnobservedException;

    protected static void OnUnobservedException(ScheduledAction act, ExceptionDispatchInfo arg)
    {
        Debug.Assert(act != null);
        Debug.Assert(arg != null);
        Trace.TraceError($"{act.GetType().Name}: {arg.SourceException}");
        UnobservedException?.Invoke(act, arg);
    }
}
