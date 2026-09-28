using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // Keyboard and preselection behaviour of the picker view and the missing-type handling of TypeField, driven in
    // a live panel because list focus and change events need one.
    internal sealed class TypeSelectorViewTests
    {
        private const string MissingName = "Missing.Namespace.GoneType, Missing.Assembly";

        private interface IViewProbe { }

        private sealed class FirstViewProbe : IViewProbe { }

        private sealed class SecondViewProbe : IViewProbe { }

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
            if (_window) Object.DestroyImmediate(_window);
        }

        [UnityTest]
        public IEnumerator MissingCurrentValue_PreselectsNothing()
        {
            var view = AddView(MissingName);
            yield return null;

            Assert.AreEqual(-1, view.Q<ListView>().selectedIndex,
                "A stored name that is not in the list must not highlight <None>, or Enter would erase it.");
        }

        [UnityTest]
        public IEnumerator EmptyCurrentValue_PreselectsNone()
        {
            var view = AddView(string.Empty);
            yield return null;

            var list = view.Q<ListView>();
            Assert.GreaterOrEqual(list.selectedIndex, 0);
            Assert.IsTrue(((TreeNode)list.selectedItem).IsNoneOption);
        }

        [UnityTest]
        public IEnumerator TypingWhileTheResultsAreFocused_EditsTheQuery()
        {
            var view = AddView(string.Empty);
            yield return null;

            var search = view.Q<ToolbarSearchField>();
            var list = view.Q<ListView>();

            search.value = "ViewProbe";
            list.Focus();
            list.selectedIndex = 0;
            yield return null;

            SendKey(list, 'X', KeyCode.None);
            Assert.AreEqual("ViewProbeX", search.value, "A printable key on the results must reach the query.");

            // The first key hands focus back to the field on the next frame; move it to the results again after that.
            yield return null;
            list.Focus();

            SendKey(list, '\0', KeyCode.Backspace);
            Assert.AreEqual("ViewProbe", search.value, "Backspace on the results must delete from the query.");
        }

        [UnityTest]
        public IEnumerator TypeField_PickingNoneForAMissingType_RaisesTheChange()
        {
            var field = new TypeField();
            _window.rootVisualElement.Add(field);
            field.SetValueFromAssemblyQualifiedNameWithoutNotify(MissingName);
            yield return null;

            var changes = 0;
            field.RegisterValueChangedCallback(_ => changes++);

            field.ApplyPicked(null);

            Assert.AreEqual(1, changes, "Clearing a missing type must notify an unbound owner.");
            Assert.IsNull(field.value);
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, field.Q<TextElement>(className: EnumField.textUssClassName).text);
        }

        private TypeSelectorView AddView(string currentAqn)
        {
            var view = new TypeSelectorView(
                new TypeSelectorFilter { Types = new[] { typeof(IViewProbe) } },
                currentAqn);

            _window.rootVisualElement.Add(view);
            return view;
        }

        private static void SendKey(VisualElement target, char character, KeyCode key)
        {
            using var evt = KeyDownEvent.GetPooled(character, key, EventModifiers.None);
            evt.target = target;
            target.SendEvent(evt);
        }
    }
}
