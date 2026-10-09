using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
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
        private string _recentsJson;

        [SetUp]
        public void SetUp()
        {
            // Choosing a type records a recent pick; keep the developer's real list.
            _recentsJson = EditorPrefs.GetString(TypeSelectorPreferences.RecentsKey, string.Empty);

            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);

            TypeSelectorPreferences.ClearRecents();
            if (!string.IsNullOrEmpty(_recentsJson))
                EditorPrefs.SetString(TypeSelectorPreferences.RecentsKey, _recentsJson);
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
        public IEnumerator EnterInTheSearchField_ChoosesTheFirstResult()
        {
            string chosen = null;
            var view = AddView(string.Empty, aqn => chosen = aqn);
            yield return null;

            var search = view.Q<ToolbarSearchField>();
            search.value = "FirstViewProbe";
            yield return null;

            search.Focus();
            yield return null;

            SendKey(search, '\n', KeyCode.Return);

            Assert.AreEqual(typeof(FirstViewProbe).AssemblyQualifiedName, chosen,
                "Enter in the search field must choose the first result.");
        }

        [UnityTest]
        public IEnumerator EnterInTheSearchField_WithoutResults_ChoosesNothing()
        {
            var chosen = false;
            var view = AddView(string.Empty, _ => chosen = true);
            yield return null;

            var search = view.Q<ToolbarSearchField>();
            search.value = "NoSuchViewProbe";
            yield return null;

            search.Focus();
            yield return null;

            SendKey(search, '\n', KeyCode.Return);

            Assert.IsFalse(chosen, "Enter with no result must not pick a type, nor <None>.");
        }

        [UnityTest]
        public IEnumerator LeftArrowInTheResults_KeepsTheLevelTheSearchStartedIn()
        {
            var view = AddView(string.Empty);
            yield return null;

            var list = view.Q<ListView>();
            var items = (List<TreeNode>)list.itemsSource;
            var folder = items.FindIndex(node => node.HasChildren && !node.IsType && !node.IsSectionTitle);
            Assert.GreaterOrEqual(folder, 0, "The root must list the probes' namespace as a folder.");

            list.selectedIndex = folder;
            SendKey(list, '\0', KeyCode.RightArrow);
            var crumbsInside = Crumbs(view).Count;
            Assert.Greater(crumbsInside, 1, "Sanity: the picker must be inside the folder.");

            var search = view.Q<ToolbarSearchField>();
            search.value = "ViewProbe";
            yield return null;

            list.selectedIndex = 0;
            SendKey(list, '\0', KeyCode.LeftArrow);

            search.value = string.Empty;
            Assert.AreEqual(crumbsInside, Crumbs(view).Count,
                "Left arrow in the results must not take the picker up a level.");
        }

        [UnityTest]
        public IEnumerator NarrowingTheResultsPastTheSelectedRow_DoesNotRecurse()
        {
            var view = AddView(string.Empty);
            yield return null;

            var search = view.Q<ToolbarSearchField>();
            var list = view.Q<ListView>();

            search.value = "ViewProbe";
            var last = list.itemsSource.Count - 1;
            Assert.Greater(last, 0, "The query must list both probes.");
            list.selectedIndex = last;

            // From Unity 6000.6 every refresh of a list that ends above the selected index reports a selection change.
            search.value = "FirstViewProbe";

            Assert.LessOrEqual(list.itemsSource.Count, last,
                "The narrower query must end the list above the selected row.");
        }

        [UnityTest]
        public IEnumerator SelectingAFolder_ShowsItsOpenedIconUntilTheSelectionMovesOn()
        {
            var view = AddView(string.Empty);
            yield return null;

            var list = view.Q<ListView>();
            var items = (List<TreeNode>)list.itemsSource;
            var none = items.FindIndex(node => node.IsNoneOption);
            var folder = items.FindIndex(node => node.HasChildren && !node.IsType && !node.IsSectionTitle);
            Assert.GreaterOrEqual(folder, 0, "The root must list the probes' namespace as a folder.");

            list.selectedIndex = folder;
            Assert.AreEqual(TypeSelectorIconResolver.Resolve("FolderOpened Icon"), RowIcon(list, folder));

            list.selectedIndex = none;
            Assert.AreEqual(TypeSelectorIconResolver.Resolve("Folder Icon"), RowIcon(list, folder));
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

        private TypeSelectorView AddView(string currentAqn, Action<string> onSelected = null)
        {
            var view = new TypeSelectorView(
                new TypeSelectorFilter { Types = new[] { typeof(IViewProbe) } },
                currentAqn,
                onSelected);

            _window.rootVisualElement.Add(view);
            return view;
        }

        private static List<Label> Crumbs(TypeSelectorView view) =>
            view.Query<Label>(className: "aspid-fasttools-type-selector__breadcrumb").ToList();

        private static Texture RowIcon(ListView list, int index)
        {
            var node = list.itemsSource[index];
            var row = list.Query<VisualElement>(className: "aspid-fasttools-type-selector__item")
                .Where(element => element.userData == node)
                .First();

            return row.Q<Image>(className: "aspid-fasttools-type-selector__item-icon").image;
        }

        private static void SendKey(VisualElement target, char character, KeyCode key)
        {
            using var evt = KeyDownEvent.GetPooled(character, key, EventModifiers.None);
            evt.target = target;
            target.SendEvent(evt);
        }
    }
}
