using System;
using System.Reflection;
using NuGet.Versioning;

namespace StirlingLabs.Utilities.Tests;

public static class Helpers
{
    public static bool IsAssemblyNewerThan(Assembly asm, string nuGetSemVer)
    {
        if (nuGetSemVer is null) throw new ArgumentNullException(nameof(nuGetSemVer));

        var infoVersion = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (infoVersion is null) return false;

        return new NuGetVersion(infoVersion).CompareTo(new NuGetVersion(nuGetSemVer)) >= 0;
    }
}
