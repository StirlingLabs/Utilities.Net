using System;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.Unsafe;

namespace StirlingLabs.Utilities;

public static partial class BinaryPrimitives
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReverseEndianness(byte value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ReverseEndianness(sbyte value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReverseEndianness(short value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReverseEndianness(ushort value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReverseEndianness(int value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReverseEndianness(uint value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReverseEndianness(long value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReverseEndianness(ulong value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReverseEndianness(float value)
        => Int32BitsToSingle(ReverseEndianness(SingleToInt32Bits(value)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReverseEndianness(double value)
        => Int64BitsToDouble(ReverseEndianness(DoubleToInt64Bits(value)));

    public static T ReverseEndianness<T>(T value) where T : unmanaged
        => value switch
        {
            sbyte v => As<sbyte, T>(ref AsRef(ReverseEndianness(v))),
            byte v => As<byte, T>(ref AsRef(ReverseEndianness(v))),
            short v => As<short, T>(ref AsRef(ReverseEndianness(v))),
            ushort v => As<ushort, T>(ref AsRef(ReverseEndianness(v))),
            int v => As<int, T>(ref AsRef(ReverseEndianness(v))),
            uint v => As<uint, T>(ref AsRef(ReverseEndianness(v))),
            long v => As<long, T>(ref AsRef(ReverseEndianness(v))),
            ulong v => As<ulong, T>(ref AsRef(ReverseEndianness(v))),
            float v => As<float, T>(ref AsRef(ReverseEndianness(v))),
            double v => As<double, T>(ref AsRef(ReverseEndianness(v))),
            _ => throw new NotSupportedException(typeof(T).AssemblyQualifiedName)
        };
}
