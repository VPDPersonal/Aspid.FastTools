using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class ICustomStyleExtensionsTests
    {
        private const string ProbeClassPrefix = "aspid-fasttools-test-custom-style--";
        private const string ProbeStyleSheetPath =
            "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements/ICustomStyleProbe.uss";

        private static readonly CustomStyleProperty<string> FontStyleProperty = new("--aspid-fasttools-test-font-style");

        private TestPanel _panel;

        [SetUp]
        public void SetUp() => _panel = new TestPanel();

        [TearDown]
        public void TearDown() => _panel.Dispose();

        [UnityTest]
        public IEnumerator TryGetByEnum_UpperCaseValue_ParsesIgnoringCase()
        {
            var result = new Result();
            yield return Resolve("upper-case", result);

            Assert.IsTrue(result.Found);
            Assert.AreEqual(FontStyle.BoldAndItalic, result.Value);
        }

        [UnityTest]
        public IEnumerator TryGetByEnum_LowerCaseValue_ParsesIgnoringCase()
        {
            var result = new Result();
            yield return Resolve("lower-case", result);

            Assert.IsTrue(result.Found);
            Assert.AreEqual(FontStyle.Italic, result.Value);
        }

        [UnityTest]
        public IEnumerator TryGetByEnum_UnknownName_ReturnsFalseWithDefault()
        {
            var result = new Result();
            yield return Resolve("unknown", result);

            Assert.IsTrue(result.Resolved);
            Assert.IsFalse(result.Found);
            Assert.AreEqual(default(FontStyle), result.Value);
        }

        [UnityTest]
        public IEnumerator TryGetByEnum_PropertyNotDeclared_ReturnsFalseWithDefault()
        {
            var result = new Result();
            yield return Resolve("other", result);

            Assert.IsTrue(result.Resolved);
            Assert.IsFalse(result.Found);
            Assert.AreEqual(default(FontStyle), result.Value);
        }

        private IEnumerator Resolve(string probe, Result result)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ProbeStyleSheetPath);
            Assert.IsNotNull(styleSheet, "The probe style sheet must load.");

            var element = new VisualElement();
            element.styleSheets.Add(styleSheet);
            element.AddToClassList(ProbeClassPrefix + probe);
            element.RegisterCallback<CustomStyleResolvedEvent>(evt =>
            {
                result.Resolved = true;
                result.Found = evt.customStyle.TryGetByEnum<FontStyle>(FontStyleProperty, out var value);
                result.Value = value;
            });

            _panel.Root.Add(element);
            yield return null;
        }

        private sealed class Result
        {
            public bool Found;
            public bool Resolved;
            public FontStyle Value;
        }
    }
}
