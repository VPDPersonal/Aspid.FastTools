using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors
{
    // A live panel for tests that need USS resolution, layout or event dispatch. In batch mode a runtime panel, because
    // only it updates without a graphics device (-nographics). In the GUI Editor a shown EditorWindow, because there
    // the player loop that updates runtime panels does not tick in Edit Mode.
    internal sealed class TestPanel : IDisposable
    {
        // A committed asset instead of CreateInstance: a new PanelSettings without a theme makes the Editor write
        // a default theme into the project's Assets folder.
        private const string SettingsPath =
            "Packages/tech.aspid.fasttools/Tests/Editor/Resources/UI/TestPanel/AspidFastToolsTestPanelSettings.asset";

        private readonly GameObject _host;
        private readonly PanelSettings _settings;
        private readonly EditorWindow _window;

        public VisualElement Root { get; }

        // runtime keeps the runtime panel in the GUI Editor too, for a test that needs a panel no Editor window owns.
        public TestPanel(bool runtime = false)
        {
            if (!runtime && !Application.isBatchMode)
            {
                _window = ScriptableObject.CreateInstance<EditorWindow>();
                _window.hideFlags = HideFlags.DontSave;
                _window.Show();

                // A child instead of the window root: the root already carries the Editor's default style sheet.
                Root = new VisualElement { style = { flexGrow = 1 } };
                _window.rootVisualElement.Add(Root);
                return;
            }

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
            if (_window) _window.Close();
            if (_host) Object.DestroyImmediate(_host);
            if (_settings) Object.DestroyImmediate(_settings);
        }
    }
}
