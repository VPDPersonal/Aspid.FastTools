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

        private const long UserRidFieldsFileId = 11400000L;
        private const long UserRidFieldsTarget = 1002L;

        // Fields of the user's own named rid hold 1002, the rid being cleared: one beside the object's other fields, the
        // first key of a list element, the last field of a struct, the first field of a managed reference, the last
        // field of a struct inside it, and the only field of another managed reference. Only _weapon and _list[0]
        // point at rid 1002.
        private const string UserRidFieldsAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: b7874533c7294db1b8aa77e7d4102c9f, type: 3}
  m_Name: UserRidFields
  rid: 1002
  _weapon:
    rid: 1002
  _list:
  - rid: 1002
  - rid: 1003
  _records:
  - rid: 1002
    _name: first
  _config:
    _name: config
    rid: 1002
  references:
    version: 2
    RefIds:
    - rid: 1002
      type: {class: GhostPistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 15
    - rid: 1003
      type: {class: Shotgun, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        rid: 1002
        _pellets: 8
        _ammo:
          _count: 4
          rid: 1002
    - rid: 1004
      type: {class: Counter, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        rid: 1002
";

        private const long BackingFieldsFileId = 11400000L;
        private const long BackingFieldsTarget = 1002L;
        private const long BackingFieldsDangling = 1005L;

        // Scalar pointers under keys that do not start with a letter or an underscore: the backing field of a
        // [field: SerializeReference] auto-property and a non-ASCII field name. Rid 1002 is pointed at three times, once
        // inside another entry's data; rid 1005 has a pointer but no RefIds entry.
        private const string BackingFieldsAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: b7874533c7294db1b8aa77e7d4102c9f, type: 3}
  m_Name: BackingFields
  <Weapon>k__BackingField:
    rid: 1002
  _оружие:
    rid: 1002
  <Holster>k__BackingField:
    rid: 1003
  <Spare>k__BackingField:
    rid: 1005
  references:
    version: 2
    RefIds:
    - rid: 1002
      type: {class: GhostPistol, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        _damage: 15
    - rid: 1003
      type: {class: Holster, ns: Aspid.FastTools.Samples.SerializeReferences, asm: Aspid.FastTools.Samples.SerializeReferences}
      data:
        <Weapon>k__BackingField:
          rid: 1002
";

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

        [Test]
        public void TryNullReference_UserFieldsNamedRid_KeepTheirValues()
        {
            var path = YamlFixtures.WriteTemp(UserRidFieldsAsset);
            try
            {
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(path, UserRidFieldsFileId, UserRidFieldsTarget));

                var after = File.ReadAllText(path);
                Assert.AreEqual(6, CountOccurrences(after, "rid: 1002"), "Every field of the user's own named rid must keep 1002.");
                StringAssert.DoesNotContain("GhostPistol", after);
                Assert.AreEqual(1, CountOccurrences(after, NullSentinelType));

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(path, UserRidFieldsFileId, "_weapon", out var weapon));
                Assert.AreEqual(-2, weapon, "The field pointer must be nulled.");

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, UserRidFieldsFileId, "_list.Array.data[0]", out var first));
                Assert.AreEqual(-2, first, "The list element pointer must be nulled.");

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, UserRidFieldsFileId, "_list.Array.data[1]", out var second));
                Assert.AreEqual(1003, second);
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void CountPointersTo_UserFieldsNamedRid_CountsOnlyPointers()
        {
            var path = YamlFixtures.WriteTemp(UserRidFieldsAsset);
            try
            {
                Assert.AreEqual(2, SerializeReferenceYamlEditor.CountPointersTo(path, UserRidFieldsFileId, UserRidFieldsTarget));
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void TryNullReference_PointersUnderBackingFieldAndNonAsciiKeys_AreNulled()
        {
            var path = YamlFixtures.WriteTemp(BackingFieldsAsset);
            try
            {
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(path, BackingFieldsFileId, BackingFieldsTarget));

                var after = File.ReadAllText(path);
                Assert.AreEqual(0, CountOccurrences(after, "rid: 1002"), "No pointer to the removed entry may survive.");
                Assert.AreEqual(4, CountOccurrences(after, "rid: -2"), "Three nulled pointers and the null sentinel.");
                StringAssert.DoesNotContain("GhostPistol", after);
                StringAssert.Contains("Holster", after);

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, BackingFieldsFileId, "<Weapon>k__BackingField", out var weapon));
                Assert.AreEqual(-2, weapon);

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(path, BackingFieldsFileId, "_оружие", out var other));
                Assert.AreEqual(-2, other);
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void CountPointersTo_PointersUnderBackingFieldAndNonAsciiKeys_CountsThem()
        {
            var path = YamlFixtures.WriteTemp(BackingFieldsAsset);
            try
            {
                Assert.AreEqual(3, SerializeReferenceYamlEditor.CountPointersTo(path, BackingFieldsFileId, BackingFieldsTarget));
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        [Test]
        public void TryNullReference_DanglingPointerUnderBackingField_IsNulled()
        {
            var path = YamlFixtures.WriteTemp(BackingFieldsAsset);
            try
            {
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(path, BackingFieldsFileId, BackingFieldsDangling));

                Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                    path, BackingFieldsFileId, "<Spare>k__BackingField", out var spare));
                Assert.AreEqual(-2, spare);
                Assert.AreEqual(1, CountOccurrences(File.ReadAllText(path), NullSentinelType));
            }
            finally
            {
                YamlFixtures.Delete(path);
            }
        }

        // On CoreCLR, Swedish formats -2 with U+2212, which Unity cannot read.
        [Test]
        [SetCulture("sv-SE")]
        public void TryNullReference_LocaleWithUnicodeMinus_WritesAsciiNullId()
        {
            var path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);
            try
            {
                Assert.IsTrue(SerializeReferenceYamlEditor.TryNullReference(
                    path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid));

                Assert.AreEqual(2, CountOccurrences(File.ReadAllText(path), "- rid: -2"),
                    "The nulled pointer and the null sentinel must use an ASCII minus.");
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
