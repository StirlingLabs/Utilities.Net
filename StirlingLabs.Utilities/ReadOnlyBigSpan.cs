using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#pragma warning disable 0809 //warning CS0809: Obsolete member 'Span<T>.Equals(object)' overrides non-obsolete member 'object.Equals(object)'

#nullable enable
namespace StirlingLabs.Utilities
{
    /// <summary>
    /// ReadOnlySpan represents a contiguous region of arbitrary memory. Unlike arrays, it can point to either managed
    /// or native memory, or to memory allocated on the stack. It is type- and memory-safe.
    /// </summary>
    [NonVersionable]
    [DebuggerDisplay("{ToString(),raw}")]
    public readonly ref struct ReadOnlyBigSpan<T> where T : unmanaged
    {
        /// <summary>A byref or a native ptr.</summary>
        internal readonly ByReference<T> _pointer;
        /// <summary>The number of elements this ReadOnlySpan contains.</summary>
        internal readonly nuint _length;

        /// <summary>
        /// Creates a new read-only span over the entirety of the target array.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyBigSpan(T[]? array)
        {
            if (array == null)
            {
                this = default;
                return; // returns default
            }

            _pointer = new(ref MemoryMarshal.GetArrayDataReference(array));
            _length = BigSpanHelpers.Is64Bit ? (nuint)array.LongLength : (nuint)array.Length;
        }

        /// <summary>
        /// Creates a new read-only span over the portion of the target array beginning
        /// at 'start' index and ending at 'end' index (exclusive).
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="start">The index at which to begin the read-only span.</param>
        /// <param name="length">The number of items in the read-only span.</param>
        /// <remarks>Returns default when <paramref name="array"/> is null.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in the range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyBigSpan(T[]? array, nuint start, nuint length)
        {
            if (array == null)
            {
                if (start != 0)
                    throw new ArgumentOutOfRangeException(nameof(start));
                if (length != 0)
                    throw new ArgumentOutOfRangeException(nameof(length));
                this = default;
                return; // returns default
            }
            // See comment in Span<T>.Slice for how this works.
            if (start + length > (nuint)array.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            _pointer = new(ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array),
                (nint)(uint)start /* force zero-extension */));
            _length = length;
        }

        /// <summary>
        /// Creates a new read-only span over the target unmanaged buffer.  Clearly this
        /// is quite dangerous, because we are creating arbitrarily typed T's
        /// out of a void*-typed block of memory.  And the length is not checked.
        /// But if this creation is correct, then all subsequent uses are correct.
        /// </summary>
        /// <param name="pointer">An unmanaged pointer to memory.</param>
        /// <param name="length">The number of <typeparamref name="T"/> elements the memory contains.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <typeparamref name="T"/> is reference type or contains pointers and hence cannot be stored in unmanaged memory.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="length"/> is negative.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlyBigSpan(void* pointer, nuint length)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                throw new NotSupportedException("Invalid type with pointers.");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            _pointer = new(ref Unsafe.As<byte, T>(ref *(byte*)pointer));
            _length = length;
        }

        // Constructor for internal use only.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyBigSpan(ref T ptr, nuint length)
        {
            Debug.Assert(length >= 0);

            _pointer = new(ref ptr);
            _length = length;
        }

        // Constructor for internal use only.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlyBigSpan(in T ptr, nuint length, bool _ = false)
        {
            Debug.Assert(length >= 0);

            _pointer = new(ref Unsafe.AsRef(ptr));
            _length = length;
        }

        /// <summary>
        /// Returns the specified element of the read-only span.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when index less than 0 or index greater than or equal to Length
        /// </exception>
        public ref readonly T this[nuint index]
        {
            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [NonVersionable]
            get {
                if ((uint)index >= (uint)_length)
                    throw new IndexOutOfRangeException();
                return ref Unsafe.Add(ref _pointer.Value, (nint)index);
            }
        }


        /// <summary>
        /// Returns the specified element of the read-only span.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when index less than 0 or index greater than or equal to Length
        /// </exception>
        public ref readonly T this[Index index]
        {
            get {
                var actualIndex = (nuint)index.Value;
                if (index.IsFromEnd) actualIndex = _length - actualIndex;
                return ref this[actualIndex];
            }
        }

        public ReadOnlyBigSpan<T> this[Range range]
        {
            get {
                var start = (nuint)range.Start.Value;
                if (range.Start.IsFromEnd) start = _length - start;
                var end = (nuint)range.End.Value;
                if (range.Start.IsFromEnd) end = _length - end;
                var length = end - start;
                return new(ref Unsafe.AsRef(this[start]), length);
            }
        }

        /// <summary>
        /// The number of items in the read-only span.
        /// </summary>
        public nuint Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [NonVersionable]
            get => _length;
        }

        /// <summary>
        /// Returns true if Length is 0.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [NonVersionable]
            get => 0 >= (uint)_length; // Workaround for https://github.com/dotnet/runtime/issues/10950
        }

        /// <summary>
        /// Returns false if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ReadOnlyBigSpan<T> left, ReadOnlyBigSpan<T> right) => !(left == right);

        /// <summary>
        /// This method is not supported as spans cannot be boxed. To compare two spans, use operator==.
        /// <exception cref="System.NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        /// </summary>
        [Obsolete("Equals() on ReadOnlySpan will always throw an exception. Use == instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj) =>
            throw new NotSupportedException();

        /// <summary>
        /// This method is not supported as spans cannot be boxed.
        /// <exception cref="System.NotSupportedException">
        /// Always thrown by this method.
        /// </exception>
        /// </summary>
        [Obsolete("GetHashCode() on ReadOnlySpan will always throw an exception.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() =>
            throw new NotSupportedException();

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="ReadOnlyBigSpan{T}"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyBigSpan<T>(T[]? array) => new(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="ArraySegment{T}"/> to a <see cref="ReadOnlyBigSpan{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyBigSpan<T>(ArraySegment<T> segment)
            => new(segment.Array, (nuint)segment.Offset, (nuint)segment.Count);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="BigSpan{T}"/> to a <see cref="ReadOnlySpan{T}"/>
        /// </summary>
        public static explicit operator ReadOnlySpan<T>(ReadOnlyBigSpan<T> bigSpan) =>
            bigSpan._length <= int.MaxValue
                ? MemoryMarshal.CreateReadOnlySpan(ref bigSpan._pointer.Value, (int)bigSpan._length)
                : throw new NotSupportedException(
                    $"Not possible to create ReadOnlySpans longer than {int.MaxValue} (maximum 32-bit integer value)");

        /// <summary>
        /// Returns a 0-length read-only span whose base is the null pointer.
        /// </summary>
        public static ReadOnlyBigSpan<T> Empty => default;

        /// <summary>Gets an enumerator for this span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new(this);

        /// <summary>Enumerates the elements of a <see cref="ReadOnlyBigSpan{T}"/>.</summary>
        public ref struct Enumerator
        {
            /// <summary>The span being enumerated.</summary>
            private readonly ReadOnlyBigSpan<T> _bigSpan;
            /// <summary>The next index to yield.</summary>
            private nuint _index;

            /// <summary>Initialize the enumerator.</summary>
            /// <param name="bigSpan">The span to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnlyBigSpan<T> bigSpan)
            {
                _bigSpan = bigSpan;
                _index = nuint.MaxValue;
            }

            /// <summary>Advances the enumerator to the next element of the span.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                var index = _index + 1;
                if (index >= _bigSpan.Length)
                    return false;

                _index = index;
                return true;

            }

            /// <summary>Gets the element at the current position of the enumerator.</summary>
            public ref readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref _bigSpan[_index];
            }
        }

        /// <summary>
        /// Returns a reference to the 0th element of the Span. If the Span is empty, returns null reference.
        /// It can be used for pinning and is required to support the use of span within a fixed statement.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ref readonly T GetPinnableReference()
        {
            // Ensure that the native code has just one forward branch that is predicted-not-taken.
            ref T ret = ref Unsafe.NullRef<T>();
            if (_length != 0) ret = ref _pointer.Value;
            return ref ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public unsafe void* GetUnsafePointer() => Unsafe.AsPointer(ref Unsafe.AsRef(GetPinnableReference()));

        /// <summary>
        /// Copies the contents of this read-only span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        ///
        /// <param name="destination">The span to copy items into.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the destination Span is shorter than the source Span.
        /// </exception>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void CopyTo(BigSpan<T> destination)
        {
            // Using "if (!TryCopyTo(...))" results in two branches: one for the length
            // check, and one for the result of TryCopyTo. Since these checks are equivalent,
            // we can optimize by performing the check once ourselves then calling Memmove directly.

            if ((uint)_length > (uint)destination.Length)
            {
                throw new ArgumentException("Too short.", nameof(destination));
            }
            BigSpanHelpers.Copy(destination.GetUnsafePointer(), GetUnsafePointer(), _length * (nuint)sizeof(T));
        }

        /// <summary>
        /// Copies the contents of this read-only span into destination span. If the source
        /// and destinations overlap, this method behaves as if the original values in
        /// a temporary location before the destination is overwritten.
        /// </summary>
        /// <returns>If the destination span is shorter than the source span, this method
        /// return false and no data is written to the destination.</returns>
        /// <param name="destination">The span to copy items into.</param>
        public unsafe bool TryCopyTo(BigSpan<T> destination)
        {
            bool retVal = false;
            if ((uint)_length <= (uint)destination.Length)
            {
                BigSpanHelpers.Copy(destination.GetUnsafePointer(), GetUnsafePointer(), _length * (nuint)sizeof(T));
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Returns true if left and right point at the same memory and have the same length.  Note that
        /// this does *not* check to see if the *contents* are equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool operator ==(ReadOnlyBigSpan<T> left, ReadOnlyBigSpan<T> right) =>
            left._length == right._length &&
            Unsafe.AreSame(ref Unsafe.AsRef<T>(left.GetUnsafePointer()), ref Unsafe.AsRef<T>(right.GetUnsafePointer()));

        /// <summary>
        /// For <see cref="ReadOnlyBigSpan{T}"/>, returns a new instance of string that represents the characters pointed to by the span.
        /// Otherwise, returns a <see cref="string"/> with the name of the type and the number of elements.
        /// </summary>
        public override string ToString()
        {
            if (typeof(T) == typeof(char) && _length <= int.MaxValue)
            {
                return new(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, char>(ref _pointer.Value), (int)_length));
            }
            return $"System.ReadOnlySpan<{typeof(T).Name}>[{_length}]";
        }

        /// <summary>
        /// Forms a slice out of the given read-only span, beginning at 'start'.
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyBigSpan<T> Slice(nuint start)
        {
            if ((uint)start > (uint)_length)
                throw new ArgumentOutOfRangeException(nameof(start));

            return new(ref Unsafe.Add(ref _pointer.Value, (nint)(uint)start /* force zero-extension */), _length - start);
        }

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        public ReadOnlySpan<T> Slice(nuint start, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (start + (nuint)length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(this[start]), length);
        }

        /// <summary>
        /// Forms a slice out of the given span, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        public ReadOnlySpan<T> Slice(int start, int length)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if ((nuint)start + (nuint)length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(this[(nuint)start]), length);
        }

        /// <summary>
        /// Forms a slice out of the given read-only span, beginning at 'start', of given length
        /// </summary>
        /// <param name="start">The index at which to begin this slice.</param>
        /// <param name="length">The desired length for the slice (exclusive).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyBigSpan<T> Slice(nuint start, nuint length)
        {
            // See comment in Span<T>.Slice for how this works.
            if (start + length > _length)
                throw new ArgumentOutOfRangeException(nameof(length));

            return new(ref Unsafe.Add(ref _pointer.Value, (nint)(uint)start /* force zero-extension */), length);
        }

        /// <summary>
        /// Copies the contents of this read-only span into a new array.  This heap
        /// allocates, so should generally be avoided, however it is sometimes
        /// necessary to bridge the gap with APIs written in terms of arrays.
        /// </summary>
        public T[] ToArray()
        {
            if (_length == 0)
                return Array.Empty<T>();

            T[] destination;
            if (BigSpanHelpers.Is64Bit)
            {
                if (_length <= long.MaxValue)
                    destination = new T[(long)_length];
                else
                    throw new NotSupportedException(
                        $"Arrays larger than {long.MaxValue} (maximum signed 64-bit integer) are not possible at this time.");
            }
            else
            {
                if (_length <= int.MaxValue)
                    destination = new T[(int)_length];
                else
                {
                    if (_length <= long.MaxValue)
                        destination = new T[(long)_length];
                    else
                        throw new NotSupportedException(
                            $"Arrays larger than {long.MaxValue} (maximum signed 64-bit integer) are not possible at this time.");
                }
            }
            CopyTo(destination);
            return destination;
        }

        public unsafe ReadOnlyBigSpan<byte> AsBytes()
            => new(ref Unsafe.As<T, byte>(ref _pointer.Value), _length * (nuint)sizeof(T));

        public unsafe int CompareMemory(BigSpan<T> other)
        {
            var lengthComparison = _length.CompareTo(other._length);
            return lengthComparison == 0
                ? UnmanagedMemory.C_CompareMemory(GetUnsafePointer(), other.GetUnsafePointer(), _length * (nuint)sizeof(T))
                : lengthComparison;
        }

        public unsafe int CompareMemory(ReadOnlyBigSpan<T> other)
        {
            var lengthComparison = _length.CompareTo(other._length);
            return lengthComparison == 0
                ? UnmanagedMemory.C_CompareMemory(GetUnsafePointer(), other.GetUnsafePointer(), _length * (nuint)sizeof(T))
                : lengthComparison;
        }
    }
}