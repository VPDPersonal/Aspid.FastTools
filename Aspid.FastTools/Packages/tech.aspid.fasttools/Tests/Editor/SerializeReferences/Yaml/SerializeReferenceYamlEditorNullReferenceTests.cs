using System.IO;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceYamlEditor.TryNullReference"/> — the most surgery-heavy, non-undoable
    /// write path: it nulls every pointer to a rid (to <c>-2</c>), removes the now-orphaned <c>RefIds</c> entry, and
    /// inserts Unity's shared null-sentinel entry exactly once when a null pointer was introduced. Also covers the
    /// companion <see cref="SerializeReferenceYamlEditor.CountPointersTo"/>, whose count the clear-confirmation dialog
    /// names so an aliased reference doesn't silently null sibling slots. These tests pin that contract against temp
    /// files so a refactor cannot silently change null-vs-real-rid semantics or drift the dialog count off the rewrite.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceYamlEditorNullReferenceTests
    {
        // The empty type identity Unity writes for the null-sentinel RefIds entry — unique to the sentinel, so its
        // occurrence count is the number of sentinels in the file.
        private const string NullSentinelType = "type: {class: , ns: , asm: }";

        private const long MixedIndentFileId = 11400000L;
        private const long TabIndentedRid = 1001L;
        private const long SpaceIndentedRid = 1002L;

        // rid 1001 has a tab-indented data line, so nulling it bails after its pointer is already nulled in memory;
        // rid 1002 is a normal space-indented entry. Built with explicit \t and \n so the indentation is unambiguous.
        private const string MixedIndentAsset =
            "%YAML 1.1\n" +
            "%TAG !u! tag:unity3d.com,2011:\n" +
            "--- !u!114 &11400000\n" +
            "MonoBehaviour:\n" +
            "  m_ObjectHideFlags: 0\n" +
            "  m_Name: MixedIndentAsset\n" +
            "  _first:\n" +
            "    rid: 1001\n" +
            "  _second:\n" +
            "    rid: 1002\n" +
            "  references:\n" +
            "    version: 2\n" +
            "    RefIds:\n" +
            "    - rid: 1001\n" +
            "      type: {class: GhostFirst, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}\n" +
            "      data:\n" +
            "\t\t\t\t\t\t\t\t_damage: 10\n" +
            "    - rid: 1002\n" +
            "      type: {class: GhostSecond, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}\n" +
            "      data:\n" +
            "        _damage: 20\n";

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
            finally
            {
                YamlFixtures.Delete(path);
            }
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
            finally
            {
                YamlFixtures.Delete(path);
            }
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
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void TryNullReference_AliasedRid_NullsEverySharedSlot_LeavesSiblingIntact()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.AliasedMissingTypePrefab);
            try
            {
                // The missing rid is aliased across _primaryWeapon and _sidearms[0]; clearing it must null both.
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid));

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, YamlFixtures.MonoBehaviourFileId, "_primaryWeapon", out var primary));
                Assert.AreEqual(-2, primary, "The first aliased slot must read the null id after the clear.");

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, YamlFixtures.MonoBehaviourFileId, "_sidearms.Array.data[0]", out var sidearm0));
                Assert.AreEqual(-2, sidearm0, "The second aliased slot must read the null id after the clear.");

                // The singly-pointed sibling that did NOT share the rid keeps its reference.
                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, YamlFixtures.MonoBehaviourFileId, "_sidearms.Array.data[1]", out var sidearm1));
                Assert.AreEqual(YamlFixtures.ShotgunRid, sidearm1, "A slot that didn't alias the rid must be untouched.");

                var after = File.ReadAllText(path);
                StringAssert.DoesNotContain("GhostPistol", after);
                Assert.AreEqual(1, CountOccurrences(after, NullSentinelType),
                    "Two nulled aliases still share Unity's single null sentinel.");
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void NullReferences_SeveralEntries_WritesWhatOneNullPerEntryWrites()
        {
            var entries = new[]
            {
                new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, storedType: default),
                new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.ShotgunRid, storedType: default),
                new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.FreezeEffectRid, storedType: default),
            };

            var path = YamlFixtures.WriteTemp(YamlFixtures.AliasedMissingTypePrefab);
            var expectedPath = YamlFixtures.WriteTemp(YamlFixtures.AliasedMissingTypePrefab);
            try
            {
                foreach (var entry in entries)
                    Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(expectedPath, entry.FileId, entry.Rid));

                Assert.AreEqual(entries.Length, SerializeReferenceYamlEditor.NullReferences(path, entries));

                var after = File.ReadAllText(path);
                Assert.AreEqual(File.ReadAllText(expectedPath), after,
                    "One write for all entries must give the same file as one write per entry.");
                Assert.AreEqual(1, CountOccurrences(after, NullSentinelType),
                    "Every nulled entry shares Unity's single null sentinel.");
            }
            finally
            {
                YamlFixtures.Delete(path);
                YamlFixtures.Delete(expectedPath);
            }
        }

        [Test]
        public void NullReferences_EntryThatBails_KeepsItsPointer_AndTheOthersAreNulled()
        {
            var entries = new[]
            {
                new MissingReferenceEntry(MixedIndentFileId, TabIndentedRid, storedType: default),
                new MissingReferenceEntry(MixedIndentFileId, SpaceIndentedRid, storedType: default),
            };

            var path = YamlFixtures.WriteTemp(MixedIndentAsset);
            try
            {
                Assert.AreEqual(1, SerializeReferenceYamlEditor.NullReferences(path, entries),
                    "The tab-indented entry must bail; the space-indented one must still be nulled.");

                // The bailed entry nulled its pointer before the indent check; that edit must not reach the file.
                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(path, MixedIndentFileId, "_first", out var first));
                Assert.AreEqual(TabIndentedRid, first, "The pointer of an entry that bailed must stay unchanged.");

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(path, MixedIndentFileId, "_second", out var second));
                Assert.AreEqual(-2, second);

                var after = File.ReadAllText(path);
                StringAssert.Contains("GhostFirst", after);
                StringAssert.DoesNotContain("GhostSecond", after);
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void NullReferences_EveryEntryStale_ReturnsZero_AndLeavesFileUnchanged()
        {
            var entries = new[]
            {
                new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, rid: 987654, storedType: default),
                new MissingReferenceEntry(fileId: 424242, YamlFixtures.GhostPistolRid, storedType: default),
            };

            var path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            try
            {
                var before = File.ReadAllText(path);
                Assert.AreEqual(0, SerializeReferenceYamlEditor.NullReferences(path, entries));
                Assert.AreEqual(before, File.ReadAllText(path), "A batch with nothing to null must not write the file.");
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void CountPointersTo_AliasedRid_CountsEverySharedSlot()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.AliasedMissingTypePrefab);
            try
            {
                // _primaryWeapon + _sidearms[0] both point at the rid; the RefIds entry header is NOT a pointer.
                Assert.AreEqual(2, SerializeReferenceYamlEditor.CountPointersTo(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid));
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void CountPointersTo_SingleSlotMissingRid_ReturnsOne()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            try
            {
                // The missing rid is pointed at by exactly one slot (_sidearms[0]) in this fixture.
                Assert.AreEqual(1, SerializeReferenceYamlEditor.CountPointersTo(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid));
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void CountPointersTo_MatchesPointersNulledByTryNullReference()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.AliasedMissingTypePrefab);
            try
            {
                // The count the dialog shows must equal the number of "rid: -2" pointers the rewrite introduces, so the
                // confirmation can never under- or over-state the damage. The fixture starts with no null pointers.
                var before = File.ReadAllText(path);
                Assert.AreEqual(0, CountOccurrences(before, "rid: -2"), "Fixture sanity: no null pointers before the clear.");

                var count = SerializeReferenceYamlEditor.CountPointersTo(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);

                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid));

                // The introduced null pointers are the field pointers (the inserted sentinel ENTRY header is "- rid: -2"
                // at the entry indent, so subtract that one occurrence to compare against the field-pointer count).
                var after = File.ReadAllText(path);
                Assert.AreEqual(count, CountOccurrences(after, "rid: -2") - 1,
                    "CountPointersTo must equal the number of field pointers TryNullReference nulls.");
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void CountPointersTo_UnknownRid_ReturnsZero()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            try
            {
                Assert.AreEqual(0, SerializeReferenceYamlEditor.CountPointersTo(
                    path, YamlFixtures.MonoBehaviourFileId, 987654));
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
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
