using System.IO;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceYamlEditor.TryNullReference"/> — the most surgery-heavy, non-undoable
    /// write path: it nulls every pointer to a rid (to <c>-2</c>), removes the now-orphaned <c>RefIds</c> entry, and
    /// inserts Unity's shared null-sentinel entry exactly once when a null pointer was introduced. These tests pin that
    /// contract against temp files so a refactor cannot silently change null-vs-real-rid semantics.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceYamlEditorNullReferenceTests
    {
        // The empty type identity Unity writes for the null-sentinel RefIds entry — unique to the sentinel, so its
        // occurrence count is the number of sentinels in the file.
        private const string NullSentinelType = "type: {class: , ns: , asm: }";

        [Test]
        public void TryNullReference_ListElement_NullsPointer_RemovesEntry_AddsSentinel()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            try
            {
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid));

                var after = File.ReadAllText(path);

                // The broken entry (type + data) is gone, and exactly one null sentinel was inserted.
                StringAssert.DoesNotContain("GhostPistol", after);
                Assert.AreEqual(1, CountOccurrences(after, NullSentinelType),
                    "Nulling a reference must insert the shared null sentinel exactly once.");

                // Sibling references are untouched.
                StringAssert.Contains("Railgun", after);
                StringAssert.Contains("Shotgun", after);

                // The reader now sees the nulled slot as the null id (-2).
                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, YamlFixtures.MonoBehaviourFileId, "_sidearms.Array.data[0]", out var rid));
                Assert.AreEqual(-2, rid);
            }
            finally { YamlFixtures.Delete(path); }
        }

        [Test]
        public void TryNullReference_SecondNull_ReusesSentinel_DoesNotAddAnother()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            try
            {
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid)); // creates the sentinel
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.ShotgunRid));     // must reuse it

                var after = File.ReadAllText(path);
                Assert.AreEqual(1, CountOccurrences(after, NullSentinelType),
                    "The null sentinel is a shared singleton — a second null must not add another.");
                StringAssert.DoesNotContain("GhostPistol", after);
                StringAssert.DoesNotContain("Shotgun", after);
            }
            finally { YamlFixtures.Delete(path); }
        }

        [Test]
        public void TryNullReference_UnknownRid_ReturnsFalse_LeavesFileUnchanged()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            try
            {
                var before = File.ReadAllText(path);
                Assert.IsFalse(SerializeReferenceYamlEditor.TryNullReference(
                    path, YamlFixtures.MonoBehaviourFileId, 987654));
                Assert.AreEqual(before, File.ReadAllText(path), "A no-op null must leave the file byte-identical.");
            }
            finally { YamlFixtures.Delete(path); }
        }

        private static int CountOccurrences(string haystack, string needle)
        {
            var count = 0;
            for (var i = haystack.IndexOf(needle, System.StringComparison.Ordinal);
                 i >= 0;
                 i = haystack.IndexOf(needle, i + needle.Length, System.StringComparison.Ordinal))
            {
                count++;
            }

            return count;
        }
    }
}
