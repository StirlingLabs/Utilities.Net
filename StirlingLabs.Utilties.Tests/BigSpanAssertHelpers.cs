using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using StirlingLabs.Utilities;

namespace StirlingLabs.Utilties.Tests
{
    internal static class BigSpanAssertHelpers
    {
        public delegate void ReadOnlyBigSpanAction<T>(ReadOnlyBigSpan<T> span) where T : unmanaged;

        public delegate void BigSpanAction<T>(BigSpan<T> span) where T : unmanaged;

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Test case assertion helper.")]
        public static TException Throws<TException, T>(ReadOnlyBigSpan<T> span, ReadOnlyBigSpanAction<T> action) where T : unmanaged where TException : Exception
        {
            Exception exception;

            try
            {
                action(span);
                exception = null;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return exception switch
            {
                null => throw new AssertionException($"Did not throw {typeof(TException).FullName}"),
                TException ex when ex.GetType() == typeof(TException) => ex,
                _ => throw new AssertionException($"Did not throw {typeof(TException).FullName}")
            };
        }

        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Test case assertion helper.")]
        public static TException Throws<TException, T>(BigSpan<T> span, BigSpanAction<T> action) where T : unmanaged where TException : Exception
        {
            Exception exception;

            try
            {
                action(span);
                exception = null;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return exception switch
            {
                null => throw new AssertionException($"Did not throw {typeof(TException).FullName}"),
                TException ex when ex.GetType() == typeof(TException) => ex,
                _ => throw new AssertionException($"Did not throw {typeof(TException).FullName}")
            };
        }
    }
}
