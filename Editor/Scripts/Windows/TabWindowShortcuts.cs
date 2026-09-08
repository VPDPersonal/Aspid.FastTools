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

        private const string HomeId = Category + "Home";
        private const string AssetReferencesId = Category + "Asset References";
        private const string ProjectReferencesId = Category + "Project References";
        private const string SettingsId = Category + "Settings";

        private const KeyCode HomeKey = KeyCode.Alpha1;
        private const KeyCode SettingsKey = KeyCode.Alpha0;
        private const KeyCode AssetReferencesKey = KeyCode.Alpha2;
        private const KeyCode ProjectReferencesKey = KeyCode.Alpha3;
        private const ShortcutModifiers TabModifiers = ShortcutModifiers.Alt;

        private static readonly TabData[] _tabData =
        {
            new(HomeId, TabType.Welcome, HomeKey),
            new(AssetReferencesId, TabType.AssetReference, AssetReferencesKey),
            new(ProjectReferencesId, TabType.ProjectReferences, ProjectReferencesKey),
            new(SettingsId, TabType.Settings, SettingsKey),
        };

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

        internal static string HintFor(TabType tab)
        {
            foreach (var tabData in _tabData)
            {
                if (tabData.Tab != tab) continue;
                return LiveBinding(tabData.Id) ?? DefaultHint(tabData.Key);
            }

            return string.Empty;
        }

        private static void SwitchFrom(ShortcutArguments args, TabType tab)
        {
            if (args.context is TabWindow window)
                window.SwitchMode(tab);
        }

        private static void CycleFrom(ShortcutArguments args, int step)
        {
            if (args.context is not TabWindow window) return;

            var currentTabIndex = IndexOf(window.CurrentTabType);
            var nextTabIndex = (currentTabIndex + step + _tabData.Length) % _tabData.Length;

            window.SwitchMode(_tabData[nextTabIndex].Tab);
        }

        private static int IndexOf(TabType tab)
        {
            for (var i = 0; i < _tabData.Length; i++)
            {
                if (_tabData[i].Tab == tab) return i;
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

        private readonly struct TabData
        {
            internal readonly string Id;
            internal readonly TabType Tab;
            internal readonly KeyCode Key;

            internal TabData(string id, TabType tab, KeyCode key)
            {
                Id = id;
                Tab = tab;
                Key = key;
            }
        }
    }
}
