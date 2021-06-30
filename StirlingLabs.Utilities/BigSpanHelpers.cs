using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace StirlingLabs.Utilities
{
    internal static partial class BigSpanHelpers
    {
        
        public static readonly unsafe bool Is64Bit = sizeof(nint) == 8;

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(int value) => (value & (value - 1)) == 0 && value > 0;

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(uint value) => (value & (value - 1)) == 0 && value != 0;
    }
}
