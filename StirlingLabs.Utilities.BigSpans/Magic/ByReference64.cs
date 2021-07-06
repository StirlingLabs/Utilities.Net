using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities.Magic
{
    [PublicAPI]
    [StructLayout(LayoutKind.Sequential)]
    public readonly ref struct ByReference<T>
    {
        private readonly Span<T> _span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(Span<T> span)
            => _span = span;

#if NETSTANDARD2_0
        [SuppressMessage("Microsoft.Design","CA1045", Justification = "Nope")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ByReference(ref T item)
            => _span = new(Unsafe.AsPointer(ref item), 1);
#else
        [SuppressMessage("Microsoft.Design","CA1045", Justification = "Nope")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(ref T item)
            => _span = MemoryMarshal.CreateSpan(ref item, 1);
#endif


        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref MemoryMarshal.GetReference(_span);
        }
    }

    // NOT SAFE TO DECLARE PUBLIC; writes int 1 beyond end
    [PublicAPI]
    [StructLayout(LayoutKind.Sequential, Size = 8)] // assuming 64-bit
    internal readonly ref struct ByReference64<T>
    {
        private readonly Span<T> _span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference64(Span<T> span)
            => _span = span;

#if NETSTANDARD2_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ByReference64(ref T item)
            => _span = new(Unsafe.AsPointer(ref item), 1);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference64(ref T item)
            => _span = MemoryMarshal.CreateSpan(ref item, 1);
#endif
        
        public ref T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref MemoryMarshal.GetReference(_span);
        }
    }
}
