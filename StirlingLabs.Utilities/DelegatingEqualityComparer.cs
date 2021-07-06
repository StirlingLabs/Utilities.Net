using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace StirlingLabs.Utilities
{
    public sealed class DelegatingEqualityComparer<T> : EqualityComparer<T>
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
