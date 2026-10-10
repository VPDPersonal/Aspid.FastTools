using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // Guards the UI Toolkit type drawer: a pick reports a ChangeEvent<string> to the elements above it, as a
    // PropertyField expects from a string field.
    internal sealed class TypeDrawerChangeEventTests
    {
        private const string MissingName = "Missing.Namespace.GoneType, Missing.Assembly";

        private static readonly string StringName = typeof(string).AssemblyQualifiedName;
        private static readonly string IntName = typeof(int).AssemblyQualifiedName;

        private readonly List<Object> _objects = new();

        private TestPanel _panel;

        [SetUp]
        public void SetUp() => _panel = new TestPanel();

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();

            foreach (var item in _objects)
                if (item) Object.DestroyImmediate(item);

            _objects.Clear();
        }

        [Test]
        public void Pick_ReportsTheNameChangeToTheParent()
        {
            var target = CreateTarget(StringName);
            using var serialized = new SerializedObject(target);
            var changes = new List<(string previous, string current)>();
            var field = Draw(serialized.FindProperty(nameof(TypeFieldTestObject.typeName)), changes);

            field.ApplyPicked(assemblyQualifiedName: IntName);

            CollectionAssert.AreEqual(new[] { (StringName, IntName) }, changes);
            Assert.AreEqual(IntName, target.typeName);
        }

        [Test]
        public void PickingTheStoredType_ReportsNothing()
        {
            var target = CreateTarget(StringName);
            using var serialized = new SerializedObject(target);
            var changes = new List<(string previous, string current)>();
            var field = Draw(serialized.FindProperty(nameof(TypeFieldTestObject.typeName)), changes);

            field.ApplyPicked(assemblyQualifiedName: StringName);

            Assert.IsEmpty(changes);
        }

        [Test]
        public void ChoosingNone_ReportsTheClearedName()
        {
            var target = CreateTarget(StringName);
            using var serialized = new SerializedObject(target);
            var changes = new List<(string previous, string current)>();
            var field = Draw(serialized.FindProperty(nameof(TypeFieldTestObject.typeName)), changes);

            field.ApplyPicked(assemblyQualifiedName: null);

            CollectionAssert.AreEqual(new[] { (StringName, string.Empty) }, changes);
            Assert.AreEqual(string.Empty, target.typeName);
        }

        [Test]
        public void RepairingAMissingType_ReportsTheStoredNameAsThePreviousOne()
        {
            var target = CreateTarget(MissingName);
            using var serialized = new SerializedObject(target);
            var changes = new List<(string previous, string current)>();
            var field = Draw(serialized.FindProperty(nameof(TypeFieldTestObject.typeName)), changes);

            field.ApplyPicked(assemblyQualifiedName: IntName);

            CollectionAssert.AreEqual(new[] { (MissingName, IntName) }, changes);
        }

        [Test]
        public void PickInMixedState_ReportsOnceAndWritesEveryTarget()
        {
            var first = CreateTarget(StringName);
            var second = CreateTarget(IntName);
            using var serialized = new SerializedObject(new Object[] { first, second });
            var changes = new List<(string previous, string current)>();
            var field = Draw(serialized.FindProperty(nameof(TypeFieldTestObject.typeName)), changes);
            Assert.IsTrue(field.showMixedValue, "Precondition: the targets differ.");

            field.ApplyPicked(assemblyQualifiedName: StringName);

            Assert.AreEqual(1, changes.Count);
            Assert.AreEqual(StringName, first.typeName);
            Assert.AreEqual(StringName, second.typeName);
        }

        [Test]
        public void DisplayTextChangeFromInsideTheDrawer_DoesNotReachTheParent()
        {
            var target = CreateTarget(StringName);
            using var serialized = new SerializedObject(target);
            var changes = new List<(string previous, string current)>();
            var field = Draw(serialized.FindProperty(nameof(TypeFieldTestObject.typeName)), changes);
            var caption = field.Q<TextElement>(className: EnumField.textUssClassName);
            Assert.IsNotNull(caption, "Precondition: the field has a caption.");

            using var evt = ChangeEvent<string>.GetPooled(previousValue: "Before", newValue: "After");
            evt.target = caption;
            caption.SendEvent(evt);

            Assert.IsEmpty(changes);
        }

        private TypeFieldTestObject CreateTarget(string typeName)
        {
            var target = ScriptableObject.CreateInstance<TypeFieldTestObject>();
            target.typeName = typeName;
            _objects.Add(target);
            return target;
        }

        // The drawer root sits under a plain parent, as it does under a PropertyField.
        private InspectorTypeField Draw(SerializedProperty property, List<(string previous, string current)> changes)
        {
            var root = TypeUIToolkitPropertyDrawer.Draw(
                label: "Type",
                property: property,
                allow: TypeAllow.All,
                types: new[] { typeof(object) },
                field: out var field);

            var parent = new VisualElement();
            parent.RegisterCallback<ChangeEvent<string>>(evt => changes.Add((evt.previousValue, evt.newValue)));
            parent.Add(root);
            _panel.Root.Add(parent);

            return field;
        }
    }
}
