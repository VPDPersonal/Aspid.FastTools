using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Aspid.FastTools.Types;
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
    // Every AbilityConfig of the project on the left, the selected one on the right, built in code with the fluent
    // extensions. Edits go through SerializedProperty setters and binding, so Undo works as in the Inspector.
    internal sealed class AbilityCatalogWindow : EditorWindow
    {
        private readonly List<AbilityConfig> _all = new();
        private readonly List<AbilityConfig> _filtered = new();

        private ListView _list;
        private TextField _search;
        private VisualElement _details;
        private StyleSheet _effectPickerStyle;
        [NonSerialized] private AbilityConfig _shown;
        [NonSerialized] private AbilityConfig _picked;
        [NonSerialized] private SerializedObject _shownObject;
        [SerializeField] private string _filter = string.Empty;

        [MenuItem("Tools/Aspid 🐍/FastTools/Samples/Ability Catalog")]
        private static void Open() =>
            GetWindow<AbilityCatalogWindow>("Ability Catalog").minSize = new Vector2(640, 420);

        private void CreateGUI()
        {
            var folder = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this)));
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(folder + "/AbilityCatalog.uss");
            _effectPickerStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(folder + "/EffectPicker.uss");

            // The theme comes first: AbilityCatalog.uss reads its tokens.
            rootVisualElement
                .ClearChildren()
                .AddClass("ability-catalog")
                .AddAspidWindowTheme()
                .AddStyleSheet(stylesheet)
                .SetFlexDirection(FlexDirection.Row);

            Reload();

            _search = new TextField()
                .SetFlexGrow(1)
                .SetTooltip("Search by name")
                .AddValueChanged(evt =>
                {
                    // The picked ability comes back once the filter lists it again.
                    ApplyFilter(evt.newValue);
                    Select(_picked);
                })
                .AddClass("ability-search");

            // On the field, not in the input: the input clips its children to its content box.
            _search.AddChild(new VisualElement()
                .SetBackgroundImage(new StyleBackground((Texture2D)EditorGUIUtility.IconContent("d_Search Icon").image))
                .SetPickingMode(PickingMode.Ignore)
                .AddClass("ability-search__icon"));

            // _filter is serialized and survives a domain reload, so show it in the new search box.
            _search.SetValueWithoutNotify(_filter);

            var create = new Button()
                .SetTextSelf("+")
                .SetTooltip("Create an ability next to the selected one")
                .AddClicked(CreateAsset)
                .AddClasses("ability-button", "ability-button--primary", "ability-button--icon");

            var toolbar = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .AddChild(_search)
                .AddChild(create)
                .AddClass("ability-toolbar");

            _list = new ListView()
                .SetItemsSource(_filtered)
                .SetFixedItemHeight(60)
                .SetSelectionType(SelectionType.Single)
                .SetShowAlternatingRowBackgrounds(AlternatingRowBackground.None)
                .SetMakeItem(MakeRow)
                .SetBindItem((element, index) =>
                {
                    var ability = _filtered[index];
                    element.userData = ability;
                    element.Q<Label>("abilityName").SetTextSelf(ability.AbilityName);
                    element.Q<Label>("abilityStats").SetTextSelf($"{ability.ManaCost} MP · {ability.Cooldown:0.##} s");
                })
                .AddSelectionChanged(selection => ShowDetails(_picked = selection.FirstOrDefault() as AbilityConfig))
                .SetFlexGrow(1);

            var sidebar = new VisualElement()
                .SetWidth(260)
                .SetFlexShrink(0)
                .AddChild(toolbar)
                .AddChild(_list)
                .AddClass("ability-sidebar");

            _details = new ScrollView()
                .SetFlexGrow(1)
                .AddClass("ability-details");

            rootVisualElement
                .AddChild(sidebar)
                .AddChild(_details);

            Select(null);
        }

        // bindItem stores the row's ability in userData, so the buttons act on what a reused row shows.
        private VisualElement MakeRow()
        {
            var row = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .AddClass("ability-row");

            var selectAsset = MakeRowButton("d_Project", "Select the asset in the Project window")
                .AddClicked(() => SelectAsset((AbilityConfig)row.userData));

            var delete = MakeRowButton("d_clear", "Move the asset to the trash")
                .AddClicked(() => DeleteAsset((AbilityConfig)row.userData))
                .AddClass("ability-row__button--danger");

            return row
                .AddChild(new VisualElement()
                    .SetFlexGrow(1)
                    .AddChild(new Label().SetName("abilityName"))
                    .AddChild(new Label().SetName("abilityStats")))
                .AddChild(new VisualElement()
                    .SetFlexDirection(FlexDirection.Row)
                    .AddChild(selectAsset)
                    .AddChild(delete)
                    .AddClass("ability-row__actions"));
        }

        private static Button MakeRowButton(string icon, string tooltip) => new Button()
            .SetTooltip(tooltip)
            .AddChild(new VisualElement()
                .SetBackgroundImage(new StyleBackground((Texture2D)EditorGUIUtility.IconContent(icon).image))
                .SetPickingMode(PickingMode.Ignore)
                .AddClass("ability-row__icon"))
            .AddClass("ability-row__button");

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

            // A shorter list drops a selection past its end and reports it as a pick of nothing; callers select again.
            _list?.SetSelectionWithoutNotify(Array.Empty<int>());
            _list?.RefreshItems();
        }

        // A SerializedObject holds a native copy of the asset: dispose it when the details move to another asset and
        // when the window closes.
        private void OnDestroy() => _shownObject?.Dispose();

        private void ShowDetails(AbilityConfig config)
        {
            _details.ClearChildren();
            _shownObject?.Dispose();
            _shownObject = null;
            _shown = config;

            // Unity's == also catches an asset deleted while it was listed.
            if (config == null)
            {
                _details.AddChild(new VisualElement()
                    .AddClass("ability-form")
                    .AddChild(new Label("Select an ability or create one with +.").AddClass("ability-empty")));
                return;
            }

            var serializedObject = _shownObject = new SerializedObject(config);
            var effectType = serializedObject.FindProperty("_effectType");

            var effectIcon = new VisualElement()
                .SetPickingMode(PickingMode.Ignore)
                .AddClass("ability-effect__icon");
            var effectName = new Label()
                .AddClass("ability-effect__name");
            var effectNote = new Label()
                .AddClass("ability-effect__note");

            // A button drawn as a drop-down that opens the [TypeSelector] picker from code, styled by EffectPicker.uss.
            var effectField = new Button()
                .SetFlexGrow(1)
                .AddChild(effectIcon)
                .AddChild(effectName)
                .AddChild(new VisualElement().AddClasses(EnumField.arrowUssClassName, "ability-effect__arrow"))
                .AddClass("ability-effect");

            effectField.TrackSerializedObjectValue(serializedObject, _ => RefreshEffect());
            effectField.AddClicked(() => TypeSelectorWindow.Show(
                GUIUtility.GUIToScreenRect(effectField.worldBound),
                new TypeSelectorFilter { Types = new[] { typeof(IAbilityEffect) } },
                effectType.stringValue,
                aqn =>
                {
                    effectType.SetStringAndApply(aqn ?? string.Empty);
                    RefreshEffect();
                },
                styleSheet: _effectPickerStyle));

            var effectRow = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .AddChild(new Label("Effect").AddClass("ability-label"))
                .AddChild(effectField);

            // Typed setters apply both values once, so one Undo reverts the reset.
            var defaults = ReadDefaults();
            var resetToDefaults = new Button()
                .SetTextSelf("Reset to defaults")
                .SetTooltip("Resets Mana Cost and Cooldown to their default values")
                .AddClicked(() =>
                {
                    serializedObject.Update();
                    serializedObject.FindProperty("_cooldown").SetFloat(defaults.Cooldown);
                    serializedObject.FindProperty("_manaCost").SetIntAndApply(defaults.ManaCost);
                })
                .AddClass("ability-button");
            resetToDefaults.TrackSerializedObjectValue(serializedObject, _ => RefreshReset());

            var actions = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .AddChild(resetToDefaults)
                .AddClass("ability-actions");

            var description = new TextField("Description") { multiline = true }
                .SetBindingPath("_description")
                .AddClass("ability-description");

            var title = new Label(config.AbilityName)
                .SetTooltip("Double-click to open the script")
                .AddOpenScriptCommand(config)
                .AddClass("ability-title");

            title.TrackSerializedObjectValue(serializedObject, _ =>
            {
                title.SetTextSelf(config.AbilityName);
                _list.RefreshItems();
            });

            _details.AddChild(new VisualElement()
                .AddClass("ability-form")
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
                .AddChild(effectNote)
                .AddChild(actions));

            RefreshEffect();
            RefreshReset();

            void RefreshReset() => resetToDefaults.SetDisplay(
                config.ManaCost != defaults.ManaCost || !Mathf.Approximately(config.Cooldown, defaults.Cooldown)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None);

            // The name, icon and tooltip that [TypeSelectorDisplay] gives the effect in the picker.
            void RefreshEffect()
            {
                var type = config.EffectType;
                var display = type?.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false);
                var icon = string.IsNullOrEmpty(display?.Icon) ? null : EditorGUIUtility.IconContent(display.Icon).image as Texture2D;
                var note = type is null ? string.Empty : ((IAbilityEffect)Activator.CreateInstance(type)).Describe(config);

                effectField.SetTooltip(display?.Tooltip);
                effectName.SetTextSelf(type is null ? "None" : display?.Name ?? type.Name);
                effectIcon
                    .SetBackgroundImage(new StyleBackground(icon))
                    .SetDisplay(icon != null ? DisplayStyle.Flex : DisplayStyle.None);
                effectNote
                    .SetTextSelf(note.Length > 0 ? char.ToUpperInvariant(note[0]) + note[1..] : note)
                    .SetDisplay(note.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None);
            }
        }

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
            Select(_picked = config);
        }

        // Undo cannot restore a deleted asset, so the window asks first and moves it to the trash.
        private void DeleteAsset(AbilityConfig config)
        {
            var path = AssetDatabase.GetAssetPath(config);
            if (!EditorUtility.DisplayDialog(
                    title: "Delete ability",
                    message: $"Move {config.AbilityName} to the trash?\n\n{path}",
                    ok: "Delete",
                    cancel: "Cancel"))
                return;

            var index = _filtered.IndexOf(config);
            var wasShown = ReferenceEquals(config, _shown);
            if (!AssetDatabase.MoveAssetToTrash(path))
                return;

            // When the shown ability is deleted, the one below takes its place.
            Reload();
            Select(!wasShown ? _shown : _filtered.Count == 0 ? null : _filtered[Math.Clamp(index, 0, _filtered.Count - 1)]);
            if (wasShown) _picked = _shown;
        }

        // A fresh instance holds the field initializers.
        private static (int ManaCost, float Cooldown) ReadDefaults()
        {
            var defaults = CreateInstance<AbilityConfig>();
            try
            {
                return (defaults.ManaCost, defaults.Cooldown);
            }
            finally
            {
                DestroyImmediate(defaults);
            }
        }

        private static void SelectAsset(AbilityConfig config)
        {
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}
