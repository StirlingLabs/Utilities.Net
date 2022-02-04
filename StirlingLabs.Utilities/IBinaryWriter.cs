namespace StirlingLabs.Utilities;

public interface IBinaryWriter
    : IWriter<sbyte>,
        IWriter<byte>,
        IWriter<short>,
        IWriter<ushort>,
        IWriter<int>,
        IWriter<uint>,
        IWriter<long>,
        IWriter<ulong>,
        IWriter<float>,
        IWriter<double>,
        IWriter<nint>,
        IWriter<nuint>
{
    bool CanWrite();

    bool CanWriteBytes(uint n);
}