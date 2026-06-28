using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    /// <summary>
    /// Exposes the Aspid editor theme settings under <c>Preferences → Aspid FastTools</c>.
    /// Lets the user pick a USS override style sheet that is layered on top of the built-in
    /// <see cref="AspidStyles.DefaultStyleSheet"/> palette.
    /// </summary>
    internal static class AspidThemeSettingsProvider
    {
        private const string SettingsPath = "Preferences/Aspid FastTools";

        private const string TemplateFileName = "Aspid-FastTools-Theme-Override";

        private const string TemplateContent =
            "/*\n" +
            " * Aspid FastTools — editor theme override.\n" +
            " * Redefine any design token below to recolour the Aspid editor UI.\n" +
            " * This sheet is layered on top of the built-in Default-Dark palette,\n" +
            " * so you only need to declare the tokens you want to change.\n" +
            " * Full token list: Packages/tech.aspid.fasttools/Unity/Editor/Resources/UI/Aspid-FastTools-Default-Dark.uss\n" +
            " */\n" +
            ":root {\n" +
            "    /* Backgrounds (surface palette, dark → light) */\n" +
            "    /* --aspid-colors-bg-darkness:  rgb(26, 26, 26); */\n" +
            "    /* --aspid-colors-bg-dark:      rgb(36, 36, 36); */\n" +
            "    /* --aspid-colors-bg-light:     rgb(46, 46, 46); */\n" +
            "    /* --aspid-colors-bg-lightness: rgb(56, 56, 56); */\n" +
            "\n" +
            "    /* Text (high-contrast content) */\n" +
            "    /* --aspid-colors-text-lightness: rgb(220, 220, 220); */\n" +
            "    /* --aspid-colors-text-light:     rgb(190, 190, 190); */\n" +
            "\n" +
            "    /* Accent example — tweak the success status base */\n" +
            "    /* --aspid-colors-status-success-dark: rgb(12, 65, 30); */\n" +
            "}\n";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.User)
            {
                label = "Aspid FastTools",
                activateHandler = static (_, root) => BuildUI(root),
                keywords = new[] { "Aspid", "FastTools", "Theme", "Style", "USS", "Color", "Palette", "Override" },
            };
        }

        private static void BuildUI(VisualElement root)
        {
            // Apply the theme to the settings UI itself so the help box reflects the active palette.
            root.AddAspidThemeStyleSheets();

            var container = new VisualElement();
            container.style.marginTop = 6;
            container.style.marginLeft = 8;
            container.style.marginRight = 8;

            var title = new Label("Editor Theme");
            title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
            title.style.marginBottom = 6;
            container.Add(title);

            var help = new HelpBox(
                "Select a USS style sheet to override the default Aspid editor palette. " +
                "Redefine any --aspid-colors-* / --aspid-icons-* token inside a :root { } block; " +
                "your sheet is layered on top of the built-in Default-Dark palette and applies live.",
                HelpBoxMessageType.Info);
            help.style.marginBottom = 8;
            container.Add(help);

            var field = new ObjectField("Override Style Sheet")
            {
                objectType = typeof(StyleSheet),
                allowSceneObjects = false,
                value = AspidThemeSettings.OverrideStyleSheet,
            };
            field.RegisterValueChangedCallback(evt =>
                AspidThemeSettings.OverrideStyleSheet = evt.newValue as StyleSheet);
            container.Add(field);

            var buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 8;

            var createButton = new Button(() => CreateTemplate(field)) { text = "Create Template" };
            var resetButton = new Button(() => field.value = null) { text = "Reset to Default" };
            resetButton.style.marginLeft = 4;

            buttons.Add(createButton);
            buttons.Add(resetButton);
            container.Add(buttons);

            root.Add(container);
        }

        private static void CreateTemplate(ObjectField field)
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

            field.value = sheet;
            EditorGUIUtility.PingObject(sheet);
        }
    }
}
