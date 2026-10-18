using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    // A live panel for tests that need USS resolution, layout or event dispatch. A runtime panel rather than a shown
    // EditorWindow, because only the former updates without a graphics device (-nographics).
    internal sealed class TestPanel : IDisposable
    {
        // A committed asset instead of CreateInstance: a new PanelSettings without a theme makes the Editor write
        // a default theme into the project's Assets folder.
        private const string SettingsPath =
            "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements/TestPanel/AspidFastToolsTestPanelSettings.asset";

        private readonly GameObject _host;
        private readonly PanelSettings _settings;

        public VisualElement Root { get; }

        public TestPanel()
        {
            // A copy per test: the panel lives as long as its PanelSettings, so destroying the copy drops it.
            _settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<PanelSettings>(SettingsPath));
            _settings.hideFlags = HideFlags.HideAndDontSave;

            _host = new GameObject(nameof(TestPanel)) { hideFlags = HideFlags.HideAndDontSave };
            var document = _host.AddComponent<UIDocument>();
            document.panelSettings = _settings;

            Root = document.rootVisualElement;
        }

        public void Dispose()
        {
            if (_host) Object.DestroyImmediate(_host);
            if (_settings) Object.DestroyImmediate(_settings);
        }

        // `yield return TestPanel.NextFrame()` in place of `yield return null`. A runtime panel updates on the player
        // loop, which the Editor runs in Edit Mode only when something asks for it. Without the request, a frame wait
        // in a batch-mode Editor on Unity 6000.3 and later lasts until the next idle update: 2-4 seconds.
        public static object NextFrame()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            return null;
        }
    }
}
