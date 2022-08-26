using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace StirlingLabs.Utilities
{
    // NOTE: safe to use but has no effect on platforms other than windows at this time
    [PublicAPI]
    public sealed class MultimediaTimerPeriod : IDisposable
    {
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        [SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
        [SuppressMessage("Design", "CA1060:Move pinvokes to native methods class")]
        internal static class Native
        {
            private const string WinMm = "winmm";

            [DllImport(WinMm, ExactSpelling = true)]
            internal static extern int timeGetDevCaps(ref TIMECAPS ptc, int cbtc);

            [DllImport(WinMm, ExactSpelling = true)]
            internal static extern int timeBeginPeriod(int uPeriod);

            [DllImport(WinMm, ExactSpelling = true)]
            internal static extern int timeEndPeriod(int uPeriod);
        }

        private static readonly TIMECAPS TimeCapabilities;

        private static int _inTimePeriod;

        private readonly int _period;

        private int _disposed;

        static unsafe MultimediaTimerPeriod()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            var result = Native.timeGetDevCaps(ref TimeCapabilities, sizeof(TIMECAPS));
            if (result != 0)
            {
#pragma warning disable CA1065
                throw new InvalidOperationException(
                    $"The request to get time capabilities was not completed because an unexpected error with code {result} occured.");
#pragma warning restore CA1065
            }
        }

        internal MultimediaTimerPeriod(int period)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            if (Interlocked.Increment(ref _inTimePeriod) != 1)
            {
                Interlocked.Decrement(ref _inTimePeriod);
                throw new NotSupportedException(
                    "The process is already within a time period. Nested time periods are not supported.");
            }

            if (period < TimeCapabilities.wPeriodMin || period > TimeCapabilities.wPeriodMax)
            {
                throw new ArgumentOutOfRangeException(nameof(period),
                    "The request to begin a time period was not completed because the resolution specified is out of range.");
            }

            BeginPeriodInternal(period);

            this._period = period;
        }

        public static void BeginPeriod(int period)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            BeginPeriodInternal(period);
        }

        public static void EndPeriod(int period)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            EndPeriodInternal(period);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        internal static void BeginPeriodInternal(int period)
        {
            var result = Native.timeBeginPeriod(period);
            if (result != 0)
                throw new InvalidOperationException(
                    "The request to begin a time period was not completed because an unexpected error with code " + result + " occured.");
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        internal static void EndPeriodInternal(int period)
        {
            var result = Native.timeEndPeriod(period);
            if (result != 0)
                throw new InvalidOperationException(
                    "The request to end a time period was not completed because an unexpected error with code " + result + " occured.");
        }

        internal static int MinimumPeriod => TimeCapabilities.wPeriodMin;

        internal static int MaximumPeriod => TimeCapabilities.wPeriodMax;

        internal int Period
        {
            get {
                if (Interlocked.CompareExchange(ref _disposed, 0, 0) != 0)
                    throw new ObjectDisposedException("The time period instance has been disposed.");

                return _period;
            }
        }

        public void Dispose()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            EndPeriodInternal(this._period);
            Interlocked.Decrement(ref _inTimePeriod);
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal readonly struct TIMECAPS
        {
            internal readonly int wPeriodMin;

            internal readonly int wPeriodMax;
        }
    }
}
