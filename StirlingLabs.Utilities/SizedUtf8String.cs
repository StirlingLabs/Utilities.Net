using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct SizedUtf8String
    : IEquatable<SizedUtf8String>,
        IComparable<SizedUtf8String>,
        IEquatable<Utf8String>,
        IComparable<Utf8String>
{
    private readonly Utf8String _string;
    public readonly nuint Length;

    public sbyte* Pointer
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _string.Pointer;
    }

    public SizedUtf8String(nuint length, Utf8String @string)
    {
        Length = length;
        _string = @string;
    }

    public SizedUtf8String(nuint length, sbyte* @string)
        : this(length, new Utf8String(@string)) { }

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
        [SuppressMessage("Design", "CA1065", Justification = "Indexers can throw IndexOutOfRangeException")]
        get {
            if (offset >= Length) throw new IndexOutOfRangeException();
            return ref Unsafe.AsRef<sbyte>(Pointer + offset);
        }
    }

    [SuppressMessage("Usage", "CA2225", Justification = "Annoying")]
    [DebuggerStepThrough]
    [System.Diagnostics.Contracts.Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<byte>(SizedUtf8String u)
        => new(u.Pointer, (int)u.Length);

    [SuppressMessage("Usage", "CA2225", Justification = "Annoying")]
    [DebuggerStepThrough]
    [System.Diagnostics.Contracts.Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<sbyte>(SizedUtf8String u)
        => new(u.Pointer, (int)u.Length);


    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public bool Equals(Utf8String other)
    {
        if (Pointer == other.Pointer)
            return true;

        return CompareTo(other) == 0;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public int CompareTo(Utf8String other)
        => CompareTo((ReadOnlySpan<byte>)other);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [System.Diagnostics.Contracts.Pure]
    public bool Equals(SizedUtf8String other)
    {
        if (Pointer == other.Pointer)
            return Length == other.Length;

        return CompareTo(other) == 0;
    }

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

    public bool Free()
        => _string.Free();

    public static SizedUtf8String Create(string str)
        => new((nuint)Encoding.UTF8.GetByteCount(str), Utf8String.Create(str));

    public static SizedUtf8String Create(ReadOnlySpan<sbyte> data)
        => new((nuint)data.Length, Utf8String.Create(data));

    public static SizedUtf8String Create(nuint size, [InstantHandle] SpanAction<sbyte> factory)
    {
        if (size is 0) return default;
        if (factory is null) throw new ArgumentNullException(nameof(factory));
        return new(size, Utf8String.Create(size, factory));
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string? ToString()
        => Pointer == default
            ? null
            : _string.IsInterned
                // ReSharper disable once RedundantOverflowCheckingContext
                // ReSharper disable once ReplaceSubstringWithRangeIndexer
                ? _string.ToString()?.Substring(0, checked((int)Length))
                : ToNewString();

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ToNewString()
        // ReSharper disable once RedundantOverflowCheckingContext
        => new(Pointer, 0, checked((int)Length), Encoding.UTF8);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SizedUtf8String Substring(nint offset, uint length)
        => new(length, Pointer + offset);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SizedUtf8String Substring(nint offset, nuint length)
        => new(length, Pointer + offset);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SizedUtf8String(string s)
        => Create(s);

    [DebuggerStepThrough]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SizedUtf8String ToSizedUtf8String(string s)
        => Create(s);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
        => obj is SizedUtf8String s && Equals(s);

    [DebuggerStepThrough]
    [System.Diagnostics.Contracts.Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
        => (int)Crc32C.Calculate(this);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(SizedUtf8String left, SizedUtf8String right)
        => left.Equals(right);

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(SizedUtf8String left, SizedUtf8String right)
        => !(left == right);
}
