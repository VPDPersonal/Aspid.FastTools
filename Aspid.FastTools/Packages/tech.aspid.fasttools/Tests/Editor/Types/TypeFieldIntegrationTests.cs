using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Types.Tests;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.Types.Editors.Tests
{
    internal sealed class TypeFieldWrapperTestObject : ScriptableObject
    {
        public SerializableType<ScriptableObject> scriptType;
    }

    internal sealed class TypeFieldHandlerTestObject : ScriptableObject
    {
        public string typeName;
        public int count;
    }

    // TypeField as consumers use it: from UXML, inside a PropertyField, on a prefab instance, through its change event
    // and its picker filter. A runtime panel dispatches the events without a graphics device.
    internal sealed class TypeFieldIntegrationTests
    {
        private const string MissingName = "Missing.Namespace.GoneType, Missing.Assembly";
        private const string MissingCaption = "<Missing Missing.Namespace.GoneType>";
        private const string UxmlPath = "Assets/__AspidTypeFieldTest__.uxml";

        // A prefab asset's root takes the file name, and the Apply item names that root.
        private const string PrefabName = "__AspidTypeFieldTestPrefab__";
        private const string PrefabPath = "Assets/" + PrefabName + ".prefab";

        private TestPanel _panel;

        [SetUp]
        public void SetUp() => _panel = new TestPanel();

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();
            AssetDatabase.DeleteAsset(UxmlPath);
            AssetDatabase.DeleteAsset(PrefabPath);
        }

        [Test]
        public void Constructor_NullProperty_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ = new TypeField(property: null));
            Assert.Throws<ArgumentNullException>(() => _ = new TypeField(label: "Type", property: null));
        }

        [Test]
        public void Constructor_NonStringProperty_Throws()
        {
            var target = ScriptableObject.CreateInstance<TypeFieldTestObject>();
            try
            {
                using var serialized = new SerializedObject(target);
                var exception = Assert.Throws<ArgumentException>(() => _ = new TypeField(serialized.FindProperty("m_Script")));
                Assert.AreEqual("property", exception.ParamName);
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void Types_DropsNullEntries_SoThePickerOpens()
        {
            var field = new TypeField { Types = new[] { null, typeof(ScriptableObject), null } };

            CollectionAssert.AreEqual(new[] { typeof(ScriptableObject) }, field.Types);
            Assert.DoesNotThrow(() => _ = new TypeSelectorView(field.CreateFilter()));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CreateFilter_CarriesTheFieldSettings(bool repair)
        {
            Func<Type, bool> predicate = type => type != typeof(TypeFieldTestObject);
            var field = new TypeField
            {
                Types = new[] { typeof(ScriptableObject) },
                Allow = TypeAllow.Abstract,
                Predicate = predicate,
                ExcludeEditorOnlyTypes = true,
            };

            var filter = field.CreateFilter(repair);

            CollectionAssert.AreEqual(field.Types, filter.Types);
            Assert.AreEqual(TypeAllow.Abstract, filter.Allow);
            Assert.AreSame(predicate, filter.Predicate);
            Assert.IsNull(filter.AdditionalTypes, "Without a closed generic base type the scan finds every candidate.");
            Assert.AreEqual(repair, filter.HideNoneOption, "Repair hides <None>, as the field's own option would.");
            Assert.IsTrue(filter.ExcludeEditorOnly);
        }

        [Test]
        public void ClosedGenericBaseType_OffersTheGenericClassesThatCloseToIt()
        {
            var field = new TypeField { Types = new[] { typeof(IResolverConverter<string, string>) } };

            CollectionAssert.Contains(field.CreateFilter().AdditionalTypes.ToArray(), typeof(ResolverSequence<string>));

            field.Predicate = type => type != typeof(ResolverSequence<string>);
            CollectionAssert.DoesNotContain(field.CreateFilter().AdditionalTypes.ToArray(), typeof(ResolverSequence<string>),
                "The predicate applies to these classes too.");
        }

        [Test]
        public void AssemblyQualifiedName_KeepsTheStoredNameOfAMissingType()
        {
            var field = new TypeField();
            Assert.AreEqual(string.Empty, field.AssemblyQualifiedName);

            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);
            Assert.IsNull(field.value);
            Assert.AreEqual(MissingName, field.AssemblyQualifiedName);

            field.SetValueWithoutNotify(newValue: typeof(string));
            Assert.AreEqual(typeof(string).AssemblyQualifiedName, field.AssemblyQualifiedName);
        }

        [Test]
        public void Tooltip_ShowsTheFullTypeName_OrTheStoredNameOfAMissingType()
        {
            var field = Add(new TypeField());
            var input = field.Q<VisualElement>(className: EnumField.inputUssClassName);
            Assert.IsTrue(string.IsNullOrEmpty(input.tooltip));

            field.SetValueWithoutNotify(newValue: typeof(string));
            Assert.AreEqual(TypeSelectorHelpers.GetTypeSelectorTooltip(typeof(string)), input.tooltip);

            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);
            Assert.AreEqual($"Missing type: {MissingName}", input.tooltip);
            Assert.AreEqual(MissingCaption, Caption(field).text, "The caption drops the assembly; the tooltip keeps it.");

            field.showMixedValue = true;
            Assert.IsTrue(string.IsNullOrEmpty(input.tooltip));
            field.showMixedValue = false;
            Assert.AreEqual($"Missing type: {MissingName}", input.tooltip);

            field.SetValueWithoutNotify(newValue: null);
            Assert.IsTrue(string.IsNullOrEmpty(input.tooltip));
        }

        [Test]
        public void Pick_SendsTheChangeEventWithThePickedType()
        {
            var field = Add(new TypeField());
            var changes = new List<(Type previous, Type next)>();
            field.RegisterValueChangedCallback(evt => changes.Add((evt.previousValue, evt.newValue)));

            field.ApplyPicked(assemblyQualifiedName: typeof(string).AssemblyQualifiedName);

            CollectionAssert.AreEqual(new[] { ((Type)null, typeof(string)) }, changes);
        }

        // The picker window picks inside its own event, so the panel queues ChangeEvent until the pick returns.
        // A pick from another element's change event gives the same order here.
        [TestCase(false)]
        [TestCase(true)]
        public void BoundPick_ChangeHandlerReadsThePickedName_AndKeepsItsOwnEdits(bool insideAnotherEvent)
        {
            var picked = typeof(string).AssemblyQualifiedName;
            var target = ScriptableObject.CreateInstance<TypeFieldHandlerTestObject>();
            target.typeName = typeof(int).AssemblyQualifiedName;
            target.count = 1;
            try
            {
                using var serialized = new SerializedObject(target);
                var typeName = serialized.FindProperty(nameof(TypeFieldHandlerTestObject.typeName));
                var field = Add(new TypeField(property: typeName));

                string readByHandler = null;
                field.RegisterValueChangedCallback(_ =>
                {
                    readByHandler = typeName.stringValue;
                    serialized.FindProperty(nameof(TypeFieldHandlerTestObject.count)).intValue = 5;
                    serialized.ApplyModifiedProperties();
                });

                if (insideAnotherEvent)
                {
                    var trigger = Add(new Toggle());
                    trigger.RegisterValueChangedCallback(_ => field.ApplyPicked(assemblyQualifiedName: picked));
                    trigger.value = true;
                }
                else
                {
                    field.ApplyPicked(assemblyQualifiedName: picked);
                }

                Assert.AreEqual(picked, readByHandler, "The handler reads the pick from the caller's own property.");
                Assert.AreEqual(picked, target.typeName, "The handler's apply does not write the old name back.");
                Assert.AreEqual(5, target.count);
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void BoundPick_AfterTheCallersSerializedObjectIsDisposed_StillWritesTheProperty()
        {
            var target = ScriptableObject.CreateInstance<TypeFieldTestObject>();
            try
            {
                TypeField field;
                using (var serialized = new SerializedObject(target))
                    field = Add(new TypeField(property: serialized.FindProperty(nameof(TypeFieldTestObject.typeName))));

                field.ApplyPicked(assemblyQualifiedName: typeof(string).AssemblyQualifiedName);

                Assert.AreEqual(typeof(string).AssemblyQualifiedName, target.typeName);
                Assert.AreEqual(typeof(string), field.value);
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void Uxml_CreatesUnboundFieldsWithTheirAttributes()
        {
            File.WriteAllText(UxmlPath,
                "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:aft=\"Aspid.FastTools.Types.Editors\">\n" +
                "    <aft:TypeField label=\"Weapon\" allow=\"Interface\" hide-none-option=\"true\" is-read-only=\"true\" exclude-editor-only-types=\"true\" />\n" +
                "    <aft:InspectorTypeField label=\"Spare\" />\n" +
                "</ui:UXML>\n");
            AssetDatabase.ImportAsset(UxmlPath, ImportAssetOptions.ForceSynchronousImport);

            var fields = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath).Instantiate().Query<TypeField>().ToList();

            Assert.AreEqual(2, fields.Count);
            Assert.AreEqual("Weapon", fields[0].label);
            Assert.AreEqual(TypeAllow.Interface, fields[0].Allow);
            Assert.IsTrue(fields[0].HideNoneOption);
            Assert.IsTrue(fields[0].IsReadOnly);
            Assert.IsTrue(fields[0].ExcludeEditorOnlyTypes);
            Assert.IsNull(fields[0].value);
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, Caption(fields[0]).text);

            Assert.IsInstanceOf<InspectorTypeField>(fields[1]);
            Assert.AreEqual("Spare", fields[1].label);
            Assert.IsTrue(fields[1].ClassListContains(PropertyField.ussClassName));
        }

        [Test]
        public void BindingPath_LogsAWarningOnAttach()
        {
            LogAssert.Expect(LogType.Warning, new Regex("^TypeField ignores binding-path \"typeName\""));

            Add(new TypeField { bindingPath = "typeName" });
        }

        [UnityTest]
        public IEnumerator PropertyField_DrawsASerializableTypeWithABoundTypeField()
        {
            var target = ScriptableObject.CreateInstance<TypeFieldWrapperTestObject>();
            target.scriptType = new SerializableType<ScriptableObject>(typeof(TypeFieldWrapperTestObject));
            var serialized = new SerializedObject(target);
            try
            {
                var propertyField = Add(new PropertyField(serialized.FindProperty(nameof(TypeFieldWrapperTestObject.scriptType))));
                propertyField.Bind(serialized);

                var deadline = EditorApplication.timeSinceStartup + 2;
                while (propertyField.Q<TypeField>() is null && EditorApplication.timeSinceStartup < deadline)
                    yield return null;

                var field = propertyField.Q<TypeField>();
                Assert.IsNotNull(field, "The SerializableType drawer builds its row from a TypeField.");
                Assert.AreEqual(typeof(TypeFieldWrapperTestObject), field.value);
                CollectionAssert.AreEqual(new[] { typeof(ScriptableObject) }, field.Types);

                field.ApplyPicked(assemblyQualifiedName: typeof(TypeFieldTestObject).AssemblyQualifiedName);

                Assert.AreEqual(typeof(TypeFieldTestObject), target.scriptType.Type);
            }
            finally
            {
                serialized.Dispose();
                Object.DestroyImmediate(target);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PrefabInstanceOverride_IsMarked_AndCanBeRevertedOrApplied(bool apply)
        {
            var root = new GameObject(PrefabName);
            root.AddComponent<TypeFieldPrefabProbe>().typeName = typeof(string).AssemblyQualifiedName;
            var asset = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            try
            {
                using var serialized = new SerializedObject(instance.GetComponent<TypeFieldPrefabProbe>());
                var field = new TypeField(property: serialized.FindProperty(nameof(TypeFieldPrefabProbe.typeName)));
                Assert.IsTrue(field.ExcludeEditorOnlyTypes, "The probe lives in a runtime assembly.");
                Assert.IsFalse(IsMarkedAsOverride(field));
                Assert.IsEmpty(GetPrefabActions(field));

                field.ApplyPicked(assemblyQualifiedName: typeof(int).AssemblyQualifiedName);

                Assert.IsTrue(IsMarkedAsOverride(field));
                var actions = GetPrefabActions(field);
                var applyName = $"Apply to Prefab '{PrefabName}'";
                CollectionAssert.AreEqual(new[] { applyName, "Revert" }, actions.Select(action => action.name));

                actions.Single(action => action.name == (apply ? applyName : "Revert")).Execute();

                var expected = apply ? typeof(int) : typeof(string);
                Assert.IsFalse(IsMarkedAsOverride(field));
                Assert.AreEqual(expected, field.value);
                Assert.AreEqual(expected.AssemblyQualifiedName,
                    AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<TypeFieldPrefabProbe>().typeName);
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void MultiObjectPoll_RepaintsOnlyOnChange_AndStopsForADestroyedTarget()
        {
            var first = ScriptableObject.CreateInstance<TypeFieldTestObject>();
            var second = ScriptableObject.CreateInstance<TypeFieldTestObject>();
            first.typeName = typeof(string).AssemblyQualifiedName;
            second.typeName = typeof(string).AssemblyQualifiedName;
            using var serialized = new SerializedObject(new Object[] { first, second });
            try
            {
                var field = new TypeField(property: serialized.FindProperty(nameof(TypeFieldTestObject.typeName)));
                Assert.IsFalse(field.PollProperty(), "Nothing changed since the field was created.");

                second.typeName = typeof(int).AssemblyQualifiedName;
                Assert.IsTrue(field.PollProperty());
                Assert.IsTrue(field.showMixedValue);
                Assert.IsFalse(field.PollProperty());

                Object.DestroyImmediate(second);
                Assert.IsFalse(field.PollProperty());
            }
            finally
            {
                Object.DestroyImmediate(first);
                if (second) Object.DestroyImmediate(second);
            }
        }

        private T Add<T>(T element)
            where T : VisualElement
        {
            _panel.Root.Add(element);
            return element;
        }

        private static TextElement Caption(TypeField field) => field.Q<TextElement>(className: EnumField.textUssClassName);

        private static bool IsMarkedAsOverride(TypeField field) =>
            field.ClassListContains(BindingExtensions.prefabOverrideUssClassName);

        private static List<DropdownMenuAction> GetPrefabActions(TypeField field)
        {
            var menu = new DropdownMenu();
            field.AddPrefabOverrideActions(menu);
            return menu.MenuItems().OfType<DropdownMenuAction>().ToList();
        }
    }
}
