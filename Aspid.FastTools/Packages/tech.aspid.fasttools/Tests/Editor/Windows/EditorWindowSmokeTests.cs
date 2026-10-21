using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Aspid.FastTools.SerializeReferences.Editors;

namespace Aspid.FastTools.Editors.Tests
{
    // Opens each editor window of the package and checks that its UI builds without a logged error.
    // A window that fails in CreateGUI or OnGUI stays blank, and no test of its parts notices.
    // The windows need a graphics device: these tests run in CI, not under -nographics.
    [TestFixture]
    [Category("Graphics")]
    internal sealed class EditorWindowSmokeTests
    {
        // The window keeps these class names private. A rename there needs a rename here.
        private const string ContainerClass = "aspid-fasttools-tab-window__container";
        private const string ActiveButtonClass = "aspid-fasttools-tab-window__toolbar-button--active";
        private const string ToolbarButtonClass = "aspid-fasttools-tab-window__toolbar-button";
        private const string FooterKeysClass = "aspid-fasttools-window-footer__keys";

        // The name prompt sets this private flag in its first OnGUI. A rename there needs a rename here.
        private const string PromptDrawnField = "_focused";

        private const int MaxFrames = 30;

        [UnityTest]
        public IEnumerator TabWindow_EveryTab_ShowsOnlyItsViewAndMarksItsButton()
        {
            var window = ScriptableObject.CreateInstance<TabWindow>();
            try
            {
                window.ShowUtility();
                yield return WaitUntil(() => FindContainer(window) is not null);

                var container = FindContainer(window);
                Assert.IsNotNull(container, "CreateGUI must build the container that hosts the tab views.");
                Assert.AreEqual(TabWindow.Tabs.Length, window.rootVisualElement.Query<Button>(className: ToolbarButtonClass).ToList().Count,
                    "The toolbar must hold one button per tab.");

                VisualElement firstWelcome = null;

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

                    var shown = container.Children().Where(view => view.resolvedStyle.display == DisplayStyle.Flex).ToList();
                    Assert.AreEqual(1, shown.Count, $"{tab} must show one view and hide the others.");
                    Assert.IsInstanceOf(viewType, shown[0], $"{tab} must show {viewType.Name}.");

                    // The window keeps a view between visits, so the second Welcome is the first one again.
                    if (tab == TabType.Welcome)
                    {
                        firstWelcome ??= shown[0];
                        Assert.AreSame(firstWelcome, shown[0], "A tab visited again must show the view it built before.");
                    }

                    Assert.AreEqual(1, window.rootVisualElement.Query<Button>(className: ActiveButtonClass).ToList().Count,
                        $"Exactly one toolbar button must be active on {tab}.");
                    Assert.AreEqual(TabWindow.KeysHintFor(tab), window.rootVisualElement.Q<Label>(className: FooterKeysClass)?.text,
                        $"The footer must show the keys of {tab}.");
                }

                Assert.AreEqual(TabWindow.Tabs.Length, container.childCount, "Each tab must build its view once.");
            }
            finally
            {
                Close(window);
            }
        }

        // The prompt is a pure IMGUI window: its OnGUI lays out a text field and two buttons by hand.
        // A logged error or a closed window fails the test. The first OnGUI also sets a private flag, which proves that
        // OnGUI ran at all: without it a window that never draws would pass.
        [UnityTest]
        public IEnumerator SerializeReferenceNamePrompt_Repaints_WithoutErrorAndStaysOpen()
        {
            var prompt = ScriptableObject.CreateInstance<SerializeReferenceNamePrompt>();
            try
            {
                prompt.ShowUtility();

                var drawnField = typeof(SerializeReferenceNamePrompt)
                    .GetField(PromptDrawnField, BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(drawnField, $"The prompt must keep its {PromptDrawnField} flag.");

                for (var frame = 0; frame < MaxFrames && !(bool)drawnField.GetValue(prompt); frame++)
                {
                    prompt.Repaint();
                    yield return null;
                }

                Assert.IsTrue((bool)drawnField.GetValue(prompt), "OnGUI must run and focus the name field.");

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
