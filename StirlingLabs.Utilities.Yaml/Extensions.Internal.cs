using System.IO;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using YamlDotNet.Serialization;


namespace StirlingLabs.Utilities.Yaml
{
    [PublicAPI]
    public static partial class Extensions
    {
        #if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [return: NotNullIfNotNull("yamlObject")]
        #endif
        private static string? SerializeJson(object? yamlObject)
            => Serialize(yamlObject, OnDemand.JsonSerializer);

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [return: NotNullIfNotNull("yamlObject")]
#endif
        private static string? SerializeYaml(object? yamlObject)
            => Serialize(yamlObject, OnDemand.YamlSerializer);

#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        [return: NotNullIfNotNull("yamlObject")]
#endif
        private static string? Serialize(object? yamlObject, ISerializer serializer)
        {
            if (yamlObject is null) return null;
            using var result = new StringWriter { NewLine = string.Empty };
            serializer.Serialize(result, yamlObject);
            return result.ToString();
        }
    }
}
