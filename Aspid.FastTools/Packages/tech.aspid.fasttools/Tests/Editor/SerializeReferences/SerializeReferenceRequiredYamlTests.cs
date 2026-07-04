using System;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Types;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A type mixing required, optional and plain fields, to prove GetRequiredFields returns only the required ones and
    // classifies each by kind (string vs [SerializeReference] managed reference).
    internal sealed class MixedRequiredObject : ScriptableObject
    {
        [SerializeReference, TypeSelector(Required = true)]
        public ITestWeapon requiredRef;

        [TypeSelector(Required = true)]
        public string requiredString;

        [SerializeReference, TypeSelector]
        public ITestWeapon optionalRef;

        [TypeSelector]
        public string optionalString;

        public int plain;
    }

    /// <summary>
    /// Coverage for the scene-safe required-field gate: <see cref="SerializeReferenceRequiredGate.GetRequiredFields"/>
    /// (reflection over a type's required fields) and <see cref="SerializeReferenceYamlEditor.FindUnsetRequiredFields"/>
    /// (the pure-YAML scan that reads scene MonoBehaviours straight from the file). The YAML tests drive the public
    /// method against temp <c>.unity</c>-shaped fixtures with an injected script→fields resolver, so the parser and the
    /// violation logic are exercised in isolation — no asset import, no <see cref="SerializedObject"/>, no AssetDatabase.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceRequiredYamlTests
    {
        // Maps the fixtures' first script guid to RequiredTestObject's required fields; every other guid is unknown.
        private static IReadOnlyList<RequiredFieldDescriptor> Resolve(string guid) =>
            guid == YamlFixtures.RequiredSceneScriptGuid
                ? SerializeReferenceRequiredGate.GetRequiredFields(typeof(RequiredTestObject))
                : Array.Empty<RequiredFieldDescriptor>();

        private string _path;

        [TearDown]
        public void TearDown() => YamlFixtures.Delete(_path);

        [Test]
        public void GetRequiredFields_ReturnsOnlyRequiredFields_ClassifiedByKind()
        {
            var byName = SerializeReferenceRequiredGate.GetRequiredFields(typeof(MixedRequiredObject))
                .ToDictionary(field => field.FieldName, field => field.IsString);

            Assert.AreEqual(2, byName.Count, "Only the two Required fields should be returned.");
            Assert.IsTrue(byName.ContainsKey("requiredRef"), "The required managed reference should be included.");
            Assert.IsTrue(byName.ContainsKey("requiredString"), "The required string field should be included.");
            Assert.IsFalse(byName["requiredRef"], "A [SerializeReference] field is a managed reference, not a string.");
            Assert.IsTrue(byName["requiredString"], "A string type field is classified as a string.");
        }

        [Test]
        public void GetRequiredFields_NoRequiredFields_ReturnsEmpty()
        {
            // LinkerTestObject has [SerializeReference] fields but none mark Required.
            Assert.AreEqual(0, SerializeReferenceRequiredGate.GetRequiredFields(typeof(LinkerTestObject)).Count);
        }

        [Test]
        public void GetRequiredFields_NullType_ReturnsEmpty()
        {
            Assert.AreEqual(0, SerializeReferenceRequiredGate.GetRequiredFields(null).Count);
        }

        [Test]
        public void FindUnsetRequiredFields_BothUnset_ReportsBoth()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.RequiredSceneUnset);

            var violations = SerializeReferenceYamlEditor.FindUnsetRequiredFields(_path, Resolve);

            Assert.AreEqual(2, violations.Count, "An unset required reference and string should both be reported.");
            Assert.IsTrue(violations.All(v => v.FileId == YamlFixtures.RequiredSceneMonoFileId),
                "Both violations belong to the single MonoBehaviour document.");

            var managed = violations.Single(v => v.FieldName == "requiredRef");
            Assert.AreEqual(-2L, managed.Rid, "An unset managed reference reads the null id (-2).");
            Assert.IsTrue(violations.Any(v => v.FieldName == "requiredString"), "The empty string field is a violation.");
        }

        [Test]
        public void FindUnsetRequiredFields_BothSet_ReportsNone()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.RequiredSceneSet);
            var violations = SerializeReferenceYamlEditor.FindUnsetRequiredFields(_path, Resolve);

            Assert.AreEqual(0, violations.Count, "A set managed reference and a populated string are not violations.");
        }

        [Test]
        public void FindUnsetRequiredFields_AbsentKeys_ReportsNone()
        {
            // Both required keys are missing from the document (object saved before the fields were added / stripped doc).
            // An absent key needs a reserialize, not a build failure, so it must not be reported as a violation.
            _path = YamlFixtures.WriteTemp(YamlFixtures.RequiredSceneAbsentKeys);
            var violations = SerializeReferenceYamlEditor.FindUnsetRequiredFields(_path, Resolve);

            Assert.AreEqual(0, violations.Count, "An absent required key is not a violation — it needs a reserialize.");
        }

        [Test]
        public void FindUnsetRequiredFields_MixedAndUnknownScript_ReportsOnlyKnownUnset()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.RequiredSceneMixedUnknownScript);
            var violations = SerializeReferenceYamlEditor.FindUnsetRequiredFields(_path, Resolve);

            Assert.AreEqual(1, violations.Count,
                "Only the empty string on the known script is reported; the unknown-script document is skipped.");
            Assert.AreEqual("requiredString", violations[0].FieldName);
            Assert.AreEqual(YamlFixtures.RequiredSceneMonoFileId, violations[0].FileId,
                "The violation must be attributed to the first MonoBehaviour, not the unknown-script one.");
        }

        [Test]
        public void FindUnsetRequiredFields_NullResolver_ReturnsEmpty()
        {
            _path = YamlFixtures.WriteTemp(YamlFixtures.RequiredSceneUnset);
            Assert.AreEqual(0, SerializeReferenceYamlEditor.FindUnsetRequiredFields(_path, null).Count);
        }
    }
}
