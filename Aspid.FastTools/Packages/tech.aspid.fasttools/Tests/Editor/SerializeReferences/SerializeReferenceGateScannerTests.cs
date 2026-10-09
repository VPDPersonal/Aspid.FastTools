using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using System.Text.RegularExpressions;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceGateScanner.IsPendingMigration"/> — the gate's pre-filter that keeps
    /// properly declared renames from ever warning or failing a build / CI run — and for <see cref="SerializeReferenceGateScanner.Scan"/>
    /// itself surfacing unset required fields (the data source for the Project References "Required violations" group).
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceGateScannerTests
    {
        private const string ProbeAssetPath = "Assets/__AspidGateScannerRequiredProbe__.asset";
        private const string EngineAssetPath = "Assets/__AspidGateScannerEngineProbe__.asset";
        private const string UnreadProbePath = "Assets/__AspidGateScannerUnreadProbe__.prefab";
        private const string ExcludedFolderPath = "Assets/__AspidGateScannerExcluded__";
        private const string ExcludedProbePath = ExcludedFolderPath + "/Probe.asset";
        private const string SharedSettingsPath = "ProjectSettings/SerializeReferenceSharedSettings.asset";

        // Scan(RequiredOnly) is the exact call the Project References "Required violations" group makes; this proves
        // it surfaces both an unset managed reference and an unset [TypeSelector(Required = true)] string field on a
        // saved asset (RequiredTestObject, shared fixture — see SerializeReferenceTestFixtures.cs).
        [Test]
        public void Scan_RequiredOnly_SurfacesUnsetRequiredFieldsOnSavedAsset()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);

                var violations = SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly);
                var forProbe = violations.Where(v => v.AssetPath == ProbeAssetPath && v.FileId == fileId).ToList();

                Assert.IsTrue(forProbe.All(v => v.Kind == GateViolationKind.RequiredUnset));
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredRef)),
                    "Unset [SerializeReference, TypeSelector(Required = true)] field must be reported.");
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredString)),
                    "Unset [TypeSelector(Required = true)] string field must be reported.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // ScanAssetRequiredFields is the scoped, single-asset entry point the Inspect Asset graph calls on every
        // Rescan; it must agree with Scan(RequiredOnly) for that one asset without sweeping the whole project.
        [Test]
        public void ScanAssetRequiredFields_SavedAsset_MatchesProjectScan()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);

                var violations = SerializeReferenceGateScanner.ScanAssetRequiredFields(ProbeAssetPath);
                var forProbe = violations.Where(v => v.FileId == fileId).ToList();

                Assert.IsTrue(forProbe.All(v => v.Kind == GateViolationKind.RequiredUnset));
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredRef)),
                    "Unset [SerializeReference, TypeSelector(Required = true)] field must be reported.");
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredString)),
                    "Unset [TypeSelector(Required = true)] string field must be reported.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // A project sweep loads every prefab and asset it audits; it must release them, or a large project ends up
        // holding all of its content in memory at once.
        [Test]
        public void Scan_RequiredOnly_UnloadsAssetsItLoaded()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                Resources.UnloadAsset(probe);
                Assume.That(AssetDatabase.IsMainAssetAtPathLoaded(ProbeAssetPath), Is.False);

                SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly);

                Assert.IsFalse(AssetDatabase.IsMainAssetAtPathLoaded(ProbeAssetPath),
                    "An asset the sweep loaded for its audit must not stay loaded after the scan.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // The sweep must also unload during the loop, not only at its end, or its peak memory still grows with the
        // project. With a batch of one file, the first probe is released before the sweep reaches the second.
        [Test]
        public void Scan_RequiredOnly_UnloadsLoadedAssetsDuringTheSweep()
        {
            const string secondProbePath = "Assets/__AspidGateScannerRequiredProbe2__.asset";

            var batchSize = SerializeReferenceGateScanner.UnloadEveryLoadedFiles;
            var probes = new[] { ProbeAssetPath, secondProbePath };
            try
            {
                foreach (var path in probes)
                {
                    var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
                    AssetDatabase.CreateAsset(probe, path);
                    Resources.UnloadAsset(probe);
                    Assume.That(AssetDatabase.IsMainAssetAtPathLoaded(path), Is.False);
                }

                SerializeReferenceGateScanner.UnloadEveryLoadedFiles = 1;

                string firstVisited = null;
                bool? firstLoadedAtSecond = null;

                SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly, (_, path) =>
                {
                    if (!probes.Contains(path)) return;

                    if (firstVisited is null) firstVisited = path;
                    else firstLoadedAtSecond = AssetDatabase.IsMainAssetAtPathLoaded(firstVisited);
                });

                Assert.AreEqual(false, firstLoadedAtSecond,
                    "An asset loaded earlier in the sweep must be unloaded before the sweep ends.");
            }
            finally
            {
                SerializeReferenceGateScanner.UnloadEveryLoadedFiles = batchSize;
                foreach (var path in probes) AssetDatabase.DeleteAsset(path);
            }
        }

        // A prefab variant or a prefab nesting an edited prefab inherits the edit, so a rescan after a bulk edit of a
        // prefab must re-audit them too; an unrelated prefab keeps its cached entries.
        [Test]
        public void RescanRequiredFields_EditedPrefab_AlsoRescansItsVariantsAndNestingPrefabs()
        {
            const string basePath = "Assets/__AspidGateScannerBase__.prefab";
            const string variantPath = "Assets/__AspidGateScannerVariant__.prefab";
            const string nestingPath = "Assets/__AspidGateScannerNesting__.prefab";
            const string unrelatedPath = "Assets/__AspidGateScannerUnrelated__.prefab";

            var temporary = new List<GameObject>();
            try
            {
                var baseRoot = new GameObject("Base");
                temporary.Add(baseRoot);
                var basePrefab = PrefabUtility.SaveAsPrefabAsset(baseRoot, basePath);

                var variantRoot = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                temporary.Add(variantRoot);
                PrefabUtility.SaveAsPrefabAsset(variantRoot, variantPath);

                var nestingRoot = new GameObject("Nesting");
                temporary.Add(nestingRoot);
                ((GameObject)PrefabUtility.InstantiatePrefab(basePrefab)).transform.SetParent(nestingRoot.transform);
                PrefabUtility.SaveAsPrefabAsset(nestingRoot, nestingPath);

                var unrelatedRoot = new GameObject("Unrelated");
                temporary.Add(unrelatedRoot);
                PrefabUtility.SaveAsPrefabAsset(unrelatedRoot, unrelatedPath);

                var cached = new[]
                {
                    new GateViolation(variantPath, 1, 0, default, GateViolationKind.RequiredUnset, "stale"),
                    new GateViolation(nestingPath, 1, 0, default, GateViolationKind.RequiredUnset, "stale"),
                    new GateViolation(unrelatedPath, 1, 0, default, GateViolationKind.RequiredUnset, "kept"),
                };

                var refreshed = SerializeReferenceGateScanner.RescanRequiredFields(cached, new[] { basePath });

                Assert.IsFalse(refreshed.Any(v => v.FieldPath == "stale"),
                    "A variant or a nesting prefab of the edited prefab must be re-audited.");
                Assert.IsTrue(refreshed.Any(v => v.AssetPath == unrelatedPath && v.FieldPath == "kept"),
                    "An unrelated prefab keeps its cached violations.");
            }
            finally
            {
                foreach (var gameObject in temporary) Object.DestroyImmediate(gameObject);
                foreach (var path in new[] { nestingPath, variantPath, unrelatedPath, basePath }) AssetDatabase.DeleteAsset(path);
            }
        }

        // After a bulk edit the Project References window re-audits only the rewritten files; the cached entries of
        // those files are replaced and every other file's are kept.
        [Test]
        public void RescanRequiredFields_ReplacesOnlyTheRescannedFiles()
        {
            const string otherPath = "Assets/__AspidGateScannerUntouched__.asset";

            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);

                var cached = new[]
                {
                    new GateViolation(ProbeAssetPath, 1, 0, default, GateViolationKind.RequiredUnset, "stale"),
                    new GateViolation(otherPath, 1, 0, default, GateViolationKind.RequiredUnset, "kept"),
                };

                var refreshed = SerializeReferenceGateScanner.RescanRequiredFields(cached, new[] { ProbeAssetPath });

                Assert.IsTrue(refreshed.Any(v => v.AssetPath == otherPath && v.FieldPath == "kept"),
                    "A file outside the rescan keeps its cached violations.");
                Assert.IsFalse(refreshed.Any(v => v.FieldPath == "stale"), "A rescanned file drops its cached entries.");
                Assert.IsTrue(refreshed.Any(v => v.AssetPath == ProbeAssetPath && v.FieldPath == nameof(RequiredTestObject.requiredRef)),
                    "A rescanned file reports its current violations.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // An asset whose file is an LFS pointer (a checkout without the LFS objects) cannot be checked for missing
        // types; the scan must hand it back as unscanned instead of passing it silently.
        [Test]
        public void Scan_MissingOnly_ReportsLfsPointerAsUnscanned()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                File.WriteAllText(ProbeAssetPath, "version https://git-lfs.github.com/spec/v1\noid sha256:0\nsize 1\n");

                var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();
                SerializeReferenceGateScanner.Scan(GateOptions.MissingOnly, unscanned: unscanned);

                CollectionAssert.Contains(unscanned, (ProbeAssetPath, AssetFileFormat.LfsPointer));
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // A binary ScriptableObject under Force Text (never re-saved, or [PreferBinarySerialization]) can hold managed
        // references the scan could not read, so the warning names it.
        [Test]
        public void Scan_BinaryScriptableObjectUnderForceText_Warns()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                File.WriteAllBytes(ProbeAssetPath, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x11 });

                var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();
                SerializeReferenceGateScanner.Scan(GateOptions.MissingOnly, unscanned: unscanned);
                CollectionAssert.Contains(unscanned, (ProbeAssetPath, AssetFileFormat.Binary));

                var notice = SerializeReferenceGateScanner.DescribeUnscanned(unscanned, SerializationMode.ForceText);
                StringAssert.Contains(ProbeAssetPath, notice);
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        [TestCase("Assets/Weapons/Pistol.prefab")]
        [TestCase("Assets/Scenes/Arena.unity")]
        public void DescribeUnscanned_BinaryPrefabOrSceneUnderForceText_Warns(string path)
        {
            var unscanned = new[] { (path, AssetFileFormat.Binary) };
            var notice = SerializeReferenceGateScanner.DescribeUnscanned(unscanned, SerializationMode.ForceText);

            StringAssert.Contains("1 binary prefab, scene or ScriptableObject file(s)", notice);
            StringAssert.Contains(path, notice);
        }

        // A main asset that is not a ScriptableObject, like the LightingData and NavMesh files Unity writes binary
        // under Force Text, cannot hold managed references and stays out of the warning.
        [Test]
        public void DescribeUnscanned_BinaryEngineAssetUnderForceText_IsSilent()
        {
            try
            {
                AssetDatabase.CreateAsset(new Mesh(), EngineAssetPath);

                var unscanned = new[] { (EngineAssetPath, AssetFileFormat.Binary) };
                Assert.IsNull(SerializeReferenceGateScanner.DescribeUnscanned(unscanned, SerializationMode.ForceText));
            }
            finally
            {
                AssetDatabase.DeleteAsset(EngineAssetPath);
            }
        }

        [TestCase(SerializationMode.ForceBinary)]
        [TestCase(SerializationMode.Mixed)]
        public void DescribeUnscanned_BinaryOutsideForceText_Warns(SerializationMode mode)
        {
            var unscanned = new[] { ("Assets/Weapons/Pistol.prefab", AssetFileFormat.Binary) };
            var notice = SerializeReferenceGateScanner.DescribeUnscanned(unscanned, mode);

            StringAssert.Contains("Force Text", notice);
            StringAssert.Contains(mode.ToString(), notice);
        }

        [Test]
        public void DescribeUnscanned_LfsPointer_WarnsWithPath()
        {
            try
            {
                AssetDatabase.CreateAsset(new Mesh(), EngineAssetPath);

                var unscanned = new[]
                {
                    (EngineAssetPath, AssetFileFormat.Binary),
                    ("Assets/Weapons/Pistol.prefab", AssetFileFormat.LfsPointer),
                };

                var notice = SerializeReferenceGateScanner.DescribeUnscanned(unscanned, SerializationMode.ForceText);

                StringAssert.Contains("1 file(s) were not checked", notice);
                StringAssert.Contains("1 Git LFS pointer(s)", notice);
                StringAssert.Contains("Assets/Weapons/Pistol.prefab", notice);
                StringAssert.DoesNotContain(EngineAssetPath, notice);
            }
            finally
            {
                AssetDatabase.DeleteAsset(EngineAssetPath);
            }
        }

        // The Project References window counts the files DescribeUnscanned names, so a binary engine asset under Force
        // Text does not turn its summary into a warning.
        [Test]
        public void CountReportableUnscanned_CountsWhatDescribeUnscannedNames()
        {
            try
            {
                AssetDatabase.CreateAsset(new Mesh(), EngineAssetPath);

                var unscanned = new[]
                {
                    (EngineAssetPath, AssetFileFormat.Binary),
                    ("Assets/Weapons/Pistol.prefab", AssetFileFormat.Binary),
                    ("Assets/Weapons/Rifle.prefab", AssetFileFormat.LfsPointer),
                };

                Assert.AreEqual(2, SerializeReferenceGateScanner.CountReportableUnscanned(unscanned, SerializationMode.ForceText));
                Assert.AreEqual(3, SerializeReferenceGateScanner.CountReportableUnscanned(unscanned, SerializationMode.Mixed));
            }
            finally
            {
                AssetDatabase.DeleteAsset(EngineAssetPath);
            }
        }

        [Test]
        public void CountReportableUnscanned_Nothing_IsZero() =>
            Assert.AreEqual(0, SerializeReferenceGateScanner.CountReportableUnscanned(
                Array.Empty<(string, AssetFileFormat)>(), SerializationMode.ForceBinary));

        // Asset References reads one picked asset: a binary prefab or an LFS pointer is not a prefab without references.
        [TestCase("version https://git-lfs.github.com/spec/v1\noid sha256:0\nsize 1\n", AssetFileFormat.LfsPointer)]
        [TestCase("\u0001\u0002binary", AssetFileFormat.Binary)]
        public void TryGetUnreadFormat_FileThatIsNotTextYaml_ReturnsItsFormat(string content, AssetFileFormat expected)
        {
            try
            {
                File.WriteAllText(UnreadProbePath, content);

                Assert.IsTrue(SerializeReferenceGateScanner.TryGetUnreadFormat(UnreadProbePath, SerializationMode.ForceText, out var format));
                Assert.AreEqual(expected, format);
            }
            finally
            {
                File.Delete(UnreadProbePath);
            }
        }

        [Test]
        public void TryGetUnreadFormat_TextYaml_IsReadable()
        {
            try
            {
                File.WriteAllText(UnreadProbePath, "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n");

                Assert.IsFalse(SerializeReferenceGateScanner.TryGetUnreadFormat(UnreadProbePath, SerializationMode.ForceText, out _));
            }
            finally
            {
                File.Delete(UnreadProbePath);
            }
        }

        [Test]
        public void TryGetUnreadFormat_MissingFile_IsNotUnread() =>
            Assert.IsFalse(SerializeReferenceGateScanner.TryGetUnreadFormat(UnreadProbePath, SerializationMode.ForceText, out _));

        [Test]
        public void TryGetUnreadFormat_ExtensionTheScansNeverRead_IsNotUnread()
        {
            const string path = "Assets/__AspidGateScannerUnreadProbe__.bytes";

            try
            {
                File.WriteAllText(path, "\u0001\u0002binary");

                Assert.IsFalse(SerializeReferenceGateScanner.TryGetUnreadFormat(path, SerializationMode.ForceText, out _));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestCase(AssetFileFormat.LfsPointer, "Git LFS pointer")]
        [TestCase(AssetFileFormat.Binary, "Force Text")]
        public void BuildUnreadAssetMessage_NamesTheCauseAndTheWayOut(AssetFileFormat format, string expected) =>
            StringAssert.Contains(expected, SerializeReferenceGraphSummary.BuildUnreadAssetMessage(format));

        [Test]
        public void DescribeUnscanned_Nothing_IsSilent() =>
            Assert.IsNull(SerializeReferenceGateScanner.DescribeUnscanned(
                Array.Empty<(string, AssetFileFormat)>(), SerializationMode.ForceBinary));

        [Test]
        public void ScanAssetRequiredFields_NonCandidatePath_ReturnsEmpty()
        {
            Assert.AreEqual(0, SerializeReferenceGateScanner.ScanAssetRequiredFields("Assets/Fake.txt").Count);
        }

        // Asset References inspects the one asset the user picked, so an excluded folder, which keeps the asset out
        // of the project audit, must not hide its required fields there, just as it does not hide its missing types.
        [Test]
        public void ScanAssetRequiredFields_AssetInExcludedFolder_StillReportsUnsetFields()
        {
            WithExcludedProbe(() =>
            {
                var violations = SerializeReferenceGateScanner.ScanAssetRequiredFields(ExcludedProbePath);

                Assert.IsTrue(violations.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredRef)),
                    "A picked asset in an excluded folder must still report its unset required reference.");
                Assert.IsTrue(violations.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredString)),
                    "A picked asset in an excluded folder must still report its unset required string field.");
            });
        }

        // The Project References audit skips excluded folders, so re-auditing an edited file there adds nothing to it.
        [Test]
        public void RescanRequiredFields_AssetInExcludedFolder_AddsNothing()
        {
            WithExcludedProbe(() =>
            {
                var refreshed = SerializeReferenceGateScanner.RescanRequiredFields(
                    Array.Empty<GateViolation>(), new[] { ExcludedProbePath });

                Assert.IsFalse(refreshed.Any(v => v.AssetPath == ExcludedProbePath),
                    "The project audit must keep skipping a file in an excluded folder after a rescan.");
            });
        }

        // Saves an unset RequiredTestObject into a folder the shared settings exclude, then removes the folder and
        // puts the settings file back byte for byte.
        private static void WithExcludedProbe(Action body)
        {
            var excludedFolders = SerializeReferenceSettings.ExcludedFolders;
            var settingsFile = File.Exists(SharedSettingsPath) ? File.ReadAllBytes(SharedSettingsPath) : null;
            try
            {
                AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: Path.GetFileName(ExcludedFolderPath));
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RequiredTestObject>(), ExcludedProbePath);
                SerializeReferenceSettings.ExcludedFolders = excludedFolders.Append(ExcludedFolderPath).ToArray();
                Assume.That(SerializeReferenceHelpers.IsScanCandidate(ExcludedProbePath), Is.False);

                body();
            }
            finally
            {
                try
                {
                    SerializeReferenceSettings.ExcludedFolders = excludedFolders;
                }
                finally
                {
                    if (settingsFile is null) File.Delete(SharedSettingsPath);
                    else File.WriteAllBytes(SharedSettingsPath, settingsFile);

                    AssetDatabase.DeleteAsset(ExcludedFolderPath);
                }
            }
        }

        // The Inspect Asset graph (SerializeReferenceGraphView) badges an empty [SerializeReference] slot as REQUIRED
        // by matching its graph field path — built independently by SerializeReferenceGraphScanner straight from
        // YAML — against this scan's GateViolation.FieldPath (a live SerializedProperty.propertyPath), after the same
        // "[i]" -> ".Array.data[i]" normalization the view applies. This proves the two paths actually agree for a
        // real saved asset, not just that each individually finds the field.
        [Test]
        public void ScanAssetRequiredFields_UnsetManagedReference_MatchesGraphScannerEmptyRootPath()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);

                var violations = SerializeReferenceGateScanner.ScanAssetRequiredFields(ProbeAssetPath);
                var document = SerializeReferenceGraphScanner.Build(ProbeAssetPath).Single(doc => doc.FileId == fileId);
                var emptyRoot = document.Roots.Single(root => root.IsEmpty);

                var normalizedGraphPath = Regex.Replace(emptyRoot.Label, @"\[(\d+)\]", ".Array.data[$1]");

                Assert.IsTrue(
                    violations.Any(v => v.FileId == fileId && v.FieldPath == normalizedGraphPath),
                    $"Normalized graph path '{normalizedGraphPath}' must match a ScanAssetRequiredFields violation's FieldPath.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // A SerializableType field is reported by the path the Inspector shows, the one the scene scan's descriptor
        // carries, not by the backing string the SerializedProperty iterator stops on; Assign Required still reaches
        // that string through it.
        [Test]
        public void ScanAssetRequiredFields_UnsetSerializableType_ReportsSceneScanPath()
        {
            var probe = ScriptableObject.CreateInstance<RequiredWrapperTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);

                var violations = SerializeReferenceGateScanner.ScanAssetRequiredFields(ProbeAssetPath)
                    .Where(v => v.FileId == fileId)
                    .ToList();
                var scenePaths = TypeSelectorRequiredGate.GetRequiredFields(typeof(RequiredWrapperTestObject))
                    .Select(field => field.Path)
                    .ToArray();

                CollectionAssert.AreEquivalent(new[] { "type", "weaponType", "script", "loadout.type" }, scenePaths);
                CollectionAssert.AreEquivalent(scenePaths, violations.Select(v => v.FieldPath));

                foreach (var violation in violations)
                {
                    Assert.IsTrue(SerializeReferenceGraphEditor.TryResolveRequiredStringProperty(
                        violation, out var serializedObject, out var property), violation.FieldPath);

                    using (serializedObject)
                    {
                        Assert.AreEqual($"{violation.FieldPath}.{SerializableTypeUtility.BackingFieldName}",
                            property.propertyPath);
                    }
                }
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // A [MovedFrom]-claimed stale name is a pending migration, not a violation: Unity migrates the reference in
        // memory at load, so the gate must accept it — a properly declared rename can never warn or fail a build /
        // CI run. A scene path exercises the trust-the-claim branch (constraints are unrecoverable for scenes).
        // (RenamedRanged is the shared [MovedFrom(..., "OldRenamedRanged")] fixture.)
        [Test]
        public void IsPendingMigration_MovedFromClaimedName_IsNotAViolation()
        {
            var stored = new ManagedTypeName(typeof(RenamedRanged).Assembly.GetName().Name, typeof(RenamedRanged).Namespace, "OldRenamedRanged");
            var entry = new MissingReferenceEntry(fileId: 1, rid: 100, stored);

            Assert.IsTrue(SerializeReferenceGateScanner.IsPendingMigration("Assets/Fake.unity", entry));
        }

        [Test]
        public void IsPendingMigration_UnknownName_StaysAViolation()
        {
            var stored = new ManagedTypeName(
                typeof(RenamedRanged).Assembly.GetName().Name, typeof(RenamedRanged).Namespace, "GhostNeverExisted");
            var entry = new MissingReferenceEntry(fileId: 1, rid: 100, stored);

            Assert.IsFalse(SerializeReferenceGateScanner.IsPendingMigration("Assets/Fake.unity", entry));
        }
    }
}
