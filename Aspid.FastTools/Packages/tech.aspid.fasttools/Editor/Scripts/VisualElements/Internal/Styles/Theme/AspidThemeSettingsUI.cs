using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidThemeSettingsUI
    {
        private const string NewThemeFileName = "My Theme";

        private const string DefaultThemeSource =
            "/*\n" +
            " * Aspid FastTools — editor theme, made from Default.\n" +
            " * Redefine any design token below to recolor the Aspid editor UI. The sheet is layered on top of the\n" +
            " * built-in palette, Default-Dark or, in the Aspid FastTools window and settings pages on the light skin,\n" +
            " * Default-Light, so you only need to declare the tokens you want to change.\n" +
            " * Built-in themes: Packages/tech.aspid.fasttools/Editor/Resources/UI/Themes\n" +
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
            "}\n" +
            "\n" +
            "/* The light editor skin: inside the Aspid FastTools window and settings pages these tokens win over :root. */\n" +
            ".aspid-fasttools-palette--light {\n" +
            "    /* --aspid-colors-accent-text-dark:  rgb(29, 132, 62); */\n" +
            "    /* --aspid-colors-accent-text-light: rgb(23, 112, 53); */\n" +
            "}\n";

        public static void BuildControls(VisualElement container)
        {
            var theme = new AspidThemeField("Theme", CreateTheme)
            {
                tooltip = "The colors of the Aspid FastTools windows, settings pages and type picker.\n"
                    + "Default: near-black cards with the green accent. Venom: the blue-black palette of the "
                    + "documentation site with its blue accent. Unity: the greys of Unity's editor skin with its blue "
                    + "accent. The success status stays green in all of them, and each has a light variant for the "
                    + "light editor skin.\n"
                    + "Below them come the themes of this project: the .uss sheets in Assets with the "
                    + $"{AspidThemeSettings.CustomThemeLabel} label. Create theme… copies the current theme into a new "
                    + "sheet, labels it and picks it.\n"
                    + "Per-user setting — stored locally; a built-in theme for every project on this machine, "
                    + "a theme of the project for this project only.",
            };
            theme.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            container.Add(theme);
        }

        private static void CreateTheme()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Aspid Theme",
                NewThemeFileName,
                "uss",
                "Choose where to save the theme. It starts as a copy of the current theme.");

            if (!string.IsNullOrEmpty(path)) CreateTheme(path);
        }

        // Writes a copy of the current theme to the project path, labels it as a theme of the project and picks it.
        internal static StyleSheet CreateTheme(string path)
        {
            File.WriteAllText(path, GetCurrentThemeSource());
            AssetDatabase.ImportAsset(path);

            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (sheet == null) return null;

            AssetDatabase.SetLabels(sheet, new[] { AspidThemeSettings.CustomThemeLabel });
            AspidThemeSettings.CustomTheme = sheet;
            EditorGUIUtility.PingObject(sheet);

            return sheet;
        }

        // A custom theme is copied as it is; a built-in one gets a header that says where it came from.
        internal static string GetCurrentThemeSource()
        {
            var custom = AspidThemeSettings.CustomTheme;
            if (custom != null) return ReadSource(custom);

            var theme = AspidThemeSettings.Theme;
            var builtIn = AspidThemeSettings.GetBuiltInStyleSheet(theme);
            if (builtIn == null) return DefaultThemeSource;

            return "/* Aspid FastTools — editor theme, made from " + theme + ". Change any token below; the full list is in\n"
                + "   Packages/tech.aspid.fasttools/Editor/Resources/UI/Aspid-FastTools-Default-Dark.uss. */\n\n"
                + ReadSource(builtIn);
        }

        private static string ReadSource(StyleSheet sheet) =>
            File.ReadAllText(FileUtil.GetPhysicalPath(AssetDatabase.GetAssetPath(sheet)));
    }
}
