using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

#nullable enable
namespace StirlingLabs.Utilities.Yaml
{
    public static partial class Extensions
    {
        public static string? ToJson(this YamlStream stream, ISerializer serializer)
            => Serialize(Deserializer.Deserialize(stream), serializer);

        public static string? ToJson(this YamlDocument doc, ISerializer serializer)
            => Serialize(Deserializer.Deserialize(doc), serializer);

        public static string? ToJson(this YamlNode node, ISerializer serializer)
            => Serialize(Deserializer.Deserialize(node), serializer);

        public static string? ToJson(this YamlStream stream)
        {
            var visitor = new YamlToJsonVisitor();
            stream.Accept(visitor);
            return visitor.ToString();
        }

        public static string? ToJson(this YamlDocument doc)
        {
            var visitor = new YamlToJsonVisitor();
            doc.Accept(visitor);
            return visitor.ToString();
        }

        public static string? ToJson(this YamlNode node)
        {
            var visitor = new YamlToJsonVisitor();
            node.Accept(visitor);
            return visitor.ToString();
        }
    }
}
