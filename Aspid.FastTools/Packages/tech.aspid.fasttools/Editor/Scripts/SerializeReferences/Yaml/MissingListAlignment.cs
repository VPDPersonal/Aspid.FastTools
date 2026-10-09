using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Matches the list a save wrote with the list before the save. Elements with an id that both lists hold are matched by
    // that id; the ones taken to have kept their place split both lists into runs. The anonymous elements of a run
    // (missing, <None>, a healthy element whose id is gone) are aligned with the slots of the same run by the fewest
    // edits, and a missing element that no run kept moves to a free null slot. A missing element never takes a slot that
    // holds an id.
    internal static class MissingListAlignment
    {
        // The largest edit table built, about 4 MB: a larger run is matched in order, and a larger list takes only the
        // longest ordered run as the elements that kept their place.
        private const long MaxRunCells = 4_000_000;

        private const byte Match = 0;
        private const byte Delete = 1;
        private const byte Insert = 2;

        // Where each element of before sits in after: its index there, or -1 when the save deleted it. guessed marks a
        // missing element the save does not pin down: placed by a guess, or dropped while the list keeps a null slot it may
        // have collapsed into.
        public static int[] Align(MissingListState before, IReadOnlyList<long> after, out bool[] guessed)
        {
            var anchors = MatchIds(before, after);

            // A save that kept a missing element kept them all: its nulls are real, and a missing element it left out was
            // deleted.
            foreach (var (fromIndex, _) in anchors)
            {
                if (!before.Collapsible[fromIndex]) continue;

                guessed = new bool[before.Count];
                return AnchorsOnly(before, anchors);
            }

            // Two readings of which matched elements kept their place. The one that needs fewer edits wins, then the one
            // that keeps more missing elements.
            var targets = AlignAround(before, after, anchors, LongestOrderedRun(anchors), out guessed);

            // An empty saved list leaves nothing to read, and its cost scale would overflow on a huge list.
            if (after.Count > 0 && (long)(before.Count + 1) * (after.Count + 1) <= MaxRunCells)
            {
                var byEdits = AlignAround(before, after, anchors, OrderedRunByEdits(before, after, anchors), out var guessedByEdits);

                var costByEdits = CountEdits(before, after, byEdits);
                var cost = CountEdits(before, after, targets);

                if (costByEdits < cost || (costByEdits == cost && CountPlaced(before, byEdits) >= CountPlaced(before, targets)))
                {
                    targets = byEdits;
                    guessed = guessedByEdits;
                }
            }

            var hasNull = false;
            for (var a = 0; a < after.Count; a++) hasNull |= after[a] < 0;

            for (var b = 0; b < before.Count; b++)
                if (before.Collapsible[b] && targets[b] < 0) guessed[b] = hasNull;

            return targets;
        }

        private static int[] AnchorsOnly(MissingListState before, List<(int before, int after)> anchors)
        {
            var targets = Unmatched(before.Count);
            foreach (var (fromIndex, toIndex) in anchors) targets[fromIndex] = toIndex;

            return targets;
        }

        private static int[] AlignAround(MissingListState before, IReadOnlyList<long> after, List<(int before, int after)> anchors,
            List<(int before, int after)> stable, out bool[] guessed)
        {
            var targets = Unmatched(before.Count);
            var claimed = new bool[after.Count];
            guessed = new bool[before.Count];

            foreach (var (fromIndex, toIndex) in anchors)
            {
                targets[fromIndex] = toIndex;
                claimed[toIndex] = true;
            }

            var beforeStart = 0;
            var afterStart = 0;

            for (var k = 0; k <= stable.Count; k++)
            {
                var beforeEnd = k < stable.Count ? stable[k].before : before.Count;
                var afterEnd = k < stable.Count ? stable[k].after : after.Count;

                AlignRun(before, after, beforeStart, beforeEnd, afterStart, afterEnd, isTail: k == stable.Count,
                    targets, guessed, claimed);

                beforeStart = beforeEnd + 1;
                afterStart = afterEnd + 1;
            }

            PlaceLeftovers(before, after, targets, guessed, claimed);
            return targets;
        }

        // The edits a placement implies: deleted elements, inserted slots, type changes, and elements out of order as moves.
        private static int CountEdits(MissingListState before, IReadOnlyList<long> after, int[] targets)
        {
            var edits = 0;
            var taken = new bool[after.Count];
            var order = new List<int>();

            for (var b = 0; b < before.Count; b++)
            {
                var slot = targets[b];
                if (slot < 0)
                {
                    edits++;
                    continue;
                }

                taken[slot] = true;
                order.Add(slot);

                var keepsItsId = before.Rids[b] >= 0 && before.Rids[b] == after[slot];
                if (!keepsItsId && MatchCost(before, b, after[slot]) != 0) edits++;
            }

            for (var a = 0; a < after.Count; a++)
                if (!taken[a]) edits++;

            // The longest increasing run of slots stays; every other placed element moved.
            var tails = new List<int>();
            foreach (var slot in order)
            {
                var index = tails.BinarySearch(slot);
                if (index < 0) index = ~index;

                if (index == tails.Count) tails.Add(slot);
                else tails[index] = slot;
            }

            return edits + order.Count - tails.Count;
        }

        private static int CountPlaced(MissingListState before, int[] targets)
        {
            var count = 0;
            for (var b = 0; b < before.Count; b++)
                if (before.Collapsible[b] && targets[b] >= 0) count++;

            return count;
        }

        // The k-th element of before with an id matches the k-th element of after with the same id.
        private static List<(int before, int after)> MatchIds(MissingListState before, IReadOnlyList<long> after)
        {
            var slots = new Dictionary<long, Queue<int>>();
            for (var a = 0; a < after.Count; a++)
            {
                if (after[a] < 0) continue;
                if (!slots.TryGetValue(after[a], out var queue)) slots[after[a]] = queue = new Queue<int>();

                queue.Enqueue(a);
            }

            var result = new List<(int before, int after)>();
            for (var b = 0; b < before.Count; b++)
            {
                var rid = before.Rids[b];
                if (rid >= 0 && slots.TryGetValue(rid, out var queue) && queue.Count > 0)
                    result.Add((b, queue.Dequeue()));
            }

            return result;
        }

        // The matched elements that kept their order in the alignment of the whole lists with the fewest edits, where a
        // matched element out of order costs one edit, a move. The runs between them are aligned the same way.
        private static List<(int before, int after)> OrderedRunByEdits(MissingListState before, IReadOnlyList<long> after,
            List<(int before, int after)> anchors)
        {
            var slotOf = Unmatched(before.Count);
            var movedIn = new bool[after.Count];

            foreach (var (fromIndex, toIndex) in anchors)
            {
                slotOf[fromIndex] = toIndex;
                movedIn[toIndex] = true;
            }

            var p = before.Count;
            var q = after.Count;
            var width = q + 1;

            // An edit costs scale; a move costs one edit plus a tie-break unit that outweighs every deleted <None>, so of
            // two alignments with as many edits the one that keeps more elements in place wins.
            long moveUnit = p + 1;
            var scale = moveUnit * (p + 1);

            var choice = new byte[(p + 1) * width];
            var next = new long[width];
            var current = new long[width];

            for (var j = q - 1; j >= 0; j--)
            {
                next[j] = (movedIn[j] ? 0 : InsertCost(appended: true) * scale) + next[j + 1];
                choice[p * width + j] = Insert;
            }

            for (var i = p - 1; i >= 0; i--)
            {
                var deleteCost = slotOf[i] >= 0 ? scale + moveUnit : DeleteCost(before, i, scale);

                current[q] = deleteCost + next[q];
                choice[i * width + q] = Delete;

                for (var j = q - 1; j >= 0; j--)
                {
                    var best = long.MaxValue;
                    var pick = Delete;

                    var matchCost = slotOf[i] >= 0 || movedIn[j]
                        ? slotOf[i] == j ? 0 : -1
                        : MatchCost(before, i, after[j]);

                    if (matchCost >= 0)
                    {
                        best = matchCost * scale + next[j + 1];
                        pick = Match;
                    }

                    if (deleteCost + next[j] < best)
                    {
                        best = deleteCost + next[j];
                        pick = Delete;
                    }

                    var insertCost = movedIn[j] ? 0 : InsertCost(appended: false) * scale;
                    if (insertCost + current[j + 1] < best)
                    {
                        best = insertCost + current[j + 1];
                        pick = Insert;
                    }

                    current[j] = best;
                    choice[i * width + j] = pick;
                }

                (next, current) = (current, next);
            }

            var result = new List<(int before, int after)>();
            for (int i = 0, j = 0; i < p || j < q;)
            {
                switch (choice[i * width + j])
                {
                    case Match:
                        if (slotOf[i] >= 0) result.Add((i, j));
                        i++;
                        j++;
                        break;

                    case Delete:
                        i++;
                        break;

                    default:
                        j++;
                        break;
                }
            }

            return result;
        }

        // The most matched elements that kept their order, the others counting as moved; among equal sets, the one whose
        // elements moved the fewest places. pairs is ordered by before.
        private static List<(int before, int after)> LongestOrderedRun(List<(int before, int after)> pairs)
        {
            var length = new int[pairs.Count];
            var shift = new long[pairs.Count];
            var previous = new int[pairs.Count];

            // A Fenwick tree over the after index holds the best run that ends at or before each slot.
            var size = 1;
            foreach (var pair in pairs) size = Math.Max(size, pair.after + 2);

            var tree = new int[size];
            for (var i = 0; i < size; i++) tree[i] = -1;

            var best = -1;
            for (var k = 0; k < pairs.Count; k++)
            {
                var end = -1;
                for (var i = pairs[k].after; i > 0; i -= i & -i)
                    if (IsBetter(tree[i], end)) end = tree[i];

                previous[k] = end;
                length[k] = end < 0 ? 1 : length[end] + 1;
                shift[k] = (end < 0 ? 0 : shift[end]) + Math.Abs(pairs[k].before - pairs[k].after);

                for (var i = pairs[k].after + 1; i < size; i += i & -i)
                    if (IsBetter(k, tree[i])) tree[i] = k;

                if (IsBetter(k, best)) best = k;
            }

            var result = new List<(int before, int after)>();
            for (var k = best; k >= 0; k = previous[k])
                result.Add(pairs[k]);

            result.Reverse();
            return result;

            bool IsBetter(int candidate, int current) =>
                candidate >= 0 && (current < 0 || length[candidate] > length[current]
                    || (length[candidate] == length[current] && shift[candidate] < shift[current]));
        }

        private static void AlignRun(MissingListState before, IReadOnlyList<long> after, int beforeStart, int beforeEnd,
            int afterStart, int afterEnd, bool isTail, int[] targets, bool[] guessed, bool[] claimed)
        {
            // An element matched by id outside the run moved out of it, and one matched by id inside it moved in.
            var run = new List<int>();
            for (var b = beforeStart; b < beforeEnd; b++)
                if (targets[b] < 0) run.Add(b);

            var slots = new List<int>();
            for (var a = afterStart; a < afterEnd; a++)
                if (!claimed[a]) slots.Add(a);

            if (run.Count == 0 || slots.Count == 0) return;

            var matched = Unmatched(run.Count);
            bool ambiguous;

            if (KeepsEverySlot(before, after, run, slots))
            {
                for (var i = 0; i < run.Count; i++) matched[i] = i;
                ambiguous = false;
            }
            else
            {
                ambiguous = (long)(run.Count + 1) * (slots.Count + 1) <= MaxRunCells
                    ? AlignByEdits(before, after, run, slots, isTail, matched)
                    : AlignInOrder(before, after, run, slots, matched);

                // Keeping the order may cost a missing element that a reorder inside the run explains with fewer edits.
                var keptInOrder = 0;
                for (var i = 0; i < run.Count; i++)
                    if (matched[i] >= 0 && before.Collapsible[run[i]]) keptInOrder++;

                var keptWithMoves = CountKeptWithMoves(before, after, run, slots);
                if (keptInOrder < keptWithMoves)
                {
                    matched = Unmatched(run.Count);
                    AlignWithMoves(before, after, run, slots, keptWithMoves, matched);
                    ambiguous = true;
                }
            }

            for (var i = 0; i < run.Count; i++)
            {
                if (matched[i] < 0) continue;

                var slot = slots[matched[i]];
                targets[run[i]] = slot;
                claimed[slot] = true;

                if (ambiguous && before.Collapsible[run[i]]) guessed[run[i]] = true;
            }
        }

        // The run kept its size and every element its slot, the usual save that only dropped missing ids.
        private static bool KeepsEverySlot(MissingListState before, IReadOnlyList<long> after, List<int> run, List<int> slots)
        {
            if (run.Count != slots.Count) return false;

            for (var i = 0; i < run.Count; i++)
                if (MatchCost(before, run[i], after[slots[i]]) != 0) return false;

            return true;
        }

        // The alignment with the fewest edits; among those, the one that keeps the most <None> elements, then the earliest
        // elements of before. Returns whether a missing or <None> element was deleted, or a null inserted before the end.
        private static bool AlignByEdits(MissingListState before, IReadOnlyList<long> after, List<int> run, List<int> slots,
            bool isTail, int[] matched)
        {
            var p = run.Count;
            var q = slots.Count;
            var width = q + 1;

            // An edit costs scale, so the tie-break on deleted <None> elements never outweighs an edit.
            long scale = p + 1;

            // cost[j] of row i: the cheapest alignment of run from i onto slots from j; choice holds its first step.
            var choice = new byte[(p + 1) * width];
            var next = new long[width];
            var current = new long[width];

            // Slots after the whole run are appended when the run ends the list.
            for (var j = q - 1; j >= 0; j--)
            {
                next[j] = InsertCost(appended: isTail) * scale + next[j + 1];
                choice[p * width + j] = Insert;
            }

            for (var i = p - 1; i >= 0; i--)
            {
                var deleteCost = DeleteCost(before, run[i], scale);

                current[q] = deleteCost + next[q];
                choice[i * width + q] = Delete;

                for (var j = q - 1; j >= 0; j--)
                {
                    var best = long.MaxValue;
                    var pick = Delete;

                    var matchCost = MatchCost(before, run[i], after[slots[j]]);
                    if (matchCost >= 0)
                    {
                        best = matchCost * scale + next[j + 1];
                        pick = Match;
                    }

                    if (deleteCost + next[j] < best)
                    {
                        best = deleteCost + next[j];
                        pick = Delete;
                    }

                    if (InsertCost(appended: false) * scale + current[j + 1] < best)
                    {
                        best = InsertCost(appended: false) * scale + current[j + 1];
                        pick = Insert;
                    }

                    current[j] = best;
                    choice[i * width + j] = pick;
                }

                (next, current) = (current, next);
            }

            var ambiguous = false;
            for (int i = 0, j = 0; i < p || j < q;)
            {
                switch (choice[i * width + j])
                {
                    case Match:
                        matched[i++] = j++;
                        break;

                    case Delete:
                        if (IsAnonymous(before, run[i])) ambiguous = true;
                        i++;
                        break;

                    default:
                        if (after[slots[j]] < 0 && !(isTail && i == p)) ambiguous = true;
                        j++;
                        break;
                }
            }

            return ambiguous;
        }

        // How many missing elements of the run survive when its elements may also have been reordered: the fewest edits,
        // then the most <None> elements kept. A missing element takes a null slot; a <None> element takes a null slot, or
        // a slot with an id after a type pick; a healthy element whose id is gone takes any slot after one edit.
        private static int CountKeptWithMoves(MissingListState before, IReadOnlyList<long> after, List<int> run, List<int> slots)
        {
            CountKinds(before, after, run, slots, out var missing, out var nulls, out var healthy, out var nullSlots, out var idSlots);

            long scale = run.Count + 1;
            var best = 0;
            var bestCost = long.MaxValue;

            for (var kept = 0; kept <= Math.Min(missing, nullSlots); kept++)
            {
                var nullSlotsLeft = nullSlots - kept;
                var nullsOnNull = Math.Min(nulls, nullSlotsLeft);
                var nullsOnId = Math.Min(nulls - nullsOnNull, idSlots);
                var nullsDeleted = nulls - nullsOnNull - nullsOnId;
                var slotsLeft = nullSlotsLeft - nullsOnNull + idSlots - nullsOnId;
                var healthyPlaced = Math.Min(healthy, slotsLeft);

                var edits = missing - kept + nullsOnId + nullsDeleted + healthy + slotsLeft - healthyPlaced;
                var cost = edits * scale + nullsDeleted;

                if (cost >= bestCost) continue;

                bestCost = cost;
                best = kept;
            }

            return best;
        }

        // Places the run as CountKeptWithMoves counts it: the first kept elements of each kind go, in order, to the null
        // slots and to the slots with an id.
        private static void AlignWithMoves(MissingListState before, IReadOnlyList<long> after, List<int> run, List<int> slots,
            int keptMissing, int[] matched)
        {
            CountKinds(before, after, run, slots, out _, out _, out _, out var nullSlots, out var idSlots);

            var toNull = new List<int>();
            var toId = new List<int>();
            var nullsOnNull = nullSlots - keptMissing;

            for (var i = 0; i < run.Count; i++)
            {
                var b = run[i];
                if (before.Collapsible[b])
                {
                    if (keptMissing-- > 0) toNull.Add(i);
                }
                else if (before.IsNull(b))
                {
                    if (nullsOnNull-- > 0) toNull.Add(i);
                    else if (toId.Count < idSlots) toId.Add(i);
                }
            }

            for (var i = 0; i < run.Count; i++)
            {
                var b = run[i];
                if (before.Collapsible[b] || before.IsNull(b)) continue;

                if (toId.Count < idSlots) toId.Add(i);
                else if (toNull.Count < nullSlots) toNull.Add(i);
            }

            toNull.Sort();
            toId.Sort();

            int nullIndex = 0, idIndex = 0;
            for (var j = 0; j < slots.Count; j++)
            {
                if (after[slots[j]] < 0)
                {
                    if (nullIndex < toNull.Count) matched[toNull[nullIndex++]] = j;
                }
                else if (idIndex < toId.Count)
                {
                    matched[toId[idIndex++]] = j;
                }
            }
        }

        private static void CountKinds(MissingListState before, IReadOnlyList<long> after, List<int> run, List<int> slots,
            out int missing, out int nulls, out int healthy, out int nullSlots, out int idSlots)
        {
            missing = nulls = healthy = nullSlots = idSlots = 0;

            foreach (var b in run)
            {
                if (before.Collapsible[b]) missing++;
                else if (before.IsNull(b)) nulls++;
                else healthy++;
            }

            foreach (var a in slots)
            {
                if (after[a] < 0) nullSlots++;
                else idSlots++;
            }
        }

        // Each element takes the next slot it can hold; a missing element skips the slots that hold an id.
        private static bool AlignInOrder(MissingListState before, IReadOnlyList<long> after, List<int> run, List<int> slots,
            int[] matched)
        {
            var j = 0;
            for (var i = 0; i < run.Count; i++)
            {
                while (j < slots.Count && MatchCost(before, run[i], after[slots[j]]) < 0) j++;
                matched[i] = j < slots.Count ? j++ : -1;
            }

            return true;
        }

        // The edits that turn the element into what the slot holds: none when it stays as it was, one for a type pick,
        // Paste or <None>. A missing element cannot take an id: replacing it is noted, which turns it into a null.
        private static long MatchCost(MissingListState before, int index, long slot)
        {
            if (before.Collapsible[index]) return slot < 0 ? 0 : -1;
            if (before.IsNull(index)) return slot < 0 ? 0 : 1;

            return 1;
        }

        // A save writes the same nulls whether the user deleted a missing element or a <None> beside it, so a deleted
        // <None> costs a little more: a missing element the user deleted stays deleted.
        private static long DeleteCost(MissingListState before, int index, long scale) =>
            scale + (before.IsNull(index) ? 1 : 0);

        // "+" appends at the end; an element anywhere else takes an extra step, such as a move.
        private static long InsertCost(bool appended) => appended ? 1 : 2;

        private static int[] Unmatched(int count)
        {
            var result = new int[count];
            for (var i = 0; i < count; i++) result[i] = -1;

            return result;
        }

        private static bool IsAnonymous(MissingListState before, int index) =>
            before.Collapsible[index] || before.IsNull(index);

        // A move explains a missing element that left its run with fewer edits than a delete plus an added null. It takes
        // a free null slot; else the null slot of a healthy element whose id is gone, which was then deleted rather than
        // set to <None>; else the null slot of a <None> element that moved to a new id, which a type pick explains with
        // fewer edits than an inserted element.
        private static void PlaceLeftovers(MissingListState before, IReadOnlyList<long> after, int[] targets, bool[] guessed,
            bool[] claimed)
        {
            var free = new List<int>();
            for (var a = 0; a < after.Count; a++)
                if (after[a] < 0 && !claimed[a]) free.Add(a);

            PlaceLeftovers(before, targets, guessed, claimed, free);

            var leftovers = new List<int>();
            for (var b = 0; b < before.Count; b++)
                if (before.Collapsible[b] && targets[b] < 0) leftovers.Add(b);

            if (leftovers.Count == 0) return;

            free.Clear();
            var nulls = new List<int>();

            for (var b = 0; b < before.Count; b++)
            {
                var slot = targets[b];
                if (slot < 0 || after[slot] >= 0 || before.Collapsible[b]) continue;

                if (before.IsNull(b))
                {
                    nulls.Add(b);
                    continue;
                }

                free.Add(slot);
                targets[b] = -1;
            }

            // A new id after every placed element was appended, and one right after an id is a duplicate; any other may be
            // a moved <None> element.
            var lastPlaced = after.Count - 1;
            while (lastPlaced >= 0 && !claimed[lastPlaced]) lastPlaced--;

            var newIds = 0;
            for (var a = 0; a < lastPlaced; a++)
                if (after[a] >= 0 && !claimed[a] && (a == 0 || after[a - 1] < 0)) newIds++;

            nulls.Sort((x, y) => Distance(x, leftovers).CompareTo(Distance(y, leftovers)));
            for (var k = 0; k < nulls.Count && k < Math.Min(newIds, leftovers.Count); k++)
            {
                free.Add(targets[nulls[k]]);
                targets[nulls[k]] = -1;
            }

            free.Sort();
            PlaceLeftovers(before, targets, guessed, claimed, free);
        }

        private static int Distance(int index, List<int> others)
        {
            var distance = int.MaxValue;
            foreach (var other in others) distance = Math.Min(distance, Math.Abs(index - other));

            return distance;
        }

        // A missing element without a slot first follows its nearest placed neighbour into the free slot beside it; the
        // ones left take the free slot nearest their old index.
        private static void PlaceLeftovers(MissingListState before, int[] targets, bool[] guessed, bool[] claimed, List<int> free)
        {
            for (var b = 0; b < before.Count && free.Count > 0; b++)
            {
                if (!before.Collapsible[b] || targets[b] >= 0) continue;

                var left = b - 1;
                while (left >= 0 && targets[left] < 0) left--;

                var right = b + 1;
                while (right < before.Count && targets[right] < 0) right++;

                if (left >= 0 && free.Contains(targets[left] + 1)) Place(b, targets[left] + 1);
                else if (right < before.Count && free.Contains(targets[right] - 1)) Place(b, targets[right] - 1);
            }

            for (var b = 0; b < before.Count && free.Count > 0; b++)
            {
                if (!before.Collapsible[b] || targets[b] >= 0) continue;

                var slot = free[0];
                foreach (var candidate in free)
                    if (Math.Abs(candidate - b) < Math.Abs(slot - b)) slot = candidate;

                Place(b, slot);
            }

            void Place(int index, int slot)
            {
                targets[index] = slot;
                claimed[slot] = true;
                guessed[index] = true;
                free.Remove(slot);
            }
        }
    }
}
