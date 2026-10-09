using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The Missing type mark of a reference whose object has unsaved edits. A list element is matched with the list in the
    // file by the rids of the other elements, a field inside a reference is read by that reference's rid, and a missing
    // reference the user replaced reads as a null until a save. Each test breaks a reference in the file and nulls it in
    // memory, as Unity loads a missing type.
    [TestFixture]
    internal sealed class SerializeReferenceMissingTypeProbeTests
    {
        private const string AssetPath = "Assets/__AspidMissingTypeProbe__.asset";
        private const long FileId = 11400000;
        private const string Gear = nameof(GearListTestObject.gear);

        // delayCall can wait many ticks in the batch-mode test runner.
        private const double DelayCallTimeoutSeconds = 10;

        private GearListTestObject _asset;
        private SerializedObject _serializedObject;

        [TearDown]
        public void TearDown()
        {
            _serializedObject?.Dispose();
            if (_asset != null) Undo.ClearUndo(_asset);

            SerializeReferenceMissingListGuard.ForgetNotes(AssetPath);
            AssetDatabase.DeleteAsset(AssetPath);
            ResetProbes();
        }

        [Test]
        public void UnchangedList_MarksTheMissingElement()
        {
            Create(new TestBlade { damage = 1 }, new TestBlade { damage = 2 }, new TestBlade { damage = 3 });
            BreakElement(1);

            AssertMissing(false, true, false);
        }

        [Test]
        public void DeletedEarlierElement_KeepsTheMarkOnTheMissingElement()
        {
            Create(new TestBlade { damage = 1 }, new TestBlade { damage = 2 }, new TestBlade { damage = 3 });
            BreakElement(1);

            Edit(list => list.DeleteArrayElementAtIndex(0));

            AssertMissing(true, false);
            Assert.AreEqual("GhostTestBlade", SerializeReferenceHelpers.GetMissingTypeName(Find(Element(0))).Class);
            Assert.IsTrue(SerializeReferenceHelpers.TryGetMissingListSlot(Find(Element(0)), out var field, out var index));
            Assert.AreEqual((Gear, 1), (field, index), "The slot in the file is the one before the delete.");
        }

        [Test]
        public void MovedMissingElement_KeepsItsMark()
        {
            Create(new TestBlade { damage = 1 }, new TestBlade { damage = 2 }, new TestBlade { damage = 3 });
            BreakElement(0);

            Edit(list => list.MoveArrayElement(0, 2));

            AssertMissing(false, false, true);
        }

        [Test]
        public void DeletedElementAmongNulls_MarksNothing()
        {
            // [Ghost, <None>, Blade] with element 0 or 1 deleted: the editor holds the same list either way, so the null
            // left may be the <None> one.
            Create(new TestBlade { damage = 1 }, null, new TestBlade { damage = 3 });
            BreakElement(0);

            Edit(list => list.DeleteArrayElementAtIndex(0));

            AssertMissing(false, false);
        }

        [Test]
        public void FieldOfAShiftedReference_IsReadByTheReferenceId()
        {
            // [Holster(Blade), Holster(Ghost)] with element 0 deleted: the holster left is slot 1 in the file.
            Create(new TestHolster { item = new TestBlade() }, new TestHolster { item = new TestBlade() });
            Break($"{Element(1)}.item", () => ((TestHolster)_asset.gear[1]).item = null);

            Edit(list => list.DeleteArrayElementAtIndex(0));

            Assert.IsTrue(IsMissing($"{Element(0)}.item"));
        }

        [Test]
        public void ListInsideAReference_KeepsTheMarkOnTheMissingElement()
        {
            Create(new TestHolster { spares = { new TestBlade { damage = 1 }, new TestBlade { damage = 2 } } });
            Break($"{Element(0)}.spares.Array.data[1]", () => ((TestHolster)_asset.gear[0]).spares[1] = null);

            Edit(_ => Find($"{Element(0)}.spares").DeleteArrayElementAtIndex(0));

            var spare = $"{Element(0)}.spares.Array.data[0]";
            Assert.IsTrue(IsMissing(spare));
            Assert.IsFalse(SerializeReferenceHelpers.TryGetMissingListSlot(Find(spare), out _, out _),
                "A list inside a reference has no slot of a top-level list.");
        }

        [Test]
        public void NoneOnTheMissingElement_ClearsTheMarkUntilUndo()
        {
            Create(new TestBlade { damage = 1 }, new TestBlade { damage = 2 }, new TestBlade { damage = 3 });
            BreakElement(1);

            var undoProbe = ScriptableObject.CreateInstance<GearListTestObject>();
            try
            {
                Undo.IncrementCurrentGroup();
                PickNone(Element(1));

                // The pick on a slot that already reads as null records nothing, so Undo needs a step of its own group.
                Undo.RecordObject(undoProbe, "Missing type probe test");
                undoProbe.name = "Changed";
                Undo.FlushUndoRecordObjects();

                AssertMissing(false, false, false);

                Undo.PerformUndo();
                ResetProbes();
                AssertMissing(false, true, false);

                Undo.PerformRedo();
                ResetProbes();
                AssertMissing(false, false, false);
            }
            finally
            {
                Undo.ClearUndo(undoProbe);
                Object.DestroyImmediate(undoProbe);
            }
        }

        [Test]
        public void UndoOfNone_BringsTheMarkBack_AfterAProbeOnTheSameStep()
        {
            Create(new TestBlade { damage = 1 }, new TestBlade { damage = 2 }, new TestBlade { damage = 3 });
            BreakElement(1);

            var undoProbe = ScriptableObject.CreateInstance<GearListTestObject>();
            var notified = 0;

            // What a UI Toolkit field does on undoRedoPerformed, which Unity may call before the guard sees the step.
            Undo.UndoRedoCallback probeOnUndo = () => IsMissing(Element(1));
            Action onNotesChanged = () => notified++;

            Undo.undoRedoPerformed += probeOnUndo;
            SerializeReferenceMissingListGuard.UndoChangedNotes += onNotesChanged;
            try
            {
                Undo.IncrementCurrentGroup();
                PickNone(Element(1));

                Undo.RecordObject(undoProbe, "Missing type probe test");
                undoProbe.name = "Changed";
                Undo.FlushUndoRecordObjects();

                Assert.IsFalse(IsMissing(Element(1)));

                Undo.PerformUndo();

                Assert.AreEqual(1, notified, "Fields must hear that the undo brought the missing reference back.");
                Assert.IsTrue(IsMissing(Element(1)), "A probe made earlier in the same step must not hide the mark.");
            }
            finally
            {
                Undo.undoRedoPerformed -= probeOnUndo;
                SerializeReferenceMissingListGuard.UndoChangedNotes -= onNotesChanged;
                Undo.ClearUndo(undoProbe);
                Object.DestroyImmediate(undoProbe);
            }
        }

        [UnityTest]
        public IEnumerator RetryFixAfterSave_FixesTheElementOnALaterTick()
        {
            Create(new TestBlade { damage = 1 }, new TestBlade { damage = 2 });
            BreakElement(1);

            SerializeReferenceHelpers.RetryFixAfterSave(Find(Element(1)), typeof(TestBlade));
            Assert.IsTrue(IsMissing(Element(1)), "The retry waits for the missing-list guard of the save.");

            yield return FlushDelayCalls();

            // The fixture class has no script asset, so the reimported asset cannot load it; the file tells the result.
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(AssetPath, FileId, Element(1), out _, out var type));
            Assert.AreEqual(nameof(TestBlade), type.Class, "The retry must repair the element.");
            StringAssert.Contains("damage: 2", File.ReadAllText(AssetPath), "The retry must keep the stored data.");
        }

        [Test]
        public void NoneAfterAnUnsavedDelete_ClearsTheMark()
        {
            Create(new TestBlade { damage = 1 }, new TestBlade { damage = 2 }, new TestBlade { damage = 3 });
            BreakElement(1);
            Edit(list => list.DeleteArrayElementAtIndex(0));

            PickNone(Element(0));

            AssertMissing(false, false);
            Assert.AreEqual(0, SerializeReferenceMissingListGuard.SnapshotMissingArrayElements(AssetPath,
                SerializeReferenceMissingListGuard.StoredTypeLoads).Count, "The next save must let the replaced element go.");
        }

        [Test]
        public void NoneOnAMissingField_ClearsTheMark()
        {
            Create(new TestHolster { item = new TestBlade() });
            var item = $"{Element(0)}.item";
            Break(item, () => ((TestHolster)_asset.gear[0]).item = null);
            Assert.IsTrue(IsMissing(item));

            PickNone(item);

            Assert.IsFalse(IsMissing(item));
        }

        private static string Element(int index) =>
            $"{Gear}.Array.data[{index}]";

        private void Create(params ITestGear[] gear)
        {
            _asset = ScriptableObject.CreateInstance<GearListTestObject>();
            _asset.gear.AddRange(gear);

            AssetDatabase.CreateAsset(_asset, AssetPath);
            _serializedObject = new SerializedObject(_asset);
        }

        private void BreakElement(int index) =>
            Break(Element(index), () => _asset.gear[index] = null);

        // Renames the stored class of the reference at path in the file; nullInMemory drops it from the loaded object.
        private void Break(string path, Action nullInMemory)
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(AssetPath, FileId, path, out var rid));

            var text = File.ReadAllText(AssetPath).Replace("\r\n", "\n");
            var entry = $"\n    - rid: {rid}\n      type: {{class: ";
            var at = text.IndexOf(entry, StringComparison.Ordinal);
            Assert.GreaterOrEqual(at, 0, $"The file must hold the entry of rid {rid}.");

            File.WriteAllText(AssetPath, text.Insert(at + entry.Length, "Ghost"));
            nullInMemory();

            _serializedObject.Update();
            ResetProbes();
        }

        private void Edit(Action<SerializedProperty> edit)
        {
            _serializedObject.Update();
            edit(Find(Gear));
            _serializedObject.ApplyModifiedPropertiesWithoutUndo();
            ResetProbes();
        }

        // What the type picker does on <None>.
        private void PickNone(string path)
        {
            var property = Find(path);
            SerializeReferenceMissingListGuard.NoteReplaced(property);

            property.managedReferenceValue = null;
            _serializedObject.ApplyModifiedProperties();
            ResetProbes();
        }

        private SerializedProperty Find(string path)
        {
            var property = _serializedObject.FindProperty(path);
            Assert.IsNotNull(property, path);
            return property;
        }

        private bool IsMissing(string path)
        {
            _serializedObject.Update();
            return SerializeReferenceHelpers.IsMissingType(Find(path));
        }

        private void AssertMissing(params bool[] expected)
        {
            _serializedObject.Update();
            Assert.AreEqual(expected.Length, Find(Gear).arraySize, "list size");

            for (var i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i], SerializeReferenceHelpers.IsMissingType(Find(Element(i))), $"element {i}");
        }

        // delayCall runs in registration order, so once this marker has run, the calls queued before it have too.
        private static IEnumerator FlushDelayCalls()
        {
            var flushed = false;
            EditorApplication.delayCall += () => flushed = true;

            var deadline = EditorApplication.timeSinceStartup + DelayCallTimeoutSeconds;
            while (!flushed && EditorApplication.timeSinceStartup < deadline)
                yield return null;

            Assert.IsTrue(flushed, "delayCall did not run in time.");
        }

        // A synchronous test runs inside one editor update, so the per-update memos must be dropped by hand.
        private static void ResetProbes()
        {
            SerializeReferenceYamlProbeCache.ClearCache();
            SerializeReferenceHelpers.InvalidateMissingTypeMemo();
        }
    }
}
