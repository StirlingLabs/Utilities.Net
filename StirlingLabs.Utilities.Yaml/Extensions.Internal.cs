using System.IO;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

#nullable enable
namespace StirlingLabs.Utilities.Yaml
{
    [PublicAPI]
    public static partial class Extensions
    {
        [return: NotNullIfNotNull("yamlObject")]
        private static string? SerializeJson(object? yamlObject)
            => Serialize(yamlObject, OnDemand.JsonSerializer);

        [return: NotNullIfNotNull("yamlObject")]
        private static string? SerializeYaml(object? yamlObject)
            => Serialize(yamlObject, OnDemand.YamlSerializer);

        [return: NotNullIfNotNull("yamlObject")]
        private static string? Serialize(object? yamlObject, ISerializer serializer)
        {
            if (yamlObject is null) return null;
            using var result = new StringWriter { NewLine = string.Empty };
            serializer.Serialize(result, yamlObject);
            return result.ToString();
        }
    }
}
