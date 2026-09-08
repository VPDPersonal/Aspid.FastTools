using System;
using System.Linq;
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

        public ReferenceGraphNode? FindNode(long rid)
        {
            foreach (var node in Nodes.Where(node => node.Rid == rid))
                return node;

            return null;
        }

        public IReadOnlyList<ReferenceGraphEdge> ChildrenOf(long rid) =>
            Edges.TryGetValue(rid, out var children) ? children : Array.Empty<ReferenceGraphEdge>();
    }
}
