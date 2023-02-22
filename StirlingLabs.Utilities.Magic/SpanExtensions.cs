using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public static class SpanExtensions
{
    internal static unsafe T* GetPointer<T>(in this ReadOnlySpan<T> span) where T : unmanaged
        => (T*)Unsafe.AsPointer(ref Unsafe.AsRef(span.GetPinnableReference()));

    internal static unsafe T* GetPointer<T>(in this Span<T> span) where T : unmanaged
        => (T*)Unsafe.AsPointer(ref span.GetPinnableReference());
}
