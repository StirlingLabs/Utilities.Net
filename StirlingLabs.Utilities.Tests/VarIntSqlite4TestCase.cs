namespace StirlingLabs.Utilities.Tests;

public readonly struct VarIntSqlite4TestCase
{
    public readonly ulong Decoded;
    public readonly byte[] Encoded;

    public VarIntSqlite4TestCase(ulong decoded, byte[] encoded)
    {
        Decoded = decoded;
        Encoded = encoded;
    }

    public int Length => Encoded.Length;

    public override string ToString()
        => $"{{{Decoded} => ({Length}) 0x{Encoded.ToHexString()}}}";
}