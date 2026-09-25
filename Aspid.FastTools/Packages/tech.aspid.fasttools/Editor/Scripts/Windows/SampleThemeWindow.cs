using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Aspid.FastTools.Editors.Internal
{
    internal enum SampleThemeMode { Authored, Dark, Light }

    [Serializable]
    internal sealed class SampleThemePalette
    {
        public Color Background;
        public Color Platform;
        public Color Edge;
        public Color Porcelain;
        public Color Accent;
        public Color Text;
        public Color MutedText;
        public Color Cyan;
        public Color Grid;
        [Range(0, 1), Tooltip("Blend animated character and flock colors toward white in the light preview.")]
        public float AnimatedColorLift = 0.45f;

        internal static SampleThemePalette Dark() => new()
        {
            Background = Hex("0E1015"), Platform = new Color(0.085f, 0.13f, 0.17f),
            Edge = new Color(0.16f, 0.24f, 0.29f), Porcelain = new Color(0.82f, 0.89f, 0.91f),
            Accent = new Color(0.65f, 0.95f, 0.4f), Text = Hex("F0F0F2"), MutedText = Hex("92959F"),
            Cyan = Hex("40D9FF"), Grid = Hex("293D4A"),
        };

        internal static SampleThemePalette Light() => new()
        {
            Background = Hex("EEF0F3"), Platform = Hex("D3D5DB"), Edge = Hex("B4B7C0"),
            Porcelain = Hex("CFD9DF"), Accent = Hex("A6D8A8"), Text = Hex("2A2C31"), MutedText = Hex("5F636B"),
            Cyan = Hex("97C7D8"), Grid = Hex("B4BDC8"),
        };

        internal static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out var color);
            return color;
        }
    }

    [Serializable]
    internal sealed class SampleSurfaceColors
    {
        public Color Grass;
        public Color Stone;
        public Color Metal;
        public Color Water;
        public Color Sand;

        internal Color Get(string surface, Color fallback) => surface switch
        {
            "Grass" => Grass, "Stone" => Stone, "Metal" => Metal,
            "Water" => Water, "Sand" => Sand, _ => fallback,
        };
    }

    [Serializable]
    internal sealed class SampleSurfaceTheme
    {
        public Color Caption = SampleThemePalette.Hex("455363");
        public SampleSurfaceColors Tiles = new()
        {
            Grass = SampleThemePalette.Hex("9ABD9F"), Stone = SampleThemePalette.Hex("BBC0CB"),
            Metal = SampleThemePalette.Hex("93A9B5"), Water = SampleThemePalette.Hex("91BFD5"),
            Sand = SampleThemePalette.Hex("DBCAA5"),
        };
        public SampleSurfaceColors Trails = new()
        {
            Grass = SampleThemePalette.Hex("78B795"), Stone = SampleThemePalette.Hex("B29BD4"),
            Metal = SampleThemePalette.Hex("D998A8"), Water = SampleThemePalette.Hex("61B4CE"),
            Sand = SampleThemePalette.Hex("D4A662"),
        };
    }

    [FilePath("ProjectSettings/AspidFastToolsSampleThemes.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class SampleThemeSettings : ScriptableSingleton<SampleThemeSettings>
    {
        [Tooltip("Shared palette for dark sample recordings.")]
        [SerializeField] private SampleThemePalette _dark = SampleThemePalette.Dark();
        [Tooltip("Shared palette for light sample recordings.")]
        [SerializeField] private SampleThemePalette _light = SampleThemePalette.Light();
        [Tooltip("Light theme colors for EnumValues tiles, trails and their captions.")]
        [SerializeField] private SampleSurfaceTheme _enumValuesLight = new();
        internal SampleSurfaceTheme EnumValuesLight => _enumValuesLight;
        internal SampleThemePalette Get(SampleThemeMode mode) => mode == SampleThemeMode.Light ? _light : _dark;
        internal void Persist() => Save(true);
    }

    [InitializeOnLoad]
    internal static class SampleThemePreview
    {
        private const string ModeKey = "Aspid.FastTools.SampleTheme";
        private static readonly Dictionary<Camera, Color> Cameras = new();
        private static readonly Dictionary<TextMesh, Color> Labels = new();
        private static readonly Dictionary<Renderer, MaterialPropertyBlock> Renderers = new();
        private static readonly Dictionary<MonoBehaviour, (FieldInfo Field, ScriptableObject Original)> SurfaceReferences = new();
        private static readonly Dictionary<ScriptableObject, ScriptableObject> SurfacePalettes = new();
        private static readonly Dictionary<Renderer, MaterialPropertyBlock> AnimatedRenderers = new();
        private static double _nextUpdate;
        private static bool _suspended;

        internal static SampleThemeMode Mode => (SampleThemeMode)SessionState.GetInt(ModeKey, 0);

        static SampleThemePreview()
        {
            EditorApplication.update += Update;
            Camera.onPreCull += BeforeCamera;
            Camera.onPostRender += _ => RestoreAnimated();
            RenderPipelineManager.beginCameraRendering += (_, camera) => BeforeCamera(camera);
            RenderPipelineManager.endCameraRendering += (_, _) => RestoreAnimated();
            AssemblyReloadEvents.beforeAssemblyReload += Restore;
            EditorApplication.quitting += Restore;
            EditorApplication.playModeStateChanged += state =>
            {
                _suspended = state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode;
                Restore();
            };
            EditorSceneManager.sceneClosing += (_, _) => Restore();
            EditorSceneManager.sceneSaving += (_, _) =>
            {
                _suspended = true;
                Restore();
                EditorApplication.delayCall += () => _suspended = false;
            };
        }

        internal static void SetMode(SampleThemeMode mode)
        {
            Restore();
            SessionState.SetInt(ModeKey, (int)mode);
            Apply();
        }

        internal static void Refresh()
        {
            Restore();
            Apply();
        }

        private static void Update()
        {
            if (EditorApplication.timeSinceStartup < _nextUpdate) return;
            _nextUpdate = EditorApplication.timeSinceStartup + 0.3;
            Apply();
        }

        internal static void Apply()
        {
            if (_suspended || Mode == SampleThemeMode.Authored) return;
            var palette = SampleThemeSettings.instance.Get(Mode);
            // Only shipped sample scenes with their framing component participate.
            var scenes = Resources.FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(component => component != null && component.GetType().Name == "SampleFrame"
                    && component.GetType().Namespace?.StartsWith("Aspid.FastTools.Samples.", StringComparison.Ordinal) == true
                    && component.gameObject.scene.IsValid() && component.gameObject.scene.isLoaded)
                .Select(component => component.gameObject.scene).Distinct();
            foreach (var scene in scenes)
                ApplyScene(scene, palette);
            SceneView.RepaintAll();
        }

        private static void ApplyScene(Scene scene, SampleThemePalette palette)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (Mode == SampleThemeMode.Light)
                    foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                        ApplySurfacePalette(component);
                foreach (var camera in root.GetComponentsInChildren<Camera>(true))
                {
                    if (Cameras.ContainsKey(camera)) continue;
                    Cameras.Add(camera, camera.backgroundColor);
                    camera.backgroundColor = palette.Background;
                }
                foreach (var text in root.GetComponentsInChildren<TextMesh>(true))
                {
                    if (Labels.ContainsKey(text)) continue;
                    if (text.name == "Surface label" && Mode != SampleThemeMode.Light) continue;
                    Labels.Add(text, text.color);
                    var color = text.name == "Surface label" ? SampleThemeSettings.instance.EnumValuesLight.Caption
                        : text.color.maxColorComponent > 0.8f ? palette.Text : palette.MutedText;
                    // Legacy TextMesh sends vertex colors directly to the font shader.
                    text.color = Mode == SampleThemeMode.Light && QualitySettings.activeColorSpace == ColorSpace.Linear
                        ? color.linear : color;
                }
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (Renderers.ContainsKey(renderer) || (renderer.HasPropertyBlock() && renderer is not LineRenderer)) continue;
                    var material = renderer.sharedMaterial;
                    if (material == null || !AssetDatabase.GetAssetPath(material).Contains("/Presentation/")) continue;
                    Color color;
                    switch (material.name)
                    {
                        case "Graphite": color = palette.Platform; break;
                        case "Edge": color = palette.Edge; break;
                        case "Porcelain": color = palette.Porcelain; break;
                        case "Venom": color = palette.Accent; break;
                        case "Cyan" when Mode == SampleThemeMode.Light: color = palette.Cyan; break;
                        case "Grid" when Mode == SampleThemeMode.Light: color = palette.Grid; break;
                        default: continue;
                    }
                    var original = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(original);
                    Renderers.Add(renderer, original);
                    var block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block);
                    block.SetColor("_Color", color);
                    block.SetColor("_BaseColor", color);
                    renderer.SetPropertyBlock(block);
                }
            }
        }

        private static void BeforeCamera(Camera camera)
        {
            RestoreAnimated();
            if (_suspended || Mode != SampleThemeMode.Light || camera == null) return;
            if (!camera.GetComponents<MonoBehaviour>().Any(component => component != null
                && component.GetType().Name == "SampleFrame"
                && component.GetType().Namespace?.StartsWith("Aspid.FastTools.Samples.", StringComparison.Ordinal) == true)) return;
            var palette = SampleThemeSettings.instance.Get(Mode);
            foreach (var root in camera.gameObject.scene.GetRootGameObjects())
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.HasPropertyBlock()) continue;
                var material = renderer.sharedMaterial;
                var path = material == null ? "" : AssetDatabase.GetAssetPath(material);
                if (!path.Contains("/Presentation/") || path.Contains("/EnumValues/")) continue;
                if (Renderers.ContainsKey(renderer)) continue;
                var original = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(original);
                var color = original.GetColor("_Color");
                if (color.a <= 0) continue;
                var themed = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(themed);
                color = Color.Lerp(color, Color.white, palette.AnimatedColorLift);
                themed.SetColor("_Color", color);
                themed.SetColor("_BaseColor", color);
                AnimatedRenderers.Add(renderer, original);
                renderer.SetPropertyBlock(themed);
            }
        }

        private static void RestoreAnimated()
        {
            foreach (var pair in AnimatedRenderers)
                if (pair.Key != null) pair.Key.SetPropertyBlock(pair.Value);
            AnimatedRenderers.Clear();
        }

        private static void ApplySurfacePalette(MonoBehaviour component)
        {
            if (component == null || SurfaceReferences.ContainsKey(component)) return;
            var type = component.GetType();
            if (type.Namespace != "Aspid.FastTools.Samples.EnumValues"
                || (type.Name != "SurfaceTile" && type.Name != "Walker")) return;
            // Samples are optional assemblies. Reflect only their known palette field, keeping
            // the package editor independent of whether the sample has been imported.
            var field = type.GetField("_palette", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field?.GetValue(component) is not ScriptableObject original) return;
            if (!SurfacePalettes.TryGetValue(original, out var preview))
            {
                preview = UnityEngine.Object.Instantiate(original);
                preview.hideFlags = HideFlags.HideAndDontSave;
                var serialized = new SerializedObject(preview);
                var theme = SampleThemeSettings.instance.EnumValuesLight;
                ApplyTable(serialized.FindProperty("_tileColors._values"), theme.Tiles);
                ApplyTable(serialized.FindProperty("_footprintColors._values"), theme.Trails);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                SurfacePalettes.Add(original, preview);
            }
            SurfaceReferences.Add(component, (field, original));
            field.SetValue(component, preview);
            RefreshTile(component);
        }

        private static void ApplyTable(SerializedProperty rows, SampleSurfaceColors colors)
        {
            if (rows == null || !rows.isArray) return;
            for (var i = 0; i < rows.arraySize; i++)
            {
                var row = rows.GetArrayElementAtIndex(i);
                var value = row.FindPropertyRelative("_value");
                value.colorValue = colors.Get(row.FindPropertyRelative("_key").stringValue, value.colorValue);
            }
        }

        private static void RefreshTile(MonoBehaviour component) => component.GetType()
            .GetMethod("Refresh", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(component, null);

        internal static void Restore()
        {
            RestoreAnimated();
            foreach (var pair in SurfaceReferences)
            {
                if (pair.Key == null) continue;
                pair.Value.Field.SetValue(pair.Key, pair.Value.Original);
                RefreshTile(pair.Key);
            }
            foreach (var preview in SurfacePalettes.Values)
                if (preview != null) UnityEngine.Object.DestroyImmediate(preview);
            SurfaceReferences.Clear(); SurfacePalettes.Clear();
            foreach (var pair in Cameras) if (pair.Key != null) pair.Key.backgroundColor = pair.Value;
            foreach (var pair in Labels) if (pair.Key != null) pair.Key.color = pair.Value;
            foreach (var pair in Renderers) if (pair.Key != null) pair.Key.SetPropertyBlock(pair.Value);
            Cameras.Clear(); Labels.Clear(); Renderers.Clear();
            SceneView.RepaintAll();
        }
    }

    internal sealed class SampleThemeWindow : EditorWindow
    {
        [MenuItem("Tools/Aspid 🐍/FastTools/Sample Themes", priority = 41)]
        private static void Open() => GetWindow<SampleThemeWindow>("Sample Themes");

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = root.style.paddingRight = 12;
            root.style.paddingTop = root.style.paddingBottom = 12;
            minSize = new Vector2(360, 360);
            var mode = new EnumField("Preview", SampleThemePreview.Mode);
            mode.RegisterValueChangedCallback(evt => SampleThemePreview.SetMode((SampleThemeMode)evt.newValue));
            root.Add(mode);
            root.Add(new HelpBox("Applies to open EnumValues, Types, SerializeReferences and ProfilerMarkers scenes, including Play Mode. Authored restores the original look. Scene and material assets keep their authored colors.", HelpBoxMessageType.Info));
            var scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            root.Add(scroll);
            var settings = new SerializedObject(SampleThemeSettings.instance);
            scroll.Add(new PropertyField(settings.FindProperty("_light"), "Light palette"));
            scroll.Add(new PropertyField(settings.FindProperty("_dark"), "Dark palette"));
            scroll.Add(new PropertyField(settings.FindProperty("_enumValuesLight"), "EnumValues light surfaces"));
            scroll.Bind(settings);
            scroll.RegisterCallback<SerializedPropertyChangeEvent>(_ =>
            {
                SampleThemeSettings.instance.Persist();
                SampleThemePreview.Refresh();
            });
            var hint = new Label("Palettes are shared by all four scenes and saved in ProjectSettings/AspidFastToolsSampleThemes.asset.");
            hint.style.whiteSpace = WhiteSpace.Normal;
            root.Add(hint);
        }
    }
}
