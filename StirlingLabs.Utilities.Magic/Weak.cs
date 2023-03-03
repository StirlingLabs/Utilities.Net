using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public readonly struct Weak<T>
    : IEquatable<Weak<T>>,
        IEquatable<WeakReference<T>>,
        IEquatable<T>
    where T : class
{
    private readonly WeakReference _handle;

    public T? Target
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _handle.Target as T;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _handle.Target = value;
    }

    public bool IsAlive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _handle.IsAlive;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Weak(T target)
        => _handle = new(target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_1_OR_GREATER || NET
    public bool TryGetTarget([NotNullWhen(true)] out T? target)
#else
    public bool TryGetTarget(out T? target)
#endif
    {
        var handle = _handle;
        if (handle is { IsAlive: true, Target: T t })
        {
            target = t;
            return true;
        }
        target = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTarget(T target)
        => Target = target;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("ReSharper", "RedundantSuppressNullableWarningExpression",
        Justification = ".NET Standard 2.0 is missing proper nullability annotations")]
    public static implicit operator WeakReference<T>?(Weak<T> weak)
        => weak.TryGetTarget(out var target) ? new(target!) : default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator WeakReference?(Weak<T> weak)
        => weak.TryGetTarget(out var target) ? new(target) : default;

    [SuppressMessage("	Usage", "CA2225", Justification = "See Constructor")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Weak<T>(T target)
        => new(target);

    [SuppressMessage("	Usage", "CA2225", Justification = "See Extension Method")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_1_OR_GREATER || NET
    // ReSharper disable once UseNullableAnnotationInsteadOfAttribute
    [return: MaybeNull]
#endif
    // ReSharper disable once UseNullableReferenceTypesAnnotationSyntax
    [CanBeNull]
    public static implicit operator T(Weak<T> weak)
        => weak.Target!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WeakReference<T>? ToTypedWeakReference()
        => this;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WeakReference<T>? ToWeakReference()
        => this;

    public override bool Equals(object? obj)
        => obj switch
        {
            Weak<T> other => Equals(other),
            WeakReference<T> otherTwr => Equals(otherTwr),
            WeakReference otherWr => Equals(otherWr),
            T otherT => Equals(otherT),
            _ => false
        };

    public bool Equals(Weak<T> other)
        => _handle == other._handle;

    public bool Equals(WeakReference<T>? other)
    {
        T? otherTarget = null;
        other?.TryGetTarget(out otherTarget);
        return Equals(otherTarget);
    }

    public bool Equals(WeakReference? other)
    {
        T? otherTarget = null;
        if (other?.IsAlive ?? false)
            otherTarget = other.Target as T;
        return Equals(otherTarget);
    }

    public bool Equals(T? other)
    {
        TryGetTarget(out var target);
        if (ReferenceEquals(target, other))
            return true;
        if (Type<T>.IsAssignableTo<IEquatable<T>>())
            return ((IEquatable<T>?)target)?.Equals(other!)
                ?? (((IEquatable<T>?)other)?.Equals(target!)
                    ?? true);
        return target == other;
    }


    public override int GetHashCode()
        => _handle.GetHashCode();

    public static bool operator ==(Weak<T> left, Weak<T> right)
        => left.Equals(right);

    public static bool operator !=(Weak<T> left, Weak<T> right)
        => !(left == right);
}

[PublicAPI]
public static class WeakExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Weak<T> ToWeak<T>(this T target)
        where T : class
        => new(target);
}
