using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public struct GcHandle<T> : IEquatable<GcHandle<T>>
    {
        private GCHandle _handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcHandle(T target)
            => _handle = GCHandle.Alloc(target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTarget(T target)
        {
            if (_handle != default) _handle.Free();
            _handle = GCHandle.Alloc(target);
        }

        public T? Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T)_handle.Target!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetTarget(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Free()
            => _handle.Free();

        [SuppressMessage("Usage", "CA2225", Justification = "See constructor")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator GcHandle<T>(T target)
            => new(target);

        [SuppressMessage("Usage", "CA2225", Justification = "See Target")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T?(GcHandle<T> handle)
            => handle.Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator GCHandle(GcHandle<T> handle)
            => handle._handle;

        public override bool Equals(object? obj)
        {
            if (obj is GcHandle<T> h) return Equals(h);
            return false;
        }

        public override int GetHashCode()
            => _handle.GetHashCode();

        public static bool operator ==(GcHandle<T> left, GcHandle<T> right)
            => left.Equals(right);

        public static bool operator !=(GcHandle<T> left, GcHandle<T> right)
            => !(left == right);

        public bool Equals(GcHandle<T> other)
            => _handle.Equals(other._handle);

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public GCHandle ToGCHandle()
            => _handle;
    }
}
