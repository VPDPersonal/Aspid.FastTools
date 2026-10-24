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
        private EditorWindow _other;
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
            if (_other) _other.Close();
            if (_window) _window.Close();
        }

        // Another window has the focus, so only the panel can lead to the window that hosts the element: the focused
        // window is the fallback when the panel matches no window.
        [UnityTest]
        public IEnumerator GetOwnerWindow_ReturnsTheWindowThatHostsTheElement()
        {
            var element = new VisualElement();
            _window.rootVisualElement.Add(new VisualElement().AddChild(element));

            _other = ScriptableObject.CreateInstance<EditorWindow>();
            _other.ShowUtility();
            _other.Focus();
            yield return null;

            Assert.AreNotSame(_window, EditorWindow.focusedWindow, "Precondition: the host window must not have the focus.");
            Assert.AreSame(_window, element.GetOwnerWindow());
        }
    }
}
