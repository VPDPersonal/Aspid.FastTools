using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // The field's stylesheet must size only its own open-in-script-editor button, not every button below it.
    [TestFixture]
    internal sealed class TypeFieldStyleScopeTests
    {
        private const string OpenButtonClass = "aspid-fasttools-type-field__open-button";

        private EditorWindow _window;
        private TypeField _field;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();

            _field = new TypeField("Type", typeof(TypeFieldStyleScopeTests));
            _window.rootVisualElement.Add(_field);
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

            var openButton = _field.Q<Button>(className: OpenButtonClass);
            Assert.IsNotNull(openButton, "The field's open-in-script-editor button must carry its own class.");
            Assert.AreEqual(18f, openButton.resolvedStyle.maxWidth.value);
            Assert.AreEqual(18f, openButton.resolvedStyle.maxHeight.value);
        }

        [UnityTest]
        public IEnumerator OtherButton_IsNotStyledAsTheOpenButton()
        {
            var icon = new VisualElement();
            var other = new Button { text = "Other action" };
            other.Add(icon);
            _field.Add(other);

            yield return null;

            Assert.AreNotEqual(18f, other.resolvedStyle.maxWidth.value,
                "Only the open button may be clamped to 18 px.");
            Assert.IsNull(icon.resolvedStyle.backgroundImage.texture,
                "A child of another button must not get the folder icon.");
        }
    }
}
