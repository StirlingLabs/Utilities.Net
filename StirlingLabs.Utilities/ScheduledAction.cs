using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using JetBrains.Annotations;
using Medallion.Collections;

#if !NETSTANDARD
using System.Runtime.Loader;
#endif

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

    [MethodImpl(MethodImplOptions.NoInlining)]
    [SuppressMessage("Design", "CA1031", Justification = "Exception is handed off, execution critical")]
    [SuppressMessage("ReSharper", "CognitiveComplexity")]
    private static void TimerThread()
    {

        static void AppDomainExitCanceller(object? o, EventArgs _)
            => TimerThreadCancellationTokenSource.Cancel();

        AppDomain.CurrentDomain.DomainUnload += AppDomainExitCanceller;
        AppDomain.CurrentDomain.ProcessExit += AppDomainExitCanceller;
#if !NETSTANDARD
        static void AlcUnloadCanceller(AssemblyLoadContext _)
            => TimerThreadCancellationTokenSource.Cancel();

        AssemblyLoadContext.GetLoadContext(typeof(ScheduledAction).Assembly)!
            .Unloading += AlcUnloadCanceller;
#endif

        var ct = TimerThreadCancellationTokenSource.Token;

        try
        {
            for (;;)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                NotEmptyEvent.Wait(1, ct);

                //if (ct.IsCancellationRequested) return;

                var nextSchedulerWake = Stopwatch.GetTimestamp() + MaxWaitTicks;
                lock (Schedule)
                {
                    var c = Schedule.Count;
                    if (c <= 0)
                        NotEmptyEvent.Reset();
                    else
                        for (var i = 0; i < c; ++i)
                        {
                            //if (ct.IsCancellationRequested) return;

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

                                    //if (ct.IsCancellationRequested) return;

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
                }

                //if (ct.IsCancellationRequested) return;

                Timestamp.WaitTicks(nextSchedulerWake - Stopwatch.GetTimestamp(), ct);

                //if (ct.IsCancellationRequested) return;
            }
        }
        catch (ThreadAbortException)
        {
            // ok
        }
        catch (OperationCanceledException oce) when (oce.CancellationToken == ct)
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
    private static readonly CancellationTokenSource TimerThreadCancellationTokenSource = new CancellationTokenSource();

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

        if (Callback is not Action act)
            return ((Func<bool>)Callback)();

        act();
        return true;
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
        Trace.TraceError($"{act!.GetType().Name}: {arg!.SourceException}");
        UnobservedException?.Invoke(act!, arg!);
    }
}
