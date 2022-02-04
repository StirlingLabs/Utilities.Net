namespace StirlingLabs.Utilities;

public interface IBinaryReader
    : IReader<sbyte>,
        IReader<byte>,
        IReader<short>,
        IReader<ushort>,
        IReader<int>,
        IReader<uint>,
        IReader<long>,
        IReader<ulong>,
        IReader<float>,
        IReader<double>,
        IReader<nint>,
        IReader<nuint>
{
    bool CanRead();

    bool CanReadBytes(uint n);
}