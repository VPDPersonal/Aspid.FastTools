using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The file-format sniff and the scanners that rely on it: a binary asset or an LFS pointer is skipped without being
    // parsed, even when its bytes happen to contain text that looks like managed-reference entries.
    [TestFixture]
    internal sealed partial class SerializeReferenceYamlFileFormatTests
    {
        private const string LfsPointer =
            "version https://git-lfs.github.com/spec/v1\n" +
            "oid sha256:4d7a214614ab2935c943f9e0ff69d22eadbb8f32b1258daaa5e2ca24d17e2393\n" +
            "size 12345\n";

        private readonly List<string> _paths = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var path in _paths)
                YamlFixtures.Delete(path);

            _paths.Clear();
        }

        [Test]
        public void SniffFileFormat_UnityYaml_IsTextYaml() =>
            Assert.AreEqual(AssetFileFormat.TextYaml, SerializeReferenceYaml.SniffFileFormat(Write(YamlFixtures.MissingTypePrefab)));

        [Test]
        public void SniffFileFormat_UnityYamlWithByteOrderMarkAndCrlf_IsTextYaml()
        {
            var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
                .Concat(Encoding.UTF8.GetBytes(YamlFixtures.MissingTypePrefab.Replace("\n", "\r\n")))
                .ToArray();

            Assert.AreEqual(AssetFileFormat.TextYaml, SerializeReferenceYaml.SniffFileFormat(WriteBytes(bytes)));
        }

        [Test]
        public void SniffFileFormat_LfsPointer_IsLfsPointer() =>
            Assert.AreEqual(AssetFileFormat.LfsPointer, SerializeReferenceYaml.SniffFileFormat(Write(LfsPointer)));

        [Test]
        public void SniffFileFormat_BinaryBytes_IsBinary() =>
            Assert.AreEqual(AssetFileFormat.Binary,
                SerializeReferenceYaml.SniffFileFormat(WriteBytes(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x11 })));

        [Test]
        public void SniffFileFormat_EmptyFile_IsBinary() =>
            Assert.AreEqual(AssetFileFormat.Binary, SerializeReferenceYaml.SniffFileFormat(Write(string.Empty)));

        [Test]
        public void SniffFileFormat_MissingFile_IsBinary() =>
            Assert.AreEqual(AssetFileFormat.Binary,
                SerializeReferenceYaml.SniffFileFormat(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".prefab")));

        // Without the %YAML / %TAG preamble the body still carries GhostPistol's RefIds entry, so a scanner that parses
        // every file line by line would report it.
        [Test]
        public void FindMissingReferences_NotTextYaml_IsSkipped()
        {
            var path = Write(StripPreamble(YamlFixtures.MissingTypePrefab));
            var missing = SerializeReferenceYamlEditor.FindMissingReferences(path, type => !type.Class.StartsWith("Ghost", StringComparison.Ordinal));

            Assert.AreEqual(0, missing.Count);
        }

        // The gate's Scan sniffs each file once and passes knownTextYaml, so the scanner must not sniff again: the same
        // preamble-less body is parsed instead of skipped.
        [Test]
        public void FindMissingReferences_KnownTextYaml_SkipsSniff()
        {
            var path = Write(StripPreamble(YamlFixtures.MissingTypePrefab));
            var missing = SerializeReferenceYamlEditor.FindMissingReferences(
                path, type => !type.Class.StartsWith("Ghost", StringComparison.Ordinal), knownTextYaml: true);

            Assert.AreEqual(1, missing.Count);
        }

        [Test]
        public void FindPrefabOverrideReferences_NotTextYaml_IsSkipped()
        {
            var path = Write(StripPreamble(SerializeReferenceYamlPrefabOverrideTests.VariantPrefab));
            Assert.AreEqual(0, SerializeReferenceYamlEditor.FindPrefabOverrideReferences(path).Count);
        }

        private static string StripPreamble(string yaml) =>
            string.Join("\n", yaml.Split('\n').Where(line => !line.StartsWith("%", StringComparison.Ordinal)));

        private string Write(string text)
        {
            var path = YamlFixtures.WriteTemp(text);
            _paths.Add(path);
            return path;
        }

        private string WriteBytes(byte[] bytes)
        {
            var path = Write(string.Empty);
            File.WriteAllBytes(path, bytes);
            return path;
        }
    }
}
