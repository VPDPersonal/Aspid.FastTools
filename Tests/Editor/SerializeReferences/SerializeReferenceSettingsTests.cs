using System;
using NUnit.Framework;
using UnityEngine.UIElements;

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
        private bool _ridColors;
        private bool _autoDeAlias;
        private string[] _excludedFolders;
        private GateSeverity _buildSeverity;

        [SetUp]
        public void SetUp()
        {
            // Snapshot the project's real settings so the assertions below can mutate them freely and restore on teardown.
            _ridColors = SerializeReferenceSettings.RidColorsEnabled;
            _autoDeAlias = SerializeReferenceSettings.AutoDeAliasEnabled;
            _excludedFolders = SerializeReferenceSettings.ExcludedFolders;
            _buildSeverity = SerializeReferenceSettings.BuildSeverity;
        }

        [TearDown]
        public void TearDown()
        {
            SerializeReferenceSettings.RidColorsEnabled = _ridColors;
            SerializeReferenceSettings.AutoDeAliasEnabled = _autoDeAlias;
            SerializeReferenceSettings.ExcludedFolders = _excludedFolders;
            SerializeReferenceSettings.BuildSeverity = _buildSeverity;
        }

        // -----------------------------------------------------------------------------------------------------
        // A — excluded folders drive the dedicated index-invalidation signal (and nothing else does)
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void ExcludedFolders_NewValue_RaisesExcludedFoldersChanged()
        {
            SerializeReferenceSettings.ExcludedFolders = Array.Empty<string>();

            var fired = 0;
            void Handler() => fired++;
            SerializeReferenceSettings.ExcludedFoldersChanged += Handler;
            try
            {
                SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Third Party/" };
                Assert.AreEqual(1, fired, "A genuinely new excluded-folder set must raise ExcludedFoldersChanged exactly once.");
            }
            finally { SerializeReferenceSettings.ExcludedFoldersChanged -= Handler; }
        }

        [Test]
        public void ExcludedFolders_SameValue_DoesNotRaiseExcludedFoldersChanged()
        {
            SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Plugins/" };

            var fired = 0;
            void Handler() => fired++;
            SerializeReferenceSettings.ExcludedFoldersChanged += Handler;
            try
            {
                // Same paths, fresh array instance: the set did not move, so the warm index must not be dropped.
                SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Plugins/" };
                Assert.AreEqual(0, fired, "Re-assigning an identical set must not raise ExcludedFoldersChanged (no needless index rebuild).");
            }
            finally { SerializeReferenceSettings.ExcludedFoldersChanged -= Handler; }
        }

        [Test]
        public void UnrelatedSetting_DoesNotRaiseExcludedFoldersChanged()
        {
            var fired = 0;
            void Handler() => fired++;
            SerializeReferenceSettings.ExcludedFoldersChanged += Handler;
            try
            {
                SerializeReferenceSettings.RidColorsEnabled = !SerializeReferenceSettings.RidColorsEnabled;
                SerializeReferenceSettings.AutoDeAliasEnabled = !SerializeReferenceSettings.AutoDeAliasEnabled;
                SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;
                Assert.AreEqual(0, fired, "Toggling an unrelated setting must never raise ExcludedFoldersChanged (the index stays warm).");
            }
            finally { SerializeReferenceSettings.ExcludedFoldersChanged -= Handler; }
        }

        [Test]
        public void AnySetting_RaisesGeneralChanged()
        {
            var fired = 0;
            void Handler() => fired++;
            SerializeReferenceSettings.Changed += Handler;
            try
            {
                SerializeReferenceSettings.RidColorsEnabled = !SerializeReferenceSettings.RidColorsEnabled;
                Assert.GreaterOrEqual(fired, 1, "Every setter must still raise the general Changed for repaint and live-sync.");
            }
            finally { SerializeReferenceSettings.Changed -= Handler; }
        }

        // -----------------------------------------------------------------------------------------------------
        // B — the shared controls mirror the store live (the two settings surfaces stay in sync)
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void BuildControls_LiveSyncsControlsFromSettings()
        {
            SerializeReferenceSettings.RidColorsEnabled = true;
            SerializeReferenceSettings.AutoDeAliasEnabled = true;
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Warn;
            SerializeReferenceSettings.ExcludedFolders = Array.Empty<string>();

            var container = new VisualElement();
            SerializeReferenceSettingsUI.BuildControls(container);

            var toggles = container.Query<Toggle>().ToList();
            Assert.AreEqual(2, toggles.Count, "BuildControls must emit the rid-colours and auto-de-alias toggles.");
            var ridColors = toggles[0];
            var autoDeAlias = toggles[1];
            var severity = container.Q<EnumField>();
            var folders = container.Q<TextField>();
            Assert.IsNotNull(severity, "BuildControls must emit the build-gate EnumField.");
            Assert.IsNotNull(folders, "BuildControls must emit the excluded-folders TextField.");

            // Mutating the shared store (as the other surface would) must reach these controls without a manual refresh:
            // each control re-reads its backing value off SerializeReferenceSettings.Changed.
            SerializeReferenceSettings.RidColorsEnabled = false;
            SerializeReferenceSettings.AutoDeAliasEnabled = false;
            SerializeReferenceSettings.BuildSeverity = GateSeverity.Fail;
            SerializeReferenceSettings.ExcludedFolders = new[] { "Assets/Plugins/", "Assets/Generated/" };

            Assert.IsFalse(ridColors.value, "The rid-colours toggle must mirror Settings live.");
            Assert.IsFalse(autoDeAlias.value, "The auto-de-alias toggle must mirror Settings live.");
            Assert.AreEqual(GateSeverity.Fail, (GateSeverity)severity.value, "The build-gate field must mirror Settings live.");
            Assert.AreEqual("Assets/Plugins/\nAssets/Generated/", folders.value, "The folders field must mirror Settings live.");
        }
    }
}
