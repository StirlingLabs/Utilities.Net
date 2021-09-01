#if NETSTANDARD2_0
using System.Collections.Generic;

namespace System
{
    public static class KeyValuePairExtensions
    {
        public static void Deconstruct<TKey, TValue>(in this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
#endif
