using System;
using UnityEngine;
using UnityEditor.ShortcutManagement;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class TabWindowShortcuts
    {
        private const string Category = "Aspid FastTools/Window/";

        private const string NextTabId = Category + "Next Tab";
        private const string PreviousTabId = Category + "Previous Tab";
        private const string NextTabArrowId = Category + "Next Tab (Arrow)";
        private const string PreviousTabArrowId = Category + "Previous Tab (Arrow)";

        internal const string HomeId = Category + "Home";
        internal const string AssetReferencesId = Category + "Asset References";
        internal const string ProjectReferencesId = Category + "Project References";
        internal const string SettingsId = Category + "Settings";

        internal const KeyCode HomeKey = KeyCode.Alpha1;
        internal const KeyCode SettingsKey = KeyCode.Alpha0;
        internal const KeyCode AssetReferencesKey = KeyCode.Alpha2;
        internal const KeyCode ProjectReferencesKey = KeyCode.Alpha3;
        private const ShortcutModifiers TabModifiers = ShortcutModifiers.Alt;

        [Shortcut(HomeId, typeof(TabWindow), HomeKey, TabModifiers)]
        private static void OnHomeShortcut(ShortcutArguments args) =>
            SwitchFrom(args, TabType.Welcome);

        [Shortcut(AssetReferencesId, typeof(TabWindow), AssetReferencesKey, TabModifiers)]
        private static void OnInspectShortcut(ShortcutArguments args) =>
            SwitchFrom(args, TabType.AssetReference);

        [Shortcut(ProjectReferencesId, typeof(TabWindow), ProjectReferencesKey, TabModifiers)]
        private static void OnProjectShortcut(ShortcutArguments args) =>
            SwitchFrom(args, TabType.ProjectReferences);

        [Shortcut(SettingsId, typeof(TabWindow), SettingsKey, TabModifiers)]
        private static void OnSettingsShortcut(ShortcutArguments args) =>
            SwitchFrom(args, TabType.Settings);

        [Shortcut(NextTabId, typeof(TabWindow), KeyCode.Tab, ShortcutModifiers.Control)]
        private static void OnNextTabShortcut(ShortcutArguments args) =>
            CycleFrom(args, +1);

        [Shortcut(PreviousTabId, typeof(TabWindow), KeyCode.Tab, ShortcutModifiers.Control | ShortcutModifiers.Shift)]
        private static void OnPreviousTabShortcut(ShortcutArguments args) =>
            CycleFrom(args, -1);

        // A shortcut has one default binding, so the arrow pair is a second id next to Ctrl+Tab.
        [Shortcut(NextTabArrowId, typeof(TabWindow), KeyCode.RightArrow, TabModifiers)]
        private static void OnNextTabArrowShortcut(ShortcutArguments args) =>
            CycleFrom(args, +1);

        [Shortcut(PreviousTabArrowId, typeof(TabWindow), KeyCode.LeftArrow, TabModifiers)]
        private static void OnPreviousTabArrowShortcut(ShortcutArguments args) =>
            CycleFrom(args, -1);

        internal static string HintFor(TabType tab)
        {
            var definition = TabWindow.DefinitionOf(tab);
            return LiveBinding(definition.ShortcutId) ?? DefaultHint(definition.Key);
        }

        private static void SwitchFrom(ShortcutArguments args, TabType tab)
        {
            if (args.context is TabWindow window)
                window.SwitchMode(tab);
        }

        private static void CycleFrom(ShortcutArguments args, int step)
        {
            if (args.context is not TabWindow window) return;

            var tabs = TabWindow.Tabs;
            var currentTabIndex = IndexOf(window.CurrentTabType);
            var nextTabIndex = (currentTabIndex + step + tabs.Length) % tabs.Length;

            window.SwitchMode(tabs[nextTabIndex].Type);
        }

        private static int IndexOf(TabType tab)
        {
            var tabs = TabWindow.Tabs;
            for (var i = 0; i < tabs.Length; i++)
            {
                if (tabs[i].Type == tab) return i;
            }

            return 0;
        }

        private static string LiveBinding(string shortcutId)
        {
            try
            {
                var binding = ShortcutManager.instance.GetShortcutBinding(shortcutId).ToString();
                return string.IsNullOrEmpty(binding) ? null : binding;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string DefaultHint(KeyCode key)
        {
            var label = key is >= KeyCode.Alpha0 and <= KeyCode.Alpha9
                ? (key - KeyCode.Alpha0).ToString()
                : key.ToString();

            return ModifierPrefix(TabModifiers) + label;
        }

        private static string ModifierPrefix(ShortcutModifiers modifiers)
        {
            var isMac = Application.platform == RuntimePlatform.OSXEditor;
            var prefix = string.Empty;

            if ((modifiers & ShortcutModifiers.Control) != 0) prefix += isMac ? "⌃" : "Ctrl+";
            if ((modifiers & ShortcutModifiers.Action) != 0) prefix += isMac ? "⌘" : "Ctrl+";
            if ((modifiers & ShortcutModifiers.Alt) != 0) prefix += isMac ? "⌥" : "Alt+";
            if ((modifiers & ShortcutModifiers.Shift) != 0) prefix += isMac ? "⇧" : "Shift+";

            return prefix;
        }
    }
}
