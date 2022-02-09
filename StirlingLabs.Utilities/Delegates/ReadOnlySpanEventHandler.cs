using System;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public delegate void ReadOnlySpanEventHandler<in TSender, T>(TSender sender, ReadOnlySpan<T> data);
