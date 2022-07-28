using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.All)]
public class NativeLibraryTests
{
    [Test]
    public void LoadFailTest()
    {
        Action a = () => NativeLibrary.Load("should-not-exist");
        a.Should().Throw<DllNotFoundException>();
    }

    [Test]
    public void LoadSuccessTest()
    {
        var loaded = NativeLibrary.Load(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "msvcp60.dll"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "libstdc++.so.6"
                    : "libstdc++.6.dylib"
        );
        loaded.Should().NotBe(default);
    }
}
