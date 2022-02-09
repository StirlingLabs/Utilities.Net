using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;


namespace StirlingLabs.Utilities;

[PublicAPI]
public static class Common
{
    public static readonly unsafe bool Is64Bit = sizeof(nint) == 8;

    public delegate void InitDelegate<in T>(T item) where T : class;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Init<T>(Func<T> initializer)
    {
        if (initializer is null)
            throw new ArgumentNullException(nameof(initializer));
        return initializer();
    }

    [SuppressMessage("Design", "CA1045", Justification = "Intentional")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Init<T>(T item, InitDelegate<T> initializer) where T : class
    {
        if (initializer is null)
            throw new ArgumentNullException(nameof(initializer));
        initializer(item);
        return item;
    }

    public unsafe delegate void InitPointerDelegate<T>(T* item) where T : unmanaged;

    [SuppressMessage("Design", "CA1045", Justification = "Intentional")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* Init<T>(T* item, InitPointerDelegate<T> initializer) where T : unmanaged
    {
        if (initializer is null)
            throw new ArgumentNullException(nameof(initializer));
        initializer(item);
        return item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> hasher)
        => new DelegatingEqualityComparer<T>(equals, hasher);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals)
        => new DelegatingEqualityComparer<T>(equals);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    ///    Assert.Throws&lt;E&gt;( () =&gt; new Span&lt;T&gt;().Discard() );
    /// </code>
    /// which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Discard<T>(in this Span<T> span)
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
    ///    Assert.Throws&lt;E&gt;( () =&gt; new ReadOnlySpan&lt;T&gt;().Discard() );
    /// </code>
    /// which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Discard<T>(in this ReadOnlySpan<T> span)
    {
        // This space intentionally left blank.
    }

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
    ///    Assert.Throws&lt;E&gt;( () =&gt; new Span&lt;T&gt;().Discard() );
    /// </code>
    /// which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Discard<T>(in this BigSpan<T> span)
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
    ///    Assert.Throws&lt;E&gt;( () =&gt; new ReadOnlySpan&lt;T&gt;().Discard() );
    /// </code>
    /// which turns the lambda return type back to "void" and eliminates the troublesome box instruction.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Discard<T>(in this ReadOnlyBigSpan<T> span)
    {
        // This space intentionally left blank.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("Design", "CA1062", Justification = "Allow null reference exception")]
    public static nuint GetLength<T>(this T[] array)
        => Is64Bit ? (nuint)array.LongLength : (nuint)array.Length;


    public static Thread RunThread(Action a)
    {
        static void Exec(object? o)
            => ((Action)o!)();

        var thread = new Thread(Exec);
        thread.Start(a);
        return thread;
    }

    public static Thread RunThread<T>(Action<T> a, T state)
    {
        static void Exec(object? o)
        {
            var (f, s) = (ValueTuple<Action<T>, T>)o!;
            f(s);
        }

        var thread = new Thread(Exec);
        thread.Start((a, state));
        return thread;
    }
}
