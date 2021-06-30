// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace System
{
    // ByReference<T> is meant to be used to represent "ref T" fields. It is working
    // around lack of first class support for byref fields in C# and IL. The JIT and
    // type loader has special handling for it that turns it into a thin wrapper around ref T.
    [NonVersionable]
    internal readonly ref struct ByReference<T>
    {
#pragma warning disable CA1823, 169 // private field '{blah}' is never used
        private readonly nint _value;
#pragma warning restore CA1823, 169

        [Intrinsic]
        public unsafe ByReference(ref T value)
        {
            // Implemented as a JIT intrinsic - This default implementation is for
            // completeness and to provide a concrete error if called via reflection
            // or if intrinsic is missed.
            //throw new PlatformNotSupportedException();
            
            // ReSharper disable once ArrangeConstructorOrDestructorBody
            _value = (nint)Unsafe.AsPointer(ref value);
        }

#pragma warning disable CA1822 // Mark members as static
        public unsafe ref T Value
        {
            // Implemented as a JIT intrinsic - This default implementation is for
            // completeness and to provide a concrete error if called via reflection
            // or if the intrinsic is missed.
            [Intrinsic]
            get => ref Unsafe.AsRef<T>((void*)_value);
            //get => throw new PlatformNotSupportedException();
        }
#pragma warning restore CA1822
    }
}
