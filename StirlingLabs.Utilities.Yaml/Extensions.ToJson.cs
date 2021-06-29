using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

#nullable enable
namespace StirlingLabs.Utilities.Yaml
{
    public static partial class Extensions
    {
        public static string? ToJson(this YamlStream stream, ISerializer serializer)
        {
            return Serialize(Deserializer.Deserialize(stream), serializer);
        }

        public static string? ToJson(this YamlDocument doc, ISerializer serializer)
            => Serialize(Deserializer.Deserialize(doc), serializer);

        public static string? ToJson(this YamlNode node, ISerializer serializer)
            => Serialize(Deserializer.Deserialize(node), serializer);

        public static string? ToJson(this YamlStream stream)
            => SerializeJson(Deserializer.Deserialize(stream));

        public static string? ToJson(this YamlDocument doc)
            => SerializeJson(Deserializer.Deserialize(doc));

        public static string? ToJson(this YamlNode node)
            => SerializeJson(Deserializer.Deserialize(node));
    }
}
