using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace StirlingLabs.Utilities;

public static partial class ThreadPoolHelpers
{
    public static void QueueUserWorkItemFast(Action action)
    {
#if NETSTANDARD2_0 || NETSTANDARD2_1
        if (action is null) throw new ArgumentNullException(nameof(action));
        var pFn = action.Method.MethodHandle.GetFunctionPointer();
        if (action.Target is not null)
            ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemInstanceFast<object>, (pFn, action.Target));
        else
            ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemStaticFast, pFn);
#else
        // ReSharper disable once IntroduceOptionalParameters.Global
        QueueUserWorkItemFast(action, false);
#endif
    }

#if !NETSTANDARD2_0 && !NETSTANDARD2_1
    public static void QueueUserWorkItemFast(Action action, bool preferLocal)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        var pFn = action.Method.MethodHandle.GetFunctionPointer();
        if (action.Target is not null)
            ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemInstanceFast, (pFn, action.Target), preferLocal);
        else
            ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemStaticFast, pFn, preferLocal);
    }
#endif

    public static unsafe void QueueUserWorkItemFast<T>(delegate* managed<T, void> pFn, T instance)
    {
#if NETSTANDARD2_0 || NETSTANDARD2_1
        if (pFn is null) throw new ArgumentNullException(nameof(pFn));
        ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemInstanceFast<T>, ((IntPtr)pFn, instance));
#else
        // ReSharper disable once IntroduceOptionalParameters.Global
        QueueUserWorkItemFast(pFn, instance, false);
#endif
    }

    public static unsafe void QueueUserWorkItemFast(delegate* managed<void> pFn)
    {
#if NETSTANDARD2_0 || NETSTANDARD2_1
        if (pFn is null) throw new ArgumentNullException(nameof(pFn));
        ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemStaticFast, (IntPtr)pFn);
#else
        // ReSharper disable once IntroduceOptionalParameters.Global
        QueueUserWorkItemFast(pFn, false);
#endif
    }

#if !NETSTANDARD2_0 && !NETSTANDARD2_1
    public static unsafe void QueueUserWorkItemFast<T>(delegate* managed<T, void> pFn, T instance, bool preferLocal)
    {
        if (pFn is null) throw new ArgumentNullException(nameof(pFn));
        ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemInstanceFast, ((IntPtr)pFn, instance), preferLocal);
    }
    public static unsafe void QueueUserWorkItemFast(delegate* managed<void> pFn, bool preferLocal)
    {
        if (pFn is null) throw new ArgumentNullException(nameof(pFn));
        ThreadPool.UnsafeQueueUserWorkItem(ExecWorkItemStaticFast, (IntPtr)pFn, preferLocal);
    }
#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1
    private static unsafe void ExecWorkItemInstanceFast<T>(object x)
    {
        var o = ((IntPtr pFn, T Target))x;
        ((delegate* managed<T, void>)o.pFn)(o.Target!);
    }
    private static unsafe void ExecWorkItemStaticFast(object p)
        => ((delegate* managed<void>)(IntPtr)p)();
#else
    private static unsafe void ExecWorkItemInstanceFast<T>((IntPtr pFn, T instance) o)
        => ((delegate* managed<T, void>)o.pFn)(o.instance!);

    private static unsafe void ExecWorkItemStaticFast(IntPtr p)
        => ((delegate* managed<void>)p)();
#endif
}
