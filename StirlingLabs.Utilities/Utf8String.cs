using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using NativeMemory = StirlingLabs.Native.NativeMemory;
#if NET5_0_OR_GREATER
#endif

namespace StirlingLabs.Utilities
{
    [PublicAPI]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct Utf8String
        : IEquatable<SizedUtf8String>,
            IComparable<SizedUtf8String>,
            IEquatable<Utf8String>,
            IComparable<Utf8String>
    {
        public readonly sbyte* Pointer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8String(sbyte* pointer) => Pointer = pointer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8String(IntPtr data) : this((sbyte*)data) { }

        private static readonly Dictionary<string, Utf8String> InternedStrings = new();
        private static readonly ConcurrentDictionary<Utf8String, string> InternedUtf8Strings = new();

        private const int MinConstPoolAllocSize = 4096;

        private static readonly LinkedList<(IntPtr Pointer, nuint Used)> ConstPool = new();
        private static readonly LinkedList<(IntPtr Pointer, nuint Size)> ConstExtents = new();

        [SuppressMessage("Design", "CA1043", Justification = "Native integer")]
        public ref sbyte this[nint offset]
        {
            [System.Diagnostics.Contracts.Pure]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref this[(nuint)offset];
        }

        [SuppressMessage("Design", "CA1043", Justification = "Native integer")]
        public ref sbyte this[nuint offset]
        {
            [System.Diagnostics.Contracts.Pure]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Unsafe.AsRef<sbyte>(Pointer + offset);
        }

        public bool Free()
        {
            if (IsInterned)
                return false;
            NativeMemory.Free(Pointer);
            return true;
        }

        public bool IsInterned
        {
            [DebuggerStepThrough]
            [System.Diagnostics.Contracts.Pure]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => InternedUtf8Strings.ContainsKey(this);
        }

        public static Utf8String Create(ReadOnlySpan<sbyte> data)
        {
            if (data.IsEmpty) return new(null);

            // loose string
            var size = (nuint)data.Length;
            var start = NativeMemory.New<sbyte>(size + 1);
            data.CopyTo(new(start, (int)size));
            return new(start);
        }

        public static Utf8String Create(nuint size, [InstantHandle] SpanAction<sbyte> writer)
        {
            if (size is 0) return new(null);
            if (writer is null) throw new ArgumentNullException(nameof(writer));

            // loose string
            var start = NativeMemory.New<sbyte>(size + 1);
            writer(new(start, (int)size));
            return new(start);
        }

        public static Utf8String Create(string? str)
        {
            if (str is null) return new(null);

            var interned = string.IsInterned(str);

            if (interned is null)
            {
                // loose string
                var utf8Size = (nuint)Encoding.UTF8.GetByteCount(str);
                var start = NativeMemory.New<sbyte>(utf8Size + 1);
                fixed (char* pChars = str)
                    // ReSharper disable once RedundantOverflowCheckingContext
                    Encoding.UTF8.GetBytes(pChars, str.Length, (byte*)start, checked((int)utf8Size));
                return new(start);
            }

            lock (ConstPool)
            {
                if (InternedStrings.TryGetValue(interned, out var u))
                    return u;

                var utf8Size = (nuint)Encoding.UTF8.GetByteCount(interned);
                var utf8SizeWithNull = utf8Size + 1;

                if (interned.Length >= MinConstPoolAllocSize)
                {
                    var alloc = NativeMemory.New<sbyte>(utf8Size + 1);

                    var p = (byte*)alloc;
                    fixed (char* pChars = interned)
                        Encoding.UTF8.GetBytes(pChars, interned.Length, p, (int)utf8Size);
                    //p![utf8Size] = 0;

                    ConstExtents.AddLast(((IntPtr)alloc, utf8SizeWithNull));

                    u = new(alloc);
                    InternFromConstPool(u, interned);
                    return u;
                }

                for (var node = ConstPool.First; node != null; node = node.Next)
                {
                    var used = node.Value.Used;
                    if (used >= MinConstPoolAllocSize) continue;

                    var start = node.Value.Pointer + (nint)used;
                    var end = used + utf8SizeWithNull;
                    if (end > MinConstPoolAllocSize) continue;

                    var p = (byte*)start;
                    fixed (char* pChars = interned)
                        Encoding.UTF8.GetBytes(pChars, interned.Length, p!, (int)utf8Size);
                    //p![utf8Size] = 0;
                    node.Value = (start + (nint)utf8SizeWithNull, used + utf8SizeWithNull);

                    u = new(start);
                    InternFromConstPool(u, interned);
                    return u;
                }
                // no buffer available, add a new one
                {
                    var start = NativeMemory.New<sbyte>(MinConstPoolAllocSize);
                    var p = (byte*)start;
                    fixed (char* pChars = interned)
                        Encoding.UTF8.GetBytes(pChars, interned.Length, p, (int)utf8Size);
                    //p![utf8Size] = 0;
                    ConstPool.AddFirst(((IntPtr)start, utf8SizeWithNull));

                    u = new(start);
                    InternFromConstPool(u, interned);
                    return u;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool InternFromConstPool(Utf8String u, string interned)
        {
            Debug.Assert(Monitor.IsEntered(ConstPool));
#if NETSTANDARD2_0 || !DEBUG
            InternedStrings.Add(interned, u);
            var success = Intern(u, interned);
#else
            var success = InternedStrings.TryAdd(interned, u);
            Debug.Assert(success);
            success = Intern(u, interned);
            Debug.Assert(success);
#endif
            return success;
        }

        private static bool Intern(Utf8String u, string interned)
            => InternedUtf8Strings.TryAdd(u, interned);

        public static bool Intern(Utf8String u)
            => Intern(u, string.Intern(u.ToString()!));

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.Contracts.Pure]
        public bool Equals(Utf8String other)
            => Pointer == other.Pointer
                ||
                Pointer != null && other.Pointer != null
                &&
                CompareTo((ReadOnlySpan<byte>)other) == 0;

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.Contracts.Pure]
        public int CompareTo(Utf8String other)
            => Pointer == null
                ? other.Pointer == null
                    ? 0
                    : -1
                : other.Pointer == null
                    ? 1
                    : CompareTo((ReadOnlySpan<byte>)other);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.Contracts.Pure]
        public bool Equals(SizedUtf8String other)
            => Pointer == other.Pointer
                ? Length == other.Length
                : CompareTo(other) == 0;

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.Contracts.Pure]
        public bool Equals(ReadOnlySpan<byte> other)
        {
            var pOther = (sbyte*)Unsafe.AsPointer(ref Unsafe.AsRef(other.GetPinnableReference()));
            if (Pointer == pOther)
                return Length == (nuint)other.Length;

            return CompareTo(other) == 0;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.Contracts.Pure]
        public int CompareTo(SizedUtf8String other)
            => CompareTo((ReadOnlySpan<byte>)other);

        public int CompareTo(ReadOnlySpan<byte> other)
        {
#if NET5_0_OR_GREATER
            var ownEnum = new Utf8RuneEnumerator(this);
            var otherEnum = new Utf8RuneEnumerator(other);
            for (;;)
            {
                var advancedOwn = ownEnum.MoveNext();
                var advancedOther = otherEnum.MoveNext();
                if (!advancedOwn)
                    return !advancedOther
                        ? 0 // same length
                        : -1; // this is shorter

                if (!advancedOther)
                    return 1; // this is longer 

                // compare runes
                var result = ownEnum.Current.CompareTo(otherEnum.Current);
                if (result != 0)
                    return result; // difference in rune values
            }
#else
            return string.Compare(ToString(), other.ToString(), StringComparison.Ordinal);
#endif
        }

        public nuint Length
        {
            [DebuggerStepThrough]
            [System.Diagnostics.Contracts.Pure]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetByteStringLength(Pointer, (uint)int.MaxValue);
        }

        [SuppressMessage("Usage", "CA2225", Justification = "Annoying")]
        [DebuggerStepThrough]
        [System.Diagnostics.Contracts.Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<byte>(Utf8String u)
            => new(u.Pointer, (int)u.Length);

        [SuppressMessage("Usage", "CA2225", Justification = "Annoying")]
        [DebuggerStepThrough]
        [System.Diagnostics.Contracts.Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<sbyte>(Utf8String u)
            => new(u.Pointer, (int)u.Length);

        [DebuggerStepThrough]
        [System.Diagnostics.Contracts.Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => (int)Crc32C.Calculate(this);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string? ToString()
        {
            if (Pointer == default)
                return null;

            return InternedUtf8Strings.TryGetValue(this, out var s)
                ? s
                : ToNewString();
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ToNewString()
            => new(Pointer, 0, (int)Length, Encoding.UTF8);

        public Utf8String SubstringCopy(nint offset, uint length)
        {
            var dest = NativeMemory.New<sbyte>(length + 1);
            Unsafe.CopyBlockUnaligned(dest, Pointer + offset, length);
            dest![length] = 0;
            return new(dest);
        }

        public Utf8String SubstringCopy(nint offset, nuint length)
        {
            var dest = NativeMemory.New<sbyte>(length + 1);
            if (length > uint.MaxValue)
            {
                nuint index = 0;
                do
                {
                    Unsafe.CopyBlockUnaligned(dest + index, Pointer + offset + index, uint.MaxValue);
                    index += uint.MaxValue;
                } while (length - index > uint.MaxValue);
                var remaining = (uint)(length - index);
                Unsafe.CopyBlockUnaligned(dest + index, Pointer + offset + index, remaining);
            }
            dest![length] = 0;
            return new(dest);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizedUtf8String Substring(nint offset, uint length)
            => new(length, Pointer + offset);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizedUtf8String Substring(nint offset, nuint length)
            => new(length, Pointer + offset);

        [DebuggerStepThrough]
        [System.Diagnostics.Contracts.Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string?(Utf8String u)
            => u.ToString();

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Utf8String(string s)
            => Create(s);

        [System.Diagnostics.Contracts.Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj)
            => obj is Utf8String u && Equals(u);

        [System.Diagnostics.Contracts.Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Utf8String left, Utf8String right)
            => left.Equals(right);

        [System.Diagnostics.Contracts.Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Utf8String left, Utf8String right)
            => !(left == right);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetByteStringLength(sbyte* p, int max)
        {
            // TODO: alignment-lead-up to large aligned reads, no unaligned page faults
            for (var i = 0; i < max; ++i)
            {
                if (p[i] == 0)
                    return i;
            }
            return max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nuint GetByteStringLength(sbyte* p, nuint max)
        {
            // TODO: alignment-lead-up to large aligned reads, no unaligned page faults
            for (nuint i = 0; i < max; ++i)
            {
                if (p[i] == 0)
                    return i;
            }
            return max;
        }

        [DebuggerStepThrough]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Utf8String ToUtf8String()
            => this;
    }
}
