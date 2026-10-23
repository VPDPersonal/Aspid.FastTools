using NUnit.Framework;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors
{
    // ToKebabCase names the enum values field after its property, so a USS selector can target it.
    internal sealed class StringExtensionsTests
    {
        [TestCase("value", ExpectedResult = "value")]
        [TestCase("Value", ExpectedResult = "value")]
        [TestCase("myValue", ExpectedResult = "my-value")]
        [TestCase("_myValue", ExpectedResult = "my-value")]
        [TestCase("m_MyValue", ExpectedResult = "m-my-value")]
        [TestCase("HTMLParser", ExpectedResult = "html-parser")]
        [TestCase("parseHTML", ExpectedResult = "parse-html")]
        [TestCase("Vector3Field", ExpectedResult = "vector3-field")]
        [TestCase("my_value name", ExpectedResult = "my-value-name")]
        [TestCase("my--value", ExpectedResult = "my-value")]
        public string ToKebabCase_SplitsWords(string value) => value.ToKebabCase();

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ToKebabCase_NullOrWhiteSpace_ReturnsTheValue(string value) =>
            Assert.AreEqual(value, value.ToKebabCase());
    }
}
