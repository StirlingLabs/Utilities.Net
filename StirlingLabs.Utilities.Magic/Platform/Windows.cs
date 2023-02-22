using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StirlingLabs.Utilities;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("Design", "CA1060", Justification = "Unnecessary")]
[SuppressMessage("Interoperability", "SYSLIB1054", Justification = "Unnecessary")]
internal static class Windows
{
    private const string Kernel32 = "Kernel32";

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(Kernel32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern nint GetCurrentThread();

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(Kernel32, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern nint SetThreadAffinityMask(nint hThread, nuint dwThreadAffinityMask);

    public static void SetCurrentThreadProcessorAffinityMask(nuint procBits)
    {
        var success = SetThreadAffinityMask(GetCurrentThread(), procBits) != 0;
        if (!success)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static void SetCurrentThreadProcessorAffinity(int procIndex)
    {
        var procCount = Environment.ProcessorCount;
        if (procIndex > procCount)
            throw new($"Processor index {procIndex} is out of range for {procCount} processors");

        SetCurrentThreadProcessorAffinityMask((nuint)(1 << procIndex));
    }

    public static void ClearCurrentThreadProcessorAffinity()
    {
        var procCount = Environment.ProcessorCount;
        var procMask = (nuint)((1 << procCount) - 1);
        SetCurrentThreadProcessorAffinityMask(procMask);
    }

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(Kernel32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern unsafe void* VirtualAlloc(void* lpAddress, nuint dwSize, uint flAllocationType, uint flProtect);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(Kernel32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern unsafe int VirtualFree(void* lpAddress, nuint dwSize, uint dwFreeType);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(Kernel32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern unsafe int VirtualProtect(void* lpAddress, nuint dwSize, uint flNewProtect, uint* lpflOldProtect);

#if NET5_0_OR_GREATER
    [SuppressGCTransition]
#endif
    [DllImport(Kernel32)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern unsafe nuint VirtualQuery(void* lpAddress, MEMORY_BASIC_INFORMATION* pBuffer, nuint dwLength);

    private static readonly ConcurrentDictionary<nint, nuint> Allocations = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool Allocate(out void* addr, nuint size, bool executable = false)
    {
        addr = VirtualAlloc(null, size, 0x3000, executable ? 0x40u : 0x04u);
        if (addr == null)
            return false;

        Allocations[(nint)addr] = size;
        return true;
    }

    public static unsafe bool Free(void* addr)
        => Allocations.TryRemove((nint)addr, out _)
            && VirtualFree(addr, 0, 0x8000) != 0;

    // there is no write without read page protection for windows
    public static unsafe bool Protect(void* addr, bool read, bool write, bool exec)
    {
        uint old;
        // ReSharper disable CommentTypo
        return Allocations.TryGetValue((nint)addr, out var size)
            && VirtualProtect(addr, size,
                read || write
                    ? write
                        ? exec
                            ? 0x40u // PAGE_EXECUTE_READWRITE
                            : 0x04u // PAGE_READWRITE
                        : exec
                            ? 0x20u // PAGE_EXECUTE_READ
                            : 0x02u // PAGE_READONLY
                    : exec
                        ? 0x10u // PAGE_EXECUTE
                        : 0x01u // PAGE_NOACCESS
                , &old) == 0;
        // ReSharper restore CommentTypo
    }

    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public struct KSYSTEM_TIME
    {
        public uint LowPart;

        public int High1Time;

        public int High2Time;
    }

    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    internal struct KUSER_SHARED_DATA
    {
        public uint TickCountLowDeprecated;

        public uint TickCountMultiplier;

        public KSYSTEM_TIME InterruptTime;

        public KSYSTEM_TIME SystemTime;
    }

    internal static unsafe ref readonly KUSER_SHARED_DATA UserSharedData
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef<KUSER_SHARED_DATA>((KUSER_SHARED_DATA*)0x7FFE0000);
    }

    public static ref readonly KSYSTEM_TIME InterruptTime
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref UserSharedData.InterruptTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    internal struct MEMORY_BASIC_INFORMATION
    {
        public unsafe void* BaseAddress;

        public unsafe void* AllocationBase;

        public PAGE_FLAGS AllocationProtect;

        public ushort PartitionId;

        public nuint RegionSize;

        public MEM_STATE State;

        public PAGE_FLAGS Protect;

        public MEM_TYPE Type;
    }

    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
    internal enum PAGE_FLAGS : uint
    {
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000,
        PAGE_REVERT_TO_FILE_MAP = 0x80000000,
        PAGE_ENCLAVE_THREAD_CONTROL = 0x80000000,
        PAGE_ENCLAVE_UNVALIDATED = 0x20000000,
        PAGE_ENCLAVE_DECOMMIT = 0x10000000
    }

    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    internal enum MEM_STATE : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_FREE = 0x10000,
        MEM_RESERVE = 0x2000
    }

    [Flags]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    internal enum MEM_TYPE : uint
    {
        MEM_IMAGE = 0x1000000,
        MEM_MAPPED = 0x40000,
        MEM_PRIVATE = 0x20000
    }

    public static unsafe bool IsMemoryWritable(byte* p)
    {
        MEMORY_BASIC_INFORMATION info = default;
        p = (byte*)((nuint)p & ~(nuint)0xFFF);
        VirtualQuery(p, &info, (nuint)sizeof(MEMORY_BASIC_INFORMATION));
        return (info.Protect & (
            PAGE_FLAGS.PAGE_GUARD
            | PAGE_FLAGS.PAGE_NOACCESS
            | PAGE_FLAGS.PAGE_READONLY
            | PAGE_FLAGS.PAGE_EXECUTE
            | PAGE_FLAGS.PAGE_EXECUTE_READ
        )) == 0;
    }
}
