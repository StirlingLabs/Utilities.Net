using System.Threading;
using YamlDotNet.Core.Tokens;

namespace StirlingLabs.Utilities.Tests;

public static class ScopedSingleton<TValue,TUniqueType>
{
    public static readonly object Lock = new();
    public static TValue? Value;
}
