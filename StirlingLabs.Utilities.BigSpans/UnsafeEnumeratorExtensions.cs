using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using InlineIL;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public static class UnsafeEnumeratorExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AsPinnedEnumerable<T>(this Span<T> a, Action<IEnumerable<T>> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            f(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult AsPinnedEnumerable<T, TResult>(this Span<T> a, Func<IEnumerable<T>, TResult> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            return f(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AsPinnedEnumerable<T>(this ReadOnlySpan<T> a, Action<IEnumerable<T>> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            f(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult AsPinnedEnumerable<T, TResult>(this ReadOnlySpan<T> a, Func<IEnumerable<T>, TResult> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            return f(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AsPinnedEnumerable<T>(this BigSpan<T> a, Action<IEnumerable<T>> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            f(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult AsPinnedEnumerable<T, TResult>(this BigSpan<T> a, Func<IEnumerable<T>, TResult> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            return f(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AsPinnedEnumerable<T>(this ReadOnlyBigSpan<T> a, Action<IEnumerable<T>> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            f(e);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult AsPinnedEnumerable<T, TResult>(this ReadOnlyBigSpan<T> a, Func<IEnumerable<T>, TResult> f)
        {
            IL.DeclareLocals(new LocalVar(typeof(T).MakeByRefType()).Pinned());

            IL.PushInRef(a.GetPinnableReference());
            IL.Emit.Stloc_0();

            var e = new UnsafeEnumerator<T>(a);
            return f(e);
        }
    }
}
