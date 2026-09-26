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
    /// <summary>
    /// Coverage for how <see cref="SerializeReferenceYamlEditor"/> writes an asset back: a read-only file is refused with
    /// a clear error and left untouched, a UTF-8 byte-order mark survives the rewrite, and no temp file is left next to
    /// the asset.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceYamlEditorWriteTests
    {
        private static readonly ManagedTypeName Pistol = new(
            "Aspid.FastTools.Samples.SerializeReferences",
            "Aspid.FastTools.Samples.SerializeReferences",
            "Pistol");

        private string _path;

        [SetUp]
        public void SetUp() =>
            _path = YamlFixtures.WriteTemp(YamlFixtures.MissingTypePrefab);

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_path)) File.SetAttributes(_path, FileAttributes.Normal);
            YamlFixtures.Delete(_path);
        }

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
        public void TryRewriteType_PreservesUtf8ByteOrderMark()
        {
            File.WriteAllText(_path, YamlFixtures.MissingTypePrefab, new UTF8Encoding(true));

            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, Pistol));

            var bytes = File.ReadAllBytes(_path);
            Assert.That(bytes.Take(3), Is.EqualTo(new byte[] { 0xEF, 0xBB, 0xBF }), "The UTF-8 byte-order mark must survive.");
            Assert.AreEqual(1, Regex.Matches(Encoding.UTF8.GetString(bytes), "﻿").Count, "The mark must not be doubled.");
        }

        [Test]
        public void TryRewriteType_WithoutByteOrderMark_WritesNone()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, Pistol));

            Assert.AreNotEqual(0xEF, File.ReadAllBytes(_path)[0], "A file without a byte-order mark must not gain one.");
        }

        [Test]
        public void TryRewriteType_LeavesNoTempFileNextToAsset()
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryRewriteType(
                _path, YamlFixtures.MonoBehaviourFileId, YamlFixtures.GhostPistolRid, Pistol));

            var leftovers = Directory.GetFiles(Path.GetDirectoryName(_path),$".{Path.GetFileName(_path)}.*");
            CollectionAssert.IsEmpty(leftovers, "The temp file used for the atomic replace must be gone.");
        }
    }
}
