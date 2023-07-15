using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using StirlingLabs.Utilities;

namespace StirlingLabs.Utilities;

/// <summary>
/// A static utility class for interacting with unmanaged memory. Provides methods for allocating, deallocating, and manipulating memory.
/// </summary>
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

    /// <summary>
    /// Allocates unmanaged memory without initializing it.
    /// </summary>
    /// <param name="size">The size of the memory to allocate.</param>
    /// <returns>A span of bytes representing the allocated memory.</returns>
    public static unsafe Span<byte> AllocUnsafe(nuint size)
        => new(New(size), (int)size);

    /// <summary>
    /// Allocates unmanaged memory and initializes it to zero.
    /// </summary>
    /// <param name="count">The number of elements to allocate.</param>
    /// <param name="size">The size of each element.</param>
    /// <returns>A span of bytes representing the allocated memory.</returns>
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

    /// <summary>
    /// Deallocates the specified memory.
    /// </summary>
    /// <param name="ptr">A pointer to the memory to deallocate.</param>
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

    /// <summary>
    /// Copies memory from one location to another.
    /// </summary>
    /// <param name="from">A pointer to the source memory.</param>
    /// <param name="to">A pointer to the destination memory.</param>
    /// <param name="size">The size of the memory to copy.</param>
    /// <returns>A pointer to the destination memory.</returns>
    public static unsafe void* Copy(void* from, void* to, nuint size)
        => memmove(to, from, size);
}

/// <summary>
/// A static utility class for interacting with unmanaged memory for a specific type. Provides methods for allocating, reallocating, and manipulating memory.
/// </summary>
/// <typeparam name="T">The type of the elements in the memory.</typeparam>
[PublicAPI]
public static class NativeMemory<T> where T : unmanaged
{
    /// <summary>
    /// Allocates memory for a specific number of items of type T.
    /// </summary>
    /// <param name="count">The number of items to allocate (default is 1).</param>
    /// <returns>A span representing the allocated memory.</returns>
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

    /// <summary>
    /// Reallocates memory for a different type and/or size.
    /// </summary>
    /// <typeparam name="TNew">The type of the new elements in the memory.</typeparam>
    /// <param name="ptr">A pointer to the memory to reallocate.</param>
    /// <returns>A pointer to the reallocated memory.</returns>
    public static unsafe TNew* ReAlloc<TNew>(T* ptr) where TNew : unmanaged
        => (TNew*)NativeMemory.realloc(ptr, (nuint)sizeof(TNew));

    public static unsafe Span<TNew> ReAlloc<TNew>(Span<T> span, nuint count) where TNew : unmanaged
    {
        var p = Unsafe.AsPointer(ref span.GetPinnableReference());
        return new(NativeMemory.realloc(p, count * (nuint)sizeof(TNew)), (int)count);
    }

    /// <summary>
    /// Resizes the allocated memory for a span.
    /// </summary>
    /// <param name="span">The span to resize.</param>
    /// <param name="count">The new size for the span.</param>
    /// <returns>A span representing the resized memory.</returns>
    public static unsafe Span<T> Resize(Span<T> span, nuint count)
    {
        var p = Unsafe.AsPointer(ref span.GetPinnableReference());
        return new(NativeMemory.realloc(p, count * (nuint)sizeof(T)), (int)count);
    }

    /// <summary>
    /// Copies memory from one location to another, potentially changing the type of the data.
    /// </summary>
    /// <typeparam name="TNew">The type of the new elements in the memory.</typeparam>
    /// <param name="from">A pointer to the source memory.</param>
    /// <param name="to">A pointer to the destination memory.</param>
    /// <returns>A pointer to the destination memory.</returns>
    public static unsafe TNew* Copy<TNew>(T* from, TNew* to) where TNew : unmanaged
        => (TNew*)NativeMemory.memmove(to, from, MinSizeOf<TNew>());
}