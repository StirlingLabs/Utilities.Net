using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities.Extensions;

[PublicAPI]
[SuppressMessage("Microsoft.Design", "CA1045", Justification = "Nope")]
public static class AtomicExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AtomicIncrement(ref this int target)
        => Interlocked.Increment(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long AtomicIncrement(ref this long target)
        => Interlocked.Increment(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AtomicDecrement(ref this int target)
        => Interlocked.Decrement(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long AtomicDecrement(ref this long target)
        => Interlocked.Decrement(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AtomicAdd(ref this int target, int value)
        => Interlocked.Add(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long AtomicAdd(ref this long target, long value)
        => Interlocked.Add(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AtomicExchange(ref this int target, int value)
        => Interlocked.Exchange(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long AtomicExchange(ref this long target, long value)
        => Interlocked.Exchange(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AtomicCompareExchange(ref this int target, int compare, int value)
        => Interlocked.CompareExchange(ref target, value, compare);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long AtomicCompareExchange(ref this long target, long compare, long value)
        => Interlocked.CompareExchange(ref target, value, compare);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicOr(ref this int target, int value)
        => LlvmAtomicOps<int>.StoreOr(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicOr(ref this long target, long value)
        => LlvmAtomicOps<long>.StoreOr(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicOr(ref this uint target, uint value)
        => LlvmAtomicOps<uint>.StoreOr(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicOr(ref this ulong target, ulong value)
        => LlvmAtomicOps<ulong>.StoreOr(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicAnd(ref this int target, int value)
        => LlvmAtomicOps<int>.StoreAnd(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicAnd(ref this long target, long value)
        => LlvmAtomicOps<long>.StoreAnd(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicAnd(ref this uint target, uint value)
        => LlvmAtomicOps<uint>.StoreAnd(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicAnd(ref this ulong target, ulong value)
        => LlvmAtomicOps<ulong>.StoreAnd(ref target, value);

#if !NETSTANDARD
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicIncrement(ref this uint target)
        => Interlocked.Increment(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicIncrement(ref this ulong target)
        => Interlocked.Increment(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicDecrement(ref this uint target)
        => Interlocked.Decrement(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicDecrement(ref this ulong target)
        => Interlocked.Decrement(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicAdd(ref this uint target, uint value)
        => Interlocked.Add(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicAdd(ref this ulong target, ulong value)
        => Interlocked.Add(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicExchange(ref this uint target, uint value)
        => Interlocked.Exchange(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicExchange(ref this ulong target, ulong value)
        => Interlocked.Exchange(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicCompareExchange(ref this uint target, uint compare, uint value)
        => Interlocked.CompareExchange(ref target, value, compare);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicCompareExchange(ref this ulong target, ulong compare, ulong value)
        => Interlocked.CompareExchange(ref target, value, compare);

#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicIncrement(ref this uint target)
        => unchecked((uint)Interlocked.Increment(ref Unsafe.As<uint, int>(ref target)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicIncrement(ref this ulong target)
        => unchecked((ulong)Interlocked.Increment(ref Unsafe.As<ulong, long>(ref target)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicDecrement(ref this uint target)
        => unchecked((uint)Interlocked.Decrement(ref Unsafe.As<uint, int>(ref target)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicDecrement(ref this ulong target)
        => unchecked((ulong)Interlocked.Decrement(ref Unsafe.As<ulong, long>(ref target)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicAdd(ref this uint target, uint value)
        => unchecked((uint)Interlocked.Add(ref Unsafe.As<uint, int>(ref target), (int)value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicAdd(ref this ulong target, ulong value)
        => unchecked((ulong)Interlocked.Add(ref Unsafe.As<ulong, long>(ref target), (long)value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint AtomicCompareExchange(ref this uint target, uint compare, uint value)
        => unchecked((uint)Interlocked.CompareExchange(ref Unsafe.As<uint, int>(ref target), (int)value, (int)compare));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AtomicCompareExchange(ref this ulong target, ulong compare, ulong value)
        => unchecked((ulong)Interlocked.CompareExchange(ref Unsafe.As<ulong, long>(ref target), (long)value, (long)compare));
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicXor(ref this int target, int value)
        => LlvmAtomicOps<int>.StoreXor(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicXor(ref this long target, long value)
        => LlvmAtomicOps<long>.StoreXor(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicXor(ref this uint target, uint value)
        => LlvmAtomicOps<uint>.StoreXor(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicXor(ref this ulong target, ulong value)
        => LlvmAtomicOps<ulong>.StoreXor(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T AtomicLoad<T>(ref this T target) where T : unmanaged
        => LlvmAtomicOps<T>.Load(ref target);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void AtomicStore<T>(ref this T target, T value) where T : unmanaged
        => LlvmAtomicOps<T>.Store(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T AtomicExchange<T>(ref this T target, T value) where T : unmanaged
        => LlvmAtomicOps<T>.Xchg(ref target, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T AtomicCompareExchange<T>(ref this T target, T compare, T exchanged) where T : unmanaged
        => LlvmAtomicOps<T>.CmpXchg(ref target, compare, exchanged);
}

/*
[PublicAPI]
public static class Atomic128Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref readonly Int128 AtomicCompareExchange(ref this Int128 target, in Int128 compare, in Int128 exchanged)
        => ref LlvmAtomicOps<Int128>.CmpXchgLarge(ref target, compare, exchanged) ? ref exchanged : ref target;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref readonly UInt128 AtomicCompareExchange(ref this UInt128 target, in UInt128 compare, in UInt128 exchanged)
        => ref LlvmAtomicOps<UInt128>.CmpXchgLarge(ref target, compare, exchanged) ? ref exchanged : ref target;
}
*/
