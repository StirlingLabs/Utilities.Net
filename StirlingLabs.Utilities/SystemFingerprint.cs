using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace StirlingLabs.Utilities;

public static class SystemFingerprint
{
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
        var uints = MemoryMarshal.Cast<byte, uint>(buffer);

        var x = 0;
        foreach (var nic in networkInterfaces)
        {
            var mac = nic.GetPhysicalAddress();
            var macBytes = mac.GetAddressBytes();
            var i = 0;
            for(;;)
            {
                ref var uintX = ref uints[x++ & 1];
                var remaining = macBytes.Length - i;
                if (remaining <= 0) break;
                if (remaining >= 3)
                {
                    uintX = Crc32C.Calculate(uintX, ref macBytes[i], 3);
                    i += 3;
                }
                else
                {
                    uintX = Crc32C.Calculate(uintX, ref macBytes[i], remaining);
                    i += remaining;
                }
            }
        }

        Span<byte> lengthBuffer = stackalloc byte[4];
        var lengthInt  = MemoryMarshal.Cast<byte, int>(lengthBuffer);
        lengthInt[0] = networkInterfaces.Length;
        uints[0] = Crc32C.Calculate(uints[0], ref lengthBuffer[0], 4);
        uints[1] = Crc32C.Calculate(uints[1], ref lengthBuffer[0], 4);

        return MemoryMarshal.Cast<byte, long>(buffer)[0];
    }
}
