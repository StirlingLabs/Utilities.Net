using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using AutoBogus;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using StirlingLabs.Utilities.Yaml;
using YamlDotNet.RepresentationModel;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace StirlingLabs.Utilities.Tests;

[Parallelizable(ParallelScope.All)]
public class YamlTests
{
    private static readonly JsonSerializer JsonNetSerializer = JsonSerializer.CreateDefault();
    private static readonly Faker<JsonMe> JsonMeFaker = new AutoFaker<JsonMe>()
        .RuleFor(f => f.number, GetActuallyRandomNumber)
        .RuleFor(f => f.texts, f => f.Make(100, _ => f.Hacker.Phrase()).ToArray())
        .RuleFor(f => f.numbers, f => f.Make(500, _ => GetActuallyRandomNumber()).ToArray());

#if NETSTANDARD2_0
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
#endif

    [SuppressMessage("ReSharper", "RedundantUnsafeContext")]
    public static unsafe double GetActuallyRandomNumber()
    {
        double number = 0;
#if !NETSTANDARD2_0
        var doubleSpan = MemoryMarshal.CreateSpan(ref number, 1);
        var bytesSpan = MemoryMarshal.AsBytes(doubleSpan);
        do RandomNumberGenerator.Fill(bytesSpan);
        while (!double.IsFinite(number));
#else
        var pNumber = &number;
        var buf = new byte[8];
        var bytesSpan = new Span<byte>(pNumber, 8);
        var longSpan = new Span<long>(pNumber, 1);
        do
        {
            Rng.GetBytes(buf);
            buf.CopyTo(bytesSpan);
        } while ((longSpan[0] & 0x7FF0000000000000) == 0x7FF0000000000000);
#endif
        return number;
    }

    [Test]
    public void Test1()
    {
        JsonMeFaker.AssertConfigurationIsValid();

        var k = new { a = JsonMeFaker.Generate(2) };

        var yml = OnDemand.YamlSerializer.Serialize(k);
        var ys = new YamlStream();
        ys.Load(new StringReader(yml));

        var sw = Stopwatch.StartNew();
        var expectedJson = ys.Serialize(OnDemand.JsonSerializer);
        var json1 = sw.ElapsedTicks;

        expectedJson.Should().NotBeNull();

        sw.Restart();
        var actualJson = ys.ToJson();
        var json2 = sw.ElapsedTicks;

        actualJson.Should().NotBeNull();

        dynamic expected = JsonNetSerializer.Deserialize(new StringReader(expectedJson!), k.GetType())!;

        dynamic actual = JsonNetSerializer.Deserialize(new StringReader(actualJson!), k.GetType())!;

        var actualList = (IList<JsonMe>)actual.a;
        var expectedList = (IList<JsonMe>)expected.a;
        
        actualList.Should().Equal(expectedList);

        Console.WriteLine($"ToJson w/ Serializer: {json1}, ToJson w/ YamlToJsonVisitor: {json2}");
    }
}
