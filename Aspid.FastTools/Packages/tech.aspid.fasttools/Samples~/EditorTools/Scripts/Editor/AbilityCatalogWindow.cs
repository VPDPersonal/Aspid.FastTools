using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools.Editors
{
    // A two-pane editor window: every AbilityConfig in the project on the left, the selected one on the right.
    // Built entirely in code with the fluent extensions; edits go through SerializedProperty setters and
    // UI Toolkit binding, so Undo and dirty tracking work as in the Inspector.
    internal sealed class AbilityCatalogWindow : EditorWindow
    {
        private enum PreviewTheme { Editor, Dark, Light }
        private const string ThemeKey = "Aspid.FastTools.AbilityCatalog.Theme";
        private readonly List<AbilityConfig> _all = new();
        private readonly List<AbilityConfig> _filtered = new();

        private ListView _list;
        private TextField _search;
        private VisualElement _details;
        [NonSerialized] private AbilityConfig _shown;
        [SerializeField] private string _filter = string.Empty;

        [MenuItem("Tools/Aspid 🐍/FastTools/Samples/Ability Catalog")]
        private static void Open() =>
            GetWindow<AbilityCatalogWindow>("Ability Catalog").minSize = new Vector2(680, 440);

        private void CreateGUI()
        {
            rootVisualElement
                .ClearChildren()
                .AddClass("ability-catalog");
            var theme = (PreviewTheme)SessionState.GetInt(ThemeKey, 0);
            ApplyTheme(theme);
            var scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                System.IO.Path.GetDirectoryName(scriptPath) + "/AbilityCatalog.uss");
            if (stylesheet != null)
                rootVisualElement.AddStyleSheet(stylesheet);

            Reload();

            _search = new TextField()
                .SetFlexGrow(1)
                .SetPlaceholder("Search abilities…")
                .AddValueChanged(evt => ApplyFilter(evt.newValue));

            // _filter is serialized and survives a domain reload, so show it in the new search box.
            _search.SetValueWithoutNotify(_filter);

            var create = new Button()
                .SetTextSelf("Create")
                .SetTooltip("Creates a new AbilityConfig asset next to the selected one")
                .AddClicked(CreateAsset)
                .AddClass("ability-primary");

            var toolbar = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .SetPaddingX(6)
                .SetPaddingY(4)
                .AddChild(_search)
                .AddChild(create)
                .AddClass("ability-toolbar");

            _list = new ListView()
                .SetItemsSource(_filtered)
                .SetFixedItemHeight(64)
                .SetSelectionType(SelectionType.Single)
                .SetShowAlternatingRowBackgrounds(AlternatingRowBackground.None)
                .SetMakeItem(() => new VisualElement()
                    .AddClass("ability-row")
                    .AddChild(new Label().SetName("abilityName"))
                    .AddChild(new Label().SetName("abilityStats")))
                .SetBindItem((element, index) =>
                {
                    var ability = _filtered[index];
                    element.Q<Label>("abilityName").SetTextSelf(ability.AbilityName);
                    element.Q<Label>("abilityStats").SetTextSelf($"{ability.ManaCost} MP   /   {ability.Cooldown:0.##} s cooldown");
                })
                .AddSelectionChanged(selection => ShowDetails(selection.FirstOrDefault() as AbilityConfig))
                .SetFlexGrow(1);

            var left = new VisualElement()
                .SetWidth(248)
                .SetBorderColor(new Color(0.2f, 0.2f, 0.2f))
                .SetBorderWidth(right: 1)
                .AddChild(toolbar)
                .AddChild(_list)
                .AddClass("ability-sidebar");

            _details = new ScrollView()
                .SetFlexGrow(1)
                .AddClass("ability-details");

            var themeField = new EnumField("Theme", theme)
                .SetPosition(Position.Absolute)
                .SetRight(20)
                .SetTop(20)
                .SetWidth(190)
                .AddValueChanged(evt =>
                {
                    var selected = (PreviewTheme)evt.newValue;
                    SessionState.SetInt(ThemeKey, (int)selected);
                    ApplyTheme(selected);
                });

            themeField.labelElement
                .SetMinWidth(44)
                .SetWidth(44);

            var header = new VisualElement()
                .AddClass("ability-header")
                .AddChild(new Label("Ability catalog").SetName("catalogTitle"))
                .AddChild(new Label("Tune your abilities. See every change.").SetName("catalogSubtitle"))
                .AddChild(themeField);

            rootVisualElement
                .AddChild(header)
                .AddChild(new VisualElement()
                    .SetFlexDirection(FlexDirection.Row)
                    .SetFlexGrow(1)
                    .AddChild(left)
                    .AddChild(_details));

            Select(null);
        }

        // Assets created, renamed or deleted in the Project window while the window is open.
        private void OnProjectChange()
        {
            if (_list is null)
                return;

            var selected = _list.selectedItem as AbilityConfig;
            Reload();
            Select(selected);
        }

        private void Reload()
        {
            _all.Clear();
            _all.AddRange(AssetDatabase.FindAssets($"t:{nameof(AbilityConfig)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AbilityConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(config => config != null)
                .OrderBy(config => config.AbilityName));
            ApplyFilter(_filter);
        }

        // Keeps the selection on the same asset after a reload, falling back to the first one. The details are
        // rebuilt only when the shown asset changes, so a field being edited keeps its focus.
        private void Select(AbilityConfig config)
        {
            var index = config == null ? -1 : _filtered.IndexOf(config);
            if (index < 0)
                index = _filtered.Count > 0 ? 0 : -1;

            var item = index < 0 ? null : _filtered[index];
            _list.SetSelectionWithoutNotify(index < 0 ? Array.Empty<int>() : new[] { index });
            if (item == null || !ReferenceEquals(item, _shown))
                ShowDetails(item);
        }

        private void ApplyFilter(string filter)
        {
            _filter = filter ?? string.Empty;
            _filtered.Clear();
            _filtered.AddRange(_all.Where(config =>
                config.AbilityName.Contains(_filter, StringComparison.OrdinalIgnoreCase)));
            _list?.RefreshItems();
        }

        private void ShowDetails(AbilityConfig config)
        {
            _details.ClearChildren();
            _shown = config;

            // Unity's == also catches an asset deleted while it was listed.
            if (config == null)
            {
                _details.AddChild(new HelpBox("Select an ability, or press Create.", HelpBoxMessageType.Info));
                return;
            }

            var serializedObject = new SerializedObject(config);
            var effectType = serializedObject.FindProperty("_effectType");

            var effectLabel = new Label()
                .SetFlexGrow(1)
                .SetWhiteSpace(WhiteSpace.Normal);
            effectLabel.TrackSerializedObjectValue(serializedObject, _ => RefreshEffect());
            var effectButton = new Button()
                .SetTextSelf("Change…");

            // The same picker the [TypeSelector] attribute opens, driven from code: anchor it to the button,
            // constrain it to IAbilityEffect implementations and write the result into the string property.
            effectButton.AddClicked(() => TypeSelectorWindow.Show(
                GUIUtility.GUIToScreenRect(effectButton.worldBound),
                new TypeSelectorFilter { Types = new[] { typeof(IAbilityEffect) } },
                effectType.stringValue,
                aqn =>
                {
                    effectType.SetStringAndApply(aqn ?? string.Empty);
                    RefreshEffect();
                }));

            var effectRow = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .SetMarginTop(8)
                .AddChild(new Label("Effect")
                    .SetWidth(120))
                .AddChild(effectLabel)
                .AddChild(effectButton)
                .AddClass("ability-effect-row");

            // A one-click balance pass: chainable typed setters, Undo included, applied once at the end.
            var halveCooldown = new Button()
                .SetTextSelf("Halve cooldown, +5 MP")
                .AddClicked(() =>
                {
                    serializedObject.Update();
                    var cooldown = serializedObject.FindProperty("_cooldown");
                    var manaCost = serializedObject.FindProperty("_manaCost");
                    cooldown.SetFloat(cooldown.floatValue * 0.5f);
                    manaCost.SetIntAndApply(manaCost.intValue + 5);
                })
                .AddClass("ability-action");

            var selectAsset = new Button()
                .SetTextSelf("Select asset")
                .AddClicked(() => Selection.activeObject = config)
                .AddClass("ability-action");

            var actions = new VisualElement()
                .AddClass("ability-actions")
                .AddChild(halveCooldown)
                .AddChild(selectAsset);

            var description = new TextField("Description") { multiline = true }
                .SetBindingPath("_description")
                .AddClass("ability-description");

            var title = new Label(config.AbilityName)
                .SetFontSize(24)
                .AddBoldUnityFontStyleAndWeight()
                .SetMarginBottom(18)
                .SetTooltip("Double-click to open the script")
                .AddOpenScriptCommand(config)
                .AddClass("ability-detail-title");

            title.TrackSerializedObjectValue(serializedObject, _ =>
            {
                title.SetTextSelf(config.AbilityName);
                _list.RefreshItems();
            });

            _details
                .AddChild(title)
                // BindTo(SerializedObject) binds every PropertyField below in one call.
                .AddChild(new VisualElement()
                    .AddChild(new PropertyField(serializedObject.FindProperty("_abilityName"))
                        .AddValueChanged(_ => _list.RefreshItems()))
                    .AddChild(description)
                    .AddChild(new PropertyField(serializedObject.FindProperty("_manaCost")))
                    .AddChild(new PropertyField(serializedObject.FindProperty("_cooldown")))
                    .BindTo(serializedObject))
                .AddChild(effectRow)
                .AddChild(actions)
                .AddChild(new Label("Changes are saved to the asset. Use Undo to restore previous values.")
                    .AddClass("ability-hint"));

            RefreshEffect();

            void RefreshEffect()
            {
                var type = config.EffectType;
                var description = type is null ? "none" : ((IAbilityEffect)Activator.CreateInstance(type)).Describe(config);
                effectLabel.SetTextSelf(type is null ? "<None>" : $"{type.Name} — {description}");
            }
        }

        private void ApplyTheme(PreviewTheme theme) => rootVisualElement.EnableClass(
            "ability-catalog--light",
            enable: theme == PreviewTheme.Light || (theme == PreviewTheme.Editor && !EditorGUIUtility.isProSkin));

        private void CreateAsset()
        {
            var selected = _list.selectedItem as AbilityConfig;
            var folder = selected == null ? "Assets" : System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(selected));
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/Ability.asset");

            var config = CreateInstance<AbilityConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            // Clear the search so the new "New Ability" is listed and can be selected.
            _search.SetValueWithoutNotify(string.Empty);
            _filter = string.Empty;
            Reload();
            Select(config);
        }
    }
}
