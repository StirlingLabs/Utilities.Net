using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
public readonly struct UnsafePtr<T>
    : IEquatable<UnsafePtr<T>>,
        IComparable<UnsafePtr<T>>,
        IEquatable<nuint>,
        IEquatable<nint>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public unsafe string DebugString
        => Unsafe.IsNullRef(ref Reference)
            ? "<null reference>"
            : Type<T>.IsPrimitive()
                ? Reference!.ToString() ?? ""
                : sizeof(nuint) == 8
                    ? $"@ 0x{(ulong)(nuint)Value:X16}"
                    : $"@ 0x{(uint)(nuint)Value:X8}";

    public readonly unsafe void* Value;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public unsafe ref T Reference => ref Unsafe.AsRef<T>(Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe UnsafePtr(void* value)
        => Value = value;

    [SuppressMessage("Usage", "CA2225", Justification = "See default constructor")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator UnsafePtr<T>(void* ptr)
        => new(ptr);

    [SuppressMessage("Usage", "CA2225", Justification = "Different intent")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* operator ~(UnsafePtr<T> ptr)
        => ptr.Value;

    [SuppressMessage("Design", "CA1043", Justification = "Native integer")]
    public ref T this[nint offset]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.Add(ref Reference, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe UnsafePtr<T> FromPointer(void* ptr)
        => ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int CompareTo(UnsafePtr<T> other)
#if NET5_0_OR_GREATER
        => ((nuint)Value).CompareTo((nuint)other.Value);
#else
        {
            return sizeof(void*) switch
            {
                4 => ((uint)Value).CompareTo((uint)other.Value),
                8 => ((ulong)Value).CompareTo((ulong)other.Value),
                _ => throw new PlatformNotSupportedException()
            };
        }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => obj is UnsafePtr<T> other && Equals(other)
            || obj is nint ni && Equals(ni)
            || obj is nuint nu && Equals(nu);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override unsafe int GetHashCode()
        => ((nint)Value).GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UnsafePtr<T> left, UnsafePtr<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UnsafePtr<T> left, UnsafePtr<T> right)
        => !(left == right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UnsafePtr<T> left, nuint right)
        => (nuint)left == right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UnsafePtr<T> left, nuint right)
        => (nuint)left != right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(nuint left, UnsafePtr<T> right)
        => left == (nuint)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(nuint left, UnsafePtr<T> right)
        => left != (nuint)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UnsafePtr<T> left, nint right)
        => (nint)left == right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UnsafePtr<T> left, nint right)
        => (nint)left != right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(nint left, UnsafePtr<T> right)
        => left == (nint)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(nint left, UnsafePtr<T> right)
        => left != (nint)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator ==(UnsafePtr<T> left, void* right)
        => (void*)left == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator !=(UnsafePtr<T> left, void* right)
        => (void*)left != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator ==(void* left, UnsafePtr<T> right)
        => left == (void*)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator !=(void* left, UnsafePtr<T> right)
        => left != (void*)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator void*(UnsafePtr<T> p)
        => p.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator nuint(UnsafePtr<T> p)
        => (nuint)p.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator nint(UnsafePtr<T> p)
        => (nint)p.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(UnsafePtr<T> other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(nint other)
        => (nint)Value == other;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(nuint other)
        => (nuint)Value == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(void* other)
        => Value == other;
}

