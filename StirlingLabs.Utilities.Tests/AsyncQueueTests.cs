using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using NUnit.Framework;
using StirlingLabs.Utilities.Collections;

namespace StirlingLabs.Utilities.Tests
{
    public class AsyncQueueTests
    {
        [Test]
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task GeneralOperations()
        {
            var objects = new object[8000];
            
            for (var o = 0; o < 1000; ++o) objects[o] = new();

            using var q = new AsyncQueue<object>(objects);

            q.IsEmpty.Should().BeFalse();
            var initCount = objects.Length;
            q.Count.Should().Be(initCount);

            using var mre1 = new ManualResetEventSlim();

            var qt1 = Common.RunThread(() => {
                mre1.Wait();
                mre1.Dispose();
                q.EnqueueRange(objects);
            });

            using var mre2 = new ManualResetEventSlim();

            var qt2 = Common.RunThread(() => {
                mre2.Wait();
                mre2.Dispose();
                q.EnqueueRange(objects);
                q.CompleteAdding();
            });

            var i = 0;
            await foreach (var item in q)
            {
                item.Should().Be(objects[i++]);
                q.IsAddingCompleted.Should().BeFalse();
                q.IsCompleted.Should().BeFalse();
                if (i >= initCount) break;
                q.IsEmpty.Should().BeFalse();
                //q.Count.Should().Be(initCount - i);
            }
            i.Should().Be(initCount);
            q.IsEmpty.Should().BeTrue();
            q.Count.Should().Be(0);

            mre1.Set();
            i = 0;
            
            await foreach (var item in q)
            {
                item.Should().Be(objects[i++]);
                q.IsAddingCompleted.Should().BeFalse();
                q.IsCompleted.Should().BeFalse();
                if (i >= initCount) break;
                q.IsEmpty.Should().BeFalse();
                //q.Count.Should().Be(initCount - i);
            }
            i.Should().Be(initCount);
            q.IsEmpty.Should().BeTrue();
            q.Count.Should().Be(0);

            mre2.Set();
            i = 0;
            await foreach (var item in q)
            {
                item.Should().Be(objects[i++]);
                if (i >= initCount) break;
                q.IsCompleted.Should().BeFalse();
                q.IsEmpty.Should().BeFalse();
                //q.Count.Should().Be(initCount - i);
            }

            q.IsAddingCompleted.Should().BeTrue();
            q.IsCompleted.Should().BeTrue();

            i.Should().Be(initCount);
            
            q.IsEmpty.Should().BeTrue();
            q.Count.Should().Be(0);

            qt1.Join();
            qt2.Join();
        }
    }
}
