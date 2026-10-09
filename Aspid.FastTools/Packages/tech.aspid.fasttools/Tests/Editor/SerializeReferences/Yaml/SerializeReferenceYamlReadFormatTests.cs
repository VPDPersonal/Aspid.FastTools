using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // How the YAML engine reads what Unity writes beyond the common case: the files it scans, quoted and escaped type
    // names, the version of the managed reference registry, and a list that is empty on disk.
    [TestFixture]
    internal sealed class SerializeReferenceYamlReadFormatTests
    {
        private const long ArsenalFileId = 11400000;
        private const long LoadoutFileId = 6500000000000000003L;

        // Unity writes a name with characters outside printable ASCII as a double-quoted scalar of "\uXXXX" escapes,
        // here a Cyrillic class and namespace.
        private const string EscapedTypeAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name: Arsenal
  _weapon:
    rid: 2001
  references:
    version: 2
    RefIds:
    - rid: 2001
      type: {class: ""\u041F\u0438\u0441\u0442\u043E\u043B\u0435\u0442"", ns: ""\u041E\u0440\u0443\u0436\u0438\u0435"", asm: Assembly-CSharp}
      data:
        _damage: 15
";

        // A SerializableType wrapper stores its name as one scalar, escaped the same way.
        private const string EscapedTypeNameAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name: Holder
  _fallback:
    _assemblyQualifiedName: ""Game.\u041C\u0435\u0447, Assembly-CSharp""
";

        private const string PistolClass = "\u041F\u0438\u0441\u0442\u043E\u043B\u0435\u0442";
        private const string WeaponsNamespace = "\u041E\u0440\u0443\u0436\u0438\u0435";

        // _sidearms and _slots[0]._weapons are empty on disk; the next key of each holds a list of its own.
        private const string EmptyListsPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &6500000000000000003
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name:
  _sidearms: []
  _backups:
  - rid: 1002
  _slots:
  - _weapons: []
    _spares:
    - rid: 1003
  references:
    version: 2
    RefIds:
    - rid: 1002
      type: {class: GhostPistol, ns: Game, asm: Assembly-CSharp}
      data:
        _damage: 15
    - rid: 1003
      type: {class: GhostRifle, ns: Game, asm: Assembly-CSharp}
      data:
        _damage: 30
";

        // Registry version 1, written before Unity 2021.2: entries keyed by id, no RefIds.
        private const string VersionOneAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name: Legacy
  _weapon:
    id: 0
  references:
    version: 1
    00000000:
      type: {class: GhostPistol, ns: Game, asm: Assembly-CSharp}
      data:
        _damage: 15
";

        private const string ReferencesFieldAsset =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name: Links
  references:
  - {fileID: 0}
  _count: 1
";

        private string _path;

        [TearDown]
        public void TearDown() =>
            YamlFixtures.Delete(_path);

        [TestCase("Assets/Weapons/Pistol.prefab")]
        [TestCase("Assets/Data/Loadout.asset")]
        [TestCase("Assets/Scenes/Arena.unity")]
        [TestCase("Assets/Animation/Enemy.controller")]
        [TestCase("Assets/Timeline/Intro.playable")]
        public void IsCandidateAssetPath_FileThatHoldsScriptObjects_IsScanned(string path) =>
            Assert.IsTrue(SerializeReferenceYaml.IsCandidateAssetPath(path));

        [TestCase("Pistol", "Pistol")]
        [TestCase("'Box`1[[Game.Ammo, Game]]'", "Box`1[[Game.Ammo, Game]]")]
        [TestCase("'It''s'", "It's")]
        [TestCase("\"\\u041F\\u0438\"", "\u041F\u0438")]
        [TestCase("\"\\uD83D\\uDE00 \\U0001F600\"", "\uD83D\uDE00 \U0001F600")]
        [TestCase("\"\\x41\\\"\\\\\\t\"", "A\"\\\t")]
        [TestCase("\"\\u12 \\q\"", "\\u12 \\q")]
        public void UnquoteScalar_ReadsQuotesAndEscapes(string scalar, string expected) =>
            Assert.AreEqual(expected, SerializeReferenceYaml.UnquoteScalar(scalar));

        [Test]
        public void TryParseInlineType_DoubleQuotedNames_AreDecoded()
        {
            Assert.IsTrue(SerializeReferenceYaml.TryParseInlineType(
                "class: \"\\u0422\\u0438\\u043F\", ns: \"\\u0418\\u0433\\u0440\\u0430\", asm: \"Assembly-CSharp\"", out var type));

            Assert.AreEqual("\u0422\u0438\u043F", type.Class);
            Assert.AreEqual("\u0418\u0433\u0440\u0430", type.Namespace);
            Assert.AreEqual("Assembly-CSharp", type.Assembly);
        }

        [Test]
        public void TryReadStoredType_EscapedNonAsciiType_IsDecoded()
        {
            _path = YamlFixtures.WriteTemp(EscapedTypeAsset);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadStoredType(_path, ArsenalFileId, "_weapon", out var rid, out var type));
            Assert.AreEqual(2001, rid);
            Assert.AreEqual(PistolClass, type.Class);
            Assert.AreEqual(WeaponsNamespace, type.Namespace);
        }

        [Test]
        public void FindMissingReferences_EscapedNonAsciiType_ResolvesByItsName()
        {
            _path = YamlFixtures.WriteTemp(EscapedTypeAsset);

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path,
                type => type.Class == PistolClass && type.Namespace == WeaponsNamespace);

            Assert.AreEqual(0, missing.Count, "A healthy class with a non-ASCII name must not be reported as missing.");
        }

        [Test]
        public void FindStoredTypeNames_EscapedNonAsciiName_IsDecoded()
        {
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(Lines(EscapedTypeNameAsset));

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("Game.\u041C\u0435\u0447, Assembly-CSharp", entries[0].TypeName);
        }

        [Test]
        public void TryReadReferenceId_ElementOfListEmptyOnDisk_IsNotFound()
        {
            _path = YamlFixtures.WriteTemp(EmptyListsPrefab);

            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, LoadoutFileId, "_sidearms.Array.data[0]", out _),
                "An empty list must not take the element of the next list.");
            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, LoadoutFileId, "_slots.Array.data[0]._weapons.Array.data[0]", out _),
                "An empty nested list must not take the element of its sibling list.");
            Assert.IsFalse(SerializeReferenceYamlEditor.TryReadStoredType(
                _path, LoadoutFileId, "_sidearms.Array.data[0]", out _, out _));

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, LoadoutFileId, "_backups.Array.data[0]", out var backup));
            Assert.AreEqual(1002, backup);

            Assert.IsTrue(SerializeReferenceYamlEditor.TryReadReferenceId(
                _path, LoadoutFileId, "_slots.Array.data[0]._spares.Array.data[0]", out var spare));
            Assert.AreEqual(1003, spare);
        }

        [Test]
        public void HasUnsupportedReferencesVersion_VersionTwo_IsFalse() =>
            Assert.IsFalse(SerializeReferenceYaml.HasUnsupportedReferencesVersion(Lines(YamlFixtures.MissingTypePrefab)));

        [TestCase("1")]
        [TestCase("3")]
        public void HasUnsupportedReferencesVersion_OtherVersion_IsTrue(string version) =>
            Assert.IsTrue(SerializeReferenceYaml.HasUnsupportedReferencesVersion(
                Lines(VersionOneAsset.Replace("version: 1", "version: " + version))));

        [Test]
        public void HasUnsupportedReferencesVersion_UserFieldNamedReferences_IsFalse() =>
            Assert.IsFalse(SerializeReferenceYaml.HasUnsupportedReferencesVersion(Lines(ReferencesFieldAsset)));

        [Test]
        public void HasUnsupportedReferencesVersion_NoLines_IsFalse() =>
            Assert.IsFalse(SerializeReferenceYaml.HasUnsupportedReferencesVersion(lines: null));

        private static string[] Lines(string yaml) =>
            yaml.Replace("\r\n", "\n").Split('\n');
    }
}
