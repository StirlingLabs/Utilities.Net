using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace StirlingLabs.Utilities.Tests;

public partial class JsonMe : IEquatable<JsonMe>
{
    public static readonly DelegatingEqualityComparer<KeyValuePair<string, object>> ArbitraryDictComparer
        = new((x, y) => {
            if (x.Key != y.Key)
                return false;
            if (x.Value != y.Value)
            {
                if (x.Value is not JObject ja || y.Value is not JObject jo)
                    return false;
                if (!JToken.DeepEquals(ja, jo))
                    return false;
            }
            return true;
        });
    // ReSharper disable once CognitiveComplexity
    public bool Equals(JsonMe? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (arbitrary != other.arbitrary)
        {
            if (arbitrary is not JObject ja || other.arbitrary is not JObject jo)
                return false;
            if (!JToken.DeepEquals(ja, jo))
                return false;
        }
        if (text != other.text)
            return false;
        if (!number.Equals(other.number))
            return false;
        if (!texts.SequenceEqual(other.texts))
            return false;
        if (!numbers.SequenceEqual(other.numbers))
            return false;
        if (stringDict.Count != other.stringDict.Count)
            return false;
        if (stringDict.Except(other.stringDict).Any())
            return false;
        if (numberDict.Count != other.numberDict.Count)
            return false;
        if (numberDict.Except(other.numberDict).Any())
            return false;
        if (numberDict.Count != other.numberDict.Count)
            return false;
        if (arbitraryDict.Except(other.arbitraryDict, ArbitraryDictComparer).Any())
            return false;
        return true;
    }
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((JsonMe)obj);
    }
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public override unsafe int GetHashCode()
    {
        Span<int> ints = stackalloc int[]
        {
            (arbitrary is not null).GetHashCode(),
#if NETSTANDARD2_0
            text.GetHashCode(),
#else
            text.GetHashCode(StringComparison.Ordinal),
#endif
            number.GetHashCode(),
            numbers.GetHashCode(),
            stringDict.GetHashCode(),
            numberDict.GetHashCode(),
            arbitraryDict.Select(kv => (kv.Key, kv.Value is not null)).GetHashCode()
        };
        var bytes = MemoryMarshal.AsBytes(ints);
        return unchecked((int)Crc32C.Calculate(bytes));
    }

    public static bool operator ==(JsonMe left, JsonMe right)
        => Equals(left, right);
    public static bool operator !=(JsonMe left, JsonMe right)
        => !Equals(left, right);
}
