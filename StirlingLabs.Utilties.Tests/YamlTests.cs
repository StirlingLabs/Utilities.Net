using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using AutoBogus;
using Bogus;
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
        private static readonly YamlToJsonVisitor YamlToJsonVisitor = new YamlToJsonVisitor();
        private static readonly JsonSerializer JsonNetSerializer = JsonSerializer.CreateDefault();
        private static readonly Faker<JsonMe> JsonMeFaker = new AutoFaker<JsonMe>()
            .RuleFor(f => f.number, GetActuallyRandomNumber)
            .RuleFor(f => f.texts, f => f.Make(2000, _ => f.Hacker.Phrase()).ToArray())
            .RuleFor(f => f.numbers, f => f.Make(2000, _ => GetActuallyRandomNumber()).ToArray());

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
            JsonMeFaker.AssertConfigurationIsValid();

            var k = new { a = JsonMeFaker.Generate(2) };

            var yml = OnDemand.YamlSerializer.Serialize(k);
            var ys = new YamlStream();
            ys.Load(new StringReader(yml));

            var expectedJson = ys.ToJson(OnDemand.JsonSerializer);

            Assert.IsNotNull(expectedJson);

            YamlToJsonVisitor.Visit(ys);

            var actualJson = YamlToJsonVisitor.ToString();

            Assert.IsNotNull(actualJson);

            dynamic expected = JsonNetSerializer.Deserialize(new StringReader(expectedJson), k.GetType());

            dynamic actual = JsonNetSerializer.Deserialize(new StringReader(actualJson), k.GetType());

            Assert.AreEqual(expected.a, actual.a);
        }
    }
}
