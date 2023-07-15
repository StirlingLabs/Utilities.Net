using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using StirlingLabs.Utilities;

namespace StirlingLabs.Utilities;

[PublicAPI]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("Performance", "CA1810", Justification = "Implementation detail")]
public static class NativeMemory
{
    private static readonly unsafe delegate* managed<nuint, void*> malloc;
    private static readonly unsafe delegate* managed<nuint, nuint, void*> calloc;
    private static readonly unsafe delegate* managed<void*, void> free;
    internal static readonly unsafe delegate* managed<void*, nuint, void*> realloc;
    internal static readonly unsafe delegate* managed<void*, void*, nuint, void*> memmove;
    static unsafe NativeMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            malloc = &ucrt.malloc;
            calloc = &ucrt.calloc;
            free = &ucrt.free;
            realloc = &ucrt.realloc;
            memmove = &ucrt.memmove;

        }
        else
        {
            malloc = &libc.malloc;
            calloc = &libc.calloc;
            free = &libc.free;
            realloc = &libc.realloc;
            memmove = &libc.memmove;
        }
    }

    public static unsafe Span<byte> AllocUnsafe(nuint size)
        => new(New(size), (int)size);

    public static unsafe Span<byte> Alloc(nuint count, nuint size)
        => new(New(count, size), (int)(count * size));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Alloc(nuint size)
        => Alloc(1, size);

    public static unsafe void* NewUnsafe(nuint size)
        => malloc(size);
    public static unsafe void* New(nuint count, nuint size)
        => calloc(count, size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void* New(nuint size)
        => New(1, size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* New<T>() where T : unmanaged
        => (T*)New(SizeOf<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* New<T>(Common.InitPointerDelegate<T> initializer) where T : unmanaged
    {
        if (initializer is null) throw new ArgumentNullException(nameof(initializer));
        var p = New<T>();
        initializer(p);
        return p;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* NewUnsafe<T>() where T : unmanaged
        => (T*)NewUnsafe(SizeOf<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* NewUnsafe<T>(Common.InitPointerDelegate<T> initializer) where T : unmanaged
    {
        if (initializer is null) throw new ArgumentNullException(nameof(initializer));
        var p = NewUnsafe<T>();
        initializer(p);
        return p;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Free(void* ptr)
        => free(ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Free<T>(T* ptr) where T : unmanaged
        => free(ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* New<T>(nuint number) where T : unmanaged
        => (T*)New(number, SizeOf<T>());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nuint SizeOf<T>() where T : unmanaged
        => (nuint)sizeof(T);

    public static unsafe void* Copy(void* from, void* to, nuint size)
        => memmove(to, from, size);
}

[PublicAPI]
public static class NativeMemory<T> where T : unmanaged
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> New(nuint count = 1)
        => new(NativeMemory.New(count, SizeOf()), (int)count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> NewUnsafe(nuint count = 1)
        => new(NativeMemory.NewUnsafe(count * SizeOf()), (int)count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nuint SizeOf()
        => (nuint)sizeof(T);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe nuint MaxSizeOf<TNew>() where TNew : unmanaged
        => (nuint)(sizeof(T) > sizeof(TNew) ? sizeof(T) : sizeof(TNew));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe nuint MinSizeOf<TNew>() where TNew : unmanaged
        => (nuint)(sizeof(T) > sizeof(TNew) ? sizeof(TNew) : sizeof(T));

    public static unsafe TNew* ReAlloc<TNew>(T* ptr) where TNew : unmanaged
        => (TNew*)NativeMemory.realloc(ptr, (nuint)sizeof(TNew));

    public static unsafe Span<TNew> ReAlloc<TNew>(Span<T> span, nuint count) where TNew : unmanaged
    {
        var p = Unsafe.AsPointer(ref span.GetPinnableReference());
        return new(NativeMemory.realloc(p, count * (nuint)sizeof(TNew)), (int)count);
    }

    public static unsafe Span<T> Resize(Span<T> span, nuint count)
    {
        var p = Unsafe.AsPointer(ref span.GetPinnableReference());
        return new(NativeMemory.realloc(p, count * (nuint)sizeof(T)), (int)count);
    }

    public static unsafe TNew* Copy<TNew>(T* from, TNew* to) where TNew : unmanaged
        => (TNew*)NativeMemory.memmove(to, from, MinSizeOf<TNew>());
}