namespace StirlingLabs.Utilities;

public interface IWriter<in T> where T : unmanaged
{
    bool TryWrite(T value);
}
