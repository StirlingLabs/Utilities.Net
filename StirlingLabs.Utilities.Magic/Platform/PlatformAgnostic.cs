using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public static class PlatformAgnostic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCurrentThreadProcessorAffinityMask(nuint procBits)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.SetCurrentThreadProcessorAffinityMask(procBits);
        else
            Posix.SetCurrentThreadProcessorAffinityMask(procBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCurrentThreadProcessorAffinity(int procBits)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.SetCurrentThreadProcessorAffinity(procBits);
        else
            Posix.SetCurrentThreadProcessorAffinity(procBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1021", Justification = "Implementation detail")]
    public static unsafe bool Allocate(out void* addr, nuint size, bool executable = false)
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Windows.Allocate(out addr, size, executable)
            : Posix.Allocate(out addr, size, executable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool Free(void* addr)
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Windows.Free(addr)
            : Posix.Free(addr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool Protect(void* addr, bool read, bool write, bool exec)
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Windows.Protect(addr, read, write, exec)
            : Posix.Protect(addr, read, write, exec);

    private static unsafe void** AllocateNullReadOnlyGuardPage()
        => Allocate(out var p, 4096)
            ? &p
            : throw new NotImplementedException();

    public static readonly unsafe void** NullReadOnlyGuardPage
        = AllocateNullReadOnlyGuardPage();

    public static unsafe bool IsMemoryWritable(byte* ptr)
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Windows.IsMemoryWritable(ptr)
            : Posix.IsMemoryWritable(ptr);
}
