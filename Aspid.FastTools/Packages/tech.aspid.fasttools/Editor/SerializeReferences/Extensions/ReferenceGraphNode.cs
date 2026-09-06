using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // One managed-reference node of a document's graph. Built purely from the asset YAML, so it surfaces references
    // at any nesting depth, including the orphaned ones Unity drops from the live object.
    internal readonly struct ReferenceGraphNode
    {
        public readonly long Rid;
        public readonly ManagedTypeName StoredType;
        public readonly bool Resolves;

        public ReferenceGraphNode(long rid, ManagedTypeName storedType, bool resolves)
        {
            Rid = rid;
            StoredType = storedType;
            Resolves = resolves;
        }

        // Row label.
        public string ShortName =>
            string.IsNullOrEmpty(StoredType.Class) ? $"rid {Rid}" : StoredType.Class;

        // Row tooltip.
        public string FullName => StoredType.FullName;
    }
}
