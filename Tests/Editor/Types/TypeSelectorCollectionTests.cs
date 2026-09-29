using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Aspid.FastTools.SerializeReferences.Editors;

namespace Aspid.FastTools.Types.Editors.Tests
{
    internal interface ICollectionTestItem { }

    [Serializable]
    internal sealed class CollectionTestItem : ICollectionTestItem { }

    internal sealed class TypeSelectorCollectionTestObject : ScriptableObject
    {
        [TypeSelector] public List<string> typeNames = new();
        [TypeSelector] public SerializableType[] types = Array.Empty<SerializableType>();
        [TypeSelector] [SerializeReference] public List<ICollectionTestItem> items = new();
    }

    // [TypeSelector] applies to the collection, so Unity hands its drawer the list instead of each element; the drawer
    // must then draw every element with the picker itself, in UI Toolkit and in IMGUI.
    [TestFixture]
    internal sealed class TypeSelectorCollectionTests
    {
        private TypeSelectorCollectionTestObject _target;
        private EditorWindow _window;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<TypeSelectorCollectionTestObject>();
            _target.typeNames.Add(string.Empty);
            _target.types = new[] { new SerializableType(type: null) };

            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);
            if (_target) Object.DestroyImmediate(_target);
        }

        [UnityTest]
        public IEnumerator DefaultInspector_TypeNameAndWrapperLists_DrawEachElementWithThePicker()
        {
            _window.rootVisualElement.Add(new InspectorElement(_target));

            yield return null;

            var lists = new[] { nameof(TypeSelectorCollectionTestObject.typeNames), nameof(TypeSelectorCollectionTestObject.types) }
                .Select(bindingPath => (bindingPath, listView: FindList(bindingPath)))
                .ToArray();

            // A list starts collapsed, as Unity's own does, and builds its rows only once expanded.
            foreach (var (_, listView) in lists)
                listView.Q<Foldout>().value = true;

            yield return null;

            foreach (var (bindingPath, listView) in lists)
            {
                Assert.IsNotNull(listView.Q<InspectorTypeField>(),
                    $"Each element of '{bindingPath}' must keep the type picker once the attribute reaches only the list.");
            }
        }

        [UnityTest]
        public IEnumerator IMGUI_ManagedReferenceList_IsDrawnByThePickerList()
        {
            var serializedObject = new SerializedObject(_target);
            var items = serializedObject.FindProperty(nameof(TypeSelectorCollectionTestObject.items));

            var drawn = false;
            var height = 0f;
            var expected = 0f;

            _window.rootVisualElement.Add(new IMGUIContainer(() =>
            {
                height = EditorGUI.GetPropertyHeight(items);
                EditorGUI.PropertyField(new Rect(0f, 0f, 300f, height), items);

                expected = SerializeReferenceIMGUIList.GetHeight(
                    listProperty: items,
                    label: new GUIContent(items.displayName),
                    elementType: typeof(ICollectionTestItem),
                    baseTypes: Array.Empty<Type>(),
                    depth: 0);
                drawn = true;
            }));

            for (var frame = 0; frame < 5 && !drawn; frame++)
            {
                _window.Repaint();
                yield return null;
            }

            // The container must not paint again once the SerializedObject is gone.
            _window.rootVisualElement.Clear();
            serializedObject.Dispose();

            Assert.IsTrue(drawn, "Precondition: the IMGUI container must be painted.");
            Assert.AreEqual(expected, height,
                "In IMGUI the drawer must draw the managed-reference list with the picker-backed add of SerializeReferenceIMGUIList.");
        }

        private ListView FindList(string bindingPath)
        {
            var listView = _window.rootVisualElement.Query<ListView>().Where(view => view.bindingPath == bindingPath).First();
            Assert.IsNotNull(listView, $"The inspector must draw a list bound to '{bindingPath}'.");
            return listView;
        }
    }
}
