using System;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Read-side coverage for the YAML engine: missing-type discovery, propertyPath -> rid resolution (top-level,
    /// list-element and nested managed chains), stored-type reading, and field-name parsing. All tests drive the public
    /// static methods on the internal <see cref="SerializeReferenceYamlEditor"/> against temp files — no asset import,
    /// no SerializedObject — so they exercise the parser in isolation.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceYamlEditorTests
    {
        private string _path;

        // A stored type is "resolvable" unless its class name starts with "Ghost" — the fixture's deliberately-missing
        // GhostPistol. This keeps the test independent of which types actually load in the test domain.
        private static bool Resolves(ManagedTypeName type) =>
            !type.Class.StartsWith("Ghost", StringComparison.Ordinal);

        [SetUp]
        public void SetUp() => _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);

        [TearDown]
        public void TearDown() => YamlFixtures.Delete(_path);

        [Test]
        public void FindMissingReferences_ReportsOnlyUnresolvableEntry()
        {
            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, Resolves);

            Assert.AreEqual(1, missing.Count, "Only GhostPistol should be reported as missing.");
            Assert.AreEqual(YamlFixtures.GhostPistolRid, missing[0].Rid);
            Assert.AreEqual(YamlFixtures.MonoBehaviourFileId, missing[0].FileId);
            Assert.AreEqual("GhostPistol", missing[0].StoredType.Class);
        }

        [Test]
        public void FindMissingReferences_AllResolvable_ReturnsEmpty()
        {
            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, _ => true);
            Assert.AreEqual(0, missing.Count);
        }

        [Test]
        public void TryReadReferenceId_SingleField_ResolvesRid()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, YamlFixtures.MonoBehaviourFileId, "_primaryWeapon", out var rid));
            Assert.AreEqual(YamlFixtures.RailgunRid, rid);
        }

        [Test]
        public void TryReadReferenceId_ListElement_ResolvesRid()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, YamlFixtures.MonoBehaviourFileId, "_sidearms.Array.data[0]", out var first));
            Assert.AreEqual(YamlFixtures.GhostPistolRid, first);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, YamlFixtures.MonoBehaviourFileId, "_sidearms.Array.data[1]", out var second));
            Assert.AreEqual(YamlFixtures.ShotgunRid, second);
        }

        [Test]
        public void TryReadReferenceId_NestedManagedChain_ResolvesRid()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, YamlFixtures.MonoBehaviourFileId, "_primaryWeapon._chargeEffect", out var rid));
            Assert.AreEqual(YamlFixtures.BurnEffectRid, rid);
        }

        [Test]
        public void TryReadReferenceId_UnknownPath_ReturnsFalse()
        {
            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, YamlFixtures.MonoBehaviourFileId, "_doesNotExist", out _));
        }

        [Test]
        public void TryReadStoredType_ReturnsRidAndType()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(
                _path, YamlFixtures.MonoBehaviourFileId, "_sidearms.Array.data[0]", out var rid, out var type));
            Assert.AreEqual(YamlFixtures.GhostPistolRid, rid);
            Assert.AreEqual("GhostPistol", type.Class);
            Assert.AreEqual("Aspid.FastTools.Samples.SerializeReferences", type.Namespace);
        }

        [Test]
        public void GetReferenceFieldNames_ReturnsTopLevelDataKeys()
        {
            var pistolFields = SerializeReferenceYamlEditor.GetReferenceFieldNames(
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid);
            CollectionAssert.AreEquivalent(new[] { "_damage", "_magazineSize" }, pistolFields);

            // Railgun's data has a nested managed ref (_chargeEffect -> rid); only the top-level keys are reported.
            var railgunFields = SerializeReferenceYamlEditor.GetReferenceFieldNames(
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.RailgunRid);
            CollectionAssert.AreEquivalent(new[] { "_chargeTime", "_chargeEffect" }, railgunFields);
        }

        [Test]
        public void ParseTopLevelFieldNames_SkipsIndentedAndSequenceLines()
        {
            var names = SerializeReferenceYamlEditor.ParseTopLevelFieldNames(
                "_damage: 15\n_magazineSize: 12\n  _nested: 3\n- 7\n");
            CollectionAssert.AreEqual(new[] { "_damage", "_magazineSize" }, names);
        }
    }
}
