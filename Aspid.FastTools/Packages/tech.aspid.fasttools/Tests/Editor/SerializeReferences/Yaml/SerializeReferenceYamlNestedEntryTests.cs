using System.IO;
using System.Linq;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A "- rid: N" list element inside an earlier entry's data block has the same shape as rid N's own RefIds entry
    // header. Every entry lookup must pick the header at the entry indent, never the nested pointer (which is followed
    // by a different entry), and a restore must never re-point a same-named list nested in another field.
    [TestFixture]
    internal sealed class SerializeReferenceYamlNestedEntryTests
    {
        private const long FileId = YamlFixtures.NestedChildrenFileId;
        private const long ChildRid = YamlFixtures.NestedChildrenChildRid;
        private const long GhostRid = YamlFixtures.NestedChildrenGhostRid;

        private string _path;

        [SetUp]
        public void SetUp() =>
            _path = YamlFixtures.WriteTemp(YamlFixtures.NestedChildrenPrefab);

        [TearDown]
        public void TearDown() =>
            YamlFixtures.Delete(_path);

        [Test]
        public void TryRewriteType_NestedPointerBeforeEntry_RewritesOnlyTargetEntry()
        {
            var before = File.ReadAllLines(_path);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(
                _path, FileId, GhostRid, new ManagedTypeName("Assembly-CSharp", "Game", "FixedChild")));

            var after = File.ReadAllLines(_path);
            var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToArray();

            Assert.AreEqual(1, changed.Length, "Exactly one line (the target entry's type) should change.");
            StringAssert.Contains("GhostChild", before[changed[0]]);
            StringAssert.Contains("FixedChild", after[changed[0]]);
            Assert.IsTrue(after.Any(line => line.Contains("class: ChildA")), "The healthy neighbour keeps its type.");
        }

        [Test]
        public void TryReadStoredType_NestedPointerBeforeEntry_ReadsOwnType()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(
                _path, FileId, "_weapons.Array.data[0]", out var rid, out var type));

            Assert.AreEqual(GhostRid, rid);
            Assert.AreEqual("GhostChild", type.Class);
        }

        [Test]
        public void GetReferenceFieldNames_NestedPointerBeforeEntry_ReadsOwnDataBlock()
        {
            CollectionAssert.AreEqual(new[] { "_a" },
                SerializeReferenceYamlEditor.GetReferenceFieldNames(_path, FileId, ChildRid));
            CollectionAssert.AreEqual(new[] { "_b" },
                SerializeReferenceYamlEditor.GetReferenceFieldNames(_path, FileId, GhostRid));
        }

        [Test]
        public void TryRemoveEntry_NestedPointerBeforeEntry_RemovesEntryNotListElement()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRemoveEntry(_path, FileId, GhostRid));

            var after = File.ReadAllLines(_path);
            Assert.IsTrue(after.Contains("        - rid: 102"), "The parent's list element must survive.");
            Assert.IsFalse(after.Contains("    - rid: 102"), "The entry header must be removed.");
            Assert.IsFalse(after.Any(line => line.Contains("GhostChild")), "The whole entry block must be removed.");
            Assert.IsTrue(after.Contains("        _a: 1"), "The neighbour entry must be untouched.");
        }

        [Test]
        public void TryReadArrayElementEntryBlock_NestedPointerBeforeEntry_CapturesEntry()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadArrayElementEntryBlock(
                _path, FileId, "_weapons.Array.data[0]", out var rid, out var entryLines));

            Assert.AreEqual(GhostRid, rid);
            Assert.AreEqual("    - rid: 102", entryLines[0]);
            Assert.AreEqual(4, entryLines.Count);
            StringAssert.Contains("GhostChild", entryLines[1]);
        }

        [Test]
        public void TryNullReference_NestedPointerBeforeEntry_KeepsNeighbourEntry()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(_path, FileId, GhostRid));

            var after = File.ReadAllLines(_path);
            Assert.IsFalse(after.Any(line => line.Contains("GhostChild")), "The missing entry is removed.");
            Assert.IsTrue(after.Any(line => line.Contains("class: ChildA")), "The healthy neighbour stays.");
            Assert.IsTrue(after.Contains("        _a: 1"));
            Assert.IsTrue(after.Contains("        - rid: -2"), "The nested pointer is nulled.");
            Assert.IsTrue(after.Contains("  - rid: -2"), "The top-level pointer is nulled.");
            Assert.IsTrue(after.Contains("    - rid: -2"), "The null sentinel entry is added.");
        }

        [Test]
        public void FindMissingReferences_NestedPointerBeforeEntry_ReportsOnlyMissingEntry()
        {
            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, type => type.Class != "GhostChild");

            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual(GhostRid, missing[0].Rid);
        }

        [Test]
        public void GraphScanner_NestedPointerBeforeEntry_NoPhantomNodes()
        {
            var documents = SerializeReferenceGraphScanner.Build(_path);
            Assert.AreEqual(1, documents.Count);

            var nodes = documents[0].Nodes;
            CollectionAssert.AreEqual(new[] { YamlFixtures.NestedChildrenSequenceRid, ChildRid, GhostRid },
                nodes.Select(node => node.Rid).ToArray(), "Only entry headers become nodes, each once.");

            Assert.AreEqual("ChildA", documents[0].FindNode(ChildRid)?.StoredType.Class);
            Assert.AreEqual("GhostChild", documents[0].FindNode(GhostRid)?.StoredType.Class);

            var children = documents[0].ChildrenOf(YamlFixtures.NestedChildrenSequenceRid).Select(edge => edge.Rid);
            CollectionAssert.AreEqual(new[] { ChildRid, GhostRid }, children.ToArray());
            CollectionAssert.IsEmpty(documents[0].Orphans);
        }

        [Test]
        public void TryFindTopLevelArrayElementForRid_SameNamedNestedList_ReturnsTopLevelSlot()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.ShadowedListPrefab);
            try
            {
                Assert.IsTrue(SerializeReferenceYamlEditor.TryFindTopLevelArrayElementForRid(
                    path, YamlFixtures.ShadowedListFileId, 300, out var field, out var index));

                Assert.AreEqual("_weapons", field);
                Assert.AreEqual(2, index, "The nested _presets[0]._weapons[1] must not be taken for the top-level list.");
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void TryRestoreArrayElementReference_SameNamedNestedList_RepointsTopLevelSlot()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.ShadowedListPrefab);
            try
            {
                var entry = new[]
                {
                    "    - rid: 200",
                    "      type: {class: GhostPistol, ns: Game, asm: Assembly-CSharp}",
                    "      data:",
                    "        _damage: 15",
                };

                Assert.IsTrue(SerializeReferenceYamlEditor.TryRestoreArrayElementReference(
                    path, YamlFixtures.ShadowedListFileId, "_weapons.Array.data[0]", entry));

                var after = File.ReadAllLines(path);
                var nested = System.Array.IndexOf(after, "    _weapons:");
                var topLevel = System.Array.IndexOf(after, "  _weapons:");

                Assert.AreEqual("    - rid: -2", after[nested + 1], "The nested list's null slot stays null.");
                Assert.AreEqual("  - rid: 301", after[topLevel + 1], "The top-level slot is re-pointed.");
                Assert.AreEqual("  - rid: -2", after[topLevel + 2]);
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }
    }
}
