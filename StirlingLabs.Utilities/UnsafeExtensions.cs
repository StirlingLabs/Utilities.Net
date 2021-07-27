using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class UnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TypeEquals(this object? a, object? b)
            => a is not null
                ? b is not null
                && TypeEqualsNotNull(a, b)
                : b is null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool TypeEqualsNotNull(object a, object b)
            => **(void***)Unsafe.AsPointer(ref a)
                == **(void***)Unsafe.AsPointer(ref b);
    }
}
