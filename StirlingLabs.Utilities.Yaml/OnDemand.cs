using System;
using YamlDotNet.Serialization;


namespace StirlingLabs.Utilities.Yaml;

public static class OnDemand
{
    [ThreadStatic]
    private static WeakReference<IDeserializer>? _yamlDeserializer;
    [ThreadStatic]
    private static WeakReference<ISerializer>? _jsonSerializer;
    [ThreadStatic]
    private static WeakReference<ISerializer>? _yamlSerializer;

    public static IDeserializer Deserializer
        => Common.OnDemand(ref _yamlDeserializer, () => new Deserializer());

    public static ISerializer JsonSerializer
        => Common.OnDemand(ref _jsonSerializer, () => new SerializerBuilder().JsonCompatible().Build());

    public static ISerializer YamlSerializer
        => Common.OnDemand(ref _yamlSerializer, () => new SerializerBuilder().Build());
}