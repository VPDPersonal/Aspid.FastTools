using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Tests;
using Aspid.FastTools.Editors;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Covers the shared write path of the inspector actions (picker, drop, Paste, Paste Template, Link to Existing):
    // a re-pick of the current type, one instance per selected object, the editor-only filter for runtime objects,
    // the note on a replaced missing list element and the expansion of the inspector's own property.
    [TestFixture]
    internal sealed class SerializeReferenceWriterTests
    {
        private const string MissingElementAssetPath = "Assets/__AspidWriterMissingElementProbe__.asset";

        private static readonly Func<ManagedTypeName, bool> Resolves = type =>
            !type.Class.StartsWith("Ghost", StringComparison.Ordinal);

        [TearDown]
        public void TearDown() => DragAndDrop.PrepareStartDrag();

        [Test]
        public void SetType_CurrentType_KeepsTheInstanceAndItsAliases()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var shared = new TestSword { damage = 3 };
                obj.a = shared;
                obj.b = shared;
                var serialized = new SerializedObject(obj);

                Assert.IsFalse(SerializeReferenceWriter.SetType(serialized.FindProperty("a"), typeof(TestSword)),
                    "Picking the type the field already holds must not write.");
                Assert.AreSame(shared, obj.a, "A re-pick of the current type must keep the instance.");
                Assert.AreSame(obj.a, obj.b, "A re-pick of the current type must not break a shared reference.");
                Assert.AreEqual(3, ((TestSword)obj.a).damage);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void SetType_OtherType_CarriesTheSharedDataAndExpands()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new TestSword { damage = 4 };
                var serialized = new SerializedObject(obj);
                var property = serialized.FindProperty("a");
                property.isExpanded = false;

                Assert.IsTrue(SerializeReferenceWriter.SetType(property, typeof(WriterTestBow)));
                Assert.IsInstanceOf<WriterTestBow>(obj.a);
                Assert.AreEqual(4, ((WriterTestBow)obj.a).damage, "A field both types declare must keep its value.");
                Assert.IsTrue(property.isExpanded, "A new value must open the field.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void SetType_None_ClearsAndCollapses()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new TestSword();
                var serialized = new SerializedObject(obj);
                var property = serialized.FindProperty("a");
                property.isExpanded = true;

                Assert.IsTrue(SerializeReferenceWriter.SetType(property, type: null));
                Assert.IsNull(obj.a);
                Assert.IsFalse(property.isExpanded);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void SetType_ThroughAnotherSerializedObject_ExpandsTheView()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var inspector = new SerializedObject(obj);
                var view = inspector.FindProperty("a");
                view.isExpanded = false;

                Assert.IsTrue(SerializeReferenceWriter.SetType(view.Persistent(), typeof(TestSword), view));
                Assert.IsTrue(inspector.FindProperty("a").isExpanded,
                    "isExpanded is cached per SerializedObject, so the inspector's own one must receive it.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void SetType_MultipleObjects_GivesEachObjectItsOwnInstance()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                first.a = new TestSword { damage = 1 };
                var serialized = new SerializedObject(new Object[] { first, second });

                Assert.IsTrue(SerializeReferenceWriter.SetType(serialized.FindProperty("a"), typeof(WriterTestBow)));
                Assert.IsInstanceOf<WriterTestBow>(first.a);
                Assert.IsInstanceOf<WriterTestBow>(second.a, "Every selected object must receive the type.");
                Assert.AreNotSame(first.a, second.a, "The selected objects must not share one instance.");
                Assert.AreEqual(1, ((WriterTestBow)first.a).damage, "Each object must keep its own data.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void SetType_MultipleObjects_KeepsTheObjectThatHoldsTheType()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var sword = new TestSword { damage = 2 };
                first.a = sword;
                second.a = new WriterTestBow { damage = 5 };
                var serialized = new SerializedObject(new Object[] { first, second });

                Assert.IsTrue(SerializeReferenceWriter.SetType(serialized.FindProperty("a"), typeof(TestSword)));
                Assert.AreSame(sword, first.a, "An object that already holds the type must keep its instance.");
                Assert.AreEqual(5, ((TestSword)second.a).damage, "The other object must get the type with its own data.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void SetType_MultipleObjects_UndoesInOneStep()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var serialized = new SerializedObject(new Object[] { first, second });

                Assert.IsTrue(SerializeReferenceWriter.SetType(serialized.FindProperty("a"), typeof(TestSword)));
                Undo.PerformUndo();

                Assert.IsNull(first.a, "One undo must revert the write on every selected object.");
                Assert.IsNull(second.a, "One undo must revert the write on every selected object.");
            }
            finally
            {
                Undo.ClearUndo(first);
                Undo.ClearUndo(second);
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Paste_MultipleObjects_GivesEachObjectItsOwnCopy()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                SerializeReferenceClipboard.Copy(new TestSword { damage = 9 });
                var serialized = new SerializedObject(new Object[] { first, second });
                var property = serialized.FindProperty("a");

                Assert.IsTrue(SerializeReferenceClipboard.CanPasteInto(typeof(ITestWeapon), SerializeReferenceWriter.WithHoldCheck(property, filter: null)));
                Assert.IsTrue(Paste(property));
                Assert.AreEqual(9, ((TestSword)first.a).damage);
                Assert.AreEqual(9, ((TestSword)second.a).damage);
                Assert.AreNotSame(first.a, second.a, "A paste must give every selected object its own copy.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Paste_EmptyReference_ClearsTheField()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new TestSword();
                SerializeReferenceClipboard.Copy(null);
                var serialized = new SerializedObject(obj);

                Assert.IsTrue(Paste(serialized.FindProperty("a")));
                Assert.IsNull(obj.a, "A copied empty reference must clear the field on paste.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Paste_EditorOnlyTypeIntoRuntimeObject_IsRefused()
        {
            var obj = ScriptableObject.CreateInstance<InMemoryRepairTestObject>();
            try
            {
                SerializeReferenceClipboard.Copy(new TestSword());
                var serialized = new SerializedObject(obj);
                var property = serialized.FindProperty(nameof(InMemoryRepairTestObject.value));

                Assert.IsFalse(SerializeReferenceClipboard.CanPasteInto(typeof(object), SerializeReferenceWriter.WithHoldCheck(property, filter: null)),
                    "Paste must not be offered for a type a player build leaves out.");
                Assert.IsFalse(Paste(property));
                Assert.IsNull(obj.value);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void SetValue_TemplateRemoved_LeavesTheFieldAsItIs()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var sword = new TestSword();
                obj.a = sword;
                var serialized = new SerializedObject(obj);

                Assert.IsFalse(SerializeReferenceWriter.SetValue(serialized.FindProperty("a"), typeof(TestSword), () => null));
                Assert.AreSame(sword, obj.a, "A template removed meanwhile must not clear the field.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Drop_MultipleObjects_AssignsTheScriptClassToEach()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                StartDrag<WriterTestBow>();
                var serialized = new SerializedObject(new Object[] { first, second });
                var property = serialized.FindProperty("a");

                Assert.IsTrue(SerializeReferenceDropHandler.TryResolveDroppedType(property, typeof(ITestWeapon), baseTypes: null, out var type));
                Assert.AreEqual(typeof(WriterTestBow), type);

                Assert.IsTrue(SerializeReferenceDropHandler.Assign(property, type));
                Assert.IsInstanceOf<WriterTestBow>(first.a);
                Assert.IsInstanceOf<WriterTestBow>(second.a);
                Assert.AreNotSame(first.a, second.a, "A drop must give every selected object its own instance.");
                Assert.IsTrue(property.isExpanded, "A drop must open the field.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Drop_NarrowedOutScript_IsRejected()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                StartDrag<WriterTestBow>();
                var serialized = new SerializedObject(obj);

                Assert.IsFalse(SerializeReferenceDropHandler.TryResolveDroppedType(serialized.FindProperty("a"),
                    typeof(ITestWeapon), new[] { typeof(TestSword) }, out _));
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Drop_EditorOnlyScriptOnRuntimeObject_IsRejected()
        {
            var obj = ScriptableObject.CreateInstance<InMemoryRepairTestObject>();
            try
            {
                StartDrag<WriterTestBow>();
                var serialized = new SerializedObject(obj);

                Assert.IsFalse(SerializeReferenceDropHandler.TryResolveDroppedType(
                    serialized.FindProperty(nameof(InMemoryRepairTestObject.value)), typeof(object), baseTypes: null, out _),
                    "A script from an editor-only assembly must not drop onto an object a player build keeps.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void LinkTo_SameInstance_DoesNotWrite()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var shared = new TestSword();
                obj.a = shared;
                obj.b = shared;
                var serialized = new SerializedObject(obj);

                Assert.IsFalse(SerializeReferenceLinker.LinkTo(serialized.FindProperty("b"), "a"));
                Assert.AreSame(obj.a, obj.b);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void HasMixedTypes_DifferentTypes_IsTrue()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                first.a = new TestSword();
                second.a = new WriterTestBow();
                var serialized = new SerializedObject(new Object[] { first, second });

                Assert.IsTrue(SerializeReferenceHelpers.HasMixedTypes(serialized.FindProperty("a")));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void HasMixedTypes_ValueAndNull_IsTrue()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                first.a = new TestSword();
                var serialized = new SerializedObject(new Object[] { first, second });

                Assert.IsTrue(SerializeReferenceHelpers.HasMixedTypes(serialized.FindProperty("a")));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void HasMixedTypes_SameTypeOrBothEmpty_IsFalse()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                first.a = new TestSword { damage = 1 };
                second.a = new TestSword { damage = 2 };
                var serialized = new SerializedObject(new Object[] { first, second });

                Assert.IsFalse(SerializeReferenceHelpers.HasMixedTypes(serialized.FindProperty("a")),
                    "The same type with other data and another rid in each object is not mixed.");
                Assert.IsFalse(SerializeReferenceHelpers.HasMixedTypes(serialized.FindProperty("b")), "Both empty");
                Assert.IsFalse(SerializeReferenceHelpers.HasMixedTypes(new SerializedObject(first).FindProperty("a")), "One object");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Drop_OnMissingListElement_NotesTheReplacement() =>
            AssertNotesTheMissingElement(element => SerializeReferenceDropHandler.Assign(element, typeof(TestSword)));

        [Test]
        public void PasteTemplate_OnMissingListElement_NotesTheReplacement() =>
            AssertNotesTheMissingElement(element =>
                SerializeReferenceWriter.SetValue(element, typeof(TestSword), () => new TestSword()));

        [Test]
        public void LinkTo_OnMissingListElement_NotesTheReplacement() =>
            AssertNotesTheMissingElement(element =>
                SerializeReferenceLinker.LinkTo(element, $"{nameof(ReferenceListTestObject.weapons)}.Array.data[1]"));

        // [GhostSword, WriterTestBow]: the write replaces the missing element, so the next save must not bring it back.
        private static void AssertNotesTheMissingElement(Func<SerializedProperty, bool> write)
        {
            var probe = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            probe.weapons.Add(new TestSword());
            probe.weapons.Add(new WriterTestBow());

            try
            {
                AssetDatabase.CreateAsset(probe, MissingElementAssetPath);

                // The file now stores a missing type while the loaded element reads as null, as a missing one does.
                File.WriteAllText(MissingElementAssetPath,
                    File.ReadAllText(MissingElementAssetPath).Replace("class: TestSword,", "class: GhostSword,"));
                probe.weapons[0] = null;

                using var serializedObject = new SerializedObject(probe);
                var element = serializedObject.FindProperty($"{nameof(ReferenceListTestObject.weapons)}.Array.data[0]");

                Assert.AreEqual(1, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(MissingElementAssetPath, Resolves).Count);

                Assert.IsTrue(write(element));
                Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(MissingElementAssetPath, Resolves).Count,
                    "A replaced missing element must be noted, or the next save restores it next to the new value.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(MissingElementAssetPath);
            }
        }

        private static bool Paste(SerializedProperty property) =>
            SerializeReferenceWriter.SetValue(property, SerializeReferenceClipboard.Type, SerializeReferenceClipboard.CreateInstance);

        private static void StartDrag<T>()
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new Object[] { MonoScriptOf<T>() };
        }

        private static MonoScript MonoScriptOf<T>()
        {
            foreach (var guid in AssetDatabase.FindAssets($"{typeof(T).Name} t:MonoScript"))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                if (script != null && script.GetClass() == typeof(T)) return script;
            }

            Assert.Fail($"No script asset declares {typeof(T).Name}.");
            return null;
        }
    }
}
