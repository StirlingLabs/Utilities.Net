#nullable enable
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities.Collections
{
    [PublicAPI]
    public sealed class AsyncQueue<T>
        : IProducerConsumerCollection<T>, IAsyncEnumerable<T>, IReadOnlyCollection<T>, IDisposable, INotifyCompletion
    {
        private readonly SemaphoreSlim _semaphore = new(0);
        private readonly ConcurrentQueue<T> _queue = new();

        private readonly CancellationTokenSource _complete = new();
        private readonly CancellationTokenSource _addingComplete = new();

        private int _isCompleted;

        private Action? _completedDispatch;

        private int _isDisposed;

        public AsyncQueue()
            => _complete.Token.Register(Completed);

        public AsyncQueue(IEnumerable<T> items) : this()
            => EnqueueRange(items);


        public bool IsAddingCompleted
            => _addingComplete.IsCancellationRequested;

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Interlocked.CompareExchange(ref _isCompleted, 0, 0) != 0;
        }

        public bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Interlocked.CompareExchange(ref _isDisposed, 0, 0) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SetIsCompleted()
            => Interlocked.Exchange(ref _isCompleted, 1) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("AsyncQueue");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SetIsDisposed()
            => Interlocked.Exchange(ref _isDisposed, 1) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T item)
        {
            if (IsAddingCompleted)
                throw new InvalidOperationException("The AsyncQueue has already completed adding.");

            _queue.Enqueue(item);
            _semaphore.Release();
        }

        public void EnqueueRange(IEnumerable<T> source)
        {
            if (IsAddingCompleted)
                throw new InvalidOperationException("The AsyncQueue has already completed adding.");

            var count = 0;
            foreach (var item in source)
            {
                _queue.Enqueue(item);
                count++;
            }
            _semaphore.Release(count);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
            => DequeueAsync(true, cancellationToken);

        public async ValueTask<T> DequeueAsync(bool continueOnCapturedContext, CancellationToken cancellationToken = default)
        {

            for (;;)
            {
                await WaitForAvailableAsync(continueOnCapturedContext, cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext);

                if (_queue.TryDequeue(out var item))
                {
                    TryToComplete();
                    return item;
                }

                if (TryToComplete())
                    throw new OperationCanceledException("The collection has completed.");

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask WaitForAvailableAsync(CancellationToken cancellationToken = default)
            => WaitForAvailableAsync(true, cancellationToken);

        public async ValueTask WaitForAvailableAsync(bool continueOnCapturedContext, CancellationToken cancellationToken)
        {
            CheckDisposed();

            if (IsCompleted)
                throw new InvalidOperationException("The AsyncQueue has already fully completed.");

            if (!IsEmpty) return;

            if (IsAddingCompleted)
                throw new OperationCanceledException("The AsyncQueue completed adding, therefore there will not be any more available items.");

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        private bool TryToComplete()
        {
            lock (_queue)
            {
                if (!_queue.IsEmpty || !_addingComplete.IsCancellationRequested)
                    return false;

                _complete.Cancel();
                return true;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            CheckDisposed();

            foreach (var item in _queue)
                yield return item;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void CopyTo(Array array, int index)
        {
            CheckDisposed();
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (index < 0 || index > array.Length) throw new ArgumentOutOfRangeException(nameof(index));

            var i = 0;
            foreach (var item in _queue)
                array.SetValue(item, index + i++);
        }

        public int Count
        {
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                CheckDisposed();

                return _queue.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            [DebuggerStepThrough]
            get {
                CheckDisposed();

                return ((ICollection)_queue).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            [DebuggerStepThrough]
            get {
                CheckDisposed();

                return ((ICollection)_queue).SyncRoot;
            }
        }

        public bool IsEmpty
        {
            [DebuggerStepThrough]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                CheckDisposed();

                return _queue.IsEmpty;
            }
        }

        public void CopyTo(T[] array, int index)
        {
            CheckDisposed();

            var i = 0;
            foreach (var item in _queue)
                array[index + i++] = item;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToArray()
        {
            CheckDisposed();

            return _queue.ToArray();
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IProducerConsumerCollection<T>.TryTake(out T item)
            => TryDequeue(out item!);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T? item)
        {
            CheckDisposed();

            var success = _queue.TryDequeue(out item);
            TryToComplete();
            return success;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            do
            {
                T item;
                try
                {
                    item = await DequeueAsync(cancellationToken)
                        .ConfigureAwait(true);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }

                yield return item;
            } while (!cancellationToken.IsCancellationRequested && !IsCompleted);

            TryToComplete();

            CheckDisposed();
        }

        public void CompleteAdding()
        {
            _addingComplete.Cancel();

            TryToComplete();
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            if (!SetIsDisposed()) Debug.Fail("Was already disposed.");

            _addingComplete.Cancel();
            _complete.Cancel();

            _queue.Clear();

            _addingComplete.Dispose();
            _complete.Dispose();
            _semaphore.Dispose();
        }

        public void OnCompleted(Action continuation)
        {
            lock (_complete)
                _completedDispatch = (Action)Delegate.Combine(_completedDispatch, continuation);
        }

        private void Completed()
        {
            Debug.Assert(!IsCompleted);
            lock (_complete)
            {
                _completedDispatch?.Invoke();
                _completedDispatch = null;
                if (!SetIsCompleted()) Debug.Fail("Was already completed.");
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Microsoft.Design", "CA1024", Justification = "Awaitable implementation")]
        public AsyncQueue<T> GetAwaiter()
            => this;
    }
}
