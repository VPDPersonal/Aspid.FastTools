using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The element ids of a top-level list before a save, and which elements hold a missing type that the save may write
    // as a null id. A missing element the user replaced counts as a plain null.
    internal sealed class MissingListState
    {
        public const long NullRid = -2;

        public readonly long[] Rids;
        public readonly bool[] Collapsible;

        public int Count => Rids.Length;

        public bool HasMissing => Array.IndexOf(Collapsible, true) >= 0;

        public MissingListState(long[] rids, bool[] collapsible)
        {
            Rids = rids;
            Collapsible = collapsible;
        }

        public static MissingListState Build(IReadOnlyList<long> rids, long fileId,
            ICollection<(long fileId, long rid)> missingRids, ICollection<(long fileId, long rid)> replaced)
        {
            var slots = new long[rids.Count];
            var collapsible = new bool[rids.Count];

            for (var i = 0; i < slots.Length; i++)
            {
                var rid = rids[i];
                var isReplaced = replaced is not null && replaced.Contains((fileId, rid));

                slots[i] = isReplaced ? NullRid : rid;
                collapsible[i] = !isReplaced && missingRids.Contains((fileId, rid));
            }

            return new MissingListState(slots, collapsible);
        }

        // A <None> element: a null the user left, not a missing element.
        public bool IsNull(int index) => !Collapsible[index] && Rids[index] < 0;
    }
}
