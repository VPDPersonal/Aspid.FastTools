using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for MissingListAlignment: where a missing list element sits after a save that wrote it as a null id. In
    // these lists an id from 2000 is a missing element, a smaller id a healthy one and N a null.
    [TestFixture]
    internal sealed class MissingListAlignmentTests
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

        // Every list of up to five healthy, <None> and missing elements, edited by every pair of list edits (remove,
        // append, insert, duplicate, move, set to <None>, retype). Histories that give the same saved list differ only in
        // what the guard cannot see, so each missing element is checked against all of them: it survives when no history
        // deletes it, stays deleted when every history deletes it and never takes a slot that holds an id. When a single
        // edit explains the save, it takes the slot the histories with the fewest edits agree on, or is marked guessed
        // when they disagree.
        [Test]
        public void EveryPairOfEdits_KeepsEachUntouchedMissingElement()
        {
            var groups = new Dictionary<string, Group>();

            foreach (var start in StartingLists(maxLength: 5))
            {
                foreach (var history in Histories(start))
                {
                    AddToGroup(groups, start, history, keepMissing: false);

                    // A save without a resize may keep the missing ids.
                    if (history.Items.Count == start.Count) AddToGroup(groups, start, history, keepMissing: true);
                }
            }

            var failures = new List<string>();
            foreach (var group in groups.Values)
                CheckGroup(group, failures);

            Assert.IsEmpty(failures, $"{failures.Count} failure(s), first ones:\n{string.Join("\n", failures.Take(20))}");
        }

        private static void CheckGroup(Group group, List<string> failures)
        {
            var targets = MissingListAlignment.Align(group.Before, group.After, out var guessed);
            var taken = new HashSet<int>();

            for (var b = 0; b < group.Before.Count; b++)
            {
                if (!group.Before.Collapsible[b]) continue;

                var fates = group.Fates[b];
                var likeliest = group.FatesAtFewest[b];
                var target = targets[b];
                var problem = default(string);

                if (target >= 0 && group.After[target] >= 0 && group.After[target] != group.Before.Rids[b])
                    problem = $"took slot {target}, which holds an id";
                else if (target >= 0 && !taken.Add(target))
                    problem = $"took slot {target} twice";
                else if (target < 0 && !fates.Contains(-1))
                    problem = "was dropped, though no history deletes it";
                else if (target >= 0 && fates.Count == 1 && fates.First() < 0)
                    problem = $"went to {target}, though every history deletes it";
                else if (group.FewestEdits <= 1 && likeliest.Count == 1 && likeliest.First() != target)
                    problem = $"went to {target}, every history with the fewest edits puts it at {likeliest.First()}";
                else if (group.FewestEdits <= 1 && likeliest.Count > 1 && target >= 0 && !guessed[b])
                    problem = $"went to {target} unmarked, though the histories with the fewest edits disagree";

                if (problem is not null)
                    failures.Add($"[{string.Join(", ", group.Before.Rids)}] -> [{string.Join(", ", group.After)}]: element {b} {problem} ({group.Example})");
            }
        }

        private static void AddToGroup(Dictionary<string, Group> groups, List<Item> start, History history, bool keepMissing)
        {
            var before = BeforeOf(start, history.Replaced);
            var after = Save(history.Items, keepMissing);
            var key = $"{string.Join(",", before.Rids)}|{string.Join(",", before.Collapsible)}|{string.Join(",", after)}";

            if (!groups.TryGetValue(key, out var group))
                groups[key] = group = new Group(before, after, history.Name);

            if (history.Edits < group.FewestEdits)
            {
                group.FewestEdits = history.Edits;
                foreach (var likeliest in group.FatesAtFewest.Values) likeliest.Clear();
            }

            for (var b = 0; b < start.Count; b++)
            {
                if (!before.Collapsible[b]) continue;

                var position = history.Items.FindIndex(item => item.Kind == Kind.Missing && item.Origin == b);
                group.Fates[b].Add(position);
                if (history.Edits == group.FewestEdits) group.FatesAtFewest[b].Add(position);
            }
        }

        private static MissingListState BeforeOf(List<Item> start, HashSet<int> replaced)
        {
            var rids = start.Select(IdOf).ToArray();
            var missing = new HashSet<(long, long)>();
            var replacedIds = new HashSet<(long, long)>();

            foreach (var item in start.Where(item => item.Kind == Kind.Missing))
            {
                missing.Add((FileId, IdOf(item)));
                if (replaced.Contains(item.Origin)) replacedIds.Add((FileId, IdOf(item)));
            }

            return MissingListState.Build(rids, FileId, missing, replacedIds);
        }

        // What Unity writes: a missing element as a null id unless keepMissing, an added element under an id of its own.
        private static long[] Save(List<Item> items, bool keepMissing)
        {
            var nextAdded = 5000L;
            return items.Select(item => item.Kind switch
            {
                Kind.Healthy => IdOf(item),
                Kind.Missing => keepMissing ? IdOf(item) : N,
                Kind.Added => nextAdded++,
                _ => N,
            }).ToArray();
        }

        private static long IdOf(Item item) => item.Kind switch
        {
            Kind.Healthy => 1000 + item.Origin,
            Kind.Missing => FirstMissingId + item.Origin,
            _ => N,
        };

        private static IEnumerable<List<Item>> StartingLists(int maxLength)
        {
            var kinds = new[] { Kind.Healthy, Kind.Null, Kind.Missing };

            for (var length = 1; length <= maxLength; length++)
            {
                var count = (int)Math.Pow(kinds.Length, length);
                for (var code = 0; code < count; code++)
                {
                    var list = new List<Item>();
                    for (int i = 0, rest = code; i < length; i++, rest /= kinds.Length)
                        list.Add(new Item(kinds[rest % kinds.Length], i));

                    yield return list;
                }
            }
        }

        private static IEnumerable<History> Histories(List<Item> start)
        {
            var origin = new History(start, new HashSet<int>(), "no edit");
            yield return origin;

            foreach (var first in Edits(origin))
            {
                yield return first;
                foreach (var second in Edits(first)) yield return second;
            }
        }

        private static IEnumerable<History> Edits(History history)
        {
            var items = history.Items;

            for (var i = 0; i < items.Count; i++)
                yield return history.With($"remove {i}", list => list.RemoveAt(i));

            yield return history.With("append <None>", list => list.Add(new Item(Kind.Null, -1)));
            yield return history.With("append a new element", list => list.Add(new Item(Kind.Added, -1)));

            // "+" only appends, so a <None> inserted before the end takes a second edit, a move.
            for (var i = 0; i < items.Count; i++)
                yield return history.With($"insert <None> at {i}", list => list.Insert(i, new Item(Kind.Null, -1)), cost: 2);

            for (var i = 0; i < items.Count; i++)
            {
                // Duplicating a healthy element gives a copy with an id of its own; a missing element is not duplicated.
                if (items[i].Kind == Kind.Missing) continue;

                var copy = items[i].Kind == Kind.Null ? Kind.Null : Kind.Added;
                yield return history.With($"duplicate {i}", list => list.Insert(i + 1, new Item(copy, -1)));
            }

            for (var from = 0; from < items.Count; from++)
            {
                for (var to = 0; to < items.Count; to++)
                {
                    if (from == to) continue;

                    yield return history.With($"move {from} to {to}", list =>
                    {
                        var item = list[from];
                        list.RemoveAt(from);
                        list.Insert(to, item);
                    });
                }
            }

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Kind == Kind.Null) continue;

                yield return history.With($"set {i} to <None>", list => list[i] = new Item(Kind.Null, -1), replace: i);
            }

            for (var i = 0; i < items.Count; i++)
                yield return history.With($"retype {i}", list => list[i] = new Item(Kind.Added, -1), replace: i);
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

        private enum Kind
        {
            Healthy,
            Null,
            Missing,
            Added,
        }

        private readonly struct Item
        {
            public readonly Kind Kind;

            // The index in the starting list; -1 for an element an edit added.
            public readonly int Origin;

            public Item(Kind kind, int origin)
            {
                Kind = kind;
                Origin = origin;
            }
        }

        private sealed class History
        {
            public readonly List<Item> Items;

            // The starting indexes of the missing elements the user replaced, which the guard is told about.
            public readonly HashSet<int> Replaced;

            public readonly string Name;
            public readonly int Edits;

            public History(List<Item> items, HashSet<int> replaced, string name, int edits = 0)
            {
                Items = items;
                Replaced = replaced;
                Name = name;
                Edits = edits;
            }

            public History With(string edit, Action<List<Item>> apply, int replace = -1, int cost = 1)
            {
                var items = new List<Item>(Items);
                var replaced = new HashSet<int>(Replaced);
                if (replace >= 0 && Items[replace].Kind == Kind.Missing) replaced.Add(Items[replace].Origin);

                apply(items);
                return new History(items, replaced, Name == "no edit" ? edit : $"{Name}, {edit}", Edits + cost);
            }
        }

        private sealed class Group
        {
            public readonly MissingListState Before;
            public readonly long[] After;
            public readonly string Example;
            public readonly Dictionary<int, HashSet<int>> Fates = new();

            // The fates in the histories with the fewest edits, the likeliest readings of the save.
            public readonly Dictionary<int, HashSet<int>> FatesAtFewest = new();
            public int FewestEdits = int.MaxValue;

            public Group(MissingListState before, long[] after, string example)
            {
                Before = before;
                After = after;
                Example = example;

                for (var b = 0; b < before.Count; b++)
                {
                    if (!before.Collapsible[b]) continue;

                    Fates[b] = new HashSet<int>();
                    FatesAtFewest[b] = new HashSet<int>();
                }
            }
        }
    }
}
