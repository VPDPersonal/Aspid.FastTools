using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class ReferenceGraphDocument
    {
        public long FileId;
        public string TypeName;

        public readonly List<ReferenceGraphNode> Nodes = new();

        // One entry per field pointer in the document body. The same rid may appear under two fields; both are kept,
        // so the window renders each subtree and Shared flags the alias.
        public readonly List<ReferenceGraphRoot> Roots = new();

        // Parent rid -> its child edges. Empty (null-sentinel) slots are kept so a cleared nested field still shows.
        public readonly Dictionary<long, List<ReferenceGraphEdge>> Edges = new();

        public readonly HashSet<long> Shared = new();

        public readonly HashSet<long> Orphans = new();

        // A lookup per card must not scan every node, so the index is built on first use and again if Nodes has grown.
        private Dictionary<long, int> _nodeIndex;
        private int _indexedCount = -1;

        public ReferenceGraphNode? FindNode(long rid)
        {
            if (_indexedCount != Nodes.Count)
            {
                _nodeIndex = new Dictionary<long, int>(Nodes.Count);
                for (var i = 0; i < Nodes.Count; i++) _nodeIndex.TryAdd(Nodes[i].Rid, i);
                _indexedCount = Nodes.Count;
            }

            return _nodeIndex.TryGetValue(rid, out var index) ? Nodes[index] : null;
        }

        public IReadOnlyList<ReferenceGraphEdge> ChildrenOf(long rid) =>
            Edges.TryGetValue(rid, out var children) ? children : Array.Empty<ReferenceGraphEdge>();
    }
}
