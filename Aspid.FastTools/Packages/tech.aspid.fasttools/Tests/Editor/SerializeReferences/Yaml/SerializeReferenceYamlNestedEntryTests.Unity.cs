using System.Linq;
using NUnit.Framework;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The tests of this fixture that need Unity or the package's Editor assembly. Aspid.FastTools.YamlTests runs
    // the rest without Unity and leaves the *.Unity.cs parts out.
    internal sealed partial class SerializeReferenceYamlNestedEntryTests
    {
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
        public void TypeUsageIndex_NestedPointerBeforeEntry_GroupsEachRidUnderOwnType()
        {
            // Seeds the index with this one file instead of warming it over the whole project.
            var indexField = typeof(SerializeReferenceTypeUsageIndex).GetField("_index", BindingFlags.NonPublic | BindingFlags.Static);
            var addAsset = typeof(SerializeReferenceTypeUsageIndex).GetMethod("AddAsset", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(indexField);
            Assert.IsNotNull(addAsset);

            var previous = indexField.GetValue(null);
            try
            {
                indexField.SetValue(null, new Dictionary<string, HashSet<SerializeReferenceTypeUsageIndex.Usage>>(System.StringComparer.Ordinal));
                addAsset.Invoke(null, new object[] { _path, "nested-children" });

                CollectionAssert.AreEquivalent(new[] { YamlFixtures.NestedChildrenSequenceRid, ChildRid, GhostRid },
                    SerializeReferenceTypeUsageIndex.AllUsages().Select(usage => usage.Rid).ToArray(), "One usage per entry.");

                var childA = SerializeReferenceTypeUsageIndex.FindUsages(
                    SerializeReferenceHelpers.StoredTypeKey(new ManagedTypeName("Assembly-CSharp", "Game", "ChildA")));
                CollectionAssert.AreEqual(new[] { ChildRid }, childA.Select(usage => usage.Rid).ToArray(),
                    "The missing rid must not be grouped under its healthy neighbour's type.");

                var ghost = SerializeReferenceTypeUsageIndex.FindUsages(
                    SerializeReferenceHelpers.StoredTypeKey(new ManagedTypeName("Assembly-CSharp", "Game", "GhostChild")));
                CollectionAssert.AreEqual(new[] { GhostRid }, ghost.Select(usage => usage.Rid).ToArray());
            }
            finally
            {
                indexField.SetValue(null, previous);
            }
        }
    }
}
