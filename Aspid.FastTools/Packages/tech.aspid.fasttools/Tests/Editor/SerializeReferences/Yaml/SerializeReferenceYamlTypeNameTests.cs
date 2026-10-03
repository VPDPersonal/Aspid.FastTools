using System.IO;
using System.Linq;
using NUnit.Framework;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The type name scan finds the _assemblyQualifiedName scalar of SerializableType and SerializableMonoScript wrappers
    // in fields, lists, managed reference data and prefab instance overrides, and rewrites it in place. The documents
    // follow Unity 6000.0 output: folded plain scalars, sequence dashes at the key's indent.
    [TestFixture]
    internal sealed class SerializeReferenceYamlTypeNameTests
    {
        private const long SpawnerFileId = 410389316;
        private const long HolderFileId = 11400000;
        private const long InstanceFileId = 7223051798917698576L;
        private const long ReferenceRid = 1001;

        private const string HostGuid = "9d25fc91e7251430aae1004a6e666a9f";
        private const string GruntGuid = "a701e3db0dfb545a4ad8c03a7db1767c";
        private const string SourceGuid = "1d4d79cb574804cd3967d8baa572e9fd";

        private const string GruntName =
            "Aspid.FastTools.Samples.Types.Grunt, Aspid.FastTools.Samples.Types, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        private const string PatternName =
            "Aspid.FastTools.Samples.Types.CirclePattern, Aspid.FastTools.Samples.Types, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        internal const string Scene =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &410389315
GameObject:
  m_ObjectHideFlags: 0
  m_Component:
  - component: {fileID: 410389317}
  - component: {fileID: 410389316}
  m_Name: Enemy Spawner
--- !u!114 &410389316
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_GameObject: {fileID: 410389315}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 9d25fc91e7251430aae1004a6e666a9f, type: 3}
  m_Name:
  m_EditorClassIdentifier: Aspid.FastTools.Samples.Types::Aspid.FastTools.Samples.Types.EnemySpawner
  _enemyType:
    _assemblyQualifiedName: Aspid.FastTools.Samples.Types.Grunt, Aspid.FastTools.Samples.Types,
      Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
    _script: {fileID: 11500000, guid: a701e3db0dfb545a4ad8c03a7db1767c, type: 3}
  _eliteType: Aspid.FastTools.Samples.Types.ArmoredGrunt, Aspid.FastTools.Samples.Types,
    Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
  _pattern:
    _assemblyQualifiedName: Aspid.FastTools.Samples.Types.CirclePattern, Aspid.FastTools.Samples.Types,
      Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
  _count: 8
  _waves:
  - _name: First
    _enemies:
    - _assemblyQualifiedName: Game.Ghost, Assembly-CSharp
    - _assemblyQualifiedName:
  - _name: Second
    _enemies:
    - _assemblyQualifiedName: 'Game.Box`1[[Game.Ghost, Assembly-CSharp]], Assembly-CSharp'
  _empty:
    _assemblyQualifiedName:
  _points:
  - {x: 0, y: 0}
  - {x: 1, y: 0}
--- !u!4 &410389317
Transform:
  m_ObjectHideFlags: 0
  m_GameObject: {fileID: 410389315}
";

        internal const string ReferenceHolder =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_Script: {fileID: 11500000, guid: 884d53b5154744d3af6948b1eef02505, type: 3}
  m_Name: Holder
  _weapon:
    rid: 1001
  _fallback:
    _assemblyQualifiedName: Game.Sword, Assembly-CSharp
  references:
    version: 2
    RefIds:
    - rid: 1001
      type: {class: Bow, ns: Game, asm: Assembly-CSharp}
      data:
        _range: 7
        _arrow:
          _assemblyQualifiedName: Game.GhostArrow, Assembly-CSharp
        _quiver:
        - _assemblyQualifiedName: Game.Arrow, Assembly-CSharp
";

        internal const string Variant =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1001 &7223051798917698576
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {fileID: 0}
    m_Modifications:
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: _enemyType._assemblyQualifiedName
      value: Game.Ghost, Assembly-CSharp, Version=0.0.0.0, Culture=neutral,
        PublicKeyToken=null
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: _enemyType._script
      value:
      objectReference: {fileID: 11500000, guid: a701e3db0dfb545a4ad8c03a7db1767c, type: 3}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: '_list.Array.data[1]._assemblyQualifiedName'
      value: Game.Sword, Assembly-CSharp
      objectReference: {fileID: 0}
    - target: {fileID: 7520176527715267199, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: m_Name
      value: Variant
      objectReference: {fileID: 0}
    m_RemovedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
";

        private string _path;

        [TearDown]
        public void TearDown() =>
            YamlFixtures.Delete(_path);

        [Test]
        public void FindStoredTypeNames_FoldedNames_AreUnfolded_AndKeepTheirPaths()
        {
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(Lines(Scene));

            var enemy = Single(entries, "_enemyType");
            Assert.AreEqual(GruntName, enemy.TypeName);
            Assert.AreEqual(SpawnerFileId, enemy.FileId);
            Assert.AreEqual(0, enemy.Rid);
            Assert.AreEqual(HostGuid, enemy.HostScriptGuid);
            Assert.AreEqual(11500000, enemy.HostScriptFileId);

            Assert.AreEqual(PatternName, Single(entries, "_pattern").TypeName);
        }

        [Test]
        public void FindStoredTypeNames_MonoScriptWrapper_ReadsItsScript_AndTypeWrapperHasNone()
        {
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(Lines(Scene));

            var enemy = Single(entries, "_enemyType");
            Assert.IsTrue(enemy.HasScript);
            Assert.AreEqual(GruntGuid, enemy.ScriptGuid);
            Assert.AreEqual(11500000, enemy.ScriptFileId);

            Assert.IsFalse(Single(entries, "_pattern").HasScript);
        }

        [Test]
        public void FindStoredTypeNames_PlainStringField_IsNotAWrapper()
        {
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(Lines(Scene));

            Assert.IsFalse(entries.Any(entry => entry.FieldPath == "_eliteType"));
        }

        [Test]
        public void FindStoredTypeNames_ListsAndNestedLists_UseUnityPropertyPaths()
        {
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(Lines(Scene));

            Assert.AreEqual("Game.Ghost, Assembly-CSharp", Single(entries, "_waves.Array.data[0]._enemies.Array.data[0]").TypeName);
            Assert.AreEqual(string.Empty, Single(entries, "_waves.Array.data[0]._enemies.Array.data[1]").TypeName);
            Assert.AreEqual("Game.Box`1[[Game.Ghost, Assembly-CSharp]], Assembly-CSharp",
                Single(entries, "_waves.Array.data[1]._enemies.Array.data[0]").TypeName);
            Assert.AreEqual(string.Empty, Single(entries, "_empty").TypeName);
            Assert.AreEqual(6, entries.Count);
        }

        [Test]
        public void FindStoredTypeNames_ManagedReferenceData_IsScopedToItsRid()
        {
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(Lines(ReferenceHolder));

            var fallback = Single(entries, "_fallback");
            Assert.AreEqual(0, fallback.Rid);
            Assert.AreEqual(HolderFileId, fallback.FileId);

            var arrow = Single(entries, "_arrow");
            Assert.AreEqual(ReferenceRid, arrow.Rid);
            Assert.AreEqual("Game.GhostArrow, Assembly-CSharp", arrow.TypeName);
            Assert.AreEqual("Bow", arrow.HostReferenceType.Class);
            Assert.AreEqual("Game", arrow.HostReferenceType.Namespace);
            Assert.AreEqual(string.Empty, arrow.HostScriptGuid);

            Assert.AreEqual(ReferenceRid, Single(entries, "_quiver.Array.data[0]").Rid);
        }

        [Test]
        public void FindStoredTypeNames_PrefabOverrides_AreReadFromModifications()
        {
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(Lines(Variant));
            Assert.AreEqual(2, entries.Count);

            var enemy = Single(entries, "_enemyType");
            Assert.IsTrue(enemy.IsOverride);
            Assert.AreEqual(InstanceFileId, enemy.FileId);
            Assert.AreEqual(6049313956132244469L, enemy.TargetFileId);
            Assert.AreEqual(SourceGuid, enemy.TargetGuid);
            Assert.AreEqual("Game.Ghost, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", enemy.TypeName);
            Assert.AreEqual(GruntGuid, enemy.ScriptGuid);

            var element = Single(entries, "_list.Array.data[1]");
            Assert.AreEqual("Game.Sword, Assembly-CSharp", element.TypeName);
            Assert.IsFalse(element.HasScript);
        }

        [Test]
        public void RewriteTypeNames_FoldedName_CollapsesToOneLine_AndLeavesTheRestAlone()
        {
            _path = YamlFixtures.WriteTemp(Scene);
            var before = File.ReadAllLines(_path);
            var pattern = Single(SerializeReferenceYamlEditor.FindStoredTypeNames(_path), "_pattern");

            var applied = SerializeReferenceYamlEditor.RewriteTypeNames(_path,
                new[] { new TypeNameEdit(pattern, "Game.Patterns.Spiral, Assembly-CSharp") });

            Assert.AreEqual(1, applied);

            var after = File.ReadAllLines(_path);
            Assert.AreEqual(before.Length - 1, after.Length);
            CollectionAssert.Contains(after, "    _assemblyQualifiedName: Game.Patterns.Spiral, Assembly-CSharp");

            var reread = SerializeReferenceYamlEditor.FindStoredTypeNames(_path);
            Assert.AreEqual("Game.Patterns.Spiral, Assembly-CSharp", Single(reread, "_pattern").TypeName);
            Assert.AreEqual(GruntName, Single(reread, "_enemyType").TypeName);
            Assert.AreEqual(6, reread.Count);
        }

        [Test]
        public void RewriteTypeNames_MonoScriptWrapper_WritesTheScriptToo()
        {
            _path = YamlFixtures.WriteTemp(Scene);
            var enemy = Single(SerializeReferenceYamlEditor.FindStoredTypeNames(_path), "_enemyType");

            var reference = SerializeReferenceYamlEditor.FormatScriptReference(11500000, "0123456789abcdef0123456789abcdef");
            Assert.AreEqual(1, SerializeReferenceYamlEditor.RewriteTypeNames(_path,
                new[] { new TypeNameEdit(enemy, "Game.Brute, Assembly-CSharp", reference) }));

            var reread = Single(SerializeReferenceYamlEditor.FindStoredTypeNames(_path), "_enemyType");
            Assert.AreEqual("Game.Brute, Assembly-CSharp", reread.TypeName);
            Assert.AreEqual("0123456789abcdef0123456789abcdef", reread.ScriptGuid);
        }

        [Test]
        public void RewriteTypeNames_SeveralEdits_ApplyInOneWrite()
        {
            _path = YamlFixtures.WriteTemp(Scene);
            var entries = SerializeReferenceYamlEditor.FindStoredTypeNames(_path);
            var first = Single(entries, "_waves.Array.data[0]._enemies.Array.data[0]");
            var second = Single(entries, "_waves.Array.data[1]._enemies.Array.data[0]");

            Assert.AreEqual(2, SerializeReferenceYamlEditor.RewriteTypeNames(_path, new[]
            {
                new TypeNameEdit(first, "Game.Grunt, Assembly-CSharp"),
                new TypeNameEdit(second, "Game.Box`1[[Game.Grunt, Assembly-CSharp]], Assembly-CSharp"),
            }));

            var reread = SerializeReferenceYamlEditor.FindStoredTypeNames(_path);
            Assert.AreEqual("Game.Grunt, Assembly-CSharp", Single(reread, "_waves.Array.data[0]._enemies.Array.data[0]").TypeName);
            Assert.AreEqual("Game.Box`1[[Game.Grunt, Assembly-CSharp]], Assembly-CSharp",
                Single(reread, "_waves.Array.data[1]._enemies.Array.data[0]").TypeName);
        }

        [Test]
        public void RewriteTypeNames_NameChangedSinceRead_IsSkipped()
        {
            _path = YamlFixtures.WriteTemp(Scene);
            var pattern = Single(SerializeReferenceYamlEditor.FindStoredTypeNames(_path), "_pattern");

            Assert.AreEqual(1, SerializeReferenceYamlEditor.RewriteTypeNames(_path,
                new[] { new TypeNameEdit(pattern, "Game.Patterns.Spiral, Assembly-CSharp") }));

            var before = File.ReadAllText(_path);
            Assert.AreEqual(0, SerializeReferenceYamlEditor.RewriteTypeNames(_path,
                new[] { new TypeNameEdit(pattern, "Game.Patterns.Line, Assembly-CSharp") }));
            Assert.AreEqual(before, File.ReadAllText(_path), "A stale edit must leave the file byte-identical.");
        }

        [Test]
        public void RewriteTypeNames_Override_RewritesTheModificationValue()
        {
            _path = YamlFixtures.WriteTemp(Variant);
            var element = Single(SerializeReferenceYamlEditor.FindStoredTypeNames(_path), "_list.Array.data[1]");

            Assert.AreEqual(1, SerializeReferenceYamlEditor.RewriteTypeNames(_path,
                new[] { new TypeNameEdit(element, "Game.Axe, Assembly-CSharp") }));

            var after = File.ReadAllLines(_path);
            CollectionAssert.Contains(after, "      value: Game.Axe, Assembly-CSharp");
            Assert.AreEqual("Game.Axe, Assembly-CSharp",
                Single(SerializeReferenceYamlEditor.FindStoredTypeNames(_path), "_list.Array.data[1]").TypeName);
        }

        [Test]
        public void FormatTypeNameScalar_QuotesOnlyWhatYamlWouldMisread()
        {
            Assert.AreEqual("Game.Sword, Assembly-CSharp", SerializeReferenceYamlEditor.FormatTypeNameScalar("Game.Sword, Assembly-CSharp"));
            Assert.AreEqual("Game.Box`1[[Game.T, A]], A", SerializeReferenceYamlEditor.FormatTypeNameScalar("Game.Box`1[[Game.T, A]], A"));
            Assert.AreEqual("'[Weird], A'", SerializeReferenceYamlEditor.FormatTypeNameScalar("[Weird], A"));
            Assert.AreEqual("'''Quoted, A'", SerializeReferenceYamlEditor.FormatTypeNameScalar("'Quoted, A"));
            Assert.AreEqual(string.Empty, SerializeReferenceYamlEditor.FormatTypeNameScalar(string.Empty));
        }

        private static string[] Lines(string yaml) =>
            yaml.Replace("\r\n", "\n").Split('\n');

        private static StoredTypeNameEntry Single(System.Collections.Generic.List<StoredTypeNameEntry> entries, string fieldPath)
        {
            var matches = entries.Where(entry => entry.FieldPath == fieldPath).ToArray();
            Assert.AreEqual(1, matches.Length, $"Expected one entry at '{fieldPath}', found: " +
                string.Join(", ", entries.Select(entry => entry.FieldPath)));
            return matches[0];
        }
    }
}
