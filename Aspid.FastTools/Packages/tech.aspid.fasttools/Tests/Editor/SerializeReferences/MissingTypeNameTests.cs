using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using Aspid.FastTools.Types;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The project-wide counterpart of the Inspector's Missing type notice: the gate and the CI report list stored type
    // names that no longer resolve, Project References rewrites them, and breakage detection reports new ones.
    [TestFixture]
    internal sealed class MissingTypeNameTests
    {
        private const string ProbeAssetPath = "Assets/__AspidMissingTypeNameProbe__.asset";

        // A class no assembly declares, as after a rename.
        private const string GhostName = "Aspid.MissingTypeNameProbe.GhostSword, Assembly-CSharp";

        private static string SwordName => typeof(TestSword).AssemblyQualifiedName;

        [TearDown]
        public void TearDown() =>
            AssetDatabase.DeleteAsset(ProbeAssetPath);

        [Test]
        public void Scan_MissingOnly_ReportsMissingTypeNames_ByWrapperPath()
        {
            SaveProbeWithBrokenNames();

            var violations = ForProbe(SerializeReferenceGateScanner.Scan(GateOptions.MissingOnly));

            CollectionAssert.AreEquivalent(new[] { "type", "weaponType" }, violations.Select(violation => violation.FieldPath));
            Assert.IsTrue(violations.All(violation => violation.Kind == GateViolationKind.MissingTypeName));
            Assert.IsTrue(violations.All(violation => violation.TypeName.Contains("GhostSword")));
        }

        [Test]
        public void Scan_MonoScriptWithLiveScript_IsNotMissing_AndWithNullScriptIs()
        {
            SaveProbeWithBrokenNames();

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(MonoScriptOf<SerializableType>()));
            ReplaceScriptBlock($"{{fileID: 11500000, guid: {guid}, type: 3}}");
            Assert.IsFalse(ForProbe(SerializeReferenceGateScanner.Scan(GateOptions.MissingOnly)).Any(v => v.FieldPath == "script"));

            ReplaceScriptBlock("{fileID: 0}");
            Assert.IsTrue(ForProbe(SerializeReferenceGateScanner.Scan(GateOptions.MissingOnly)).Any(v => v.FieldPath == "script"));
        }

        [Test]
        public void BuildReport_MissingTypeName_WritesTheWholeStoredName()
        {
            var entry = Entry(GhostName, fieldPath: "weaponType");
            var report = SerializeReferenceCiGate.BuildReport(new[] { GateViolation.ForTypeName("Assets/Probe.asset", entry) });

            StringAssert.Contains($"MissingTypeName\tAssets/Probe.asset\t11400000\t0\t{GhostName}\tweaponType\t", report);
        }

        [Test]
        public void ToString_MissingTypeName_NamesTheFieldAndTheType()
        {
            var violation = GateViolation.ForTypeName("Assets/Probe.asset", Entry(GhostName, fieldPath: "weaponType"));

            Assert.AreEqual("Assets/Probe.asset : weaponType -> missing type name Aspid.MissingTypeNameProbe.GhostSword",
                violation.ToString());
        }

        [Test]
        public void Rewrite_ThenRevert_RoundTripsTheFiles()
        {
            SaveProbeWithBrokenNames();

            var missing = MissingTypeNames.FindInFile(ProbeAssetPath);
            Assert.AreEqual(2, missing.Count);

            Assert.AreEqual(2, MissingTypeNameRepair.Rewrite(missing, typeof(TestSword), "Test"));
            CollectionAssert.IsEmpty(MissingTypeNames.FindInFile(ProbeAssetPath));

            Assert.AreEqual(2, MissingTypeNameRepair.Revert(missing, SwordName, "Test"));
            Assert.AreEqual(2, MissingTypeNames.FindInFile(ProbeAssetPath).Count);
        }

        [Test]
        public void Revert_FieldChangedSinceTheFix_IsLeftAlone()
        {
            SaveProbeWithBrokenNames();

            var missing = MissingTypeNames.FindInFile(ProbeAssetPath);
            MissingTypeNameRepair.Rewrite(missing, typeof(TestSword), "Test");

            // Another fix since: the field holds a different type than the one the receipt applied.
            var other = typeof(RelocatedRanged).AssemblyQualifiedName;
            var stored = SerializeReferenceYamlEditor.FindStoredTypeNames(ProbeAssetPath).Single(entry => entry.FieldPath == "type");
            SerializeReferenceYamlEditor.RewriteTypeNames(ProbeAssetPath, new[] { new TypeNameEdit(stored, other) });

            Assert.AreEqual(1, MissingTypeNameRepair.Revert(missing, SwordName, "Test"));
            Assert.AreEqual(other, SerializeReferenceYamlEditor.FindStoredTypeNames(ProbeAssetPath).Single(entry => entry.FieldPath == "type").TypeName);
        }

        // A file the YAML pass cannot read has no missing names, which is not the same as having none: the caller gets
        // it back as unscanned.
        [TestCase("version https://git-lfs.github.com/spec/v1\noid sha256:0\nsize 1\n", AssetFileFormat.LfsPointer)]
        [TestCase("\u0001\u0002binary", AssetFileFormat.Binary)]
        public void FindInFile_FileThatIsNotTextYaml_IsReportedAsUnscanned(string content, AssetFileFormat format)
        {
            SaveProbeWithBrokenNames();
            File.WriteAllText(ProbeAssetPath, content);

            var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();

            CollectionAssert.IsEmpty(MissingTypeNames.FindInFile(ProbeAssetPath, unscanned));
            CollectionAssert.AreEqual(new[] { (ProbeAssetPath, format) }, unscanned);
        }

        [Test]
        public void FindInFile_TextYaml_IsNotReportedAsUnscanned()
        {
            SaveProbeWithBrokenNames();

            var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();

            Assert.AreEqual(2, MissingTypeNames.FindInFile(ProbeAssetPath, unscanned).Count);
            CollectionAssert.IsEmpty(unscanned);
        }

        [Test]
        public void FindInFile_MissingFile_IsNotReportedAsUnscanned()
        {
            var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();

            CollectionAssert.IsEmpty(MissingTypeNames.FindInFile(ProbeAssetPath, unscanned));
            CollectionAssert.IsEmpty(unscanned);
        }

        [Test]
        public void ScanProject_LfsPointer_AppearsInUnscanned()
        {
            SaveProbeWithBrokenNames();
            File.WriteAllText(ProbeAssetPath, "version https://git-lfs.github.com/spec/v1\noid sha256:0\nsize 1\n");

            var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();
            var missing = MissingTypeNames.ScanProject(unscanned);

            CollectionAssert.Contains(unscanned, (ProbeAssetPath, AssetFileFormat.LfsPointer));
            Assert.IsFalse(missing.Any(location => location.AssetPath == ProbeAssetPath));
        }

        [Test]
        public void TryGetFieldConstraint_ReadsTheWrapperTypeArgument()
        {
            Assert.IsTrue(MissingTypeNames.TryGetFieldConstraint(Location("weapon"), out var constraint));

            CollectionAssert.AreEqual(new[] { typeof(ITestWeapon) }, constraint.Types);
            Assert.AreEqual(TypeAllow.All, constraint.Allow);
            Assert.IsFalse(constraint.RequiresScript);
        }

        [Test]
        public void TryGetFieldConstraint_ListElement_ReadsTheListAttribute()
        {
            Assert.IsTrue(MissingTypeNames.TryGetFieldConstraint(Location("concrete.Array.data[2]"), out var constraint));

            Assert.AreEqual(TypeAllow.None, constraint.Allow);
        }

        [Test]
        public void TryGetFieldConstraint_MonoScriptField_RequiresAScript()
        {
            Assert.IsTrue(MissingTypeNames.TryGetFieldConstraint(Location("script"), out var constraint));

            Assert.IsTrue(constraint.RequiresScript);
        }

        [Test]
        public void TryGetFieldConstraint_UnknownField_IsFalse() =>
            Assert.IsFalse(MissingTypeNames.TryGetFieldConstraint(Location("renamedAway"), out _));

        [TestCase("Ns.Spear, Asm, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null", "Ns.Spear, Asm", "Ns.Spear", "Spear")]
        [TestCase("Ns.Outer+Inner, Asm", "Ns.Outer+Inner, Asm", "Ns.Outer+Inner", "Inner")]
        [TestCase("Ns.Box`1[[Ns.T, Asm, Version=0.0.0.0]], Asm, Version=0.0.0.0", "Ns.Box`1[[Ns.T, Asm, Version=0.0.0.0]], Asm", "Ns.Box`1[[Ns.T, Asm, Version=0.0.0.0]]", "Box")]
        [TestCase("Spear", "Spear", "Spear", "Spear")]
        public void Names_SplitTheStoredName(string typeName, string groupKey, string fullName, string shortName)
        {
            Assert.AreEqual(groupKey, MissingTypeNames.GroupKey(typeName));
            Assert.AreEqual(fullName, MissingTypeNames.FullName(typeName));
            Assert.AreEqual(shortName, MissingTypeNames.ShortName(typeName));
        }

        [Test]
        public void Build_GroupsOneTypeWrittenWithDifferentVersions()
        {
            var groups = MissingTypeNameGroup.Build(new[]
            {
                new MissingTypeNameLocation("Assets/A.asset", Entry("Ns.Spear, Asm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "a")),
                new MissingTypeNameLocation("Assets/B.asset", Entry("Ns.Spear, Asm, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null", "b")),
                new MissingTypeNameLocation("Assets/B.asset", Entry("Ns.Axe, Asm", "c")),
            });

            CollectionAssert.AreEqual(new[] { "Axe", "Spear" }, groups.Select(group => group.ShortName));
            Assert.AreEqual(2, groups[1].Entries.Count);
            Assert.AreEqual(2, groups[1].FileCount);
        }

        private static void SaveProbeWithBrokenNames()
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);

            var probe = ScriptableObject.CreateInstance<RequiredWrapperTestObject>();
            probe.type = new SerializableType(typeof(TestSword));
            probe.weaponType = new SerializableType<ITestWeapon>(typeof(TestSword));

            AssetDatabase.CreateAsset(probe, ProbeAssetPath);
            AssetDatabase.SaveAssets();

            // A rename of TestSword, as the files see it.
            File.WriteAllText(ProbeAssetPath, File.ReadAllText(ProbeAssetPath).Replace(nameof(TestSword), "GhostSword"));
        }

        // The SerializableMonoScript field "script": its stored name broken, its script reference replaced.
        private static void ReplaceScriptBlock(string scriptReference)
        {
            var text = File.ReadAllText(ProbeAssetPath);
            text = Regex.Replace(text, @"(?m)^  script:\n(    .*\n)+",
                $"  script:\n    _assemblyQualifiedName: {GhostName}\n    _script: {scriptReference}\n");
            File.WriteAllText(ProbeAssetPath, text);
        }

        private static List<GateViolation> ForProbe(IEnumerable<GateViolation> violations) =>
            violations.Where(violation => violation.AssetPath == ProbeAssetPath).ToList();

        private static MonoScript MonoScriptOf<T>()
        {
            foreach (var guid in AssetDatabase.FindAssets($"{typeof(T).Name} t:MonoScript"))
            {
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
                if (script != null && script.GetClass() == typeof(T)) return script;
            }

            Assert.Fail($"No script asset declares {typeof(T).Name}.");
            return null;
        }

        private static MissingTypeNameLocation Location(string fieldPath) =>
            new("Assets/Probe.asset", new StoredTypeNameEntry(
                fileId: 11400000, rid: 1001, fieldPath, GhostName, scriptGuid: null, scriptFileId: 0,
                hostScriptGuid: null, hostScriptFileId: 0, ManagedTypeName.FromType(typeof(TypeNameConstraintHolder)),
                isOverride: false, targetFileId: 0, targetGuid: null, valueStart: 0, valueEnd: 1, valueHead: null,
                scriptStart: -1, scriptEnd: -1, scriptHead: null));

        private static StoredTypeNameEntry Entry(string typeName, string fieldPath) =>
            new(fileId: 11400000, rid: 0, fieldPath, typeName, scriptGuid: null, scriptFileId: 0, hostScriptGuid: null,
                hostScriptFileId: 0, hostReferenceType: default, isOverride: false, targetFileId: 0, targetGuid: null,
                valueStart: 0, valueEnd: 1, valueHead: null, scriptStart: -1, scriptEnd: -1, scriptHead: null);
    }

    // Breakage detection for stored type names: a baseline of the names that resolved, re-resolved after imports.
    [TestFixture]
    internal sealed class TypeNameBreakageDetectorTests
    {
        private const string ProbeAssetPath = "Assets/__AspidTypeNameBreakageProbe__.asset";
        private const string BrokenKey = "Aspid.TypeNameBreakageProbe.RenamedAway, Assembly-CSharp\u001f:0";

        private bool _breakageDetection;
        private bool _established;
        private string _baseline;
        private Delegate _subscribers;
        private readonly List<TypeNameBreakageReport> _reports = new();

        private static readonly FieldInfo _breakageDetected = typeof(TypeNameBreakageDetector)
            .GetField(nameof(TypeNameBreakageDetector.BreakageDetected), BindingFlags.NonPublic | BindingFlags.Static);

        [SetUp]
        public void SetUp()
        {
            _breakageDetection = SerializeReferenceSettings.BreakageDetectionEnabled;
            _established = SessionState.GetBool(TypeNameBreakageDetector.EstablishedKey, false);
            _baseline = SessionState.GetString(TypeNameBreakageDetector.BaselineKey, string.Empty);

            TypeNameBreakageDetector.ResetForTests();

            _subscribers = (Delegate)_breakageDetected.GetValue(null);
            _breakageDetected.SetValue(null, null);
            _reports.Clear();
            TypeNameBreakageDetector.BreakageDetected += _reports.Add;

            SerializeReferenceSettings.BreakageDetectionEnabled = true;
            SessionState.EraseBool(TypeNameBreakageDetector.EstablishedKey);
            SessionState.EraseString(TypeNameBreakageDetector.BaselineKey);
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);
            TypeNameBreakageDetector.ResetForTests();
            _breakageDetected.SetValue(null, _subscribers);

            SerializeReferenceSettings.BreakageDetectionEnabled = _breakageDetection;
            SessionState.SetBool(TypeNameBreakageDetector.EstablishedKey, _established);
            SessionState.SetString(TypeNameBreakageDetector.BaselineKey, _baseline);
        }

        [Test]
        public void Scan_EstablishesBaselineFromResolvableNamesOnly()
        {
            var probe = ScriptableObject.CreateInstance<RequiredWrapperTestObject>();
            probe.type = new SerializableType(typeof(TestSword));
            AssetDatabase.CreateAsset(probe, ProbeAssetPath);
            AssetDatabase.SaveAssets();

            TypeNameBreakageDetector.Scan(changedAssets: null);
            TypeNameBreakageDetector.CompleteSweep();

            Assert.IsTrue(TypeNameBreakageDetector.IsEstablished);
            CollectionAssert.AreEquivalent(new[] { typeof(TestSword).AssemblyQualifiedName + "\u001f:0" },
                TypeNameBreakageDetector.GetBaselineKeys(ProbeAssetPath));
            CollectionAssert.IsEmpty(_reports);
        }

        [Test]
        public void Scan_BaselineNameNoLongerResolves_ReportsItOnce()
        {
            SessionState.SetString(TypeNameBreakageDetector.BaselineKey, $"{ProbeAssetPath}\t{BrokenKey}");
            SessionState.SetBool(TypeNameBreakageDetector.EstablishedKey, true);

            TypeNameBreakageDetector.Scan(changedAssets: null);
            TypeNameBreakageDetector.Scan(changedAssets: null);

            Assert.AreEqual(1, _reports.Count);
            CollectionAssert.AreEqual(new[] { "Aspid.TypeNameBreakageProbe.RenamedAway" }, _reports[0].TypeNames);
            Assert.AreEqual(1, _reports[0].FileCount);
            CollectionAssert.IsEmpty(TypeNameBreakageDetector.GetBaselineKeys(ProbeAssetPath));
        }
    }
}
