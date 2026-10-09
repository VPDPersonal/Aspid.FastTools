using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The cards that Asset References draws for one document, in drawing order. A reference expands its children once,
    // where it first appears; a later appearance keeps its own card but has no children below it. Without that,
    // references that point at each other give a card per path, and the number of paths grows exponentially.
    internal sealed class SerializeReferenceGraphPlan
    {
        // Each card is a few dozen visual elements, so the element count of the window, not the file, needs the bound.
        public const int MaxCards = 1000;

        public enum CardKind
        {
            // A reference drawn with its children below it.
            Node,

            // A reference drawn above already: its own card and field path, but no children.
            Repeat,

            // A reference that is an ancestor of the card: a compact marker that closes a cycle.
            BackEdge,

            // An unassigned field.
            Empty,

            // A reference no field points at.
            Orphan,
        }

        public readonly struct Card
        {
            public readonly CardKind Kind;
            public readonly long Rid;
            public readonly string Path;

            public Card(CardKind kind, long rid, string path)
            {
                Kind = kind;
                Rid = rid;
                Path = path;
            }
        }

        public readonly ReferenceGraphDocument Document;

        public readonly List<Card> Cards = new();

        // Cards left out because the budget ran out.
        public int Hidden { get; private set; }

        private int _budget;

        private readonly HashSet<long> _expanded = new();

        // The references on the walk's current path.
        private readonly HashSet<long> _ancestors = new();

        // An explicit stack: a chain of thousands of references must not overflow the call stack.
        private readonly Stack<(Card card, bool leave)> _steps = new();

        private SerializeReferenceGraphPlan(ReferenceGraphDocument document, int budget)
        {
            Document = document;
            _budget = budget;
        }

        public static SerializeReferenceGraphPlan Build(ReferenceGraphDocument document, int budget)
        {
            var plan = new SerializeReferenceGraphPlan(document, budget);

            // Missing roots go first, so their subtrees are expanded before a healthy root can reach them.
            foreach (var root in document.Roots)
            {
                if (root.IsEmpty || !SerializeReferenceGraphAnalysis.RootIsMissing(document, root.Rid)) continue;
                plan.Walk(root.Rid, root.Label);
            }

            foreach (var root in document.Roots)
            {
                if (root.IsEmpty)
                {
                    plan.Add(CardKind.Empty, root.Rid, root.Label);
                    continue;
                }

                if (SerializeReferenceGraphAnalysis.RootIsMissing(document, root.Rid)) continue;
                plan.Walk(root.Rid, root.Label);
            }

            foreach (var node in document.Nodes)
            {
                if (document.Orphans.Contains(node.Rid)) plan.Add(CardKind.Orphan, node.Rid, path: null);
            }

            return plan;
        }

        private void Walk(long rootRid, string rootPath)
        {
            _steps.Push((new Card(CardKind.Node, rootRid, rootPath), leave: false));

            while (_steps.Count > 0)
            {
                var (card, leave) = _steps.Pop();

                if (leave)
                {
                    _ancestors.Remove(card.Rid);
                    continue;
                }

                if (card.Kind == CardKind.Empty)
                {
                    Add(CardKind.Empty, card.Rid, card.Path);
                    continue;
                }

                if (_ancestors.Contains(card.Rid))
                {
                    Add(CardKind.BackEdge, card.Rid, path: null);
                    continue;
                }

                if (!_expanded.Add(card.Rid))
                {
                    Add(CardKind.Repeat, card.Rid, card.Path);
                    continue;
                }

                Add(CardKind.Node, card.Rid, card.Path);

                _ancestors.Add(card.Rid);
                _steps.Push((card, leave: true));

                // Pushed in reverse, so the children pop in their own order.
                var children = Document.ChildrenOf(card.Rid);
                for (var i = children.Count - 1; i >= 0; i--)
                {
                    var edge = children[i];

                    // Past the budget the walk only counts, so it stops building path strings.
                    var path = _budget > 0 ? SerializeReferenceGraphAnalysis.CombinePath(card.Path, edge.Label) : null;
                    _steps.Push((new Card(edge.IsEmpty ? CardKind.Empty : CardKind.Node, edge.Rid, path), leave: false));
                }
            }
        }

        private void Add(CardKind kind, long rid, string path)
        {
            if (_budget <= 0)
            {
                Hidden++;
                return;
            }

            _budget--;
            Cards.Add(new Card(kind, rid, path));
        }
    }
}
