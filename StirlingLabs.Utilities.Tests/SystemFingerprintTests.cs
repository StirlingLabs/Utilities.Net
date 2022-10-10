using System;
using FluentAssertions;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.All)]
public static class SystemFingerprintTests
{
    
    [Test]
    public static void SelfConsistentTest()
    {
        var a = SystemFingerprint.GetNetworkFingerprint();
        
        a.Should().Be(SystemFingerprint.GetNetworkFingerprint());
        
        var low = (uint)a;
        low.Should().NotBe(0u);
        
        var high = (uint)((ulong)a >> 32);
        high.Should().NotBe(0u);
        
        low.Should().NotBe(high);
    }
    
}
