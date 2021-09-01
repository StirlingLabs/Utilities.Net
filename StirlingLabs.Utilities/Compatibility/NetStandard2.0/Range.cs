#if NETSTANDARD2_0
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace System
{
    [PublicAPI]
    public readonly struct Range : IEquatable<Range>
    {
        public Index Start { get; }

        public Index End { get; }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Range(Index start, Index end)
        {
            Start = start;
            End = end;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? value)
            => value is Range r &&
                r.Start.Equals(Start) &&
                r.End.Equals(End);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Range other)
            => other.Start.Equals(Start) && other.End.Equals(End);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
            => Start.GetHashCode() * 31 + End.GetHashCode();

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"{Start}..{End}";

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Range StartAt(Index start)
            => new(start, Index.End);

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Range EndAt(Index end)
            => new(Index.Start, end);

        public static Range All
        {
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Index.Start, Index.End);
        }

        [DebuggerStepThrough]
        public (int Offset, int Length) GetOffsetAndLength(int length)
        {
            int start;
            var startIndex = Start;
            if (startIndex.IsFromEnd)
                start = length - startIndex.Value;
            else
                start = startIndex.Value;

            int end;
            var endIndex = End;
            if (endIndex.IsFromEnd)
                end = length - endIndex.Value;
            else
                end = endIndex.Value;

            if ((uint)end > (uint)length || (uint)start > (uint)end)
                throw new ArgumentOutOfRangeException(nameof(length));

            return (start, end - start);
        }
    }
}
#endif
