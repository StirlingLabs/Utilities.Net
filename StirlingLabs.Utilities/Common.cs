using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

#nullable enable
namespace StirlingLabs.Utilities
{
    [PublicAPI]
    public class Common
    {
        public static T OnDemand<T>(ref WeakReference<T>? cache, Func<T> factory)
            where T : class
        {
            T? d;
            if (cache is null)
                cache = new WeakReference<T>(d = factory());
            else if (!cache.TryGetTarget(out d))
                cache.SetTarget(d = factory());
            return d;
        }
    }
}
