using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences.Yaml
{
    // Coverage for MissingListAlignment: where a missing list element sits after a save that wrote it as a null id. In
    // these lists an id from 2000 is a missing element, a smaller id a healthy one and N a null.
    [TestFixture]
    internal sealed partial class MissingListAlignmentTests
    {
        private const long N = MissingListState.NullRid;
        private const long FirstMissingId = 2000;
        private const long FileId = 1;

        [Test]
        public void UnchangedList_KeepsEverySlot()
        {
            AssertPlaced(Before(2001, 1003), After(N, 1003), index: 0, expected: 0, guessed: false);
        }

        [Test]
        public void SurvivingMissingElement_KeepsItsId()
        {
            // A ScriptableObject saved without a resize keeps the missing element.
            AssertPlaced(Before(2001, 1003), After(1003, 2001), index: 0, expected: 1, guessed: false);
        }

        [Test]
        public void DeletedSibling_FollowsTheShift()
        {
            AssertPlaced(Before(1003, 2001), After(N), index: 1, expected: 0, guessed: false);
        }

        [Test]
        public void Append_KeepsTheSlot()
        {
            AssertPlaced(Before(2001, 1003), After(N, 1003, N), index: 0, expected: 0, guessed: false);
            AssertPlaced(Before(1003, 2001), After(1003, N, N), index: 1, expected: 1, guessed: false);
        }

        [Test]
        public void InsertBefore_FollowsTheShift()
        {
            AssertPlaced(Before(1003, 2001), After(N, 1003, N), index: 1, expected: 2, guessed: false);
        }

        [Test]
        public void DuplicatedSibling_FollowsTheShift()
        {
            // The copy of 1003 has a new id.
            AssertPlaced(Before(1003, 2001), After(1003, 5000, N), index: 1, expected: 2, guessed: false);
        }

        [Test]
        public void ReplacedMissingAndAppend_KeepsTheOtherInPlace()
        {
            // [Ghost1, Ghost2]: Ghost1 retyped through the picker (noted), then "+".
            var before = MissingListState.Build(new long[] { 2001, 2002 }, FileId,
                new HashSet<(long, long)> { (FileId, 2001), (FileId, 2002) }, new HashSet<(long, long)> { (FileId, 2001) });

            AssertPlaced(before, After(5000, N, N), index: 1, expected: 1, guessed: false);
        }

        [Test]
        public void Reorder_FollowsTheMovedElement()
        {
            AssertPlaced(Before(2001, 1003), After(1003, N), index: 0, expected: 1, guessed: false);
        }

        [Test]
        public void MovedHealthyElement_KeepsTheMissingOneBetweenItsNeighbours()
        {
            // 1006 moved to the front.
            AssertPlaced(Before(1003, 2001, 1005, 1006), After(1006, 1003, N, 1005), index: 1, expected: 2, guessed: false);
        }

        [Test]
        public void NoneMovedToTheTop_KeepsTheMissingElementBetweenItsNeighbours()
        {
            // [Sword, Ghost, Shotgun, <None>]: the <None> dragged to the top.
            AssertPlaced(Before(1000, 2001, 1002, N), After(N, 1000, N, 1002), index: 1, expected: 2, guessed: false);
        }

        [Test]
        public void DuplicateBeforeANone_FollowsTheShift()
        {
            // [Sword, <None>, Ghost]: Sword duplicated; the copy has an id of its own or shares Sword's.
            AssertPlaced(Before(1000, N, 2002), After(1000, 5000, N, N), index: 2, expected: 3, guessed: false);
            AssertPlaced(Before(1000, N, 2002), After(1000, 1000, N, N), index: 2, expected: 3, guessed: false);
        }

        [Test]
        public void SingleEditWithTwoReadings_IsMarkedGuessed()
        {
            // [<None>, Ghost] saved with one more null: "+" keeps Ghost at 1, a duplicate of the <None> moves it to 2.
            AssertPlaced(Before(N, 2001), After(N, N, N), index: 1, expected: 1, guessed: true);
        }

        [Test]
        public void KeptMissingIdInAResizedList_KeepsTheOtherMissingElement()
        {
            // Unity loaded 2001, which the guard took for missing, and kept its id while the list grew: the save still
            // dropped 2002, which comes back.
            AssertPlaced(Before(2001, 2002, 1003), After(2001, N, 1003, N), index: 1, expected: 1, guessed: false);
        }

        [Test]
        public void RemoveAndAdd_FollowsTheHealthyNeighbours()
        {
            // 1003 deleted and "+" pressed: the old index 1 now holds 1005.
            AssertPlaced(Before(1003, 2001, 1005), After(N, 1005, N), index: 1, expected: 0, guessed: false);
        }

        [Test]
        public void DeletedSiblingAndAnotherSetToNone_KeepsTheMissingElement()
        {
            AssertPlaced(Before(1003, 1005, 2001), After(N, N), index: 2, expected: 1, guessed: false);
            AssertPlaced(Before(2001, 1003, 1005), After(N, N), index: 0, expected: 0, guessed: false);
        }

        [Test]
        public void DuplicateAndAppend_KeepsTheSlot()
        {
            AssertPlaced(Before(2001, 1003), After(N, 1003, 5000, N), index: 0, expected: 0, guessed: false);
        }

        [Test]
        public void AssignedSlot_IsNeverTaken()
        {
            // The old slot holds a new id, so the element goes to the free null slot.
            AssertPlaced(Before(2001, 1003), After(1007, 1003, N), index: 0, expected: 2, guessed: false);
        }

        [Test]
        public void SharedMissingElement_ReturnsToBothSlots()
        {
            var before = Before(2001, 1003, 2001);
            var targets = MissingListAlignment.Align(before, After(N, 1003, N), out _);

            Assert.AreEqual(0, targets[0]);
            Assert.AreEqual(2, targets[2]);
        }

        [Test]
        public void DeletedMissingElement_StaysDeleted()
        {
            // [Ghost, <None>] with one element deleted saves [N] either way; the guard keeps the <None>.
            AssertDropped(Before(2001, N), After(N), index: 0);
            AssertDropped(Before(N, 2001), After(N), index: 1);
            AssertDropped(Before(2001, 1003), After(1003), index: 0);
        }

        // MatchSlots reads the list the editor holds, where a missing element is a null, against the list in the file.
        [Test]
        public void MatchSlots_UneditedList_MatchesEverySlotInPlace()
        {
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, MissingListAlignment.MatchSlots(Before(1003, 2001, N), After(1003, N, N)));
        }

        [Test]
        public void MatchSlots_DeletedSibling_FollowsTheShift()
        {
            CollectionAssert.AreEqual(new[] { 1 }, MissingListAlignment.MatchSlots(Before(1003, 2001), After(N)));
            CollectionAssert.AreEqual(new[] { 1, 2 }, MissingListAlignment.MatchSlots(Before(1003, 2001, 1004), After(N, 1004)));
        }

        [Test]
        public void MatchSlots_MovedMissingElement_FollowsTheMove()
        {
            CollectionAssert.AreEqual(new[] { 1, 2, 0 }, MissingListAlignment.MatchSlots(Before(2001, 1003, 1004), After(1003, 1004, N)));
        }

        [Test]
        public void MatchSlots_AddedSlot_HoldsNoElementOfTheFile()
        {
            CollectionAssert.AreEqual(new[] { 0, 1, -1 }, MissingListAlignment.MatchSlots(Before(2001, 1003), After(N, 1003, N)));
        }

        [Test]
        public void MatchSlots_DeletedAmongNulls_HoldsNoMissingElement()
        {
            // One element of [Ghost, <None>] or of [Ghost1, Ghost2] deleted: the null left may be either of them.
            Assert.AreEqual(1, MissingListAlignment.MatchSlots(Before(2001, N), After(N))[0]);
            Assert.AreEqual(-1, MissingListAlignment.MatchSlots(Before(2001, 2002), After(N))[0]);
        }

        [Test]
        public void ShrunkRunOfMissingElements_KeepsTheFirstInOrder()
        {
            var before = Before(2001, 2002, 2003, N, 1003);
            var targets = MissingListAlignment.Align(before, After(N, N, N, 1003), out var guessed);

            CollectionAssert.AreEqual(new[] { 0, 1, -1 }, targets.Take(3).ToArray());
            Assert.IsTrue(guessed[0] && guessed[1], "The saved nulls do not say which missing element was deleted.");
        }

        [Test]
        public void LongListOfMissingElements_AlignsInOrder()
        {
            // Too large for the edit table: every element but the last keeps its order.
            var rids = Enumerable.Range(0, 5000).Select(i => FirstMissingId + i).ToArray();
            var after = Enumerable.Repeat(N, rids.Length - 1).ToArray();

            var targets = MissingListAlignment.Align(Before(rids), after, out _);

            for (var i = 0; i < after.Length; i++) Assert.AreEqual(i, targets[i]);
            Assert.AreEqual(-1, targets[^1]);
        }

        [Test]
        public void LongMixedList_FollowsEveryShift()
        {
            // Healthy and missing elements alternate; every tenth healthy element is deleted.
            var rids = new List<long>();
            var collapsible = new List<bool>();
            var after = new List<long>();
            var expected = new List<int>();

            for (var i = 0; i < 4000; i++)
            {
                rids.Add(1 + i);
                collapsible.Add(false);
                if (i % 10 != 0) after.Add(1 + i);

                rids.Add(100_000 + i);
                collapsible.Add(true);
                expected.Add(after.Count);
                after.Add(N);
            }

            var before = new MissingListState(rids.ToArray(), collapsible.ToArray());
            var targets = MissingListAlignment.Align(before, after, out var guessed);

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], targets[2 * i + 1]);
                Assert.IsFalse(guessed[2 * i + 1]);
            }
        }

        private static MissingListState Before(params long[] rids) =>
            new(rids, rids.Select(rid => rid >= FirstMissingId).ToArray());

        private static long[] After(params long[] rids) => rids;

        private static void AssertPlaced(MissingListState before, long[] after, int index, int expected, bool guessed)
        {
            var targets = MissingListAlignment.Align(before, after, out var flags);

            Assert.AreEqual(expected, targets[index], "slot");
            Assert.AreEqual(guessed, flags[index], "guessed");
        }

        private static void AssertDropped(MissingListState before, long[] after, int index)
        {
            var targets = MissingListAlignment.Align(before, after, out _);
            Assert.AreEqual(-1, targets[index]);
        }
    }
}
