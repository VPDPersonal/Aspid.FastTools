using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using Aspid.FastTools.UIElements;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The field's stylesheet sits on its root, and child PropertyFields are built inside it, so its rules must only
    // match the field's own header and open button — never a button or foldout of a nested drawer or list.
    // The tree mirrors SerializeReferenceField (root > Foldout > toggle + content), so only the stylesheet is under
    // test, without the field's binding and layout.
    [TestFixture]
    internal sealed class SerializeReferenceFieldStyleScopeTests
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";
        private const string BlockClass = "aspid-fasttools-serialize-reference";
        private const string OpenButtonClass = BlockClass + "__open-button";

        private EditorWindow _window;
        private Foldout _header;
        private Button _openButton;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();

            _header = new Foldout();
            _openButton = new Button().AddClass(OpenButtonClass).AddChild(new VisualElement());
            _header.Q<Toggle>().Add(_openButton);

            var field = new VisualElement()
                .AddClass(BlockClass)
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddChild(_header);

            _window.rootVisualElement.Add(field);
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);
        }

        [UnityTest]
        public IEnumerator OpenButton_KeepsItsCompactIconStyle()
        {
            yield return null;

            Assert.AreEqual(18f, _openButton.resolvedStyle.maxWidth.value);
            Assert.AreEqual(18f, _openButton.resolvedStyle.maxHeight.value);
        }

        [UnityTest]
        public IEnumerator NestedButton_IsNotStyledAsTheOpenButton()
        {
            var icon = new VisualElement();
            var nested = new Button { text = "Custom drawer action" };
            nested.Add(icon);
            _header.Add(nested);

            yield return null;

            Assert.AreNotEqual(18f, nested.resolvedStyle.maxWidth.value,
                "A button of a nested drawer must not be clamped to the open button's 18 px.");
            Assert.AreNotEqual(18f, nested.resolvedStyle.maxHeight.value,
                "A button of a nested drawer must not be clamped to the open button's 18 px.");
            Assert.IsNull(icon.resolvedStyle.backgroundImage.texture,
                "A child of a nested button must not get the folder icon.");
        }

        [UnityTest]
        public IEnumerator NestedFoldout_KeepsItsOwnHeaderLayout()
        {
            var reference = new Foldout();
            _window.rootVisualElement.Add(reference);

            var nested = new Foldout();
            _header.Add(nested);

            yield return null;

            var referenceInput = reference.Q(className: Foldout.inputUssClassName);
            var nestedInput = nested.Q(className: Foldout.inputUssClassName);
            var headerInput = _header.Q(className: Foldout.inputUssClassName);

            Assert.AreEqual(0f, headerInput.resolvedStyle.flexGrow, "Precondition: the header rules must apply.");
            Assert.AreEqual(referenceInput.resolvedStyle.flexGrow, nestedInput.resolvedStyle.flexGrow,
                "The field's header rules must not reach a nested foldout.");
            Assert.AreEqual(referenceInput.resolvedStyle.flexShrink, nestedInput.resolvedStyle.flexShrink,
                "The field's header rules must not reach a nested foldout.");
        }
    }
}
