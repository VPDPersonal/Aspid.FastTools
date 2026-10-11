using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;
using Aspid.FastTools.Editors.Internal;
using Aspid.FastTools.SerializeReferences.Editors;

namespace Aspid.FastTools.Editors.Tests
{
    // Opens each editor window of the package and checks that its UI builds without a logged error.
    // A window that fails in CreateGUI or OnGUI stays blank, and no test of its parts notices.
    // The windows need a graphics device: these tests run in CI, not under -nographics.
    [TestFixture]
    internal sealed class EditorWindowSmokeTests
    {
        // The window keeps these class names private. A rename there needs a rename here.
        private const string ContainerClass = "aspid-fasttools-serialize-reference-window__container";
        private const string ActiveButtonClass = "aspid-fasttools-serialize-reference-window__toolbar-button--active";
        private const string ToolbarButtonClass = "aspid-fasttools-serialize-reference-window__toolbar-button";
        private const string FooterKeysClass = "aspid-fasttools-window-footer__keys";

        private const int MaxFrames = 30;

        [UnityTest]
        public IEnumerator TabWindow_EveryTab_ReplacesTheViewAndMarksItsButton()
        {
            var window = ScriptableObject.CreateInstance<TabWindow>();
            try
            {
                window.ShowUtility();
                yield return WaitUntil(() => FindContainer(window) is not null);

                var container = FindContainer(window);
                Assert.IsNotNull(container, "CreateGUI must build the container that hosts the tab views.");
                Assert.AreEqual(4, window.rootVisualElement.Query<Button>(className: ToolbarButtonClass).ToList().Count,
                    "The toolbar must hold one button per tab.");

                foreach (var (tab, viewType) in new (TabType, Type)[]
                         {
                             (TabType.Welcome, typeof(WelcomeView)),
                             (TabType.AssetReference, typeof(SerializeReferenceGraphView)),
                             (TabType.ProjectReferences, typeof(SerializeReferenceProjectView)),
                             (TabType.Settings, typeof(SettingsView)),
                             (TabType.Welcome, typeof(WelcomeView)),
                         })
                {
                    window.SwitchMode(tab);
                    yield return null;

                    Assert.AreEqual(tab, window.CurrentTabType);
                    Assert.AreEqual(1, container.childCount, $"{tab} must replace the previous view, not stack on it.");
                    Assert.IsInstanceOf(viewType, container[0], $"{tab} must show {viewType.Name}.");
                    Assert.AreEqual(1, window.rootVisualElement.Query<Button>(className: ActiveButtonClass).ToList().Count,
                        $"Exactly one toolbar button must be active on {tab}.");
                    Assert.AreEqual(TabWindow.KeysHintFor(tab), window.rootVisualElement.Q<Label>(className: FooterKeysClass)?.text,
                        $"The footer must show the keys of {tab}.");
                }
            }
            finally
            {
                Close(window);
            }
        }

        [UnityTest]
        public IEnumerator SampleThemeWindow_BuildsThePaletteForm()
        {
            var window = ScriptableObject.CreateInstance<SampleThemeWindow>();
            try
            {
                window.ShowUtility();
                yield return WaitUntil(() => window.rootVisualElement.Q<ScrollView>() is not null);

                var root = window.rootVisualElement;
                var palettes = root.Q<ScrollView>();
                Assert.IsNotNull(palettes, "CreateGUI must build the palette list.");
                Assert.IsNotNull(root.Q<HelpBox>(), "CreateGUI must explain what the preview applies to.");

                // A bound field builds children of the same type, so only the direct ones count. VisualElement.parent
                // is the logical parent (the ScrollView); hierarchy.parent is the content container.
                var content = palettes.contentContainer;
                var fields = content.Query<PropertyField>().Where(field => field.hierarchy.parent == content).ToList();
                Assert.AreEqual(3, fields.Count, "The window must show the light, the dark and the EnumValues light palette.");
            }
            finally
            {
                Close(window);
            }
        }

        // The prompt is a pure IMGUI window: its OnGUI lays out a text field and two buttons by hand.
        // The test cannot prove that OnGUI ran: it fails on a logged error or a closed window, nothing else.
        // Closing the prompt with a sent Escape event would prove more, but it could not be tried without a display.
        [UnityTest]
        public IEnumerator SerializeReferenceNamePrompt_Repaints_WithoutErrorAndStaysOpen()
        {
            var prompt = ScriptableObject.CreateInstance<SerializeReferenceNamePrompt>();
            try
            {
                prompt.ShowUtility();

                for (var frame = 0; frame < 5; frame++)
                {
                    prompt.Repaint();
                    yield return null;
                }

                Assert.IsTrue(prompt != null, "Repainting the form must not close the prompt.");
            }
            finally
            {
                Close(prompt);
            }
        }

        // Close() takes the native window down with the view. DestroyImmediate leaves it on Linux, where its animated
        // content keeps repainting through software rendering and slows every later test.
        private static void Close(EditorWindow window)
        {
            if (window != null) window.Close();
        }

        private static VisualElement FindContainer(EditorWindow window) =>
            window.rootVisualElement.Q(className: ContainerClass);

        private static IEnumerator WaitUntil(Func<bool> condition)
        {
            for (var frame = 0; frame < MaxFrames && !condition(); frame++)
                yield return null;
        }
    }
}
