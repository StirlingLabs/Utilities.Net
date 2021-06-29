using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using AutoBogus;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StirlingLabs.Utilities;
using StirlingLabs.Utilities.Yaml;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ValueDeserializers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace StirlingLabs.Utilties.Tests
{


    public class YamlTests
    {
        public static double GetActuallyRandomNumber()
        {
            double number = 0;
            do
            {
                RandomNumberGenerator.Fill(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref number, 1)));
            } while (!double.IsFinite(number));
            return number;
        }

        [Test]
        public void Test1()
        {
            var faker = new AutoFaker<JsonMe>()
                .RuleFor(f => f.number, GetActuallyRandomNumber)
                .RuleFor(f => f.numbers, f => f.Make(8000, _ => GetActuallyRandomNumber()).ToArray());

            faker.AssertConfigurationIsValid();

            var k = new { a = faker.Generate(2) };

            var yszr = new SerializerBuilder().Build();

            var yml = yszr.Serialize(k);
            var ys = new YamlStream();
            ys.Load(new StringReader(yml));

            var expectedJson = ys.ToJson(OnDemand.JsonSerializer);

            Assert.IsNotNull(expectedJson);

            var v = new YamlToJsonVisitor();

            v.Visit(ys);

            var actualJson = v.ToString();

            Assert.IsNotNull(actualJson);

            var jszr = JsonSerializer.CreateDefault();

            dynamic expected = jszr.Deserialize(new StringReader(expectedJson), k.GetType());

            dynamic actual = jszr.Deserialize(new StringReader(actualJson), k.GetType());

            Assert.AreEqual(expected.a, actual.a);
        }
    }
}
