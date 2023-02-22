using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public static class SpanExtensions
{
    [SuppressMessage("Design", "CA1045", Justification = "Intentional")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Advance<T>(ref this Span<T> span, int length)
        => span = span.Slice(length);

    [SuppressMessage("Design", "CA1045", Justification = "Intentional")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Advance<T>(ref this BigSpan<T> span, nuint length)
        => span = span.Slice(length);

    [SuppressMessage("Design", "CA1045", Justification = "Intentional")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Advance<T>(ref this ReadOnlySpan<T> span, int length)
        => span = span.Slice(length);

    [SuppressMessage("Design", "CA1045", Justification = "Intentional")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Advance<T>(ref this ReadOnlyBigSpan<T> span, nuint length)
        => span = span.Slice(length);
}
