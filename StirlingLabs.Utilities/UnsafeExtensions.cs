using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class UnsafeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TypeEquals<T1, T2>(this T1? a, T2? b)
            where T1 : class where T2 : class
            => a is not null
                ? b is not null
                && TypeEqualsNotNull(a, b)
                : b is null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool TypeEqualsNotNull<T1, T2>(T1 a, T2 b)
            where T1 : class where T2 : class
            => **(void***)Unsafe.AsPointer(ref a)
                == **(void***)Unsafe.AsPointer(ref b);
    }
}
