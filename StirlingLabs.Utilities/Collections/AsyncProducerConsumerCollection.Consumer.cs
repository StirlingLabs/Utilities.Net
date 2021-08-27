using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace StirlingLabs.Utilities.Collections
{
    public sealed partial class AsyncProducerConsumerCollection<T>
    {
        [SuppressMessage("Microsoft.Design", "CA1034", Justification = "Nested class has private member access")]
        public readonly struct Consumer : IAsyncConsumer<T>, IEquatable<Consumer>
        {
            private readonly AsyncProducerConsumerCollection<T> _producerConsumerCollection;


            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Consumer(AsyncProducerConsumerCollection<T> producerConsumerCollection)
                => _producerConsumerCollection = producerConsumerCollection;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Equals(Consumer other)
                => _producerConsumerCollection.Equals(other._producerConsumerCollection);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Equals(object? obj)
                => obj is Consumer other && Equals(other);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override int GetHashCode()
                => _producerConsumerCollection.GetHashCode();

            public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                _producerConsumerCollection.CheckDisposed();

                do
                {
                    T item;
                    try
                    {
                        item = await _producerConsumerCollection.TakeAsync(cancellationToken)
                            .ConfigureAwait(true);
                    }
                    catch (OperationCanceledException)
                    {
                        yield break;
                    }

                    yield return item;
                } while (!cancellationToken.IsCancellationRequested && !_producerConsumerCollection.IsCompleted);

                _producerConsumerCollection.TryToComplete();

                _producerConsumerCollection.CheckDisposed();
            }

            public IEnumerator<T> GetEnumerator()
            {
                _producerConsumerCollection.CheckDisposed();

                do
                {
                    T item;
                    try
                    {
                        if (!_producerConsumerCollection.TryTake(out item!))
                        {
                            break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        yield break;
                    }

                    yield return item;
                } while (!_producerConsumerCollection.IsCompleted);

                _producerConsumerCollection.TryToComplete();

                _producerConsumerCollection.CheckDisposed();
            }

            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(Consumer left, Consumer right)
                => left.Equals(right);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(Consumer left, Consumer right)
                => !left.Equals(right);

            public bool IsEmpty
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _producerConsumerCollection.IsEmpty;
            }

            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _producerConsumerCollection.IsCompleted;
            }
        }
    }
}
