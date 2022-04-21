using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
public readonly struct Ptr<T>
    : IEquatable<Ptr<T>>,
        IComparable<Ptr<T>>,
        IEquatable<nuint>,
        IEquatable<nint>
    where T : unmanaged
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public unsafe string DebugString
        => Unsafe.IsNullRef(ref Reference)
            ? "<null reference>"
            : Type<T>.IsPrimitive()
                ? Reference.ToString() ?? ""
                : sizeof(nuint) == 8
                    ? $"@ 0x{(ulong)(nuint)Value:X16}"
                    : $"@ 0x{(uint)(nuint)Value:X8}";

    public readonly unsafe T* Value;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public unsafe ref T Reference => ref Unsafe.AsRef<T>(Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe Ptr(T* value)
        => Value = value;

    [SuppressMessage("Usage", "CA2225", Justification = "See Target")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator T*(Ptr<T> ptr)
        => ptr.Value;

    [SuppressMessage("Usage", "CA2225", Justification = "See default constructor")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator Ptr<T>(T* ptr)
        => new(ptr);

    [SuppressMessage("Usage", "CA2225", Justification = "Different intent")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe T* operator ~(Ptr<T> ptr)
        => ptr.Value;

    [SuppressMessage("Design", "CA1043", Justification = "Native integer")]
    public ref T this[nint offset]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.Add(ref Reference, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Ptr<T> FromPointer(T* ptr)
        => ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int CompareTo(Ptr<T> other)
#if NET5_0_OR_GREATER
        => ((nuint)Value).CompareTo((nuint)other.Value);
#else
        {
            return sizeof(T*) switch
            {
                4 => ((uint)Value).CompareTo((uint)other.Value),
                8 => ((ulong)Value).CompareTo((ulong)other.Value),
                _ => throw new PlatformNotSupportedException()
            };
        }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => obj is Ptr<T> other && Equals(other)
            || obj is nint ni && Equals(ni)
            || obj is nuint nu && Equals(nu);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override unsafe int GetHashCode()
        => ((nint)Value).GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Ptr<T> left, Ptr<T> right)
        => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Ptr<T> left, Ptr<T> right)
        => !(left == right);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Ptr<T> left, nuint right)
        => (nuint)left == right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Ptr<T> left, nuint right)
        => (nuint)left != right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(nuint left, Ptr<T> right)
        => left == (nuint)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(nuint left, Ptr<T> right)
        => left != (nuint)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Ptr<T> left, nint right)
        => (nint)left == right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Ptr<T> left, nint right)
        => (nint)left != right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(nint left, Ptr<T> right)
        => left == (nint)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(nint left, Ptr<T> right)
        => left != (nint)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator ==(Ptr<T> left, void* right)
        => (void*)left == right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator !=(Ptr<T> left, void* right)
        => (void*)left != right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator ==(void* left, Ptr<T> right)
        => left == (void*)right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool operator !=(void* left, Ptr<T> right)
        => left != (void*)right;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator void*(Ptr<T> p)
        => p.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator nuint(Ptr<T> p)
        => (nuint)p.Value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe implicit operator nint(Ptr<T> p)
        => (nint)p.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(Ptr<T> other)
        => Value == other.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(nint other)
        => (nint)Value == other;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(nuint other)
        => (nuint)Value == other;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool Equals(T* other)
        => Value == other;
}
