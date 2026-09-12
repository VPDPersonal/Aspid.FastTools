using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspid.FastTools.DevTools
{
    [InitializeOnLoad]
    internal static class AyuLightTheme
    {
        private const string MenuPath = "Tools/Aspid/Ayu Light/Enabled";
        private const string StylePath = "Assets/DevTools/AyuLight/Editor/AyuLight.uss";
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic |
                                           BindingFlags.Instance | BindingFlags.Static;
        private static readonly string Preference = "Aspid.AyuLight." + Hash128.Compute(Application.dataPath);
        private static readonly List<Action> Restorations = new List<Action>();
        private static readonly HashSet<StyleSheet> Sheets = new HashSet<StyleSheet>();
        private static readonly HashSet<VisualElement> Roots = new HashSet<VisualElement>();
        private static readonly HashSet<GUIStyle> Styles = new HashSet<GUIStyle>();
        private static readonly FieldInfo SheetColors = typeof(StyleSheet).GetField("colors", Flags);
        private static readonly FieldInfo SheetHash = typeof(StyleSheet).GetField("m_ContentHash", Flags);
        private static object _catalog;
        private static StyleSheet _overrides;
        private static double _nextUpdate;
        private static bool _applied;

        static AyuLightTheme()
        {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Restore;
            EditorApplication.quitting += Restore;
        }

        [MenuItem(MenuPath)]
        private static void Toggle() => SetEnabled(!EditorPrefs.GetBool(Preference));

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(MenuPath, EditorPrefs.GetBool(Preference));
            return true;
        }

        internal static void SetEnabled(bool enabled)
        {
            if (enabled == EditorPrefs.GetBool(Preference))
                return;

            if (enabled)
                EditorPrefs.SetBool(Preference + ".PreviousDark", EditorGUIUtility.isProSkin);

            EditorPrefs.SetBool(Preference, enabled);
            if (!enabled)
            {
                Restore();
                if (EditorGUIUtility.isProSkin != EditorPrefs.GetBool(Preference + ".PreviousDark"))
                    InternalEditorUtility.SwitchSkinAndRepaintAllViews();
            }
            _nextUpdate = 0;
        }

        private static void Update()
        {
            if (!EditorPrefs.GetBool(Preference) || EditorApplication.isCompiling ||
                EditorApplication.timeSinceStartup < _nextUpdate)
                return;

            _nextUpdate = EditorApplication.timeSinceStartup + 1;
            try
            {
                if (EditorGUIUtility.isProSkin)
                {
                    if (_applied)
                    {
                        // Respect a manual switch to Dark in Unity Preferences.
                        EditorPrefs.SetBool(Preference, false);
                        Restore();
                    }
                    else
                        InternalEditorUtility.SwitchSkinAndRepaintAllViews();
                    return;
                }

                _overrides = _overrides != null ? _overrides : AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
                if (_overrides == null)
                    return;

                bool changed = ApplyCatalog();
                foreach (var sheet in Resources.FindObjectsOfTypeAll<StyleSheet>())
                {
                    string name = sheet.name.ToLowerInvariant();
                    if (sheet == _overrides || Sheets.Contains(sheet) ||
                        AssetDatabase.GetAssetPath(sheet) != "Library/unity editor resources" ||
                        name.Contains("dark") || !(name.Contains("light") || name.Contains("common")))
                        continue;

                    if (SheetColors == null || SheetHash == null)
                        throw new NotSupportedException("Unity stylesheet internals have changed.");

                    var original = (Color[])SheetColors.GetValue(sheet);
                    if (original == null)
                        continue;

                    var mapped = Array.ConvertAll(original, MapColor);
                    int hash = (int)SheetHash.GetValue(sheet);
                    Restorations.Add(() =>
                    {
                        if (sheet == null) return;
                        SheetColors.SetValue(sheet, original);
                        SheetHash.SetValue(sheet, hash);
                    });
                    SheetColors.SetValue(sheet, mapped);
                    SheetHash.SetValue(sheet, unchecked(hash + 7919));
                    Sheets.Add(sheet);
                    changed = true;
                }

                foreach (EditorSkin kind in Enum.GetValues(typeof(EditorSkin)))
                {
                    if (kind == EditorSkin.Game) continue;
                    var skin = EditorGUIUtility.GetBuiltinSkin(kind);
                    foreach (GUIStyle style in skin)
                        ApplyText(style);
                }

                foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
                {
                    var root = window.rootVisualElement;
                    if (root.panel != null)
                        root = root.panel.visualTree;
                    if (Roots.Add(root))
                    {
                        root.AddToClassList("ayu-light");
                        root.styleSheets.Add(_overrides);
                        changed = true;
                    }
                }

                if (changed)
                    RefreshPanels();
                _applied = true;
            }
            catch (Exception exception)
            {
                SetEnabled(false);
                Debug.LogWarning("Ayu Light was disabled and restored: " + exception.Message);
            }
        }

        private static bool ApplyCatalog()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.Experimental.EditorResources");
            var catalog = type?.GetField("s_StyleCatalog", Flags)?.GetValue(null);
            if (catalog == null || ReferenceEquals(catalog, _catalog))
                return false;

            var buffers = catalog.GetType().GetProperty("buffers", Flags)?.GetValue(catalog);
            var field = buffers?.GetType().GetField("colors", Flags);
            if (field == null)
                throw new NotSupportedException("Unity IMGUI style catalog has changed.");

            var colors = (Color[])field.GetValue(buffers);
            var original = (Color[])colors.Clone();
            Restorations.Add(() => Array.Copy(original, colors, original.Length));
            for (int i = 0; i < colors.Length; i++)
                colors[i] = MapColor(colors[i]);
            ApplySelectionText(catalog, colors);
            _catalog = catalog;
            return true;
        }

        private static void ApplySelectionText(object catalog, Color[] colors)
        {
            var assembly = typeof(Editor).Assembly;
            int colorKey = (int)assembly.GetType("UnityEditor.StyleSheets.StyleCatalogKeyword")
                .GetField("color", Flags).GetValue(null);
            int foreground = Array.FindIndex(colors, color => color == Hex(0x5c6166));
            if (foreground < 0) return;
            var blocks = (Array)catalog.GetType().GetField("m_Blocks", Flags).GetValue(catalog);
            foreach (var block in blocks)
            {
                var values = (Array)block.GetType().GetField("values", Flags).GetValue(block);
                Array original = null;
                for (int i = 0; i < values.Length; i++)
                {
                    var value = values.GetValue(i);
                    var type = value.GetType();
                    if ((int)type.GetField("key", Flags).GetValue(value) != colorKey ||
                        type.GetField("type", Flags).GetValue(value).ToString() != "Color")
                        continue;
                    string state = type.GetField("state", Flags).GetValue(value).ToString();
                    var index = type.GetField("index", Flags);
                    Color color = colors[(int)index.GetValue(value)];
                    if (color.r < .98f || color.g < .98f || color.b < .98f ||
                        !(state.Contains("checked") || state.Contains("hover")))
                        continue;
                    if (original == null) original = (Array)values.Clone();
                    index.SetValue(value, foreground);
                    values.SetValue(value, i);
                }
                if (original != null)
                    Restorations.Add(() => Array.Copy(original, values, original.Length));
            }
        }

        private static void ApplyText(GUIStyle style)
        {
            if (style == null || !Styles.Add(style)) return;
            var states = new[] { style.normal, style.hover, style.active, style.focused,
                style.onNormal, style.onHover, style.onActive, style.onFocused };
            foreach (var state in states)
            {
                Color original = state.textColor;
                Restorations.Add(() => state.textColor = original);
                // Pale Ayu selections use dark text; dedicated white overlay labels keep their colour.
                bool white = original.r > .98f && original.g > .98f && original.b > .98f;
                bool selection = style.name.IndexOf("selected", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    style.name.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    style.name.IndexOf("label", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    !style.name.StartsWith("White", StringComparison.OrdinalIgnoreCase);
                state.textColor = white ? (selection ? Hex(0x5c6166) : original) : MapColor(original);
            }
        }

        private static Color MapColor(Color source)
        {
            if (source.a == 0) return source;
            Color result = source;
            float min = Mathf.Min(source.r, Mathf.Min(source.g, source.b));
            float max = Mathf.Max(source.r, Mathf.Max(source.g, source.b));
            if (max - min < .025f)
            {
                int value = Mathf.RoundToInt(source.r * 255);
                result = value < 24 ? Hex(0x3d424d) :
                    value < 100 ? Hex(0x5c6166) :
                    value < 160 ? Hex(0x828e9f) :
                    value < 185 ? Hex(0xd7dde3) :
                    value < 205 ? Hex(0xf8f9fa) :
                    value < 225 ? Hex(0xfcfcfc) : Hex(0xffffff);
            }
            else
            {
                // Only Unity's selection/focus blues; semantic and asset colours stay intact.
                int rgb = (Mathf.RoundToInt(source.r * 255) << 16) |
                          (Mathf.RoundToInt(source.g * 255) << 8) | Mathf.RoundToInt(source.b * 255);
                switch (rgb)
                {
                    case 0x3a72b0: case 0x3d80df: case 0x3e75c2: case 0x4e8dd3:
                        result = Hex(0xd6e4f8); break;
                    case 0x0c6ccb: case 0x018cff: case 0x4c7eff: case 0x0032e6:
                    case 0x3356da: case 0x3351e2: result = Hex(0xf29718); break;
                    case 0x003c88: case 0x1d5087: case 0x356aa3: result = Hex(0xb86b08); break;
                    case 0x96c3fb: case 0xb0d2fc: case 0xb0d3ff: case 0x91bae1:
                        result = Hex(0xe5eaf0); break;
                }
            }
            result.a = source.a;
            return result;
        }

        private static Color Hex(int rgb) => new Color(((rgb >> 16) & 255) / 255f,
            ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);

        private static void RefreshPanels()
        {
            ClearStyleCache();
            foreach (var root in Roots)
            {
                if (_overrides == null) continue;
                root.styleSheets.Remove(_overrides);
                root.styleSheets.Add(_overrides);
                root.MarkDirtyRepaint();
            }
            InternalEditorUtility.RepaintAllViews();
        }

        private static void Restore()
        {
            for (int i = Restorations.Count - 1; i >= 0; i--)
                Restorations[i]();
            Restorations.Clear();
            ClearStyleCache();
            foreach (var root in Roots)
            {
                root.RemoveFromClassList("ayu-light");
                if (_overrides != null) root.styleSheets.Remove(_overrides);
                root.MarkDirtyRepaint();
            }
            Roots.Clear();
            Sheets.Clear();
            Styles.Clear();
            _catalog = null;
            _applied = false;
            InternalEditorUtility.RepaintAllViews();
        }

        private static void ClearStyleCache()
        {
            typeof(VisualElement).Assembly.GetType("UnityEngine.UIElements.StyleCache")?
                .GetMethod("ClearStyleCache", Flags)?.Invoke(null, null);
        }
    }
}
