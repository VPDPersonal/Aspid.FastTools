using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.VisualElements
{
    // A shown window: only its panel ties an element to the window, and -nographics does not show windows.
    [TestFixture]
    [Category("Graphics")]
    internal sealed class VisualElementOwnerWindowTests
    {
        private EditorWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) _window.Close();
        }

        [UnityTest]
        public IEnumerator GetOwnerWindow_ReturnsTheWindowThatHostsTheElement()
        {
            var element = new VisualElement();
            _window.rootVisualElement.Add(new VisualElement().AddChild(element));
            yield return null;

            Assert.AreSame(_window, element.GetOwnerWindow());
        }
    }
}
