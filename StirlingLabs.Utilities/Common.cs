using System;
using System.Collections.Generic;
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
                cache = new(d = factory());
            else if (!cache.TryGetTarget(out d))
                cache.SetTarget(d = factory());
            return d;
        }

        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals, Func<T, int> hasher)
            => new DelegatingEqualityComparer<T>(equals, hasher);
        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T?, T?, bool> equals)
            => new DelegatingEqualityComparer<T>(equals);
        public static EqualityComparer<T> CreateEqualityComparer<T>(Func<T, int> hasher)
            => new DelegatingEqualityComparer<T>(hasher);
    }

    public class DelegatingEqualityComparer<T> : EqualityComparer<T>
    {
        private readonly Func<T?, T?, bool> _equals;
        private readonly Func<T, int> _hasher;

        public DelegatingEqualityComparer(Func<T?, T?, bool> equals, Func<T, int> hasher)
        {
            _equals = equals ?? throw new ArgumentNullException(nameof(equals));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }
        public DelegatingEqualityComparer(Func<T?, T?, bool> equals)
        {
            _equals = equals ?? throw new ArgumentNullException(nameof(equals));
            _hasher = Default.GetHashCode;
        }
        public DelegatingEqualityComparer(Func<T, int> hasher)
        {
            _equals = Default.Equals;
            _hasher = obj => RuntimeHelpers.GetHashCode(obj);
        }

        public override bool Equals(T? x, T? y)
            => _equals(x, y);

        public override int GetHashCode(T obj)
            => _hasher(obj);
    }
}
