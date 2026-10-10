using System;
using System.Linq;
using NUnit.Framework;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The tests of this fixture that need Unity or the package's Editor assembly. Aspid.FastTools.YamlTests runs
    // the rest without Unity and leaves the *.Unity.cs parts out.
    internal sealed partial class SerializeReferenceYamlFileFormatTests
    {
        [Test]
        public void FindUnsetRequiredFields_NotTextYaml_IsSkipped()
        {
            var path = Write(StripPreamble(YamlFixtures.RequiredSceneUnset));
            var violations = SerializeReferenceYamlEditor.FindUnsetRequiredFields(path, guid =>
                guid == YamlFixtures.RequiredSceneScriptGuid
                    ? TypeSelectorRequiredGate.GetRequiredFields(typeof(RequiredTestObject))
                    : Array.Empty<RequiredFieldDescriptor>());

            Assert.AreEqual(0, violations.Count);
        }

        [Test]
        public void FindUnsetRequiredFields_KnownTextYaml_SkipsSniff()
        {
            var path = Write(StripPreamble(YamlFixtures.RequiredSceneUnset));
            var violations = SerializeReferenceYamlEditor.FindUnsetRequiredFields(path, guid =>
                guid == YamlFixtures.RequiredSceneScriptGuid
                    ? TypeSelectorRequiredGate.GetRequiredFields(typeof(RequiredTestObject))
                    : Array.Empty<RequiredFieldDescriptor>(), knownTextYaml: true);

            Assert.AreNotEqual(0, violations.Count);
        }

        [Test]
        public void GraphScannerBuild_NotTextYaml_IsSkipped()
        {
            var path = Write(StripPreamble(YamlFixtures.MissingTypePrefab));
            Assert.AreEqual(0, SerializeReferenceGraphScanner.Build(path, resolveTypeNames: false).Count);
        }

        // The usage index and both delete-guard sweeps read through CollectUsages: its RefIds pass (the prefab) and
        // its override pass (the variant) share one read, which skips the preamble-less body before either runs.
        [TestCase(false)]
        [TestCase(true)]
        public void CollectUsages_NotTextYaml_IsSkipped(bool variant)
        {
            var yaml = variant ? SerializeReferenceYamlPrefabOverrideTests.VariantPrefab : YamlFixtures.MissingTypePrefab;

            Assert.IsNotEmpty(SerializeReferenceTypeUsageIndex.CollectUsages(Write(yaml), guid: null).ToList());
            Assert.IsEmpty(SerializeReferenceTypeUsageIndex.CollectUsages(Write(StripPreamble(yaml)), guid: null).ToList());
        }
    }
}
