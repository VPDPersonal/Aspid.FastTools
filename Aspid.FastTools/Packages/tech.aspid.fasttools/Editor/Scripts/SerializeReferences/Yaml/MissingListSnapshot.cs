using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A top-level list of one object before a save, with the RefIds entry and stored type of each missing element it holds.
    internal sealed class MissingListSnapshot
    {
        public readonly long FileId;
        public readonly string Field;
        public readonly MissingListState Before;
        public readonly Dictionary<long, (List<string> entryLines, ManagedTypeName storedType)> Entries;

        public MissingListSnapshot(long fileId, string field, MissingListState before,
            Dictionary<long, (List<string> entryLines, ManagedTypeName storedType)> entries)
        {
            FileId = fileId;
            Field = field;
            Before = before;
            Entries = entries;
        }
    }
}
