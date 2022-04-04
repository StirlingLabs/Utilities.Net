using System.Threading;

namespace StirlingLabs.Utilities.Tests;

public class TestSyncCtx<T> : SynchronizationContext
{
    public T? Value;

    public TestSyncCtx() { }

    public TestSyncCtx(T? value)
        => Value = value;
}