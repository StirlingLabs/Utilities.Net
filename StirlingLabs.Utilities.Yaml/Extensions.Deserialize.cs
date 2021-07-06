using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using StirlingLabs.Utilities.Yaml.Events;


namespace StirlingLabs.Utilities.Yaml
{
    public static partial class Extensions
    {

        public static object? Deserialize(this IDeserializer deserializer, YamlStream? stream)
            => stream is null ? null : deserializer.Deserialize(new YamlEventsAdapter(stream));
        public static object? Deserialize(this IDeserializer deserializer, YamlDocument? stream)
            => stream is null ? null : deserializer.Deserialize(new YamlEventsAdapter(stream));
        public static object? Deserialize(this IDeserializer deserializer, YamlNode? stream)
            => stream is null ? null : deserializer.Deserialize(new YamlEventsAdapter(stream));

        public static object? Deserialize(this YamlStream stream)
            => OnDemand.Deserializer.Deserialize(stream);

        public static object? Deserialize(this YamlDocument doc)
            => OnDemand.Deserializer.Deserialize(doc);

        public static object? Deserialize(this YamlNode node)
            => OnDemand.Deserializer.Deserialize(node);
    }
}
