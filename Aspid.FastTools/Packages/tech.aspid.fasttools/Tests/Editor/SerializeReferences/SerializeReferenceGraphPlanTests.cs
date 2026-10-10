using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using CardKind = Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceGraphPlan.CardKind;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Pins what SerializeReferenceGraphPlan draws for a graph that the YAML scanner would build: each reference
    // expands its children once, under a field that the Editor can open, references that point at each other stay
    // linear, and the card budget only counts what it leaves out. Works on hand-built documents, so it needs no asset.
    [TestFixture]
    internal sealed class SerializeReferenceGraphPlanTests
    {
        private const int Budget = 10_000;

        private static ReferenceGraphDocument Document(params long[] rids)
        {
            var document = new ReferenceGraphDocument { FileId = 7 };
            foreach (var rid in rids)
                document.Nodes.Add(new ReferenceGraphNode(rid, new ManagedTypeName("Asm", "Ns", "Type" + rid), resolves: true));

            return document;
        }

        private static void Root(ReferenceGraphDocument document, long rid, string label) =>
            document.Roots.Add(new ReferenceGraphRoot(rid, label));

        private static void Edge(ReferenceGraphDocument document, long parent, long child, string label)
        {
            if (!document.Edges.TryGetValue(parent, out var children))
                document.Edges[parent] = children = new List<ReferenceGraphEdge>();

            children.Add(new ReferenceGraphEdge(child, label));
        }

        private static string[] Describe(SerializeReferenceGraphPlan plan) =>
            plan.Cards.Select(card => $"{card.Kind} {card.Rid} {card.Path}".TrimEnd()).ToArray();

        [Test]
        public void Build_SharedChild_ExpandsItOnce()
        {
            var document = Document(1, 2, 3, 4, 5);
            Root(document, rid: 1, label: "a");
            Edge(document, parent: 1, child: 2, label: "b");
            Edge(document, parent: 1, child: 3, label: "c");
            Edge(document, parent: 2, child: 4, label: "d");
            Edge(document, parent: 3, child: 4, label: "d");
            Edge(document, parent: 4, child: 5, label: "e");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            // The second path to 4 keeps its card and field path, but 5 is not drawn under it again.
            CollectionAssert.AreEqual(
                new[] { "Node 1 a", "Node 2 a.b", "Node 4 a.b.d", "Node 5 a.b.d.e", "Node 3 a.c", "Repeat 4 a.c.d" },
                Describe(plan));
            Assert.AreEqual(0, plan.Hidden);
        }

        [Test]
        public void Build_Cycle_ClosesWithCompactBackEdge()
        {
            var document = Document(1, 2);
            Root(document, rid: 1, label: "r");
            Edge(document, parent: 1, child: 2, label: "b");
            Edge(document, parent: 2, child: 1, label: "a");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(new[] { "Node 1 r", "Node 2 r.b", "BackEdge 1" }, Describe(plan));
        }

        // Every reference points at every other one: the old walk drew a card per simple path, about 108 million here.
        [Test]
        public void Build_EveryReferenceReachesEveryOther_CardsStayLinear()
        {
            const int count = 12;

            var document = Document(Enumerable.Range(0, count).Select(rid => (long)rid).ToArray());
            Root(document, rid: 0, label: "r");

            for (var parent = 0; parent < count; parent++)
            {
                for (var child = 0; child < count; child++)
                {
                    if (child != parent) Edge(document, parent, child, "f" + child);
                }
            }

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            // The root, then one card per edge.
            Assert.AreEqual(1 + count * (count - 1), plan.Cards.Count);
            Assert.AreEqual(count, plan.Cards.Count(card => card.Kind == CardKind.Node));
            Assert.AreEqual(0, plan.Hidden);
        }

        [Test]
        public void Build_AliasedRoots_SecondRootKeepsItsOwnCard()
        {
            var document = Document(1, 2);
            Root(document, rid: 1, label: "_a");
            Root(document, rid: 1, label: "_b");
            Edge(document, parent: 1, child: 2, label: "x");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(new[] { "Node 1 _a", "Node 2 _a.x", "Repeat 1 _b" }, Describe(plan));
        }

        private static void Missing(ReferenceGraphDocument document, long rid) =>
            document.Nodes.Add(new ReferenceGraphNode(rid, new ManagedTypeName("Asm", "Ns", "Ghost" + rid), resolves: false));

        // Serialization does not enter a missing reference, so a healthy field is the only path where the children of
        // a shared reference can be edited and where the required-field scan reports them.
        [Test]
        public void Build_MissingRoot_IsDrawnFirstAndYieldsTheSharedSubtreeToAHealthyField()
        {
            var document = Document(1, 3, 4);
            Missing(document, rid: 2);
            Root(document, rid: 1, label: "_healthy");
            Root(document, rid: 2, label: "_missing");
            Edge(document, parent: 1, child: 3, label: "x");
            Edge(document, parent: 2, child: 3, label: "y");
            Edge(document, parent: 3, child: 4, label: "z");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(
                new[] { "Node 2 _missing", "Repeat 3 _missing.y", "Node 1 _healthy", "Node 3 _healthy.x", "Node 4 _healthy.x.z" },
                Describe(plan));
        }

        [Test]
        public void Build_MissingRootFirstInTheFile_StillYieldsTheSharedSubtreeToAHealthyField()
        {
            var document = Document(1, 3, 4);
            Missing(document, rid: 2);
            Root(document, rid: 2, label: "_missing");
            Root(document, rid: 1, label: "_healthy");
            Edge(document, parent: 1, child: 3, label: "x");
            Edge(document, parent: 2, child: 3, label: "y");
            Edge(document, parent: 3, child: 4, label: "z");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(
                new[] { "Node 2 _missing", "Repeat 3 _missing.y", "Node 1 _healthy", "Node 3 _healthy.x", "Node 4 _healthy.x.z" },
                Describe(plan));
        }

        [Test]
        public void Build_MissingNestedReference_YieldsTheSharedSubtreeToAHealthyField()
        {
            var document = Document(1, 3, 4);
            Missing(document, rid: 2);
            Root(document, rid: 1, label: "_a");
            Edge(document, parent: 1, child: 2, label: "gone");
            Edge(document, parent: 1, child: 3, label: "x");
            Edge(document, parent: 2, child: 3, label: "y");
            Edge(document, parent: 3, child: 4, label: "z");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(
                new[] { "Node 1 _a", "Node 2 _a.gone", "Repeat 3 _a.gone.y", "Node 3 _a.x", "Node 4 _a.x.z" },
                Describe(plan));
        }

        [Test]
        public void Build_SubtreeOnlyBelowAMissingReference_IsStillDrawnThere()
        {
            var document = Document(3, 4);
            Missing(document, rid: 2);
            Root(document, rid: 2, label: "_missing");
            Edge(document, parent: 2, child: 3, label: "y");
            Edge(document, parent: 3, child: 4, label: "z");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(
                new[] { "Node 2 _missing", "Node 3 _missing.y", "Node 4 _missing.y.z" },
                Describe(plan));
        }

        [Test]
        public void Build_EmptySlots_AreCardsOnlyUnderAnExpandedParent()
        {
            var document = Document(1);
            Root(document, rid: 1, label: "_a");
            Root(document, rid: 1, label: "_b");
            Root(document, rid: -2, label: "_c");
            Edge(document, parent: 1, child: -2, label: "slot");

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(
                new[] { "Node 1 _a", "Empty -2 _a.slot", "Repeat 1 _b", "Empty -2 _c" },
                Describe(plan));

            var paths = SerializeReferenceGraphAnalysis.CollectEmptySlotPaths(new List<SerializeReferenceGraphPlan> { plan });
            CollectionAssert.AreEquivalent(new[] { (7L, "_a.slot"), (7L, "_c") }, paths);
        }

        [Test]
        public void Build_Orphans_ComeAfterTheGraph()
        {
            var document = Document(1, 9);
            Root(document, rid: 1, label: "_a");
            document.Orphans.Add(9);

            var plan = SerializeReferenceGraphPlan.Build(document, Budget);

            CollectionAssert.AreEqual(new[] { "Node 1 _a", "Orphan 9" }, Describe(plan));
        }

        [Test]
        public void Build_BudgetRunsOut_CountsTheCardsItLeavesOut()
        {
            var document = Document(1, 2, 3, 4, 5);
            Root(document, rid: 1, label: "r");
            for (var rid = 1; rid < 5; rid++) Edge(document, rid, rid + 1, "n");

            var plan = SerializeReferenceGraphPlan.Build(document, budget: 3);

            CollectionAssert.AreEqual(new[] { "Node 1 r", "Node 2 r.n", "Node 3 r.n.n" }, Describe(plan));
            Assert.AreEqual(2, plan.Hidden);
        }

        [Test]
        public void Build_ZeroBudget_HidesEveryCard()
        {
            var document = Document(1);
            Root(document, rid: 1, label: "r");
            Root(document, rid: -2, label: "s");

            var plan = SerializeReferenceGraphPlan.Build(document, budget: 0);

            Assert.IsEmpty(plan.Cards);
            Assert.AreEqual(2, plan.Hidden);
        }

        [Test]
        public void Build_BudgetRunsOut_CountsTheEmptyFieldsItLeavesOut()
        {
            var document = Document(1);
            Root(document, rid: 1, label: "r");
            Edge(document, parent: 1, child: -2, label: "a");
            Edge(document, parent: 1, child: -2, label: "b");

            var plan = SerializeReferenceGraphPlan.Build(document, budget: 2);

            CollectionAssert.AreEqual(new[] { "Node 1 r", "Empty -2 r.a" }, Describe(plan));
            Assert.AreEqual(1, plan.Hidden);
            Assert.AreEqual(1, plan.HiddenEmpty);
        }

        [Test]
        public void BuildAll_SharesOneBudgetAcrossTheDocuments()
        {
            var documents = new List<ReferenceGraphDocument>();
            for (var i = 0; i < 3; i++)
            {
                var document = Document(1, 2, 3);
                Root(document, rid: 1, label: "r");
                Edge(document, parent: 1, child: 2, label: "b");
                Edge(document, parent: 2, child: 3, label: "c");
                documents.Add(document);
            }

            var plans = SerializeReferenceGraphPlan.BuildAll(documents, budget: 5);

            CollectionAssert.AreEqual(new[] { 3, 2, 0 }, plans.Select(plan => plan.Cards.Count));
            CollectionAssert.AreEqual(new[] { 0, 1, 3 }, plans.Select(plan => plan.Hidden));
            Assert.AreSame(documents[2], plans[2].Document);
        }

        // A recursive walk overflowed the stack on a long chain; Unity cannot recover from that.
        [Test]
        public void Build_LongChain_DoesNotOverflowTheStack()
        {
            const int length = 50_000;

            var document = Document(Enumerable.Range(0, length).Select(rid => (long)rid).ToArray());
            Root(document, rid: 0, label: "r");
            for (var rid = 0; rid < length - 1; rid++) Edge(document, rid, rid + 1, "n");

            var plan = SerializeReferenceGraphPlan.Build(document, budget: 100);

            Assert.AreEqual(100, plan.Cards.Count);
            Assert.AreEqual(length - 100, plan.Hidden);
        }

        private static GateViolation Required(long rid, string path) =>
            new(assetPath: "Assets/A.prefab", fileId: 7, rid: rid, storedType: default, kind: GateViolationKind.RequiredUnset, fieldPath: path);

        [Test]
        public void SelectRequiredCards_WithinTheBudget_KeepsEveryViolation()
        {
            var violations = new List<GateViolation> { Required(-2, "a"), Required(0, "b") };

            var cards = SerializeReferenceGraphAnalysis.SelectRequiredCards(violations, budget: 2, out var hidden);

            Assert.AreEqual(2, cards.Count);
            Assert.AreEqual(0, hidden);
        }

        // A required string field has no node in the graph, so it has no other card; a reference past the budget does.
        [Test]
        public void SelectRequiredCards_BudgetSpent_LeavesOutReferencesButKeepsStrings()
        {
            var violations = new List<GateViolation>
            {
                Required(-2, "a"), Required(0, "b"), Required(-2, "c"), Required(0, "d"), Required(-2, "e"),
            };

            var cards = SerializeReferenceGraphAnalysis.SelectRequiredCards(violations, budget: 1, out var hidden);

            CollectionAssert.AreEqual(new[] { "a", "b", "d" }, cards.Select(card => card.FieldPath));
            Assert.AreEqual(2, hidden);
        }

        [Test]
        public void SelectRequiredCards_NoBudget_StillKeepsStrings()
        {
            var violations = new List<GateViolation> { Required(-2, "a"), Required(0, "b") };

            var cards = SerializeReferenceGraphAnalysis.SelectRequiredCards(violations, budget: 0, out var hidden);

            CollectionAssert.AreEqual(new[] { "b" }, cards.Select(card => card.FieldPath));
            Assert.AreEqual(1, hidden);
        }

        [Test]
        public void FindNode_FindsFirstMatchAndNodesAddedLater()
        {
            var document = Document(1, 2);
            Assert.AreEqual(2, document.FindNode(2)?.Rid);
            Assert.IsNull(document.FindNode(3));

            document.Nodes.Add(new ReferenceGraphNode(3, default, resolves: true));
            Assert.AreEqual(3, document.FindNode(3)?.Rid);
        }
    }
}
