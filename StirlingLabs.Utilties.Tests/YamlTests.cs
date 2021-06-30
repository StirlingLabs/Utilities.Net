using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using AutoBogus;
using Bogus;
using NUnit.Framework;
using StirlingLabs.Utilities.Yaml;
using YamlDotNet.RepresentationModel;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace StirlingLabs.Utilties.Tests
{
    public class YamlTests
    {
        private static readonly YamlToJsonVisitor YamlToJsonVisitor = new YamlToJsonVisitor();
        private static readonly JsonSerializer JsonNetSerializer = JsonSerializer.CreateDefault();
        private static readonly Faker<JsonMe> JsonMeFaker = new AutoFaker<JsonMe>()
            .RuleFor(f => f.number, GetActuallyRandomNumber)
            .RuleFor(f => f.texts, f => f.Make(100, _ => f.Hacker.Phrase()).ToArray())
            .RuleFor(f => f.numbers, f => f.Make(500, _ => GetActuallyRandomNumber()).ToArray());

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

            Assert.AreEqual((IList<JsonMe>)expected.a, (IList<JsonMe>)actual.a);
        }
    }
}
