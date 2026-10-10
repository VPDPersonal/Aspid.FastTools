using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson"/>, which turns the payload of a
    /// missing reference into the JSON a Fix in memory carries onto the replacement instance.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceYamlPayloadTests
    {
        // Unity's serializedData for a payload with these values, copied from a 6000.4 Editor: floats in exponent
        // form, non-ASCII and control characters as escapes in double quotes, wrapped long strings.
        private static readonly string UnityPayload = string.Join("\n",
            @"i: -42",
            @"big: 9007199254740993",
            @"small: 0.0000999",
            @"large: 3.4e+38",
            @"dbl: 1e+300",
            @"inf: Infinity",
            @"ninf: -Infinity",
            @"nan: NaN",
            @"flag: 1",
            @"uni: ""\u041F\u0440\u0438\u0432\u0435\u0442, \u043C\u0438\u0440""",
            @"colon: 'a: b'",
            @"quote: it's ""q""",
            @"multi: 'line one",
            @"",
            @"  line two'",
            @"longText: Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod",
            @"  tempor incididunt ut labore et dolore magna aliqua.",
            @"longUni: ""\u0414\u043B\u0438\u043D\u043D\u0430\u044F \u0441\u0442\u0440\u043E\u043A\u0430",
            @"  \u043D\u0430 \u0440\u0443\u0441\u0441\u043A\u043E\u043C \u044F\u0437\u044B\u043A\u0435,",
            @"  \u043A\u043E\u0442\u043E\u0440\u0430\u044F \u0442\u043E\u0447\u043D\u043E \u043D\u0435",
            @"  \u043F\u043E\u043C\u0435\u0441\u0442\u0438\u0442\u0441\u044F \u0432 \u043E\u0434\u043D\u0443",
            @"  \u0441\u0442\u0440\u043E\u043A\u0443 YAML \u0448\u0438\u0440\u0438\u043D\u043E\u0439",
            @"  \u0432\u043E\u0441\u0435\u043C\u044C\u0434\u0435\u0441\u044F\u0442 \u0441\u0438\u043C\u0432\u043E\u043B\u043E\u0432.""",
            @"numeric: 123",
            "empty: ",
            @"lead: ' lead'",
            @"tab: ""a\tb""",
            @"emoji: ""\U0001F600""",
            @"backslash: C:\path",
            @"nullish: null",
            @"trail: 'x '",
            @"sq: '''a'''",
            @"dq: '""a""'",
            @"blank2: 'a",
            @"",
            @"",
            @"  b'",
            @"");

        [Test]
        public void ReadPayloadScalarsAsJson_UnityPayload_DecodesEveryScalar()
        {
            var scalars = ToDictionary(SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson(UnityPayload));

            Assert.AreEqual("-42", scalars["i"]);
            Assert.AreEqual("9007199254740993", scalars["big"]);
            Assert.AreEqual("0.0000999", scalars["small"]);
            Assert.AreEqual("3.4e+38", scalars["large"], "A float in exponent form must stay a number.");
            Assert.AreEqual("1e+300", scalars["dbl"]);
            Assert.AreEqual("Infinity", scalars["inf"], "JsonUtility reads Infinity only bare.");
            Assert.AreEqual("-Infinity", scalars["ninf"]);
            Assert.AreEqual("NaN", scalars["nan"]);
            Assert.AreEqual("1", scalars["flag"]);
            Assert.AreEqual("\"Привет, мир\"", scalars["uni"], "Non-ASCII text must lose its quotes and escapes.");
            Assert.AreEqual("\"a: b\"", scalars["colon"]);
            Assert.AreEqual("\"it's \\\"q\\\"\"", scalars["quote"]);
            Assert.AreEqual("\"line one\\nline two\"", scalars["multi"], "An empty line in a quoted scalar is a line break.");
            Assert.AreEqual(
                "\"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.\"",
                scalars["longText"], "A wrapped plain scalar must be joined with spaces.");
            Assert.AreEqual(
                "\"Длинная строка на русском языке, которая точно не поместится в одну строку YAML шириной восемьдесят символов.\"",
                scalars["longUni"], "A wrapped double-quoted scalar must be joined with spaces.");
            Assert.AreEqual("123", scalars["numeric"]);
            Assert.AreEqual("\"\"", scalars["empty"]);
            Assert.AreEqual("\" lead\"", scalars["lead"]);
            Assert.AreEqual("\"a\\tb\"", scalars["tab"]);
            Assert.AreEqual("\"\U0001F600\"", scalars["emoji"]);
            Assert.AreEqual("\"C:\\\\path\"", scalars["backslash"], "A plain scalar has no escapes.");
            Assert.AreEqual("\"null\"", scalars["nullish"]);
            Assert.AreEqual("\"x \"", scalars["trail"]);
            Assert.AreEqual("\"'a'\"", scalars["sq"]);
            Assert.AreEqual("\"\\\"a\\\"\"", scalars["dq"]);
            Assert.AreEqual("\"a\\n\\nb\"", scalars["blank2"]);
        }

        [Test]
        public void ReadPayloadScalarsAsJson_TextField_KeepsNumberLikeScalarsAsStrings()
        {
            const string payload = "label: 1.5\ncode: 007\nword: Infinity\nratio: 1.5\n";
            var textFields = new HashSet<string> { "label", "code", "word" };

            var scalars = ToDictionary(SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson(payload, textFields.Contains));

            Assert.AreEqual("\"1.5\"", scalars["label"], "JsonUtility writes a number into a string field as 1.500000.");
            Assert.AreEqual("\"007\"", scalars["code"]);
            Assert.AreEqual("\"Infinity\"", scalars["word"]);
            Assert.AreEqual("1.5", scalars["ratio"]);
        }

        [Test]
        public void ReadPayloadScalarsAsJson_NotAJsonNumber_IsQuoted()
        {
            var scalars = ToDictionary(SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson("a: 007\nb: +1\nc: .5\nd: 1.\n"));

            Assert.AreEqual("\"007\"", scalars["a"]);
            Assert.AreEqual("\"+1\"", scalars["b"]);
            Assert.AreEqual("\".5\"", scalars["c"]);
            Assert.AreEqual("\"1.\"", scalars["d"]);
        }

        [Test]
        public void ReadPayloadScalarsAsJson_NestedValues_AreSkipped()
        {
            const string payload =
                "nested:\n  a: 1\n  b: 2\nlist:\n- 1\n- 2\nflow: {x: 1.5, y: 2}\nitems: []\nafter: 5\n";

            var scalars = SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson(payload);

            CollectionAssert.AreEqual(new[] { "after" }, scalars.Select(scalar => scalar.Key).ToArray());
            Assert.AreEqual("5", scalars[0].Value);
        }

        [Test]
        public void ReadPayloadScalarsAsJson_EscapedLineBreak_JoinsWithoutSpace()
        {
            const string payload = "a: \"abc\\\n  def\"\nb: \"one\\\n  \\ two\"\n";

            var scalars = ToDictionary(SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson(payload));

            Assert.AreEqual("\"abcdef\"", scalars["a"]);
            Assert.AreEqual("\"one two\"", scalars["b"]);
        }

        [Test]
        public void ReadPayloadScalarsAsJson_MalformedScalar_IsSkippedAlone()
        {
            const string payload = "open: \"abc\nbad: \"\\q\"\nok: 1\n";

            var scalars = SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson(payload);

            CollectionAssert.AreEqual(new[] { "ok" }, scalars.Select(scalar => scalar.Key).ToArray());
        }

        [Test]
        public void ReadPayloadScalarsAsJson_CrLfLineEnds_ReadLikeLf()
        {
            var scalars = ToDictionary(SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson("a: 1\r\nb: 'x\r\n\r\n  y'\r\n"));

            Assert.AreEqual("1", scalars["a"]);
            Assert.AreEqual("\"x\\ny\"", scalars["b"]);
        }

        [Test]
        public void BuildJsonObject_JoinsMembersInOrder()
        {
            var json = SerializeReferenceYamlEditor.BuildJsonObject(
                SerializeReferenceYamlEditor.ReadPayloadScalarsAsJson("x: 3\nname: \"\\u041F\"\nf: 9.99e-05\n"));

            Assert.AreEqual("{\"x\":3,\"name\":\"П\",\"f\":9.99e-05}", json);
        }

        private static Dictionary<string, string> ToDictionary(List<KeyValuePair<string, string>> scalars) =>
            scalars.ToDictionary(scalar => scalar.Key, scalar => scalar.Value);
    }
}
