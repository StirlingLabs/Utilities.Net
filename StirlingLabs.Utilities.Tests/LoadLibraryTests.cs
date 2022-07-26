using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            OperatingSystem.IsWindows()
                ? "msvcp60.dll"
                : OperatingSystem.IsLinux()
                    ? "libstdc++.so.6"
                    : "libstdc++.6.dylib"
        );
        loaded.Should().NotBe(default);
    }
}
