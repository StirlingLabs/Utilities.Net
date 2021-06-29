using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using StirlingLabs.Utilities.Yaml.Events;

#nullable enable
namespace StirlingLabs.Utilities.Yaml
{
    public static partial class Extensions
    {
        public static T? Deserialize<T>(this IDeserializer deserializer, YamlStream? stream)
            => stream is null ? default : deserializer.Deserialize<T>(new YamlEventsAdapter(stream));

        public static T? Deserialize<T>(this IDeserializer deserializer, YamlDocument? stream)
            => stream is null ? default : deserializer.Deserialize<T>(new YamlEventsAdapter(stream));
        
        public static T? Deserialize<T>(this IDeserializer deserializer, YamlNode? stream)
            => stream is null ? default : deserializer.Deserialize<T>(new YamlEventsAdapter(stream));

        public static T? Deserialize<T>(this YamlStream stream)
            => Deserializer.Deserialize<T>(stream);

        public static T? Deserialize<T>(this YamlDocument doc)
            => Deserializer.Deserialize<T>(doc);

        public static T? Deserialize<T>(this YamlNode node)
            => Deserializer.Deserialize<T>(node);
    }
}
