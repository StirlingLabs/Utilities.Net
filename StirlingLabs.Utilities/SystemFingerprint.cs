using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace StirlingLabs.Utilities;

public static class SystemFingerprint
{
    public static long GetSystemFingerprint()
    {
        var osDesc = RuntimeInformation.OSDescription;
        var machineName = Environment.MachineName;
        var machineType = (byte)RuntimeInformation.OSArchitecture;

        var osDescSize = osDesc.Length * 2;
        var machineNameSize = machineName.Length * 2;
        var bufSize = osDescSize + machineNameSize + (4 + 4 + 1);

        Span<byte> buffer = stackalloc byte[bufSize];

        MemoryMarshal.Cast<byte, int>(buffer.Slice(0, 4))[0] = osDesc.Length;
        MemoryMarshal.Cast<byte, int>(buffer.Slice(osDescSize + 4, 4))[0] = machineName.Length;

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        Encoding.Unicode.GetBytes(osDesc, buffer.Slice(4, osDescSize));
        Encoding.Unicode.GetBytes(machineName, buffer.Slice(osDescSize + (4 + 4), machineNameSize));
#else
        unsafe
        {
            fixed (char* pOsDesc = osDesc)
            {
                var osDescSpan = new ReadOnlySpan<char>(pOsDesc, osDesc.Length);
                osDescSpan.CopyTo(MemoryMarshal.Cast<byte, char>(buffer.Slice(4, osDescSize)));
            }
            fixed (char* pMachineName = machineName)
            {
                var osDescSpan = new ReadOnlySpan<char>(pMachineName, machineName.Length);
                osDescSpan.CopyTo(MemoryMarshal.Cast<byte, char>(buffer.Slice(4, machineNameSize)));
            }
        }
#endif

        buffer.Slice(osDescSize + machineNameSize + (4 + 4), 1)[0] = machineType;

        return Crc32C.Calculate2(buffer);
    }

    public static long GetNetworkFingerprint()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        Array.Sort(networkInterfaces,
            (lhs, rhs)
                => {
                var lpa = lhs.GetPhysicalAddress();
                var rpa = rhs.GetPhysicalAddress();
                return lpa.Equals(rpa)
                    ? 0
                    : new ReadOnlyBigSpan<byte>(lpa.GetAddressBytes())
                        .SequenceCompare(rpa.GetAddressBytes());
            });

        Span<byte> buffer = stackalloc byte[8];
        var longs = MemoryMarshal.Cast<byte, long>(buffer);
        var uints = MemoryMarshal.Cast<byte, uint>(buffer);

        Span<byte> lengthBuffer = stackalloc byte[4];
        var lengthInt = MemoryMarshal.Cast<byte, int>(lengthBuffer);
        lengthInt[0] = networkInterfaces.Length;
        uints[0] = Crc32C.Calculate(uints[0], ref lengthBuffer[0], 4);
        uints[1] = Crc32C.Calculate(uints[1], ref lengthBuffer[0], 4);

        foreach (var nic in networkInterfaces)
        {
            var mac = nic.GetPhysicalAddress();
            var macBytes = mac.GetAddressBytes();
            Crc32C.Calculate2(ref longs[0], macBytes);
        }

        return MemoryMarshal.Cast<byte, long>(buffer)[0];
    }
}
