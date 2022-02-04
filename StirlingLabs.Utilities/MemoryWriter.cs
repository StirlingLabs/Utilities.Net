using System;

namespace StirlingLabs.Utilities;

public class MemoryWriter<T> : IWriter<T> where T : unmanaged
{
    public Memory<T> Memory;

    public MemoryWriter(Memory<T> memory)
        => Memory = memory;

    public bool TryWrite(T value)
    {
        if (Memory.Length <= 0)
            return false;
        Memory.Span[0] = value;
        Memory = Memory.Slice(1);
        return true;
    }
}