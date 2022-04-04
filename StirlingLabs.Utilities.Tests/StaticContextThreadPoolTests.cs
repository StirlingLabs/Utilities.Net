using System;
using System.Threading;
using FluentAssertions;
using NUnit;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.All)]
public class InstancedContextThreadPoolTests
{
    [Test]
    [Repeat(6000)]
    public void ThreadPoolHelpersQueueWorkItemFastTest1()
    {
        var manipValue = false;
        using var mre = new ManualResetEventSlim();
        ThreadPoolHelpers.QueueUserWorkItemFast(() => {
            manipValue = true;
            // ReSharper disable once AccessToDisposedClosure
            mre.Set();
        });
        mre.Wait(125).Should().BeTrue();
        manipValue.Should().BeTrue();
    }
    [Test]
    [Repeat(6000)]
    public unsafe void ThreadPoolHelpersQueueWorkItemFastTest2()
    {
        using var mre = new ManualResetEventSlim();

        static void Action(ManualResetEventSlim mre)
            => mre.Set();

        ThreadPoolHelpers.QueueUserWorkItemFast(&Action, mre);
        mre.Wait(125).Should().BeTrue();
    }
}

[Parallelizable(ParallelScope.None)]
public class StaticContextThreadPoolTests
{
    [Test]
    [Repeat(1000)]
    public void ThreadPoolHelpersQueueWorkItemFastTest1()
    {
        lock (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Lock)
            ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value = new();

        static void Action()
        {
            for (var i = 0; i < 125; ++i)
            {
                lock (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Lock)
                    if (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value is not null)
                        break;
                Thread.Sleep(1);
            }

            lock (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Lock)
                ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value!.Set();
        }

        ThreadPoolHelpers.QueueUserWorkItemFast(Action);

        ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value.Wait(125).Should().BeTrue();
        ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value.IsSet.Should().BeTrue();
    }
    [Test]
    [Repeat(1000)]
    public unsafe void ThreadPoolHelpersQueueWorkItemFastTest2()
    {
        lock (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Lock)
            ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value = new();

        static void Action()
        {
            for (var i = 0; i < 125; ++i)
            {
                lock (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Lock)
                    if (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value is not null)
                        break;
                Thread.Sleep(1);
            }

            lock (ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Lock)
                ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value!.Set();
        }

        ThreadPoolHelpers.QueueUserWorkItemFast(&Action);

        ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value.Wait(125).Should().BeTrue();
        ScopedSingleton<ManualResetEventSlim, InstancedContextThreadPoolTests>.Value.IsSet.Should().BeTrue();
    }
}
