using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    internal sealed class DuplicateGuardTestObject : ScriptableObject
    {
        [SerializeReference] public List<ITestWeapon> weapons = new();
        public List<ListAddTestLoadout> loadouts = new();
    }

    /// <summary>
    /// Covers <see cref="SerializeReferenceDuplicateGuard"/>: the fresh-duplicate detector on its own, and the
    /// Observe → deferred fix chain on a live list, which must split a just-copied element and leave intentional
    /// aliases alone.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceDuplicateGuardTests
    {
        private const string ListPath = "weapons";
        private const string LoadoutsPath = "loadouts";

        // The guard defers its fix to delayCall, which the batch-mode test runner can hold back for hundreds of ticks.
        private const double DelayCallTimeoutSeconds = 10;

        private bool _autoDeAlias;
        private DuplicateGuardTestObject _target;
        private SerializedObject _serialized;

        [SetUp]
        public void SetUp()
        {
            _autoDeAlias = SerializeReferenceSettings.AutoDeAliasEnabled;
            SerializeReferenceSettings.AutoDeAliasEnabled = true;

            _target = ScriptableObject.CreateInstance<DuplicateGuardTestObject>();
            _serialized = new SerializedObject(_target);
        }

        [TearDown]
        public void TearDown()
        {
            _serialized.Dispose();
            Undo.ClearUndo(_target);
            Object.DestroyImmediate(_target);
            SerializeReferenceSettings.AutoDeAliasEnabled = _autoDeAlias;
        }

        [Test]
        public void TryFindFreshDuplicate_AppendedCopyOfLast_FlagsTheNewElement()
        {
            Assert.IsTrue(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(Map(10, 20), Map(10, 20, 20), out var index));
            Assert.AreEqual(2, index);
        }

        [Test]
        public void TryFindFreshDuplicate_DuplicateInTheMiddle_FlagsTheCopy()
        {
            Assert.IsTrue(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(Map(10, 20, 30), Map(10, 20, 20, 30), out var index));
            Assert.AreEqual(2, index, "The copy sits right after its source; the shifted tail is not a duplicate.");
        }

        [Test]
        public void TryFindFreshDuplicate_Reorder_IsNotADuplicate()
        {
            Assert.IsFalse(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(Map(10, 20, 30), Map(30, 10, 20), out _));
        }

        [Test]
        public void TryFindFreshDuplicate_ExistingAliasPlusCopy_FlagsOnlyTheNewElement()
        {
            Assert.IsTrue(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(Map(10, 10), Map(10, 10, 10), out var index));
            Assert.AreEqual(2, index, "The alias that was already there is intentional and must be kept.");
        }

        [Test]
        public void TryFindFreshDuplicate_ExistingAliasPlusNewValue_IsNotADuplicate()
        {
            Assert.IsFalse(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(Map(10, 10), Map(10, 10, 30), out _));
        }

        [Test]
        public void TryFindFreshDuplicate_ListOfAnotherParent_IsNotADuplicate()
        {
            // The array path names a position: after an outer element is deleted it holds the next parent's list.
            Assert.IsFalse(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(Map(10, 11), Map(20, 20, 21), out _),
                "Another list that merely has one element more is not an insertion into the recorded one.");
            Assert.IsFalse(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(Map(10), Map(20, 20), out _));
        }

        [Test]
        public void TryFindFreshDuplicate_CopyBetweenElementsWithoutId_ShiftsOnlyTheTail()
        {
            // An element without a valid id has no entry in the map, so the inserted copy leaves a hole behind it.
            var previous = new Dictionary<int, long> { [0] = 10, [2] = 20 };
            var current = new Dictionary<int, long> { [0] = 10, [1] = 10, [3] = 20 };

            Assert.IsTrue(SerializeReferenceDuplicateGuard.TryFindFreshDuplicate(previous, current, out var index));
            Assert.AreEqual(1, index);
        }

        [UnityTest]
        public IEnumerator Observe_ElementAddedAsCopy_GetsItsOwnInstanceWithTheSameData()
        {
            Assign(0, new TestSword { damage = 3 });
            Assert.IsFalse(Observe(0), "The first sight of a list only records its layout.");

            DuplicateLast();
            Assert.AreEqual(Rid(0), Rid(1), "Precondition: duplicating an element copies its reference id.");
            Assert.IsTrue(Observe(1), "A single-element growth onto an existing reference is a fresh duplicate.");

            yield return FlushDelayCalls();

            Assert.AreNotEqual(Rid(0), Rid(1), "The copied element must get its own managed reference.");
            Assert.AreEqual(3, ((TestSword)Element(1).managedReferenceValue).damage, "The split copy must keep the source's data.");
        }

        [UnityTest]
        public IEnumerator Observe_GrowthByTwo_KeepsTheAlias()
        {
            Assign(0, new TestSword());
            Observe(0);

            DuplicateLast();
            DuplicateLast();
            Assert.IsFalse(Observe(1), "A multi-element growth is a bulk restore, not a fresh duplicate.");
            Assert.IsFalse(Observe(2));

            yield return FlushDelayCalls();

            Assert.AreEqual(Rid(0), Rid(1));
            Assert.AreEqual(Rid(0), Rid(2));
        }

        [UnityTest]
        public IEnumerator Observe_LinkThenCopyInTheSameTick_KeepsTheLink()
        {
            Assign(0, new TestSword { damage = 1 });
            Assign(1, new TestSword { damage = 2 });
            Observe(0);
            Observe(1);

            Assert.IsTrue(SerializeReferenceLinker.LinkTo(Element(1), Element(0).propertyPath));
            _serialized.Update();
            Observe(0);

            DuplicateLast();
            Assert.IsTrue(Observe(2), "The appended copy is a fresh duplicate.");

            yield return FlushDelayCalls();

            Assert.AreEqual(Rid(0), Rid(1), "The earlier Link to Existing is intentional and must survive the fix.");
            Assert.AreNotEqual(Rid(0), Rid(2), "Only the appended copy is split.");
        }

        [UnityTest]
        public IEnumerator Observe_AfterUndoOfTheFix_KeepsTheRestoredAlias()
        {
            Assign(0, new TestSword());
            Observe(0);

            DuplicateLast();
            Assert.IsTrue(Observe(1));

            // The fix lands in its own undo group, so undoing it restores exactly the aliased layout.
            Undo.IncrementCurrentGroup();
            yield return FlushDelayCalls();
            Assert.AreNotEqual(Rid(0), Rid(1), "Precondition: the fix split the copy.");

            Undo.PerformUndo();
            _serialized.Update();
            Assert.AreEqual(Rid(0), Rid(1), "Precondition: the undo restored the alias.");

            Assert.IsFalse(Observe(1), "An undo drops the baseline, so the restored alias is only re-recorded.");
        }

        [UnityTest]
        public IEnumerator Observe_NestedListElementAddedAsCopy_GetsItsOwnInstance()
        {
            AssignNested(0, 0, new TestSword { damage = 3 });
            Assert.IsFalse(ObserveNested(0, 0));

            DuplicateNested(0);
            Assert.AreEqual(NestedRid(0, 0), NestedRid(0, 1), "Precondition: duplicating an element copies its reference id.");
            Assert.IsTrue(ObserveNested(0, 1), "A list nested in a by-value array element is guarded as well.");

            yield return FlushDelayCalls();

            Assert.AreNotEqual(NestedRid(0, 0), NestedRid(0, 1));
            Assert.AreEqual(3, ((TestSword)NestedElement(0, 1).managedReferenceValue).damage);
        }

        [UnityTest]
        public IEnumerator Observe_OuterElementDeleted_KeepsTheAliasOfTheListThatTookItsPlace()
        {
            AssignNested(0, 0, new TestSword { damage = 1 });
            AssignNested(1, 0, new TestSword { damage = 2 });
            DuplicateNested(1);
            Assert.AreEqual(NestedRid(1, 0), NestedRid(1, 1), "Precondition: the second loadout holds an intentional alias.");
            Assert.IsFalse(ObserveNested(0, 0), "The first sight of the first loadout's list only records its layout.");

            var loadouts = _serialized.FindProperty(LoadoutsPath);
            loadouts.DeleteArrayElementAtIndex(0);
            _serialized.ApplyModifiedProperties();
            _serialized.Update();
            Assert.AreEqual(NestedRid(0, 0), NestedRid(0, 1), "Precondition: the alias moved into the first loadout.");

            Assert.IsFalse(ObserveNested(0, 0), "The path now holds another list: that is not an insertion.");

            yield return FlushDelayCalls();

            Assert.AreEqual(NestedRid(0, 0), NestedRid(0, 1), "The deleted loadout must not make the next one lose its link.");
        }

        // Index to rid, the shape the guard builds from a list.
        private static Dictionary<int, long> Map(params long[] rids)
        {
            var map = new Dictionary<int, long>();
            for (var i = 0; i < rids.Length; i++) map[i] = rids[i];
            return map;
        }

        private SerializedProperty Element(int index) =>
            _serialized.FindProperty(ListPath).GetArrayElementAtIndex(index);

        private long Rid(int index) => Element(index).managedReferenceId;

        private bool Observe(int index) => SerializeReferenceDuplicateGuard.Observe(Element(index));

        private void Assign(int index, ITestWeapon value)
        {
            var list = _serialized.FindProperty(ListPath);
            if (list.arraySize <= index) list.arraySize = index + 1;

            list.GetArrayElementAtIndex(index).managedReferenceValue = value;
            _serialized.ApplyModifiedProperties();
            _serialized.Update();
        }

        // What Duplicate Array Element does on the last element: the copy shares its reference id.
        private void DuplicateLast()
        {
            var list = _serialized.FindProperty(ListPath);
            list.InsertArrayElementAtIndex(list.arraySize - 1);
            _serialized.ApplyModifiedProperties();
            _serialized.Update();
        }

        private SerializedProperty NestedElement(int loadout, int index) =>
            _serialized.FindProperty($"{LoadoutsPath}.Array.data[{loadout}].weapons").GetArrayElementAtIndex(index);

        private long NestedRid(int loadout, int index) => NestedElement(loadout, index).managedReferenceId;

        private bool ObserveNested(int loadout, int index) => SerializeReferenceDuplicateGuard.Observe(NestedElement(loadout, index));

        private void AssignNested(int loadout, int index, ITestWeapon value)
        {
            var loadouts = _serialized.FindProperty(LoadoutsPath);
            if (loadouts.arraySize <= loadout) loadouts.arraySize = loadout + 1;
            _serialized.ApplyModifiedProperties();
            _serialized.Update();

            var list = _serialized.FindProperty($"{LoadoutsPath}.Array.data[{loadout}].weapons");
            if (list.arraySize <= index) list.arraySize = index + 1;

            list.GetArrayElementAtIndex(index).managedReferenceValue = value;
            _serialized.ApplyModifiedProperties();
            _serialized.Update();
        }

        private void DuplicateNested(int loadout)
        {
            var list = _serialized.FindProperty($"{LoadoutsPath}.Array.data[{loadout}].weapons");
            list.InsertArrayElementAtIndex(list.arraySize - 1);
            _serialized.ApplyModifiedProperties();
            _serialized.Update();
        }

        // delayCall runs in registration order, so once this marker has run, the guard's queued fix has too.
        private IEnumerator FlushDelayCalls()
        {
            var flushed = false;
            EditorApplication.delayCall += () => flushed = true;

            var deadline = EditorApplication.timeSinceStartup + DelayCallTimeoutSeconds;
            while (!flushed && EditorApplication.timeSinceStartup < deadline)
                yield return null;

            Assert.IsTrue(flushed, "delayCall did not run in time.");
            _serialized.Update();
        }
    }
}
