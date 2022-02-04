namespace StirlingLabs.Utilities;

public interface IReader<T> where T : unmanaged
{
    bool TryRead(out T value);
}