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
#if NETSTANDARD2_0
        if (action is null) throw new ArgumentNullException(nameof(action));
        var pFn = action.Method.MethodHandle.GetFunctionPointer();
        if (action.Target is not null)
            ThreadPool.QueueUserWorkItem(ExecWorkItemInstanceFast, (pFn, action.Target));
        else
            ThreadPool.QueueUserWorkItem(ExecWorkItemStaticFast, pFn);
#else
        // ReSharper disable once IntroduceOptionalParameters.Global
        QueueUserWorkItemFast(action, false);
#endif
    }

#if !NETSTANDARD2_0
    public static void QueueUserWorkItemFast(Action action, bool preferLocal)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        var pFn = action.Method.MethodHandle.GetFunctionPointer();
        if (action.Target is not null)
            ThreadPool.QueueUserWorkItem(ExecWorkItemInstanceFast, (pFn, action.Target), preferLocal);
        else
            ThreadPool.QueueUserWorkItem(ExecWorkItemStaticFast, pFn, preferLocal);
    }
#endif


#if NETSTANDARD2_0
    private static unsafe void ExecWorkItemInstanceFast(object x)
    {
        var (pFn, target) = ((IntPtr pFn, object? Target))x;
        ((delegate* managed<object, void>)pFn)(target!);
    }

    private static unsafe void ExecWorkItemStaticFast(object x)
        => ((delegate* managed<void>)(IntPtr)x)();
#else
    private static unsafe void ExecWorkItemInstanceFast((IntPtr pFn, object? Target) o)
        => ((delegate* managed<object, void>)o.pFn)(o.Target!);

    private static unsafe void ExecWorkItemStaticFast(IntPtr p)
        => ((delegate* managed<void>)p)();
#endif
}
