using System;
using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidThemeSettingsUI
    {
        private const string TemplateFileName = "Aspid-FastTools-Theme-Override";

        private const string TemplateContent =
            "/*\n" +
            " * Aspid FastTools — editor theme override.\n" +
            " * Redefine any design token below to recolor the Aspid editor UI.\n" +
            " * This sheet is layered on top of the built-in palette, Default-Dark or, in the\n" +
            " * Aspid FastTools window and settings pages on the light skin, Default-Light, and\n" +
            " * on top of the theme and accent picked under Appearance, so you only need to\n" +
            " * declare the tokens you want to change.\n" +
            " * Built-in themes and accents: Packages/tech.aspid.fasttools/Editor/Resources/UI/Themes\n" +
            " * and .../UI/Accents\n" +
            " * Full token list: Packages/tech.aspid.fasttools/Editor/Resources/UI/Aspid-FastTools-Default-Dark.uss\n" +
            " * Fields drawn inside a regular Inspector start from Default-Dark on both skins and take part of their\n" +
            " * colors from Unity's editor theme, so these tokens change only part of their look.\n" +
            " */\n" +
            ":root {\n" +
            "    /* Backgrounds (surface palette, dark → light) */\n" +
            "    /* --aspid-colors-bg-darkness:  rgb(26, 26, 26); */\n" +
            "    /* --aspid-colors-bg-dark:      rgb(36, 36, 36); */\n" +
            "    /* --aspid-colors-bg-light:     rgb(46, 46, 46); */\n" +
            "    /* --aspid-colors-bg-lightness: rgb(56, 56, 56); */\n" +
            "\n" +
            "    /* Cards and panels */\n" +
            "    /* --aspid-colors-surface-card: rgb(11, 11, 11); */\n" +
            "\n" +
            "    /* Text (high-contrast content) */\n" +
            "    /* --aspid-colors-text-lightness: rgb(220, 220, 220); */\n" +
            "    /* --aspid-colors-text-light:     rgb(190, 190, 190); */\n" +
            "\n" +
            "    /* Accent: links, focus borders, the active tab, the Welcome title */\n" +
            "    /* --aspid-colors-accent-text-dark:  rgb(92, 180, 107); */\n" +
            "    /* --aspid-colors-accent-text-light: rgb(124, 194, 136); */\n" +
            "\n" +
            "    /* Status example — tweak the success status base */\n" +
            "    /* --aspid-colors-status-success-dark: rgb(12, 65, 30); */\n" +
            "\n" +
            "    /* Switches (unset by default, so they follow the editor skin) */\n" +
            "    /* --aspid-colors-switch-accent:        rgb(92, 180, 107); */\n" +
            "    /* --aspid-colors-switch-track-border:  rgb(82, 82, 87); */\n" +
            "    /* --aspid-colors-switch-handle:        rgba(189, 189, 196, 0.85); */\n" +
            "    /* --aspid-colors-switch-handle-shadow: rgba(0, 0, 0, 0.15); */\n" +
            "}\n";

        public static void BuildControls(VisualElement container)
        {
            var theme = new EnumField("Theme", AspidThemeSettings.Theme)
            {
                tooltip = "The colors of the Aspid FastTools windows, settings pages and type picker.\n"
                    + "Default: near-black cards. Venom: the blue-black palette of the documentation site.\n"
                    + "Each theme has a light variant for the light editor skin. A theme override is layered on top.\n"
                    + "Per-user setting — stored locally, for every project on this machine.",
            };
            theme.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            theme.RegisterValueChangedCallback(evt => AspidThemeSettings.Theme = (AspidTheme)evt.newValue);
            SyncFromSettings<EnumField, Enum>(theme, () => AspidThemeSettings.Theme);
            container.Add(theme);

            var accent = new EnumField("Accent", AspidThemeSettings.Accent)
            {
                tooltip = "The color of links, focus and hover borders, the active tab, buttons, switches, and the "
                    + "Welcome title and logo, on top of the theme. The accents of the documentation site; "
                    + "the success status stays green whatever the accent.\n"
                    + "Per-user setting — stored locally, for every project on this machine.",
            };
            accent.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            accent.RegisterValueChangedCallback(evt => AspidThemeSettings.Accent = (AspidAccent)evt.newValue);
            SyncFromSettings<EnumField, Enum>(accent, () => AspidThemeSettings.Accent);
            container.Add(accent);

            var overrideField = new ObjectField("Theme override")
            {
                objectType = typeof(StyleSheet),
                allowSceneObjects = false,
                value = AspidThemeSettings.OverrideStyleSheet,
                tooltip = "A USS sheet layered on top of the built-in palette (Default-Dark, or Default-Light in "
                    + "the Aspid FastTools window and settings pages on the light skin), the chosen theme and accent; "
                    + "redefine any --aspid-colors-* / --aspid-icons-* token inside a :root block. Applies live; "
                    + "clear to return to the theme.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            overrideField.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            overrideField.RegisterValueChangedCallback(evt =>
                AspidThemeSettings.OverrideStyleSheet = evt.newValue as StyleSheet);
            SyncFromSettings(overrideField, () => (UnityEngine.Object)AspidThemeSettings.OverrideStyleSheet);
            container.Add(overrideField);

            container.Add(BuildTemplateRow());
        }

        private static VisualElement BuildTemplateRow()
        {
            var row = new VisualElement().AddClass(AspidSettingsUI.RowClass).WithScopeStripe(AspidSettingsUI.UserScopeClass);
            row.tooltip = "Create a starter override .uss with the palette's tokens listed in comments, and assign it "
                + "as the theme override.";

            var caption = new Label("Override template").AddClass(AspidSettingsUI.RowCaptionClass);

            var create = new Button(CreateTemplate) { text = "Create template…" }
                .AddClass(AspidSettingsUI.ActionClass)
                .AddClass(AspidSettingsUI.ActionInfoClass);

            return row
                .AddChild(caption)
                .AddChild(AspidSettingsUI.BuildButtonGroup(create));
        }

        private static void CreateTemplate()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Aspid Theme Override",
                TemplateFileName,
                "uss",
                "Choose where to save the theme override style sheet.");

            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, TemplateContent);
            AssetDatabase.ImportAsset(path);

            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (sheet == null) return;

            AspidThemeSettings.OverrideStyleSheet = sheet;
            EditorGUIUtility.PingObject(sheet);
        }

        private static void SyncFromSettings<TControl, TValue>(TControl control, Func<TValue> read)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {
            AspidSettingsUI.SyncFromSettings(
                control,
                read,
                handler => AspidThemeSettings.Changed += handler,
                handler => AspidThemeSettings.Changed -= handler);
        }
    }
}
