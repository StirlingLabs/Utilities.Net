using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using StirlingLabs.Utilities.Yaml.Events;


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
            => OnDemand.Deserializer.Deserialize<T>(stream);

        public static T? Deserialize<T>(this YamlDocument doc)
            => OnDemand.Deserializer.Deserialize<T>(doc);

        public static T? Deserialize<T>(this YamlNode node)
            => OnDemand.Deserializer.Deserialize<T>(node);
    }
}
