#pragma warning disable CA1021, CA1045
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
[DebuggerStepThrough, DebuggerNonUserCode]
public static class IntentionallyUnused
{
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Parameter<T>(T value) { }

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Variable<T>(T value) { }

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reference<T>(ref T value) { }

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadOnlyReference<T>(in T value) { }

    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Out<T>(out T value)
        => Unsafe.SkipInit(out value);
}
