using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

#nullable enable
namespace StirlingLabs.Utilities.Yaml
{
    public static partial class Extensions
    {
        public static string? ToJson(this YamlStream stream, ISerializer serializer)
        {
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(stream);
            return Serialize(yamlObject, serializer);
        }
        public static string? ToJson(this YamlDocument doc, ISerializer serializer)
        {
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(doc);
            return Serialize(yamlObject, serializer);
        }
        public static string? ToJson(this YamlNode node, ISerializer serializer)
        {
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(node);
            return Serialize(yamlObject, serializer);
        }

        public static string? ToJson(this YamlStream stream)
        {
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(stream);
            return SerializeJson(yamlObject);
        }
        public static string? ToJson(this YamlDocument doc)
        {
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(doc);
            return SerializeJson(yamlObject);
        }
        public static string? ToJson(this YamlNode node)
        {
            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(node);
            return SerializeJson(yamlObject);
        }
    }
}
