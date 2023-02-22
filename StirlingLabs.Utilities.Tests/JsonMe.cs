using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StirlingLabs.Utilities.Tests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class JsonMe
{
    public object? arbitrary;

    public string? text;

    public double number;

    public string[]? texts;

    public double[]? numbers;

    public Dictionary<string, string>? stringDict;

    public Dictionary<string, double>? numberDict;

    public Dictionary<string, object>? arbitraryDict;
}