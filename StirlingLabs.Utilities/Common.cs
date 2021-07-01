using System;
using System.Collections.Generic;
using JetBrains.Annotations;

#nullable enable
namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class Common
    {
        
        public static readonly unsafe bool Is64Bit = sizeof(nint) == 8;

        public static T OnDemand<T>(ref WeakReference<T>? cache, Func<T> factory)
            where T : class
        {
            T? d;
            if (cache is null)
                cache = new(d = factory());
            else if (!cache.TryGetTarget(out d))
                cache.SetTarget(d = factory());
            return d;
        }

        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> hasher)
            => new DelegatingEqualityComparer<T>(equals, hasher);
        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals)
            => new DelegatingEqualityComparer<T>(equals);
        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T, int> hasher)
            => new DelegatingEqualityComparer<T>(hasher);

        /// <summary>
        /// Prevents generation of boxing instructions in certain situations.
        /// </summary>
        /// <remarks>
        /// The innocent looking construct:
        /// <code>
        ///    Assert.Throws&lt;E&gt;( () =&gt; new Span&lt;T&gt;() );
        /// </code>
        /// generates a hidden box of the Span as the return value of the lambda. This makes the IL illegal and unloadable on
        /// runtimes that enforce the actual Span rules (never mind that we expect never to reach the box instruction...)
        ///
        /// The workaround is to code it like this:
        /// <code>
        ///    Assert.Throws&lt;E&gt;( () =&gt; new Span&lt;T&gt;().DontBox() );
        /// </code>
        /// which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
        /// </remarks>
        public static void DontBox<T>(this Span<T> span)
        {
            // This space intentionally left blank.
        }

        /// <summary>
        /// Prevents generation of boxing instructions in certain situations.
        /// </summary>
        /// <remarks>
        /// The innocent looking construct:
        /// <code>
        ///    Assert.Throws&lt;E&gt;( () =&gt; new ReadOnlySpan&lt;T&gt;() );
        /// </code>
        /// generates a hidden box of the Span as the return value of the lambda. This makes the IL illegal and unloadable on
        /// runtimes that enforce the actual Span rules (never mind that we expect never to reach the box instruction...)
        ///
        /// The workaround is to code it like this:
        /// <code>
        ///    Assert.Throws&lt;E&gt;( () =&gt; new ReadOnlySpan&lt;T&gt;().DontBox() );
        /// </code>
        /// which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
        /// </remarks>
        public static void DontBox<T>(this ReadOnlySpan<T> span)
        {
            // This space intentionally left blank.
        }

        public static nuint GetLength<T>(this T[] array)
            => Is64Bit ? (nuint)array.LongLength : (nuint)array.Length;
        
    }
}
