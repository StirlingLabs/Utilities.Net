using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
#if !NET7_0_OR_GREATER
using static InlineIL.IL;
using static InlineIL.IL.Emit;
#endif

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    [StructLayout(LayoutKind.Sequential)]
    public readonly ref struct ByReference<T>
    {
#if NET7_0_OR_GREATER
        private readonly ref T _ref;
#else
        private readonly ReadOnlySpan<T> _ref;
#endif

#if NET7_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(Span<T> span)
            => _ref = MemoryMarshal.GetReference(span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(ReadOnlySpan<T> span)
            => _ref = MemoryMarshal.GetReference(span);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(Span<T> span)
            => _ref = span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(ReadOnlySpan<T> span)
            => _ref = span;
#endif

#if NET7_0_OR_GREATER
        [SuppressMessage("Microsoft.Design", "CA1045", Justification = "Nope")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(ref T item)
            => _ref = ref item;
#elif NETSTANDARD2_0
        [SuppressMessage("Microsoft.Design","CA1045", Justification = "Nope")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ByReference(ref T item)
            => _ref = new(Unsafe.AsPointer(ref item), 1);
#else
        [SuppressMessage("Microsoft.Design", "CA1045", Justification = "Nope")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ByReference(ref T item)
            => _ref = MemoryMarshal.CreateReadOnlySpan(ref item, 1);
#endif

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ref T Value
        {
#if NET7_0_OR_GREATER
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _ref;
#else
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref MemoryMarshal.GetReference(_ref);
#endif
        }

#if !NET7_0_OR_GREATER
        public ref nuint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                Ldarg_0();
                Sizeof(typeof(void*));
                Add();
                return ref ReturnRef<nuint>();
            }
        }
#endif
    }
}
