using System.IO;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The tests of this fixture that need Unity or the package's Editor assembly. Aspid.FastTools.YamlTests runs
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
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, Pistol);

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
                    ProjectAssetPath, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, Pistol);

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
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, Pistol)),
                new MissingReferenceLocation(_path,
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.ShotgunRid, Pistol)),
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
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, 999999, Pistol)),
                new MissingReferenceLocation(_path,
                    new MissingReferenceEntry(YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, Pistol)),
            };

            // A stale entry fails before any write, so the file must be checked out before the entry loop, not by it.
            LogAssert.Expect(LogType.Error, new Regex("is read-only"));
            var applied = SerializeReferenceBatchEditor.Null(entries, "Test");

            Assert.AreEqual(0, applied, "Nothing can be applied to a read-only file.");
        }
    }
}
