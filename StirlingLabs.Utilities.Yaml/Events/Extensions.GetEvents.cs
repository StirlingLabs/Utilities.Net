using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;


namespace StirlingLabs.Utilities.Yaml.Events
{
    [PublicAPI]
    public static class Extensions
    {
        public static IEnumerable<ParsingEvent> GetEvents(this YamlStream stream)
        {
            yield return new StreamStart();

            foreach (var document in stream.Documents)
            {
                foreach (var evt in GetEvents(document))
                    yield return evt;
            }

            yield return new StreamEnd();
        }
        public static IEnumerable<ParsingEvent> GetEvents(this YamlDocument document)
        {
            yield return new DocumentStart();

            foreach (var evt in GetEvents(document.RootNode))
                yield return evt;

            yield return new DocumentEnd(false);
        }
        public static IEnumerable<ParsingEvent> GetEvents(this YamlNode node)
            => node switch
            {
                YamlScalarNode scalar => GetEvents(scalar),
                YamlSequenceNode sequence => GetEvents(sequence),
                YamlMappingNode mapping => GetEvents(mapping),
                _ => throw new NotSupportedException($"Unsupported node type: {node.GetType().Name}")
            };
        private static IEnumerable<ParsingEvent> GetEvents(YamlScalarNode scalar)
        {
            yield return new Scalar(scalar.Anchor, scalar.Tag, scalar.Value!, scalar.Style, false, false);
        }
        private static IEnumerable<ParsingEvent> GetEvents(YamlSequenceNode sequence)
        {
            yield return new SequenceStart(sequence.Anchor, sequence.Tag, false, sequence.Style);

            foreach (var node in sequence.Children)
            {
                foreach (var evt in GetEvents(node))
                    yield return evt;
            }

            yield return new SequenceEnd();
        }
        private static IEnumerable<ParsingEvent> GetEvents(YamlMappingNode mapping)
        {
            yield return new MappingStart(mapping.Anchor, mapping.Tag, false, mapping.Style);

            foreach (var (key, value) in mapping.Children)
            {
                foreach (var evt in GetEvents(key))
                    yield return evt;

                foreach (var evt in GetEvents(value))
                    yield return evt;
            }

            yield return new MappingEnd();
        }
    }
}
