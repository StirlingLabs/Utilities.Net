using System;
using JetBrains.Annotations;

namespace StirlingLabs.Utilities;

[PublicAPI]
public delegate void SpanEventHandler<in TSender, T>(TSender sender, Span<T> data);
