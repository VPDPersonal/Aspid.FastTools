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
        private const string SearchIcon = "d_Search Icon";
        private const string SelectAssetIcon = "d_Project";
        private const string DeleteIcon = "d_clear";

        // The classes and names that AbilityCatalog.uss styles.
        private const string CatalogClass = "ability-catalog";
        private const string SidebarClass = "ability-sidebar";
        private const string ToolbarClass = "ability-toolbar";
        private const string SearchClass = "ability-search";
        private const string SearchIconClass = SearchClass + "__icon";

        private const string RowClass = "ability-row";
        private const string RowIconClass = RowClass + "__icon";
        private const string RowActionsClass = RowClass + "__actions";
        private const string RowButtonClass = RowClass + "__button";
        private const string RowButtonDangerClass = RowButtonClass + "--danger";
        private const string RowNameLabel = "abilityName";
        private const string RowStatsLabel = "abilityStats";

        private const string DetailsClass = "ability-details";
        private const string FormClass = "ability-form";
        private const string EmptyClass = "ability-empty";
        private const string TitleClass = "ability-title";
        private const string LabelClass = "ability-label";
        private const string DescriptionClass = "ability-description";
        private const string ActionsClass = "ability-actions";

        private const string EffectClass = "ability-effect";
        private const string EffectIconClass = EffectClass + "__icon";
        private const string EffectNameClass = EffectClass + "__name";
        private const string EffectArrowClass = EffectClass + "__arrow";
        private const string EffectNoteClass = EffectClass + "__note";

        private const string ButtonClass = "ability-button";
        private const string ButtonPrimaryClass = ButtonClass + "--primary";
        private const string ButtonIconClass = ButtonClass + "--icon";

        [SerializeField] private string _filter = string.Empty;

        private ListView _list;
        private TextField _search;
        private VisualElement _details;
        private StyleSheet _effectPickerStyle;

        private readonly List<AbilityConfig> _all = new();
        private readonly List<AbilityConfig> _filtered = new();

        [NonSerialized] private AbilityConfig _shown;
        [NonSerialized] private AbilityConfig _picked;
        [NonSerialized] private SerializedObject _shownObject;

        [MenuItem("Tools/Aspid 🐍/FastTools/Samples/Ability Catalog")]
        private static void Open() =>
            GetWindow<AbilityCatalogWindow>("Ability Catalog").minSize = new Vector2(640, 420);

        private void CreateGUI()
        {
            _effectPickerStyle = LoadStyleSheet(fileName: "EffectPicker.uss");

            _search = MakeSearch();
            _list = MakeList();
            _details = new ScrollView()
                .SetFlexGrow(1)
                .AddClass(DetailsClass);

            var create = new Button()
                .SetTextSelf("+")
                .SetTooltip("Create an ability next to the selected one")
                .AddClicked(CreateAsset)
                .AddClasses(ButtonClass, ButtonPrimaryClass, ButtonIconClass);

            var toolbar = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .AddChild(_search)
                .AddChild(create)
                .AddClass(ToolbarClass);

            var sidebar = new VisualElement()
                .SetWidth(260)
                .SetFlexShrink(0)
                .AddChild(toolbar)
                .AddChild(_list)
                .AddClass(SidebarClass);

            rootVisualElement
                .AddAspidWindowTheme()
                .AddStyleSheet(LoadStyleSheet(fileName: "AbilityCatalog.uss"))
                .AddClass(CatalogClass)
                .SetFlexDirection(FlexDirection.Row)
                .AddChild(sidebar)
                .AddChild(_details);

            Reload();
            Select(null);
        }

        private void OnProjectChange()
        {
            if (_list is null)
                return;

            var selected = _list.selectedItem as AbilityConfig;
            Reload();
            Select(selected);
        }

        // A SerializedObject holds a native copy of the asset: dispose it when the details move to another asset and
        // when the window closes.
        private void OnDestroy() => _shownObject?.Dispose();

        private TextField MakeSearch() => new TextField()
            .SetFlexGrow(1)
            .SetTooltip("Search by name")
            // _filter is serialized and survives a domain reload, so show it in the new search box.
            .SetValue(_filter, notify: false)
            .AddValueChanged(evt =>
            {
                // The picked ability comes back once the filter lists it again.
                ApplyFilter(evt.newValue);
                Select(_picked);
            })
            // On the field, not in the input: the input clips its children to its content box.
            .AddChild(MakeIcon(LoadIcon(SearchIcon)).AddClass(SearchIconClass))
            .AddClass(SearchClass);

        private ListView MakeList() => new ListView()
            .SetItemsSource(_filtered)
            .SetFixedItemHeight(60)
            .SetSelectionType(SelectionType.Single)
            .SetShowAlternatingRowBackgrounds(AlternatingRowBackground.None)
            .SetMakeItem(MakeRow)
            .SetBindItem(BindRow)
            .AddSelectionChanged(selection =>
            {
                _picked = selection.FirstOrDefault() as AbilityConfig;
                ShowDetails(_picked);
            })
            .SetFlexGrow(1);

        // BindRow stores the row's ability in userData, so the buttons act on what a reused row shows.
        private VisualElement MakeRow()
        {
            var row = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .AddClass(RowClass);

            var selectAsset = MakeRowButton(SelectAssetIcon, "Select the asset in the Project window")
                .AddClicked(() => SelectAsset((AbilityConfig)row.userData));

            var delete = MakeRowButton(DeleteIcon, "Move the asset to the trash")
                .AddClicked(() => DeleteAsset((AbilityConfig)row.userData))
                .AddClass(RowButtonDangerClass);

            return row
                .AddChild(new VisualElement()
                    .SetFlexGrow(1)
                    .AddChild(new Label().SetName(RowNameLabel))
                    .AddChild(new Label().SetName(RowStatsLabel)))
                .AddChild(new VisualElement()
                    .SetFlexDirection(FlexDirection.Row)
                    .AddChild(selectAsset)
                    .AddChild(delete)
                    .AddClass(RowActionsClass));
        }

        private void BindRow(VisualElement row, int index)
        {
            var ability = _filtered[index];
            row.userData = ability;
            row.Q<Label>(RowNameLabel).SetTextSelf(ability.AbilityName);
            row.Q<Label>(RowStatsLabel).SetTextSelf($"{ability.ManaCost} MP · {ability.Cooldown:0.##} s");
        }

        private static Button MakeRowButton(string icon, string tooltip) => new Button()
            .SetTooltip(tooltip)
            .AddChild(MakeIcon(LoadIcon(icon)).AddClass(RowIconClass))
            .AddClass(RowButtonClass);

        private void Reload()
        {
            _all.Clear();
            _all.AddRange(AssetDatabase.FindAssets($"t:{nameof(AbilityConfig)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AbilityConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(config => config != null)
                .OrderBy(config => config.AbilityName));
            ApplyFilter(_filter);
        }

        private void ApplyFilter(string filter)
        {
            _filter = filter ?? string.Empty;
            _filtered.Clear();
            _filtered.AddRange(_all.Where(config =>
                config.AbilityName.Contains(_filter, StringComparison.OrdinalIgnoreCase)));

            // A shorter list drops a selection past its end and reports it as a pick of nothing; callers select again.
            _list.SetSelectionWithoutNotify(Array.Empty<int>());
            _list.RefreshItems();
        }

        // Keeps the selection on the same asset after a reload, falling back to the first one. The details are
        // rebuilt only when the shown asset changes, so a field being edited keeps its focus.
        private void Select(AbilityConfig config)
        {
            var index = config == null ? -1 : _filtered.IndexOf(config);
            if (index < 0 && _filtered.Count > 0)
                index = 0;

            var item = index < 0 ? null : _filtered[index];
            _list.SetSelectionWithoutNotify(index < 0 ? Array.Empty<int>() : new[] { index });
            if (item == null || !ReferenceEquals(item, _shown))
                ShowDetails(item);
        }

        private void ShowDetails(AbilityConfig config)
        {
            _details.ClearChildren();
            _shownObject?.Dispose();
            _shownObject = null;
            _shown = config;

            var form = new VisualElement().AddClass(FormClass);
            _details.AddChild(form);

            // Unity's == also catches an asset deleted while it was listed.
            if (config == null)
            {
                form.AddChild(new Label("Select an ability or create one with +.").AddClass(EmptyClass));
                return;
            }

            var serializedObject = new SerializedObject(config);
            _shownObject = serializedObject;

            form
                .AddChild(MakeTitle(config, serializedObject))
                .AddChild(MakeFields(serializedObject))
                .AddChild(MakeEffect(config, serializedObject))
                .AddChild(MakeActions(config, serializedObject));
        }

        // GetDisplayName names the script without "(Script)": "Ability Config".
        private Label MakeTitle(AbilityConfig config, SerializedObject serializedObject)
        {
            var title = new Label(config.AbilityName)
                .SetTooltip($"Double-click to open the {config.GetDisplayName()} script")
                .AddOpenScriptCommand(config)
                .AddClass(TitleClass);

            title.TrackSerializedObjectValue(serializedObject, _ =>
            {
                title.SetTextSelf(config.AbilityName);
                _list.RefreshItems();
            });

            return title;
        }

        private VisualElement MakeFields(SerializedObject serializedObject) => new VisualElement()
            .AddChild(new PropertyField(serializedObject.FindProperty("_abilityName"))
                .AddValueChanged(_ => _list.RefreshItems()))
            .AddChild(new TextField("Description") { multiline = true }
                .SetBindingPath("_description")
                .AddClass(DescriptionClass))
            .AddChild(new PropertyField(serializedObject.FindProperty("_manaCost")))
            .AddChild(new PropertyField(serializedObject.FindProperty("_cooldown")))
            .BindTo(serializedObject);

        // A button drawn as a drop-down that opens the [TypeSelector] picker from code, styled by EffectPicker.uss.
        private VisualElement MakeEffect(AbilityConfig config, SerializedObject serializedObject)
        {
            var effectType = serializedObject.FindProperty("_effectType");

            var effectIcon = MakeIcon(image: null).AddClass(EffectIconClass);
            var effectName = new Label().AddClass(EffectNameClass);
            var effectNote = new Label().AddClass(EffectNoteClass);

            var effectField = new Button()
                .SetFlexGrow(1)
                .AddChild(effectIcon)
                .AddChild(effectName)
                .AddChild(new VisualElement().AddClasses(EnumField.arrowUssClassName, EffectArrowClass))
                .AddClass(EffectClass);

            effectField.AddClicked(() => TypeSelectorWindow.Show(
                screenRect: GUIUtility.GUIToScreenRect(effectField.worldBound),
                filter: new TypeSelectorFilter { Types = new[] { typeof(IAbilityEffect) } },
                assemblyQualifiedName: effectType.stringValue,
                onSelected: aqn =>
                {
                    effectType.SetStringAndApply(aqn ?? string.Empty);
                    Refresh();
                },
                styleSheet: _effectPickerStyle));

            effectField.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            Refresh();

            return new VisualElement()
                .AddChild(new VisualElement()
                    .SetFlexDirection(FlexDirection.Row)
                    .SetAlignItems(Align.Center)
                    .AddChild(new Label("Effect").AddClass(LabelClass))
                    .AddChild(effectField))
                .AddChild(effectNote);

            void Refresh()
            {
                var type = config.EffectType;
                var display = type?.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false);
                var icon = string.IsNullOrEmpty(display?.Icon) ? null : LoadIcon(display.Icon);

                // Describe returns the end of a sentence, "deals damage every 1s", so the note capitalizes it.
                var note = type is null
                    ? string.Empty
                    : ((IAbilityEffect)Activator.CreateInstance(type)).Describe(config);
                if (note.Length > 0)
                    note = char.ToUpperInvariant(note[0]) + note[1..];

                effectField.SetTooltip(display?.Tooltip);
                effectName.SetTextSelf(type is null ? "None" : display?.Name ?? type.Name);

                effectIcon
                    .SetBackgroundImage(new StyleBackground(icon))
                    .SetDisplay(DisplayIf(icon != null));

                effectNote
                    .SetTextSelf(note)
                    .SetDisplay(DisplayIf(note.Length > 0));
            }
        }

        private static VisualElement MakeActions(AbilityConfig config, SerializedObject serializedObject)
        {
            var defaults = ReadDefaults();

            var resetToDefaults = new Button()
                .SetTextSelf("Reset to defaults")
                .SetTooltip("Resets Mana Cost and Cooldown to their default values")
                .AddClicked(() =>
                {
                    // Typed setters apply both values once, so one Undo reverts the reset.
                    serializedObject.Update();
                    serializedObject.FindProperty("_cooldown").SetFloat(defaults.Cooldown);
                    serializedObject.FindProperty("_manaCost").SetIntAndApply(defaults.ManaCost);
                })
                .AddClass(ButtonClass);

            resetToDefaults.TrackSerializedObjectValue(serializedObject, _ => Refresh());
            Refresh();

            return new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .AddChild(resetToDefaults)
                .AddClass(ActionsClass);

            void Refresh() => resetToDefaults.SetDisplay(DisplayIf(
                config.ManaCost != defaults.ManaCost || !Mathf.Approximately(config.Cooldown, defaults.Cooldown)));
        }

        private void CreateAsset()
        {
            var selected = _list.selectedItem as AbilityConfig;
            var folder = selected == null ? "Assets" : GetFolder(AssetDatabase.GetAssetPath(selected));
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/Ability.asset");

            var config = CreateInstance<AbilityConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            // Clear the search so the new "New Ability" is listed and can be selected.
            _search.SetValueWithoutNotify(string.Empty);
            _filter = string.Empty;
            Reload();

            _picked = config;
            Select(config);
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

            Reload();

            // When the shown ability is deleted, the one below takes its place.
            if (wasShown)
                Select(_filtered.Count > 0 ? _filtered[Math.Clamp(index, 0, _filtered.Count - 1)] : null);
            else
                Select(_shown);

            if (ReferenceEquals(config, _picked))
                _picked = _shown;
        }

        private static void SelectAsset(AbilityConfig config)
        {
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

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

        // The sheets lie in the Styles folder next to Scripts, wherever the sample is imported. A renamed Scripts
        // folder leaves the window unstyled but working.
        private StyleSheet LoadStyleSheet(string fileName)
        {
            var script = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var scripts = script.LastIndexOf("/Scripts/", StringComparison.Ordinal);

            return scripts < 0
                ? null
                : AssetDatabase.LoadAssetAtPath<StyleSheet>($"{script[..scripts]}/Styles/{fileName}");
        }

        // Asset paths use '/' on every platform; Path.GetDirectoryName would turn it into '\' on Windows.
        private static string GetFolder(string assetPath) =>
            assetPath[..assetPath.LastIndexOf('/')];

        private static VisualElement MakeIcon(Texture2D image) => new VisualElement()
            .SetBackgroundImage(new StyleBackground(image))
            .SetPickingMode(PickingMode.Ignore);

        private static Texture2D LoadIcon(string name) =>
            EditorGUIUtility.IconContent(name).image as Texture2D;

        private static DisplayStyle DisplayIf(bool visible) =>
            visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
