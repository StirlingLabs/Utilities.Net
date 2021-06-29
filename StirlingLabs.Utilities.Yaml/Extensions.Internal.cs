using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using YamlDotNet.Serialization;
using static StirlingLabs.Utilities.Common;

#nullable enable
namespace StirlingLabs.Utilities.Yaml
{
    [PublicAPI]
    public static partial class Extensions
    {
        [ThreadStatic]
        private static WeakReference<IDeserializer>? _yamlDeserializer;

        [ThreadStatic]
        private static WeakReference<ISerializer>? _jsonSerializer;

        [ThreadStatic]
        private static WeakReference<ISerializer>? _yamlSerializer;

        private static IDeserializer Deserializer
            => OnDemand(ref _yamlDeserializer, () => new Deserializer());

        private static ISerializer JsonSerializer
            => OnDemand(ref _jsonSerializer, () => new SerializerBuilder().JsonCompatible().Build());

        private static ISerializer YamlSerializer
            => OnDemand(ref _yamlSerializer, () => new SerializerBuilder().Build());

        [return: NotNullIfNotNull("yamlObject")]
        private static string? SerializeJson(object? yamlObject)
            => Serialize(yamlObject, JsonSerializer);

        [return: NotNullIfNotNull("yamlObject")]
        private static string? SerializeYaml(object? yamlObject)
            => Serialize(yamlObject, YamlSerializer);

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
