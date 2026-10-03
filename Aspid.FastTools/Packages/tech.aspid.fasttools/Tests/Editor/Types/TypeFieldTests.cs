using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Types.Editors.Tests
{
    internal sealed class TypeFieldTestObject : ScriptableObject
    {
        public string typeName;
    }

    internal sealed class TypeFieldTests
    {
        private const string MissingName = "Missing.Namespace.GoneType, Missing.Assembly";
        private const string MissingTextClass = "aspid-fasttools-type-field__text--missing";
        private const string OpenButtonClass = "aspid-fasttools-type-field__open-button";

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

        [TestCase(false)]
        [TestCase(true)]
        public void ClearingMissing_NotifiesOnce(bool usePicker)
        {
            var field = AddField();
            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);
            var changes = 0;
            field.RegisterValueChangedCallback(evt =>
            {
                Assert.IsNull(evt.previousValue);
                Assert.IsNull(evt.newValue);
                changes++;
            });

            if (usePicker) field.ApplyPicked(assemblyQualifiedName: null);
            else ((BaseField<Type>)field).value = null;

            field.value = null;
            Assert.AreEqual(1, changes);
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, Caption(field).text);
            Assert.IsFalse(Caption(field).ClassListContains(MissingTextClass));
        }

        [Test]
        public void ClearingMissingWithoutNotify_DoesNotSendAChange()
        {
            var field = AddField();
            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);
            var changes = 0;
            field.RegisterValueChangedCallback(_ => changes++);

            field.SetValueWithoutNotify(newValue: null);

            Assert.AreEqual(0, changes);
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, Caption(field).text);
        }

        [Test]
        public void ClearingMissingWithoutAPanel_DoesNotSendAChange()
        {
            var field = new TypeField();
            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);
            var changes = 0;
            field.RegisterValueChangedCallback(_ => changes++);

            field.value = null;

            Assert.AreEqual(0, changes);
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, Caption(field).text);
        }

        [Test]
        public void MixedValue_HidesTheTypeAndRestoresMissingWhenCleared()
        {
            var field = AddField();
            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);

            field.showMixedValue = true;
            field.SetValueWithoutNotify(newValue: typeof(string));

            Assert.AreEqual("—", Caption(field).text);
            Assert.IsFalse(Caption(field).ClassListContains(MissingTextClass));
            Assert.AreEqual(DisplayStyle.None, field.Q<Button>(className: OpenButtonClass).style.display.value);

            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);
            Assert.AreEqual("—", Caption(field).text);
            field.showMixedValue = false;

            Assert.IsTrue(Caption(field).text.Contains(MissingName));
            Assert.IsTrue(Caption(field).ClassListContains(MissingTextClass));
        }

        [Test]
        public void ChoosingNoneInMixedState_NotifiesAndClearsTheMixedState()
        {
            var field = AddField();
            field.showMixedValue = true;
            var changes = 0;
            field.RegisterValueChangedCallback(_ => changes++);

            field.ApplyPicked(assemblyQualifiedName: null);

            Assert.AreEqual(1, changes);
            Assert.IsFalse(field.showMixedValue);
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, Caption(field).text);
        }

        [UnityTest]
        public IEnumerator BoundField_TracksMixedChangesWhenTheFirstTargetStaysTheSame()
        {
            var first = ScriptableObject.CreateInstance<TypeFieldTestObject>();
            var second = ScriptableObject.CreateInstance<TypeFieldTestObject>();
            first.typeName = typeof(string).AssemblyQualifiedName;
            second.typeName = typeof(int).AssemblyQualifiedName;
            using var serialized = new SerializedObject(new Object[] { first, second });
            try
            {
                var field = new TypeField(property: serialized.FindProperty(nameof(TypeFieldTestObject.typeName)));
                _window.rootVisualElement.Add(field);
                Assert.IsTrue(field.showMixedValue);
                Assert.AreEqual("—", Caption(field).text);
                yield return null;
                yield return null;
                yield return null;

                using (var secondSerialized = new SerializedObject(second))
                {
                    secondSerialized.FindProperty(nameof(TypeFieldTestObject.typeName)).stringValue = first.typeName;
                    secondSerialized.ApplyModifiedPropertiesWithoutUndo();
                }

                var deadline = EditorApplication.timeSinceStartup + 2;
                while (field.showMixedValue && EditorApplication.timeSinceStartup < deadline)
                    yield return null;

                Assert.IsFalse(field.showMixedValue);
                Assert.AreEqual(typeof(string), field.value);
                Assert.AreEqual(nameof(String), Caption(field).text);

                using (var secondSerialized = new SerializedObject(second))
                {
                    secondSerialized.FindProperty(nameof(TypeFieldTestObject.typeName)).stringValue = typeof(int).AssemblyQualifiedName;
                    secondSerialized.ApplyModifiedPropertiesWithoutUndo();
                }

                deadline = EditorApplication.timeSinceStartup + 2;
                while (!field.showMixedValue && EditorApplication.timeSinceStartup < deadline)
                    yield return null;
                Assert.IsTrue(field.showMixedValue);

                field.ApplyPicked(assemblyQualifiedName: typeof(string).AssemblyQualifiedName);
                Assert.IsFalse(field.showMixedValue);
                Assert.AreEqual(first.typeName, second.typeName);
            }
            finally
            {
                _window.rootVisualElement.Clear();
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [UnityTest]
        public IEnumerator NavigationSubmit_OpensTheSelector()
        {
            var field = AddField();
            field.Types = new[] { typeof(TypeFieldTestObject) };
            field.showMixedValue = true;
            yield return null;

            try
            {
                Submit(field);
                yield return null;

                var selectors = Resources.FindObjectsOfTypeAll<TypeSelectorWindow>();
                Assert.AreEqual(1, selectors.Length);
                Assert.AreEqual(-1, selectors[0].rootVisualElement.Q<ListView>().selectedIndex);
            }
            finally
            {
                foreach (var selector in Resources.FindObjectsOfTypeAll<TypeSelectorWindow>())
                    selector.Close();
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NavigationSubmit_ReadOnlyOrDisabled_DoesNotOpenTheSelector(bool readOnly)
        {
            var field = AddField();
            if (readOnly) field.IsReadOnly = true;
            else field.SetEnabled(false);

            Submit(field);

            Assert.IsEmpty(Resources.FindObjectsOfTypeAll<TypeSelectorWindow>());
        }

        private TypeField AddField()
        {
            var field = new TypeField();
            _window.rootVisualElement.Add(field);
            return field;
        }

        private static TextElement Caption(TypeField field) => field.Q<TextElement>(className: EnumField.textUssClassName);

        private static void Submit(TypeField field)
        {
            var input = field.Q<VisualElement>(className: EnumField.inputUssClassName);
            input.Focus();
            using var evt = NavigationSubmitEvent.GetPooled();
            evt.target = input;
            input.SendEvent(evt);
        }
    }
}
