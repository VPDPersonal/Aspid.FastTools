using NUnit.Framework;
using UnityEngine.UIElements;
#if UNITY_6000_6_OR_NEWER
using System;
using UnityEngine;
using System.Text;
using System.Collections;
using UnityEngine.TestTools;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;
#endif

namespace Aspid.FastTools.UIElements.Tests
{
    internal sealed class TextElementExtensionsTests
    {
        [Test]
        public void SetTextSelf_ReturnsElementForChaining()
        {
            var button = new Button();

            var result = button
                .SetTextSelf("Apply")
                .SetTooltip("Applies the changes");

            Assert.AreSame(button, result);
            Assert.AreEqual("Apply", result.text);
        }

#if UNITY_6000_6_OR_NEWER
        // A label outside a panel takes the string fallback, so these tests attach the label to reach TextElement.SetText.
        [UnityTest]
        public IEnumerator SetTextSelf_Number_SetsText()
        {
            using var panel = new TestPanel();
            var label = new Label();
            panel.Root.Add(label);
            yield return null;

            var result = label
                .SetTextSelf(42)
                .SetTooltip("Count");
            Assert.AreSame(label, result);
            Assert.AreEqual("42", label.text);

            label.SetTextSelf(2.6f, "F0");
            Assert.AreEqual("3", label.text);

            label.SetTextSelf(2f);
            Assert.AreEqual("2", label.text);
        }

        [UnityTest]
        public IEnumerator SetTextSelf_WideIntegers_KeepAllDigits()
        {
            using var panel = new TestPanel();
            var label = new Label();
            panel.Root.Add(label);
            yield return null;

            label.SetTextSelf(123456789012L);
            Assert.AreEqual("123456789012", label.text);

            label.SetTextSelf(ulong.MaxValue);
            Assert.AreEqual("18446744073709551615", label.text);

            label.SetTextSelf(16777217u);
            Assert.AreEqual("16777217", label.text);
        }

        [Test]
        public void SetTextSelf_ElementOutsidePanel_SetsTextWithoutWarning()
        {
            var warnings = 0;
            void OnLog(string message, string stackTrace, LogType type)
            {
                if (type == LogType.Warning) warnings++;
            }

            var label = new Label();
            Application.logMessageReceived += OnLog;
            try
            {
                label.SetTextSelf(42);
                Assert.AreEqual("42", label.text);

                label.SetTextSelf(123456789012L);
                Assert.AreEqual("123456789012", label.text);

                label.SetTextSelf(ulong.MaxValue);
                Assert.AreEqual("18446744073709551615", label.text);

                label.SetTextSelf(2.6f, "F0");
                Assert.AreEqual("3", label.text);

                label.SetTextSelf("abc".AsSpan());
                Assert.AreEqual("abc", label.text);

                label.SetTextSelf(new StringBuilder("hello"));
                Assert.AreEqual("hello", label.text);

                label.SetTextSelf(new[] { 'a', 'b', 'c', 'd' }, 1, 2);
                Assert.AreEqual("bc", label.text);
            }
            finally
            {
                Application.logMessageReceived -= OnLog;
            }

            Assert.AreEqual(0, warnings);
        }

        [UnityTest]
        public IEnumerator SetTextSelf_CharacterSources_SetText()
        {
            using var panel = new TestPanel();
            var label = new Label();
            panel.Root.Add(label);
            yield return null;

            label.SetTextSelf("abc".AsSpan());
            Assert.AreEqual("abc", label.text);

            label.SetTextSelf(new StringBuilder("hello"));
            Assert.AreEqual("hello", label.text);

            label.SetTextSelf(new[] { 'a', 'b', 'c', 'd' }, 1, 2);
            Assert.AreEqual("bc", label.text);
        }
#endif
    }
}
