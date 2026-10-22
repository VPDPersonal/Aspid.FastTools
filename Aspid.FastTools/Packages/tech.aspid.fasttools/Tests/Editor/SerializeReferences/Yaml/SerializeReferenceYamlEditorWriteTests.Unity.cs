using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The tests of this fixture that need Unity or the package's Editor assembly. DotnetTests/Yaml runs
    // the rest without Unity and leaves the *.Unity.cs parts out.
    internal sealed partial class SerializeReferenceYamlEditorWriteTests
    {
        // Never imported: it only gives AssetDatabase.MakeEditable a path inside the project.
        private const string ProjectAssetPath = "Assets/__AspidYamlWriteProbe__.prefab";

        [Test]
        public void TryRewriteType_ReadOnlyFile_RefusesWithClearError_AndLeavesFileUntouched()
        {
            var before = File.ReadAllText(_path);
            File.SetAttributes(_path, FileAttributes.ReadOnly);

            LogAssert.Expect(LogType.Error, new Regex("is read-only"));
            var rewritten = SerializeReferenceYamlEditor.TryRewriteType(
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, _pistol);

            Assert.IsFalse(rewritten, "A read-only asset must not be reported as rewritten.");
            Assert.AreEqual(before, File.ReadAllText(_path), "A refused rewrite must leave the file byte-identical.");
        }

        [Test]
        public void TryRewriteType_ReadOnlyFileUnderAssets_RefusesWithClearError_AndLeavesFileUntouched()
        {
            File.WriteAllText(ProjectAssetPath, YamlFixtures.MissingTypePrefab);

            try
            {
                File.SetAttributes(ProjectAssetPath, FileAttributes.ReadOnly);

                LogAssert.Expect(LogType.Error, new Regex("is read-only"));
                var rewritten = SerializeReferenceYamlEditor.TryRewriteType(
                    ProjectAssetPath, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, _pistol);

                Assert.IsFalse(rewritten, "A read-only asset must not be reported as rewritten.");
                Assert.AreEqual(YamlFixtures.MissingTypePrefab, File.ReadAllText(ProjectAssetPath),
                    "A refused rewrite must leave the file byte-identical.");
            }
            finally
            {
                File.SetAttributes(ProjectAssetPath, FileAttributes.Normal);
                File.Delete(ProjectAssetPath);
                if (File.Exists(ProjectAssetPath + ".meta")) File.Delete(ProjectAssetPath + ".meta");
            }
        }

        [Test]
        public void BatchNull_ReadOnlyFile_ReportsTheRefusalOnce()
        {
            File.SetAttributes(_path, FileAttributes.ReadOnly);
            var entries = new[]
            {
                new MissingReferenceLocation(_path,
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, _pistol)),
                new MissingReferenceLocation(_path,
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.ShotgunRid, _pistol)),
            };

            // A second "is read-only" error would be unexpected and fail the test.
            LogAssert.Expect(LogType.Error, new Regex("is read-only"));
            var applied = SerializeReferenceBatchEditor.Null(entries, "Test");

            Assert.AreEqual(0, applied, "Nothing can be applied to a read-only file.");
        }

        [Test]
        public void BatchNull_ReadOnlyFile_StaleFirstEntry_StillReportsTheRefusal()
        {
            File.SetAttributes(_path, FileAttributes.ReadOnly);
            var entries = new[]
            {
                new MissingReferenceLocation(_path,
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, 999999, _pistol)),
                new MissingReferenceLocation(_path,
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, _pistol)),
            };

            // A stale entry fails before any write, so the file must be checked out before the entry loop, not by it.
            LogAssert.Expect(LogType.Error, new Regex("is read-only"));
            var applied = SerializeReferenceBatchEditor.Null(entries, "Test");

            Assert.AreEqual(0, applied, "Nothing can be applied to a read-only file.");
        }

        [Test]
        public void TryRewriteType_InvalidUtf8_RefusesAndLeavesFileByteIdentical()
        {
            // A Latin-1 "é" in the name: decoded as U+FFFD, a rewrite would change it on disk.
            var name = YamlFixtures.MissingTypePrefab.IndexOf("LoadoutMissingType", StringComparison.Ordinal);
            var bytes = Encoding.UTF8.GetBytes(YamlFixtures.MissingTypePrefab.Substring(0, name))
                .Concat(new byte[] { 0xE9 })
                .Concat(Encoding.UTF8.GetBytes(YamlFixtures.MissingTypePrefab.Substring(name)))
                .ToArray();
            File.WriteAllBytes(_path, bytes);

            LogAssert.Expect(LogType.Error, new Regex("is not valid UTF-8"));
            var rewritten = SerializeReferenceYamlEditor.TryRewriteType(
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, _pistol);

            Assert.IsFalse(rewritten, "A file that is not valid UTF-8 must not be rewritten.");
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(_path), "A refused rewrite must leave the file byte-identical.");
        }

        [Test]
        public void TryWritePreservingNewlines_FileChangedAfterTheRead_RefusesAndKeepsTheNewText()
        {
            var source = File.ReadAllLines(_path);
            var edited = (string[])source.Clone();
            var type = Array.FindIndex(source, line => line.Contains("class: GhostPistol,"));
            edited[type] = edited[type].Replace("class: GhostPistol,", "class: Pistol,");

            // A save between the read and the write; the edit must not revert it.
            var saved = File.ReadAllText(_path).Replace("_damage: 15", "_damage: 20");
            File.WriteAllText(_path, saved);

            LogAssert.Expect(LogType.Error, new Regex("changed on disk"));
            var written = SerializeReferenceYamlEditor.TryWritePreservingNewlines(_path, source, edited,
                YamlFixtures.MonoBehaviourFileId, removedRid: YamlFixtures.GhostPistolRid, addedRid: YamlFixtures.GhostPistolRid);

            Assert.IsFalse(written, "An edit computed from an older text must not be written.");
            Assert.AreEqual(saved, File.ReadAllText(_path), "The newer text must stay.");
        }

        [Test]
        public void TryWritePreservingNewlines_EditFailsTheCheck_RefusesAndLeavesFileUntouched()
        {
            var before = File.ReadAllText(_path);
            var source = File.ReadAllLines(_path);

            // Removes the rid 1002 entry and the header of the rid 1003 entry after it.
            var header = Array.IndexOf(source, "    - rid: 1002");
            var edited = source.Take(header).Concat(source.Skip(header + 6)).ToArray();

            LogAssert.Expect(LogType.Error, new Regex("failed its safety check"));
            var written = SerializeReferenceYamlEditor.TryWritePreservingNewlines(_path, source, edited,
                YamlFixtures.MonoBehaviourFileId, removedRid: YamlFixtures.GhostPistolRid);

            Assert.IsFalse(written, "A mis-bounded edit must not be written.");
            Assert.AreEqual(before, File.ReadAllText(_path), "A refused edit must leave the file byte-identical.");
        }
    }
}
