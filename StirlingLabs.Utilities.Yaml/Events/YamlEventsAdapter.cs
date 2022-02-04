using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;


namespace StirlingLabs.Utilities.Yaml.Events;

[PublicAPI]
public sealed class YamlEventsAdapter : YamlDotNet.Core.IParser, IEnumerator<ParsingEvent>
{
    private readonly IEnumerator<ParsingEvent> _enumerator;

    public YamlEventsAdapter(IEnumerable<ParsingEvent> events)
        => _enumerator = events.GetEnumerator();

    public YamlEventsAdapter(YamlStream stream)
        => _enumerator = stream.GetEvents().GetEnumerator();

    public YamlEventsAdapter(YamlDocument doc)
        => _enumerator = doc.GetEvents().GetEnumerator();

    public YamlEventsAdapter(YamlNode node)
        => _enumerator = node.GetEvents().GetEnumerator();

    public void Reset()
        => _enumerator.Reset();

    public ParsingEvent Current
        => _enumerator.Current;

    object IEnumerator.Current
        => Current;

    public bool MoveNext()
        => _enumerator.MoveNext();

    public void Dispose()
        => _enumerator.Dispose();
}