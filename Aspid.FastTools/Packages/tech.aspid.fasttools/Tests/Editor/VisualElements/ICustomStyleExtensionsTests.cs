using System;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class ICustomStyleExtensionsTests
    {
        internal enum Mode
        {
            Off = 0,
            On = 1,
            Auto = 2,
        }

        [Flags]
        internal enum Layers
        {
            None = 0,
            Ground = 1,
            Water = 2,
            Sky = 4,
        }

        private static readonly CustomStyleProperty<string> Property = new("--test-value");

        [TestCase("On", Mode.On)]
        [TestCase("on", Mode.On)]
        [TestCase("AUTO", Mode.Auto)]
        [TestCase("1", Mode.On)]
        public void TryGetByEnum_DefinedValue_ReturnsIt(string text, Mode expected)
        {
            var result = new StubCustomStyle(text).TryGetByEnum(Property, out Mode value);

            Assert.IsTrue(result);
            Assert.AreEqual(expected, value);
        }

        [TestCase("7")]
        [TestCase("-1")]
        [TestCase("On, Auto")]
        [TestCase("Off, On")]
        [TestCase("On,On")]
        [TestCase("Missing")]
        [TestCase("")]
        public void TryGetByEnum_UndefinedValue_ReturnsFalseAndDefault(string text)
        {
            var result = new StubCustomStyle(text).TryGetByEnum(Property, out Mode value);

            Assert.IsFalse(result);
            Assert.AreEqual(default(Mode), value);
        }

        [Test]
        public void TryGetByEnum_MissingProperty_ReturnsFalseAndDefault()
        {
            var result = new StubCustomStyle(null).TryGetByEnum(Property, out Mode value);

            Assert.IsFalse(result);
            Assert.AreEqual(default(Mode), value);
        }

        [TestCase("None", Layers.None)]
        [TestCase("Water", Layers.Water)]
        [TestCase("ground, SKY", Layers.Ground | Layers.Sky)]
        [TestCase("Ground,Water,Sky", Layers.Ground | Layers.Water | Layers.Sky)]
        public void TryGetByEnum_FlagsNames_ReturnsCombination(string text, Layers expected)
        {
            var result = new StubCustomStyle(text).TryGetByEnum(Property, out Layers value);

            Assert.IsTrue(result);
            Assert.AreEqual(expected, value);
        }

        [TestCase("0")]
        [TestCase("1")]
        [TestCase("7")]
        [TestCase("8")]
        [TestCase("-1")]
        [TestCase("+1")]
        [TestCase(" 8")]
        [TestCase("Ground, 8")]
        [TestCase("Ground,8")]
        [TestCase("Ground, -1")]
        public void TryGetByEnum_FlagsNumber_ReturnsFalseAndDefault(string text)
        {
            var result = new StubCustomStyle(text).TryGetByEnum(Property, out Layers value);

            Assert.IsFalse(result);
            Assert.AreEqual(default(Layers), value);
        }

        // Implements the members of ICustomStyle; only the string one returns data.
        private sealed class StubCustomStyle : ICustomStyle
        {
            private readonly string _value;

            public StubCustomStyle(string value) =>
                _value = value;

            public bool TryGetValue(CustomStyleProperty<string> property, out string value)
            {
                value = _value;
                return _value is not null;
            }

            public bool TryGetValue(CustomStyleProperty<float> property, out float value) =>
                Unsupported(out value);

            public bool TryGetValue(CustomStyleProperty<int> property, out int value) =>
                Unsupported(out value);

            public bool TryGetValue(CustomStyleProperty<bool> property, out bool value) =>
                Unsupported(out value);

            public bool TryGetValue(CustomStyleProperty<Color> property, out Color value) =>
                Unsupported(out value);

            public bool TryGetValue(CustomStyleProperty<Texture2D> property, out Texture2D value) =>
                Unsupported(out value);

            public bool TryGetValue(CustomStyleProperty<Sprite> property, out Sprite value) =>
                Unsupported(out value);

            public bool TryGetValue(CustomStyleProperty<VectorImage> property, out VectorImage value) =>
                Unsupported(out value);

            bool ICustomStyle.TryGetValue<T>(CustomStyleProperty<T> property, out T value) =>
                Unsupported(out value);

            private static bool Unsupported<T>(out T value)
            {
                value = default;
                return false;
            }
        }
    }
}
