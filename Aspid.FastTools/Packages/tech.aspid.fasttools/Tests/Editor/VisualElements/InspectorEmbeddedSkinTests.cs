using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Enums.Editors;
using Aspid.FastTools.Types.Editors;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// UI embedded in a regular Inspector must follow the editor skin: built-in icons resolve to the variant of the
    /// current skin and backgrounds come from Unity's theme instead of the dark Aspid palette.
    /// </summary>
    /// <remarks>
    /// The skin of a test run cannot be switched, so colours and icons are compared with probes that resolve Unity's
    /// theme variables in the same window, and the folder icons are checked through the light-skin class directly.
    /// </remarks>
    [TestFixture]
    internal sealed class InspectorEmbeddedSkinTests
    {
        private const string EnumValuesStyleSheet = "UI/Enums/Aspid-FastTools-EnumValues";
        private const string SerializeReferenceStyleSheet = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";
        private const string ProbeStyleSheetPath =
            "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements/InspectorEmbeddedSkinProbe.uss";

        private const string WarningTextProbeClass = "aspid-fasttools-test-probe--warning-text";
        private const string HelpBoxTextProbeClass = "aspid-fasttools-test-probe--helpbox-text";
        private const string TitlebarProbeClass = "aspid-fasttools-test-probe--titlebar";
        private const string TitlebarBorderProbeClass = "aspid-fasttools-test-probe--titlebar-border";
        private const string HelpBoxProbeClass = "aspid-fasttools-test-probe--helpbox";
        private const string WarnIconProbeClass = "aspid-fasttools-test-probe--warn-icon";
        private const string SwitchTokensClass = "aspid-fasttools-test-switch-tokens";

        private EditorWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();

            var probeStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ProbeStyleSheetPath);
            Assert.IsNotNull(probeStyleSheet, "The probe style sheet must load.");
            _window.rootVisualElement.AddStyleSheet(probeStyleSheet);
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);
        }

        [UnityTest]
        public IEnumerator InspectorNotice_Icon_MatchesEditorSkin()
        {
            var notice = new InspectorNotice();
            notice.Set("Message", actionText: null, detail: null, onAction: null);
            _window.rootVisualElement.Add(notice);
            yield return null;

            AssertSkinIcon(notice.Q(className: "aspid-fasttools-inspector-notice__icon"), "console.warnicon");
        }

        [UnityTest]
        public IEnumerator InspectorNotice_MatchesUnityTheme()
        {
            var notice = new InspectorNotice();
            notice.Set("Message", actionText: null, detail: null, onAction: null);
            var textProbe = AddProbe(WarningTextProbeClass);
            var iconProbe = AddProbe(WarnIconProbeClass);
            _window.rootVisualElement.Add(notice);
            yield return null;

            AssertColor(textProbe.resolvedStyle.color,
                notice.Q(className: "aspid-fasttools-inspector-notice__message").resolvedStyle.color, "notice message");

            var expected = iconProbe.resolvedStyle.backgroundImage.texture;
            Assert.IsNotNull(expected, "The Unity warning icon must resolve.");
            Assert.AreEqual(expected,
                notice.Q(className: "aspid-fasttools-inspector-notice__icon").resolvedStyle.backgroundImage.texture);
        }

        [UnityTest]
        public IEnumerator SerializeReference_MissingType_UsesUnityWarningColor()
        {
            var caption = new TextElement().AddClass("unity-enum-field__text");
            var stripe = new VisualElement()
                .AddClass("aspid-fasttools-serialize-reference__stripe")
                .AddClass("aspid-fasttools-serialize-reference__stripe--warning");
            var probe = AddProbe(WarningTextProbeClass);

            _window.rootVisualElement
                .AddAspidThemeStyleSheets()
                .AddChild(new VisualElement()
                    .AddStyleSheetFromResources(SerializeReferenceStyleSheet)
                    .AddChild(new VisualElement()
                        .AddClass("aspid-fasttools-serialize-reference__dropdown--missing")
                        .AddChild(caption))
                    .AddChild(stripe));
            yield return null;

            AssertColor(probe.resolvedStyle.color, caption.resolvedStyle.color, "missing-type caption");
            AssertColor(probe.resolvedStyle.backgroundColor, stripe.resolvedStyle.backgroundColor, "warning stripe");
        }

        [UnityTest]
        public IEnumerator InspectorNoticeGUI_MatchesUnityTheme()
        {
            var warningProbe = AddProbe(WarningTextProbeClass);
            var infoProbe = AddProbe(HelpBoxTextProbeClass);
            yield return null;

            AssertColor(warningProbe.resolvedStyle.color, InspectorNoticeGUI.NoticeColor, "IMGUI notice");
            AssertColor(infoProbe.resolvedStyle.color, InspectorNoticeGUI.InfoNoticeColor, "IMGUI info notice");
        }

        [UnityTest]
        public IEnumerator EnumValuesIMGUI_MatchesUnityTheme()
        {
            var titlebarProbe = AddProbe(TitlebarProbeClass);
            var borderProbe = AddProbe(TitlebarBorderProbeClass);
            var helpBoxProbe = AddProbe(HelpBoxProbeClass);
            yield return null;

            AssertColor(titlebarProbe.resolvedStyle.backgroundColor, EnumValuesIMGUIPropertyDrawer.HeaderColor,
                "IMGUI header");
            AssertColor(borderProbe.resolvedStyle.backgroundColor, EnumValuesIMGUIPropertyDrawer.BorderColor,
                "IMGUI border");
            AssertColor(helpBoxProbe.resolvedStyle.backgroundColor, EnumValuesIMGUIPropertyDrawer.ContainerColor,
                "IMGUI container");
        }

        [TestCase(TypeIMGUIPropertyDrawer.FolderClosedIconPath)]
        [TestCase(TypeIMGUIPropertyDrawer.FolderOpenedIconPath)]
        public void TypeIMGUI_FolderIcon_MatchesEditorSkin(string iconPath)
        {
            Assert.IsFalse(iconPath.StartsWith("d_"), "IconContent picks the skin variant only for the plain name.");

            var texture = EditorGUIUtility.IconContent(iconPath).image;
            Assert.IsNotNull(texture, $"The {iconPath} icon must resolve.");
            Assert.AreEqual(EditorGUIUtility.isProSkin, texture.name.StartsWith("d_"),
                $"'{texture.name}' must be the {(EditorGUIUtility.isProSkin ? "dark" : "light")} skin variant.");
        }

        [Test]
        public void ThemeStyleSheets_MarkLightSkin()
        {
            var host = new VisualElement().AddAspidThemeStyleSheets();

            Assert.AreEqual(!EditorGUIUtility.isProSkin, host.ClassListContains(AspidStyles.SkinLightClass));
        }

        [UnityTest]
        public IEnumerator ThemeStyleSheets_Restyle_RestoresSkinClass()
        {
            var host = new VisualElement().AddAspidThemeStyleSheets();
            _window.rootVisualElement.Add(host);
            yield return null;

            host.EnableInClassList(AspidStyles.SkinLightClass, EditorGUIUtility.isProSkin);
            yield return null;

            Assert.AreEqual(!EditorGUIUtility.isProSkin, host.ClassListContains(AspidStyles.SkinLightClass),
                "A restyle must bring the skin class back in line with the editor skin.");
        }

        [UnityTest]
        public IEnumerator TypeField_FolderIcon_DarkSkin() => AssertFolderIcon(lightSkin: false, "d_Folder Icon");

        [UnityTest]
        public IEnumerator TypeField_FolderIcon_LightSkin() => AssertFolderIcon(lightSkin: true, "Folder Icon");

        [UnityTest]
        public IEnumerator SerializeReference_FolderIcon_DarkSkin() =>
            AssertSerializeReferenceFolderIcon(lightSkin: false, "d_Folder Icon");

        [UnityTest]
        public IEnumerator SerializeReference_FolderIcon_LightSkin() =>
            AssertSerializeReferenceFolderIcon(lightSkin: true, "Folder Icon");

        [UnityTest]
        public IEnumerator SerializeReference_NestedButton_LightSkin_HasNoFolderIcon()
        {
            // A drawer's own button below the field must not take the open button's light-skin icon.
            var icon = new VisualElement();
            _window.rootVisualElement.Add(new VisualElement()
                .AddStyleSheetFromResources(SerializeReferenceStyleSheet)
                .AddClass("aspid-fasttools-serialize-reference")
                .AddClass(AspidStyles.SkinLightClass)
                .AddChild(new Button().AddChild(icon)));
            yield return null;

            Assert.IsNull(icon.resolvedStyle.backgroundImage.texture, "A nested button must keep its own look.");
        }

        [UnityTest]
        public IEnumerator TypeField_NestedButton_LightSkin_HasNoFolderIcon()
        {
            var icon = new VisualElement();
            _window.rootVisualElement.Add(new VisualElement()
                .AddClass(AspidStyles.SkinLightClass)
                .AddChild(new TypeField("Type")
                    .AddChild(new Button().AddChild(icon))));
            yield return null;

            Assert.IsNull(icon.resolvedStyle.backgroundImage.texture, "A button inside a TypeField must keep its own look.");
        }

        [UnityTest]
        public IEnumerator EnumValues_Background_FollowsEditorSkin()
        {
            var header = new VisualElement().AddClass("aspid-fasttools-enum-values__header");
            var container = new VisualElement().AddClass("aspid-fasttools-enum-values__container");
            var titlebarProbe = AddProbe(TitlebarProbeClass);
            var helpBoxProbe = AddProbe(HelpBoxProbeClass);

            _window.rootVisualElement
                .AddAspidThemeStyleSheets()
                .AddChild(new VisualElement()
                    .AddStyleSheetFromResources(EnumValuesStyleSheet)
                    .AddChild(header)
                    .AddChild(container));
            yield return null;

            AssertSkinBackground(header.resolvedStyle.backgroundColor, "header");
            AssertSkinBackground(container.resolvedStyle.backgroundColor, "container");
            AssertColor(titlebarProbe.resolvedStyle.backgroundColor, header.resolvedStyle.backgroundColor, "header");
            AssertColor(helpBoxProbe.resolvedStyle.backgroundColor, container.resolvedStyle.backgroundColor, "container");
        }

        [UnityTest]
        public IEnumerator Switch_UnsetPaletteTokens_KeepSkinDefaults()
        {
            var toggle = new AspidSwitch("Switch");
            _window.rootVisualElement.AddAspidThemeStyleSheets().Add(toggle);
            yield return null;

            var handle = toggle.Q(className: BaseField<bool>.inputUssClassName)[0][0];
            var expected = EditorGUIUtility.isProSkin
                ? new Color(0.74f, 0.74f, 0.77f, 0.85f)
                : new Color(0.35f, 0.35f, 0.38f, 0.9f);

            var actual = handle.resolvedStyle.backgroundColor;
            Assert.AreEqual(expected.r, actual.r, 0.01f, "An unset switch token must keep the skin default handle.");
            Assert.AreEqual(expected.g, actual.g, 0.01f, "An unset switch token must keep the skin default handle.");
            Assert.AreEqual(expected.b, actual.b, 0.01f, "An unset switch token must keep the skin default handle.");
            Assert.AreEqual(expected.a, actual.a, 0.01f, "An unset switch token must keep the skin default handle.");
        }

        [UnityTest]
        public IEnumerator Switch_UnsetPaletteTokens_RestyleRestoresSkinDefaults()
        {
            // A skin switch only restyles the panel, so the defaults must come back from a style resolve.
            var toggle = new AspidSwitch("Switch");
            _window.rootVisualElement.AddAspidThemeStyleSheets().Add(toggle);
            yield return null;

            var handle = toggle.Q(className: BaseField<bool>.inputUssClassName)[0][0];
            handle.style.backgroundColor = Color.magenta;
            toggle.AddToClassList("aspid-fasttools-test-restyle");
            yield return null;

            var expected = EditorGUIUtility.isProSkin
                ? new Color(0.74f, 0.74f, 0.77f, 0.85f)
                : new Color(0.35f, 0.35f, 0.38f, 0.9f);

            AssertColor(expected, handle.resolvedStyle.backgroundColor, "switch handle");
        }

        [UnityTest]
        public IEnumerator Switch_PaletteTokens_RecolorSwitch()
        {
            var toggle = new AspidSwitch("Switch");
            _window.rootVisualElement
                .AddAspidThemeStyleSheets()
                .AddChild(new VisualElement()
                    .AddClass(SwitchTokensClass)
                    .AddChild(toggle));
            yield return null;

            var track = toggle.Q(className: BaseField<bool>.inputUssClassName)[0];
            AssertColor(Color.red, track[0].resolvedStyle.backgroundColor, "switch handle");
            AssertColor(Color.blue, track.resolvedStyle.borderTopColor, "switch track border");
        }

        private IEnumerator AssertFolderIcon(bool lightSkin, string expected)
        {
            // The field keeps its own skin class in line with the editor, so the forced skin sits on its parent.
            var field = new TypeField("Type");
            _window.rootVisualElement.Add(new VisualElement()
                .EnableClass(AspidStyles.SkinLightClass, lightSkin)
                .AddChild(field));
            yield return null;

            var texture = field.Q<Button>(className: "aspid-fasttools-type-field__open-button")[0].resolvedStyle.backgroundImage.texture;
            Assert.IsNotNull(texture, "The folder icon must resolve.");
            Assert.AreEqual(expected, texture.name);
        }

        private IEnumerator AssertSerializeReferenceFolderIcon(bool lightSkin, string expected)
        {
            // The rule needs the skin class on the field root itself, next to the block class.
            var icon = new VisualElement();
            _window.rootVisualElement.Add(new VisualElement()
                .AddStyleSheetFromResources(SerializeReferenceStyleSheet)
                .AddClass("aspid-fasttools-serialize-reference")
                .EnableClass(AspidStyles.SkinLightClass, lightSkin)
                .AddChild(new Button().AddClass("aspid-fasttools-serialize-reference__open-button").AddChild(icon)));
            yield return null;

            var texture = icon.resolvedStyle.backgroundImage.texture;
            Assert.IsNotNull(texture, "The folder icon must resolve.");
            Assert.AreEqual(expected, texture.name);
        }

        private VisualElement AddProbe(string className)
        {
            var probe = new VisualElement().AddClass(className);
            _window.rootVisualElement.Add(probe);
            return probe;
        }

        private static void AssertColor(Color expected, Color actual, string part)
        {
            Assert.Greater(expected.a, 0f, $"The expected {part} colour must resolve.");

            var message = $"The {part} colour {actual} must be {expected}.";
            Assert.AreEqual(expected.r, actual.r, 0.01f, message);
            Assert.AreEqual(expected.g, actual.g, 0.01f, message);
            Assert.AreEqual(expected.b, actual.b, 0.01f, message);
            Assert.AreEqual(expected.a, actual.a, 0.01f, message);
        }

        private static void AssertSkinIcon(VisualElement icon, string iconName)
        {
            Assert.IsNotNull(icon);

            var texture = icon.resolvedStyle.backgroundImage.texture;
            Assert.IsNotNull(texture, $"The {iconName} icon must resolve.");
            StringAssert.Contains(iconName, texture.name);
            Assert.AreEqual(EditorGUIUtility.isProSkin, texture.name.StartsWith("d_"),
                $"'{texture.name}' must be the {(EditorGUIUtility.isProSkin ? "dark" : "light")} skin variant.");
        }

        private static void AssertSkinBackground(Color color, string part)
        {
            Assert.Greater(color.a, 0f, $"The {part} background must resolve.");

            var isDark = color.grayscale < 0.5f;
            Assert.AreEqual(EditorGUIUtility.isProSkin, isDark,
                $"The {part} background {color} must match the {(EditorGUIUtility.isProSkin ? "dark" : "light")} skin.");
        }
    }
}
