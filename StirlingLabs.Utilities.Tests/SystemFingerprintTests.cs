using System;
using FluentAssertions;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.All)]
public static class SystemFingerprintTests
{
    
    [Test]
    public static void SelfConsistentSystemFingerprintTest()
    {
        if (!Helpers.IsAssemblyNewerThan(typeof(BigSpan).Assembly, "23.2.0"))
            throw new InconclusiveException("BigSpan needs to be at least v23.2.0 for this test.");
        
        var a = SystemFingerprint.GetSystemFingerprint();
        
        a.Should().Be(SystemFingerprint.GetSystemFingerprint());
        
        var low = (uint)a;
        low.Should().NotBe(0u);
        
        var high = (uint)((ulong)a >> 32);
        high.Should().NotBe(0u);
        
        low.Should().NotBe(high);
    }
    
    [Test]
    public static void SelfConsistentNetworkFingerprintTest()
    {
        if (!Helpers.IsAssemblyNewerThan(typeof(BigSpan).Assembly, "23.2.0"))
            throw new InconclusiveException("BigSpan needs to be at least v23.2.0 for this test.");

        var a = SystemFingerprint.GetNetworkFingerprint();
        
        a.Should().Be(SystemFingerprint.GetNetworkFingerprint());
        
        var low = (uint)a;
        low.Should().NotBe(0u);
        
        var high = (uint)((ulong)a >> 32);
        high.Should().NotBe(0u);
        
        low.Should().NotBe(high);
    }
    
}
