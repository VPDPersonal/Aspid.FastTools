using System;
using System.IO;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using System.Text.RegularExpressions;
using Aspid.FastTools.UIElements.Editors.Internal;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Behavioural coverage for the SerializeReference settings propagation contract (ASP-22):
    /// <list type="bullet">
    /// <item>the excluded-folder set raises the dedicated <see cref="SerializeReferenceSettings.ExcludedFoldersChanged"/>
    /// signal only when it genuinely changes — so the usage index drops its warm copy on a real change but an unrelated
    /// setting never triggers a costly index rebuild;</item>
    /// <item>the shared controls built by <see cref="SerializeReferenceSettingsUI"/> mirror the store live, so the
    /// in-window Settings tab and the Project Settings page stay in sync when either edits a value.</item>
    /// </list>
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceSettingsTests
    {
        // Every shared setter saves this file, so restoring the values alone would still leave a new or rewritten file.
        private const string SharedSettingsPath = "ProjectSettings/SerializeReferenceSharedSettings.asset";

        private bool _autoDeAlias;
        private bool _breakageDetection;
        private string[] _excludedFolders;
        private GateSeverity _buildSeverity;
        private byte[] _sharedSettingsFile;

        [SetUp]
        public void SetUp()
        {
            // Snapshot the project's real settings so the assertions below can mutate them freely and restore on teardown.
            _autoDeAlias = SerializeReferenceSettings.AutoDeAliasEnabled;
            _breakageDetection = SerializeReferenceSettings.BreakageDetectionEnabled;
            _excludedFolders = SerializeReferenceSettings.ExcludedFolders;
            _buildSeverity = SerializeReferenceSettings.BuildSeverity;
            _sharedSettingsFile = File.Exists(SharedSettingsPath) ? File.ReadAllBytes(SharedSettingsPath) : null;
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                SerializeReferenceSettings.AutoDeAliasEnabled = _autoDeAlias;
                SerializeReferenceSettings.BreakageDetectionEnabled = _breakageDetection;
                SerializeReferenceSettings.ExcludedFolders = _excludedFolders;
                SerializeReferenceSettings.BuildSeverity = _buildSeverity;
            }
            finally
            {
                // The values above already match the snapshot in memory; this puts the file back byte for byte,
                // even if one of the setters threw.
                if (_sharedSettingsFile is null) File.Delete(SharedSettingsPath);
                else File.WriteAllBytes(SharedSettingsPath, _sharedSettingsFile);
            }
        }

        // -----------------------------------------------------------------------------------------------------
        // A — excluded folders drive the dedicated index-invalidation signal (and nothing else does)
        // -----------------------------------------------------------------------------------------------------

        // Counts how many times ExcludedFoldersChanged fires while `mutate` runs, leaving the static event clean.
        private static int ExcludedFoldersChangedCount(Action mutate)
        {
            var fired = 0;
            void Handler() => fired++;
            SerializeReferenceSettings.ExcludedFoldersChanged += Handler;
            try { mutate(); }
            finally { SerializeReferenceSettings.ExcludedFoldersChanged -= Handler; }
            return fired;
        }

        [Test]
        public void ExcludedFolders_NewValue_RaisesExcludedFoldersChanged()
        {
            SerializeReferenceSettings.ExcludedFolders = Array.Empty<string>();

            var fired = ExcludedFoldersChangedCount(() =>
                SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Third Party/" });

            Assert.AreEqual(1, fired, "A genuinely new excluded-folder set must raise ExcludedFoldersChanged exactly once.");
        }

        [Test]
        public void ExcludedFolders_SameValue_DoesNotRaiseExcludedFoldersChanged()
        {
            SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Plugins/" };

            // Same paths, fresh array instance: the set did not move, so the warm index must not be dropped.
            var fired = ExcludedFoldersChangedCount(() =>
                SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Plugins/" });

            Assert.AreEqual(0, fired, "Re-assigning an identical set must not raise ExcludedFoldersChanged (no needless index rebuild).");
        }

        [Test]
        public void ExcludedFolders_MutatingAssignedArray_DoesNotChangeSettingsUntilReassigned()
        {
            SerializeReferenceSettings.ExcludedFolders = Array.Empty<string>();
            var folders = new[] { "Assets/Plugins/" };
            SerializeReferenceSettings.ExcludedFolders = folders;

            folders[0] = "Assets/Generated/";

            Assert.IsTrue(SerializeReferenceSettings.IsExcluded("Assets/Plugins/Example.asset"));
            Assert.IsFalse(SerializeReferenceSettings.IsExcluded("Assets/Generated/Example.asset"));

            var fired = ExcludedFoldersChangedCount(() =>
                SerializeReferenceSettings.ExcludedFolders = folders);

            Assert.AreEqual(1, fired);
            Assert.IsFalse(SerializeReferenceSettings.IsExcluded("Assets/Plugins/Example.asset"));
            Assert.IsTrue(SerializeReferenceSettings.IsExcluded("Assets/Generated/Example.asset"));
        }

        [Test]
        public void UnrelatedSetting_DoesNotRaiseExcludedFoldersChanged()
        {
            var fired = ExcludedFoldersChangedCount(() =>
            {
                SerializeReferenceSettings.BreakageDetectionEnabled = !SerializeReferenceSettings.BreakageDetectionEnabled;
                SerializeReferenceSettings.AutoDeAliasEnabled = !SerializeReferenceSettings.AutoDeAliasEnabled;
                SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;
            });

            Assert.AreEqual(0, fired, "Toggling an unrelated setting must never raise ExcludedFoldersChanged (the index stays warm).");
        }

        [Test]
        public void AnySetting_RaisesGeneralChanged()
        {
            var fired = 0;
            void Handler() => fired++;
            SerializeReferenceSettings.Changed += Handler;
            try
            {
                SerializeReferenceSettings.BreakageDetectionEnabled = !SerializeReferenceSettings.BreakageDetectionEnabled;
                Assert.GreaterOrEqual(fired, 1, "Every setter must still raise the general Changed for repaint and live-sync.");
            }
            finally { SerializeReferenceSettings.Changed -= Handler; }
        }

        // -----------------------------------------------------------------------------------------------------
        // B — the per-scope resets restore exactly their own defaults
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void ResetSharedToDefaults_RestoresCommittedDefaults_AndLeavesUserSettingsAlone()
        {
            SerializeReferenceSettings.AutoDeAliasEnabled = false;
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;
            SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Third Party/" };
            SerializeReferenceSettings.BreakageDetectionEnabled = false;

            SerializeReferenceSettings.ResetSharedToDefaults();

            Assert.IsTrue(SerializeReferenceSettings.AutoDeAliasEnabled, "The shared reset must restore auto de-alias to on.");
            Assert.AreEqual(GateSeverity.Warn, SerializeReferenceSettings.BuildSeverity, "The shared reset must restore the gate to Warn.");
            Assert.IsEmpty(SerializeReferenceSettings.ExcludedFolders, "The shared reset must drop every excluded folder.");
            Assert.IsFalse(SerializeReferenceSettings.BreakageDetectionEnabled,
                "The shared reset must not touch the per-user breakage-detection setting.");
        }

        [Test]
        public void ResetUserToDefaults_RestoresBreakageDetection_AndLeavesSharedSettingsAlone()
        {
            SerializeReferenceSettings.BreakageDetectionEnabled = false;
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;

            SerializeReferenceSettings.ResetUserToDefaults();

            Assert.IsTrue(SerializeReferenceSettings.BreakageDetectionEnabled, "The per-user reset must restore breakage detection to on.");
            Assert.AreEqual(GateSeverity.Fail, SerializeReferenceSettings.BuildSeverity,
                "The per-user reset must not touch the shared gate severity.");
        }

        // -----------------------------------------------------------------------------------------------------
        // C — the shared controls mirror the store live (the two settings surfaces stay in sync)
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void BuildControls_LiveSyncsControlsFromSettings()
        {
            SerializeReferenceSettings.AutoDeAliasEnabled = true;
            SerializeReferenceSettings.BreakageDetectionEnabled = true;
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Warn;
            SerializeReferenceSettings.ExcludedFolders = Array.Empty<string>();

            var container = new VisualElement();
            SerializeReferenceSettingsUI.BuildControls(container);

            // The two boolean settings render as iOS-style AspidSwitch fields (BaseField<bool>), not plain Toggles.
            // Looked up by label, so reordering the rows never silently swaps the assertions.
            var switches = container.Query<AspidSwitch>().ToList();
            Assert.AreEqual(2, switches.Count,
                "BuildControls must emit the breakage-detection and auto-de-alias switches.");
            var breakageDetection = switches.Single(s => s.label == "Breakage detection");
            var autoDeAlias = switches.Single(s => s.label.StartsWith("Auto de-alias"));
            var severity = container.Q<EnumField>();
            var folders = container.Q<SerializeReferenceExcludedFoldersField>();
            Assert.IsNotNull(severity, "BuildControls must emit the build-gate EnumField.");
            Assert.IsNotNull(folders, "BuildControls must emit the excluded-folders field.");

            // Mutating the shared store (as the other surface would) must reach these controls without a manual refresh:
            // the switches and the gate re-read their value off SerializeReferenceSettings.Changed, and the folders list
            // rebuilds off the dedicated ExcludedFoldersChanged signal.
            SerializeReferenceSettings.AutoDeAliasEnabled = false;
            SerializeReferenceSettings.BreakageDetectionEnabled = false;
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;
            SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Plugins/", "Assets/Generated/" };

            Assert.IsFalse(autoDeAlias.value, "The auto-de-alias switch must mirror Settings live.");
            Assert.IsFalse(breakageDetection.value, "The breakage-detection switch must mirror Settings live.");
            Assert.AreEqual(GateSeverity.Fail, (GateSeverity)severity.value, "The build-gate field must mirror Settings live.");

            // The list-based folders field renders one path Label per excluded folder; both new paths must appear live.
            var listedPaths = folders.Query<Label>().ToList().Select(label => label.text).ToList();
            Assert.Contains("Assets/Plugins/", listedPaths, "The folders field must list every excluded path live.");
            Assert.Contains("Assets/Generated/", listedPaths, "The folders field must list every excluded path live.");
        }

        // -----------------------------------------------------------------------------------------------------
        // D — the scope filter routes each control to the page that owns its storage
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void BuildControls_UserScope_EmitsOnlyPerUserControls()
        {
            var container = new VisualElement();
            SerializeReferenceSettingsUI.BuildControls(container, AspidSettingsScope.User);

            var labels = container.Query<AspidSwitch>().ToList().Select(s => s.label).ToList();
            Assert.AreEqual(1, labels.Count, "The user scope must emit exactly the one locally-stored switch.");
            Assert.IsTrue(labels.Contains("Breakage detection"), "Breakage detection is per-user and belongs to the user scope.");
            Assert.IsNull(container.Q<EnumField>(), "The build gate is shared and must not leak onto a per-user page.");
            Assert.IsNull(container.Q<SerializeReferenceExcludedFoldersField>(), "Excluded folders are shared and must not leak onto a per-user page.");
        }

        [Test]
        public void BuildControls_SharedScope_EmitsOnlyTeamWideControls()
        {
            var container = new VisualElement();
            SerializeReferenceSettingsUI.BuildControls(container, AspidSettingsScope.Shared);

            var labels = container.Query<AspidSwitch>().ToList().Select(s => s.label).ToList();
            Assert.AreEqual(1, labels.Count, "The shared scope must emit exactly the auto-de-alias switch.");
            Assert.IsTrue(labels.Single().StartsWith("Auto de-alias"), "Auto de-alias is the one shared switch.");
            Assert.IsNotNull(container.Q<EnumField>(), "The build gate is shared and must render on the shared page.");
            Assert.IsNotNull(container.Q<SerializeReferenceExcludedFoldersField>(), "Excluded folders are shared and must render on the shared page.");
        }

        // -----------------------------------------------------------------------------------------------------
        // E — the folder picker accepts only folders the project scans walk
        // -----------------------------------------------------------------------------------------------------

        [TestCase("Assets", ExpectedResult = true)]
        [TestCase("Assets/Plugins", ExpectedResult = true)]
        [TestCase("Packages/com.example.tool", ExpectedResult = false)]
        [TestCase("Library/PackageCache/com.example.tool", ExpectedResult = false)]
        [TestCase("ProjectSettings", ExpectedResult = false)]
        [TestCase("AssetsBackup", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool IsScannedFolder_AcceptsOnlyAssets(string relative) =>
            SerializeReferenceExcludedFoldersField.IsScannedFolder(relative);

        // -----------------------------------------------------------------------------------------------------
        // F — the shared settings follow the file on disk and respect a read-only file
        // -----------------------------------------------------------------------------------------------------

        // Saves every shared setting, so the file exists and holds exactly the values set here.
        private static void SaveSharedDefaults()
        {
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Warn;
            SerializeReferenceSettings.AutoDeAliasEnabled = true;
            SerializeReferenceSettings.ExcludedFolders = Array.Empty<string>();
        }

        // Rewrites the shared settings file behind the Editor, as a pull from the team repository does.
        private static void EditSharedSettingsFile(string pattern, string replacement)
        {
            var text = File.ReadAllText(SharedSettingsPath);
            var edited = Regex.Replace(text, pattern, replacement);

            Assert.AreNotEqual(text, edited, $"The shared settings file has no match for '{pattern}'.");
            File.WriteAllText(SharedSettingsPath, edited);
        }

        [Test]
        public void Setter_AfterExternalEdit_KeepsTheEditedValueOfAnotherSetting()
        {
            SaveSharedDefaults();
            EditSharedSettingsFile("_buildSeverity: .*", "_buildSeverity: " + (int)GateSeverity.Fail);

            SerializeReferenceSettings.AutoDeAliasEnabled = false;

            Assert.AreEqual(GateSeverity.Fail, SerializeReferenceSettings.BuildSeverity,
                "A setter must start from the file on disk, not from the copy loaded before the external edit.");
            var saved = File.ReadAllText(SharedSettingsPath);
            StringAssert.Contains("_buildSeverity: 2", saved, "The save must not overwrite the externally edited severity.");
            StringAssert.Contains("_autoDeAlias: 0", saved);
        }

        [Test]
        public void ReloadShared_AfterExternalEdit_PicksUpTheFile_AndRaisesTheChangeEvents()
        {
            SaveSharedDefaults();
            EditSharedSettingsFile("_buildSeverity: .*", "_buildSeverity: " + (int)GateSeverity.Fail);
            EditSharedSettingsFile(@"_excludedFolders: \[\]", "_excludedFolders:\n  - Assets/Pulled");

            var changed = 0;
            void Handler() => changed++;
            SerializeReferenceSettings.Changed += Handler;
            try
            {
                var foldersChanged = ExcludedFoldersChangedCount(SerializeReferenceSettings.ReloadShared);

                Assert.AreEqual(GateSeverity.Fail, SerializeReferenceSettings.BuildSeverity);
                CollectionAssert.AreEqual(new[] { "Assets/Pulled" }, SerializeReferenceSettings.ExcludedFolders);
                Assert.AreEqual(1, changed, "A reload that moved a value must raise Changed once.");
                Assert.AreEqual(1, foldersChanged, "A reload that moved the folders must drop the warm usage index.");
            }
            finally { SerializeReferenceSettings.Changed -= Handler; }
        }

        [Test]
        public void ReloadShared_UnchangedFile_RaisesNothing()
        {
            SaveSharedDefaults();

            var changed = 0;
            void Handler() => changed++;
            SerializeReferenceSettings.Changed += Handler;
            try
            {
                var foldersChanged = ExcludedFoldersChangedCount(SerializeReferenceSettings.ReloadShared);

                Assert.AreEqual(0, changed, "A reload that moved nothing must not raise Changed.");
                Assert.AreEqual(0, foldersChanged, "A reload that moved nothing must keep the usage index warm.");
            }
            finally { SerializeReferenceSettings.Changed -= Handler; }
        }

        [Test]
        public void UpdateExcludedFolders_Add_AfterExternalEdit_KeepsThePulledFolder()
        {
            SaveSharedDefaults();
            EditSharedSettingsFile(@"_excludedFolders: \[\]", "_excludedFolders:\n  - Assets/Pulled");

            // The facade still holds the empty list from before the edit, as the folders list does after a pull.
            SerializeReferenceSettings.UpdateExcludedFolders(current => current.Append("Assets/Added").ToArray());

            CollectionAssert.AreEqual(new[] { "Assets/Pulled", "Assets/Added" }, SerializeReferenceSettings.ExcludedFolders);
            var saved = File.ReadAllText(SharedSettingsPath);
            StringAssert.Contains("Assets/Pulled", saved, "The save must not drop the folder added by the external edit.");
            StringAssert.Contains("Assets/Added", saved);
        }

        [Test]
        public void UpdateExcludedFolders_Remove_AfterExternalEdit_KeepsThePulledFolder()
        {
            SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Removed" };
            EditSharedSettingsFile("  - Assets/Removed", "  - Assets/Removed\n  - Assets/Pulled");

            SerializeReferenceSettings.UpdateExcludedFolders(current => current.Where(f => f != "Assets/Removed").ToArray());

            CollectionAssert.AreEqual(new[] { "Assets/Pulled" }, SerializeReferenceSettings.ExcludedFolders);
            var saved = File.ReadAllText(SharedSettingsPath);
            StringAssert.Contains("Assets/Pulled", saved, "The save must not drop the folder added by the external edit.");
            StringAssert.DoesNotContain("Assets/Removed", saved);
        }

        [Test]
        public void BuildGate_AfterExternalEdit_ReadsTheEditedSeverity()
        {
            SaveSharedDefaults();
            EditSharedSettingsFile("_buildSeverity: .*", "_buildSeverity: " + (int)GateSeverity.Off);

            // Off returns before any scan; a stale Warn would scan the whole project.
            new SerializeReferenceBuildGate().OnPreprocessBuild(null);

            Assert.AreEqual(GateSeverity.Off, SerializeReferenceSettings.BuildSeverity,
                "The build gate must re-read the settings file before it reads the severity.");
        }

        [Test]
        public void Setter_ReadOnlyFile_RefusesWithClearError_AndKeepsTheStoredValue()
        {
            SaveSharedDefaults();
            var before = File.ReadAllBytes(SharedSettingsPath);

            var changed = 0;
            void Handler() => changed++;
            SerializeReferenceSettings.Changed += Handler;
            File.SetAttributes(SharedSettingsPath, FileAttributes.ReadOnly);
            try
            {
                LogAssert.Expect(LogType.Error, new Regex("is read-only"));
                SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;

                Assert.AreEqual(GateSeverity.Warn, SerializeReferenceSettings.BuildSeverity,
                    "A refused write must leave the stored value.");
                Assert.AreEqual(1, changed, "Changed must fire so the controls return to the stored value.");
                CollectionAssert.AreEqual(before, File.ReadAllBytes(SharedSettingsPath),
                    "A refused write must leave the file byte-identical.");
            }
            finally
            {
                SerializeReferenceSettings.Changed -= Handler;
                File.SetAttributes(SharedSettingsPath, FileAttributes.Normal);
            }
        }
    }
}
