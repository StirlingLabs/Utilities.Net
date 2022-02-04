using System;
using System.Runtime.CompilerServices;

namespace StirlingLabs.Utilities;

public class MemoryReader<T> : IReader<T> where T : unmanaged
{
    public Memory<T> Memory;

    public MemoryReader(Memory<T> memory)
        => Memory = memory;
    public bool TryRead(out T value)
    {
        if (Memory.Length <= 0)
        {
            Unsafe.SkipInit(out value);
            return false;
        }
        value = Memory.Span[0];
        Memory = Memory.Slice(1);
        return true;
    }
}
