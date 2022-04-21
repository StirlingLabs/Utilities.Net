using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
// ReSharper disable UseNameofExpression, NotResolvedInText
[DebuggerDisplay("Pointer = {Pointer,X}, Length = {Length}", Type = "{Type}")]
// ReSharper restore UseNameofExpression, NotResolvedInText
public readonly unsafe struct UnsafeMemoryView<T> : IEnumerable<UnsafePtr<T>>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public Type Type => typeof(T);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public readonly void* Pointer;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public readonly nuint Length;

    public UnsafeMemoryView(void* pointer, nuint length)
    {
        Pointer = pointer;
        Length = length;
    }

    public IEnumerator<UnsafePtr<T>> GetEnumerator()
    {
        for (var i = (nuint)0; i < Length; ++i)
            yield return this[(nint)i];
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public UnsafePtr<T> this[nint index]
        => new((void*)((nuint)Pointer + (nuint)Unsafe.SizeOf<T>() * (nuint)index));
}