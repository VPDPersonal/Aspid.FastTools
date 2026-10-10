using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for SerializeReferenceYaml.ReadLinesIfContainsAny: the byte probe the breakage detectors run
    // before they decode an asset, which must find a marker wherever it sits and must skip a file without one.
    [TestFixture]
    internal sealed class SerializeReferenceYamlMarkerProbeTests
    {
        private const string Preamble = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n";

        private static readonly byte[][] _markers = { Encoding.ASCII.GetBytes("RefIds:"), Encoding.ASCII.GetBytes("managedReferences[") };

        private readonly List<string> _paths = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var path in _paths)
                YamlFixtures.Delete(path);

            _paths.Clear();
        }

        [Test]
        public void ReadLinesIfContainsAny_FileWithMarker_ReturnsLines()
        {
            var lines = SerializeReferenceYaml.ReadLinesIfContainsAny(Write(Preamble + "  RefIds:\n  - rid: 1\n"), _markers);

            CollectionAssert.AreEqual(new[] { "%YAML 1.1", "%TAG !u! tag:unity3d.com,2011:", "  RefIds:", "  - rid: 1" }, lines);
        }

        [Test]
        public void ReadLinesIfContainsAny_SecondMarker_ReturnsLines()
        {
            var lines = SerializeReferenceYaml.ReadLinesIfContainsAny(
                Write(Preamble + "      propertyPath: a.managedReferences[7]\n"), _markers);

            Assert.IsNotNull(lines);
        }

        [Test]
        public void ReadLinesIfContainsAny_FileWithoutMarker_ReturnsNull() =>
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(Write(Preamble + "  m_Name: Plain\n"), _markers));

        [Test]
        public void ReadLinesIfContainsAny_LargeFileWithoutMarker_ReturnsNull()
        {
            var body = string.Concat(Enumerable.Repeat("  m_Name: Plain value of one line\n", 4000));

            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(Write(Preamble + body), _markers));
        }

        [Test]
        public void ReadLinesIfContainsAny_MarkerInLastChunk_ReturnsLines()
        {
            var body = string.Concat(Enumerable.Repeat("  m_Name: Plain value of one line\n", 4000));
            var lines = SerializeReferenceYaml.ReadLinesIfContainsAny(Write(Preamble + body + "  RefIds:\n"), _markers);

            Assert.AreEqual("  RefIds:", lines.Last());
        }

        // Every offset around the chunk edge, so the marker is split at each of its possible positions.
        [Test]
        public void ReadLinesIfContainsAny_MarkerAcrossChunkEdge_IsFound()
        {
            for (var shift = -20; shift <= 20; shift++)
            {
                var filler = new string('a', SerializeReferenceYaml.MarkerProbeChunkSize - Preamble.Length + shift);
                var path = Write(Preamble + filler + "managedReferences[1]\n");

                Assert.IsNotNull(SerializeReferenceYaml.ReadLinesIfContainsAny(path, _markers), $"shift {shift}");
            }
        }

        [Test]
        public void ReadLinesIfContainsAny_ByteOrderMarkAndCrlf_ReturnsLines()
        {
            var bytes = new byte[] { 0xEF, 0xBB, 0xBF }
                .Concat(Encoding.UTF8.GetBytes((Preamble + "  RefIds:\n").Replace("\n", "\r\n")))
                .ToArray();

            var lines = SerializeReferenceYaml.ReadLinesIfContainsAny(WriteBytes(bytes), _markers);

            Assert.AreEqual("  RefIds:", lines.Last());
        }

        [Test]
        public void ReadLinesIfContainsAny_NotTextYaml_ReturnsNull()
        {
            // Binary and LFS pointer bodies hold the marker text, which a text scan must not parse.
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(Write("  RefIds:\n"), _markers));
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(
                Write("version https://git-lfs.github.com/spec/v1\noid sha256:4d7a\nsize 12\n  RefIds:\n"), _markers));
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(
                WriteBytes(new byte[] { 0x00, 0x00, 0x00, 0x00 }.Concat(Encoding.ASCII.GetBytes("RefIds:")).ToArray()), _markers));
        }

        [Test]
        public void ReadLinesIfContainsAny_MissingOrEmptyPath_ReturnsNull()
        {
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(null, _markers));
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(string.Empty, _markers));
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(
                Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".prefab"), _markers));
        }

        [Test]
        public void ReadLinesIfContainsAny_EmptyFile_ReturnsNull() =>
            Assert.IsNull(SerializeReferenceYaml.ReadLinesIfContainsAny(Write(string.Empty), _markers));

        private string Write(string content)
        {
            var path = YamlFixtures.WriteTemp(content);
            _paths.Add(path);

            return path;
        }

        private string WriteBytes(byte[] bytes)
        {
            var path = YamlFixtures.WriteTemp(string.Empty);
            File.WriteAllBytes(path, bytes);
            _paths.Add(path);

            return path;
        }
    }
}
