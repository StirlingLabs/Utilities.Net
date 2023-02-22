using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
#if !NETSTANDARD
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif

namespace StirlingLabs.Utilities;

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("Design", "CA1060", Justification = "Unnecessary")]
[SuppressMessage("Interoperability", "SYSLIB1054", Justification = "Unnecessary")]
[SuppressMessage("Performance", "CA1810", Justification = "Implementation detail")]
internal static class Mac
{
#if !NETSTANDARD
    private static readonly int[] _ProcessorIdIndex;
#endif


#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport("System")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe void* pthread_mach_thread_np(nint pthreadId);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport("System")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern nint mach_thread_self();

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport("System")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern nint mach_task_self();

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport("System")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern nint mach_port_deallocate(nint targetTask, nint name);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport("System")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe MachKernReturn thread_policy_set(nint machThread, ThreadPolicyFlavor flavor, void* policyInfo,
        int policyInfoCount);

#if !NETSTANDARD
#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [MustUseReturnValue]
    [DllImport("System", EntryPoint = "sysctlbyname")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static extern unsafe int sysctlbyname(byte* name, void* oldp, nuint* oldlenp, void* newp, nuint newlen);

    [MustUseReturnValue]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int sysctlbyname<T>(ReadOnlySpan<byte> name, T* oldp, nuint* oldlenp, T* newp, nuint newlen)
        where T : unmanaged
        => sysctlbyname(
            (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(name)),
            oldp,
            oldlenp,
            newp,
            newlen);

    [MustUseReturnValue]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int sysctlbyname<T>(ReadOnlySpan<byte> name, ref T oldp, ref nuint oldlenp, ref T newp, nuint newlen)
        where T : unmanaged
        => sysctlbyname(
            name,
            (T*)Unsafe.AsPointer(ref Unsafe.AsRef(oldp)),
            (nuint*)Unsafe.AsPointer(ref Unsafe.AsRef(oldlenp)),
            (T*)Unsafe.AsPointer(ref Unsafe.AsRef(newp)),
            newlen);

    [MustUseReturnValue]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int sysctlbyname<T>(ReadOnlySpan<byte> name, T* oldp, ref nuint oldlenp, ref T newp, nuint newlen)
        where T : unmanaged
        => sysctlbyname(
            name,
            oldp,
            (nuint*)Unsafe.AsPointer(ref Unsafe.AsRef(oldlenp)),
            (T*)Unsafe.AsPointer(ref Unsafe.AsRef(newp)),
            newlen);

    [MustUseReturnValue]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int sysctlbyname<T>(ReadOnlySpan<byte> name, T* oldp, ref nuint oldlenp, T* newp, nuint newlen)
        where T : unmanaged
        => sysctlbyname(
            name,
            oldp,
            (nuint*)Unsafe.AsPointer(ref Unsafe.AsRef(oldlenp)),
            newp,
            newlen);

    [MustUseReturnValue]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int sysctlbyname<T>(ReadOnlySpan<byte> name, ref T oldp, ref nuint oldlenp, T* newp, nuint newlen)
        where T : unmanaged
        => sysctlbyname(
            name,
            (T*)Unsafe.AsPointer(ref Unsafe.AsRef(oldp)),
            (nuint*)Unsafe.AsPointer(ref Unsafe.AsRef(oldlenp)),
            newp,
            newlen);
#endif

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    internal enum ThreadPolicyFlavor
    {
        ThreadStandardPolicy = 1,

        ThreadExtendedPolicy = 1,

        ThreadTimeConstraint = 2,

        ThreadPrecedencePolicy = 3,

        ThreadAffinityPolicy = 4,

        ThreadBackgroundPolicy = 5,

        ThreadPolicyState = 6,

        ThreadBackgroundPolicyDarwinBg = 0x1000,

        ThreadLatencyQosPolicy = 7,

        ThreadThroughputQosPolicy = 8,

        ThreadQosPolicy = 9,

        ThreadQosPolicyOverride = 10,
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    internal enum MachKernReturn
    {
        Success = 0,

        InvalidAddress = 1,

        ProtectionFailure = 2,

        NoSpace = 3,

        InvalidArgument = 4,

        Failure = 5,

        ResourceShortage = 6,

        NotReceiver = 7,

        NoAccess = 8,

        MemoryFailure = 9,

        MemoryError = 10,

        AlreadyInSet = 11,

        NotInSet = 12,

        NameExists = 13,

        Aborted = 14,

        InvalidName = 15,

        InvalidTask = 16,

        InvalidRight = 17,

        InvalidValue = 18,

        URefsOverflow = 19,

        InvalidCapability = 20,

        RightExists = 21,

        InvalidHost = 22,

        MemoryPresent = 23,

        MemoryDataMoved = 24,

        MemoryRestartCopy = 25,

        InvalidProcessorSet = 26,

        PolicyLimit = 27,

        InvalidPolicy = 28,

        InvalidObject = 29,

        AlreadyWaiting = 30,

        DefaultSet = 31,

        ExceptionProtected = 32,

        InvalidLedger = 33,

        InvalidMemoryControl = 34,

        InvalidSecurity = 35,

        NotDepressed = 36,

        Terminated = 37,

        LockSetDestroyed = 38,

        LockUnstable = 39,

        LockOwned = 40,

        LockOwnedSelf = 41,

        SemaphoreDestroyed = 42,

        RpcServerTerminated = 43,

        RpcTerminateOrphan = 44,

        RpcContinueOrphan = 45,

        NotSupported = 46,

        NodeDown = 47,

        NotWaiting = 48,

        OperationTimedOut = 49,

        CodesignError = 50,

        PolicyStatic = 51,

        InsufficientBufferSize = 52,

        Denied = 53,

        MissingKc = 54,

        InvalidKc = 55,

        NotFound = 56
    }

#if !NETSTANDARD
    [DebuggerHidden, DebuggerNonUserCode, DebuggerStepperBoundary, DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetProcId()
    {
#if NETSTANDARD
        return -1;
#else
        return X86Base.IsSupported ? X86Base.CpuId(0xB, 0).Edx : -1;
#endif
    }

    static unsafe Mac()
    {
        var procCount = Environment.ProcessorCount;
        var physCpuCount = 0;
        nuint physCpuSize = 4;
        if (sysctlbyname("hw.physicalcpu_max\0"u8, ref physCpuCount, ref physCpuSize, null, 0) != 0)
            physCpuCount = Environment.ProcessorCount;
        for (;;)
        {
            if (physCpuCount == 0) break;
            var affinityTag = 1;
            var machThread = mach_thread_self();
            thread_policy_set(machThread, ThreadPolicyFlavor.ThreadAffinityPolicy, &affinityTag, 1);
            mach_port_deallocate(mach_task_self(), machThread);
            break;
        }
        Console.WriteLine($"Identifying {procCount} ({physCpuCount}) processors");
        var timeStart = Stopwatch.GetTimestamp();
        var procIds = new ConcurrentDictionary<int, int>(procCount, procCount);
        procIds.TryAdd(GetProcId(), 0);
        for (var i = 1; i < procCount; ++i)
        {
            var thread = new Thread(static o => {
                var attempts = 0uL;
                try
                {
                    var (affinityTag, physCpuCount, procIds) = ((int, int, ConcurrentDictionary<int, int>))o!;
                    Console.WriteLine($"{Environment.CurrentManagedThreadId} ({affinityTag}) starting");
                    var procCount = Environment.ProcessorCount;
                    for (;;)
                    {
                        if (physCpuCount == 0) break;
                        var machThread = mach_thread_self();
                        thread_policy_set(machThread, ThreadPolicyFlavor.ThreadAffinityPolicy, &affinityTag, 1);
                        mach_port_deallocate(mach_task_self(), machThread);
                        break;
                    }
                    for (; procIds.Count < procCount; ++attempts)
                    {
                        var procId = GetProcId();
                        var added = procIds.TryAdd(procId, 0);
                        if (added)
                            Thread.Sleep(1);

#if NET7_0_OR_GREATER
                        if (X86Base.IsSupported)
                            X86Base.Pause();
                        else if (ArmBase.IsSupported)
                            ArmBase.Yield();
#else
                        else
                            Thread.Yield();
#endif
                    }
                }
                finally
                {
                    Console.WriteLine($"{Environment.CurrentManagedThreadId} took {attempts} attempts");
                }
            }) { IsBackground = true, Priority = ThreadPriority.Highest };
            var affinityTag = 1 + (int)Math.Floor(i / (double)procCount * physCpuCount);
            thread.Start((affinityTag, physCpuCount, procIds));
        }

        {
            Console.WriteLine($"{Environment.CurrentManagedThreadId} starting");
            var attempts = 0uL;
            for (; procIds.Count < procCount; ++attempts)
            {
                var procId = GetProcId();
                var added = procIds.TryAdd(procId, 0);
                if (added)
                    Thread.Sleep(1);

#if NET7_0_OR_GREATER
                if (X86Base.IsSupported)
                    X86Base.Pause();
                else if (ArmBase.IsSupported)
                    ArmBase.Yield();
#else
                else
                    Thread.Yield();
#endif
            }

            Console.WriteLine($"{Environment.CurrentManagedThreadId} took {attempts} attempts");
        }
#if NET7_0_OR_GREATER
        Console.WriteLine($"Mac took {Stopwatch.GetElapsedTime(timeStart).TotalMilliseconds:F3}ms");
#else
        Console.WriteLine($"Mac took {new TimeSpan((Stopwatch.GetTimestamp() - timeStart) * Stopwatch.Frequency).TotalMilliseconds:F3}ms");
#endif

        Debug.Assert(procIds.Count == procCount);

        {
            var affinityTag = 0; // clear affinity
            var machThread = mach_thread_self();
            thread_policy_set(machThread, ThreadPolicyFlavor.ThreadAffinityPolicy, &affinityTag, 1);
            mach_port_deallocate(mach_task_self(), machThread);
        }

        var maxProcId = procIds.Keys.Max();
        var mapping = new int[maxProcId + 1];
        _ProcessorIdIndex = mapping;
        _ProcessorIdIndex.AsSpan().Fill(-1);
        var procIndex = 0;
        foreach (var (procId, _) in procIds)
            _ProcessorIdIndex[procId] = procIndex++;
        procIds.Clear();
    }
#endif

    [DebuggerHidden, DebuggerNonUserCode, DebuggerStepperBoundary, DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD
    [Obsolete("Not supported on this .NET Standard due to missing intrinsics support.", true)]
#endif
    public static int GetCurrentProcessorIndex()
    {
#if NETSTANDARD
        return -1;
#else
        var procId = GetProcId();
        ref var index = ref MemoryMarshal.GetArrayDataReference(_ProcessorIdIndex);
        return Unsafe.Add(ref index, procId);
#endif
    }

    public static unsafe void SetCurrentThreadProcessorAffinity(int procIndex)
    {
        var affinityTag = 1 + procIndex;
        var machThread = mach_thread_self();
        thread_policy_set(machThread, ThreadPolicyFlavor.ThreadAffinityPolicy, &affinityTag, 1);
        mach_port_deallocate(mach_task_self(), machThread);
    }

    public static unsafe void ClearCurrentThreadProcessorAffinity()
    {
        var affinityTag = 0;
        var machThread = mach_thread_self();
        thread_policy_set(machThread, ThreadPolicyFlavor.ThreadAffinityPolicy, &affinityTag, 1);
        mach_port_deallocate(mach_task_self(), machThread);
    }
}
