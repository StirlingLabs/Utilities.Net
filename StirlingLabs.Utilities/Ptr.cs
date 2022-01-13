using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public readonly struct Ptr<T> : IEquatable<Ptr<T>>, IComparable<Ptr<T>> where T : unmanaged
    {
        public readonly unsafe T* Target;

        public unsafe ref T Reference => ref Unsafe.AsRef<T>(Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Ptr(T* target)
            => Target = target;

        [SuppressMessage("Usage", "CA2225", Justification = "See Target")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator T*(Ptr<T> ptr)
            => ptr.Target;

        [SuppressMessage("Usage", "CA2225", Justification = "See default constructor")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator Ptr<T>(T* ptr)
            => new(ptr);

        [SuppressMessage("Usage", "CA2225", Justification = "Different intent")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe T* operator ~(Ptr<T> ptr)
            => ptr.Target;

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
            => ((nuint)Target).CompareTo((nuint)other.Target);
#else
        {
            return sizeof(T*) switch
            {
                4 => ((uint)Target).CompareTo((uint)other.Target),
                8 => ((ulong)Target).CompareTo((ulong)other.Target),
                _ => throw new PlatformNotSupportedException()
            };
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj)
        {
            if (obj is Ptr<T> p)
                return Equals(p);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe int GetHashCode()
            => ((nint)Target).GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Ptr<T> left, Ptr<T> right)
            => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Ptr<T> left, Ptr<T> right)
            => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool Equals(Ptr<T> other)
            => Target == other.Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool Equals(IntPtr other)
            => Target == (T*)other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool Equals(T* other)
            => Target == other;
    }
}
