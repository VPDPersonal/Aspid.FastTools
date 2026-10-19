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
    // A two-pane editor window: every AbilityConfig in the project on the left, the selected one on the right.
    // Built entirely in code with the fluent extensions; edits go through SerializedProperty setters and
    // UI Toolkit binding, so Undo and dirty tracking work as in the Inspector. AddAspidTheme gives the window the
    // FastTools palette and theme, and AbilityCatalog.uss colours it only with their --aspid-* tokens.
    internal sealed class AbilityCatalogWindow : EditorWindow
    {
        private readonly List<AbilityConfig> _all = new();
        private readonly List<AbilityConfig> _filtered = new();

        private ListView _list;
        private TextField _search;
        private VisualElement _details;
        private StyleSheet _effectPickerStyle;
        [NonSerialized] private AbilityConfig _shown;
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

            // The theme goes first, so the catalog's own sheet can read its tokens.
            rootVisualElement
                .ClearChildren()
                .AddClass("ability-catalog")
                .AddAspidTheme()
                .AddStyleSheet(stylesheet)
                .SetFlexDirection(FlexDirection.Row);

            Reload();

            _search = new TextField()
                .SetFlexGrow(1)
                .SetTooltip("Search by name")
                .AddValueChanged(evt =>
                {
                    // The list keeps the selected index, not the item: select the shown ability again by item.
                    ApplyFilter(evt.newValue);
                    Select(_shown);
                })
                .AddClass("ability-search");

            // A magnifier in place of a placeholder: the light editor icon, tinted by the theme in AbilityCatalog.uss.
            // It sits on the field, over the input's left padding: the input clips its children to its content box.
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
                .AddSelectionChanged(selection => ShowDetails(selection.FirstOrDefault() as AbilityConfig))
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

        // A row: the name and the stats, then two icon buttons for the row's ability, shown on hover and on the
        // selected row. bindItem keeps the ability in userData, so the buttons follow the row when it is reused.
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

        // Light editor icons with a @2x version, so they stay sharp on a Retina screen; AbilityCatalog.uss tints them.
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

            // A button drawn as a drop-down. It opens the same picker the [TypeSelector] attribute opens, driven
            // from code: anchor it to the field, constrain it to IAbilityEffect implementations, style it like the
            // catalog with EffectPicker.uss and write the result into the string property.
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

            // Shown only while the balance differs from the defaults. Chainable typed setters write both values and
            // apply them once, so a single Undo reverts the reset.
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

            // The field shows the name, icon and tooltip that [TypeSelectorDisplay] gives the type in the picker.
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
            Select(config);
        }

        // Undo does not bring a deleted asset back, so the window asks first and moves the file to the trash.
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

            // The shown ability stays selected; when it was the one deleted, the ability below takes its place.
            Reload();
            Select(!wasShown ? _shown : _filtered.Count == 0 ? null : _filtered[Math.Clamp(index, 0, _filtered.Count - 1)]);
        }

        // The defaults are the field initializers of AbilityConfig, so a fresh instance holds them.
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
