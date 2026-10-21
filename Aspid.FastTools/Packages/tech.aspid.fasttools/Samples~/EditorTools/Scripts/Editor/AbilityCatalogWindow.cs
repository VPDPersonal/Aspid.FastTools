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
using Aspid.FastTools.Samples.EditorTools.Effects;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools.Editors
{
    internal sealed class AbilityCatalogWindow : EditorWindow
    {
        private readonly List<AbilityConfig> _abilities = new();

        private ListView _list;
        private TextField _search;
        private VisualElement _details;
        private SerializedObject _shown;
        private StyleSheet _effectPickerStyle;

        [MenuItem("Tools/Aspid 🐍/FastTools/Samples/Ability Catalog")]
        private static void Open() =>
            GetWindow<AbilityCatalogWindow>("Ability Catalog").minSize = new Vector2(x: 640, y: 420);

        private void CreateGUI()
        {
            _effectPickerStyle = LoadStyleSheet("EffectPicker.uss");

            _search = new TextField()
                .SetFlexGrow(1)
                .SetTooltip("Search by name")
                .AddValueChanged(_ => Reload())
                .AddChild(MakeIcon("d_Search Icon").AddClass("ability-search__icon"))
                .AddClass("ability-search");

            _list = new ListView()
                .SetItemsSource(_abilities)
                .SetFixedItemHeight(60)
                .SetShowAlternatingRowBackgrounds(AlternatingRowBackground.None)
                .SetMakeItem(MakeRow)
                .SetBindItem(BindRow)
                .AddSelectionChanged(selection => ShowDetails(selection.FirstOrDefault() as AbilityConfig))
                .SetFlexGrow(1);

            _details = new ScrollView()
                .SetFlexGrow(1)
                .AddClass("ability-details");

            var create = new Button()
                .SetTextSelf("+")
                .SetTooltip("Create an ability")
                .AddClicked(CreateAsset)
                .AddClasses("ability-button", "ability-button--primary", "ability-button--icon");

            var sidebar = new VisualElement()
                .SetWidth(260)
                .SetFlexShrink(0)
                .AddChild(new VisualElement()
                    .SetFlexDirection(FlexDirection.Row)
                    .SetAlignItems(Align.Center)
                    .AddChild(_search)
                    .AddChild(create)
                    .AddClass("ability-toolbar"))
                .AddChild(_list)
                .AddClass("ability-sidebar");

            rootVisualElement
                .AddAspidWindowTheme()
                .AddStyleSheet(LoadStyleSheet("AbilityCatalog.uss"))
                .AddClass("ability-catalog")
                .SetFlexDirection(FlexDirection.Row)
                .AddChild(sidebar)
                .AddChild(_details);

            Reload();
        }

        private void OnProjectChange()
        {
            if (_list is not null)
                Reload();
        }

        private void OnDestroy() =>
            _shown?.Dispose();

        private void BindRow(VisualElement row, int index)
        {
            var ability = _abilities[index];
            row.userData = ability;
            row.Q<Label>("abilityName").SetTextSelf(ability.AbilityName);
            row.Q<Label>("abilityStats").SetTextSelf($"{ability.ManaCost} MP · {ability.Cooldown:0.##} s");
        }

        private static Button MakeRowButton(string icon, string tooltip) => new Button()
            .SetTooltip(tooltip)
            .AddChild(MakeIcon(icon).AddClass("ability-row__icon"))
            .AddClass("ability-row__button");

        private void Reload(AbilityConfig select = null)
        {
            select ??= _list.selectedItem as AbilityConfig;

            _abilities.Clear();
            _abilities.AddRange(AssetDatabase.FindAssets($"t:{nameof(AbilityConfig)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AbilityConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(ability => ability != null
                    && ability.AbilityName.Contains(
                        value: _search.value,
                        comparisonType: StringComparison.OrdinalIgnoreCase))
                .OrderBy(ability => ability.AbilityName, StringComparer.Ordinal));
            _list.RefreshItems();

            var index = Math.Max(val1: 0, val2: _abilities.IndexOf(select));

            var shown = _abilities.Count == 0
                ? null
                : _abilities[index];

            _list.SetSelectionWithoutNotify(shown == null
                ? Array.Empty<int>()
                : new[] { index });

            if (shown == null || !ReferenceEquals(shown, _shown?.targetObject))
                ShowDetails(shown);
        }

        private void ShowDetails(AbilityConfig ability)
        {
            _details.ClearChildren();
            _shown?.Dispose();
            _shown = null;

            var form = new VisualElement().AddClass("ability-form");
            _details.AddChild(form);

            if (ability == null)
            {
                form.AddChild(new Label("Select an ability or create one with +.").AddClass("ability-empty"));
                return;
            }

            _shown = new SerializedObject(ability);

            var detailsTitle = new Label(ability.AbilityName)
                .SetTooltip($"Double-click to open the {ability.GetDisplayName()} script")
                .AddOpenScriptCommand(ability)
                .AddClass("ability-title");

            detailsTitle.TrackSerializedObjectValue(obj: _shown, callback: _ =>
            {
                detailsTitle.SetTextSelf(ability.AbilityName);
                _list.RefreshItems();
            });

            form
                .AddChild(detailsTitle)
                .AddChild(new VisualElement()
                    .AddChild(new PropertyField(_shown.FindProperty("_abilityName")))
                    .AddChild(new TextField("Description") { multiline = true }
                        .SetBindingPath("_description")
                        .AddClass("ability-description"))
                    .AddChild(new PropertyField(_shown.FindProperty("_manaCost")))
                    .AddChild(new PropertyField(_shown.FindProperty("_cooldown")))
                    .BindTo(_shown))
                .AddChild(MakeEffect(ability, _shown))
                .AddChild(MakeReset(ability, _shown));
        }

        private VisualElement MakeEffect(AbilityConfig ability, SerializedObject serializedObject)
        {
            var effectType = serializedObject.FindProperty("_effectType");
            var icon = new VisualElement().SetPickingMode(PickingMode.Ignore).AddClass("ability-effect__icon");
            var effectName = new Label().AddClass("ability-effect__name");
            var note = new Label().AddClass("ability-effect__note");

            var field = new Button()
                .SetFlexGrow(1)
                .AddChild(icon)
                .AddChild(effectName)
                .AddChild(new VisualElement().AddClasses(EnumField.arrowUssClassName, "ability-effect__arrow"))
                .AddClass("ability-effect");

            field.AddClicked(() => TypeSelectorWindow.Show(
                screenRect: GUIUtility.GUIToScreenRect(field.worldBound),
                filter: new TypeSelectorFilter
                {
                    Types = new[] { typeof(IAbilityEffect) }
                },
                assemblyQualifiedName: effectType.stringValue,
                onSelected: picked => effectType.SetStringAndApply(picked ?? string.Empty),
                styleSheet: _effectPickerStyle));

            field.TrackSerializedObjectValue(obj: serializedObject, callback: _ => Refresh());
            Refresh();

            return new VisualElement()
                .AddChild(new VisualElement()
                    .SetFlexDirection(FlexDirection.Row)
                    .SetAlignItems(Align.Center)
                    .AddChild(new Label("Effect").AddClass("ability-label"))
                    .AddChild(field))
                .AddChild(note);

            void Refresh()
            {
                var type = ability.EffectType;
                var display = type?.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false);
                var description = type is null
                    ? string.Empty
                    : ((IAbilityEffect)Activator.CreateInstance(type)).Describe(ability);

                effectName.SetTextSelf(type is null
                    ? "None"
                    : display?.Name ?? type.Name);

                icon.SetBackgroundImage(LoadIcon(display?.Icon))
                    .SetDisplay(string.IsNullOrEmpty(display?.Icon)
                        ? DisplayStyle.None
                        : DisplayStyle.Flex);

                note.SetTextSelf(description.Length > 0
                        ? char.ToUpperInvariant(description[0]) + description[1..]
                        : "")
                    .SetDisplay(description.Length > 0
                        ? DisplayStyle.Flex
                        : DisplayStyle.None);
            }
        }

        private static VisualElement MakeReset(AbilityConfig ability, SerializedObject serializedObject)
        {
            var reset = new Button()
                .SetTextSelf("Reset to defaults")
                .SetTooltip("Resets Mana Cost and Cooldown to their default values")
                .AddClicked(() =>
                {
                    serializedObject.FindProperty("_cooldown").Update().SetFloat(AbilityConfig.DefaultCooldown);
                    serializedObject.FindProperty("_manaCost").SetIntAndApply(AbilityConfig.DefaultManaCost);
                })
                .AddClass("ability-button");

            reset.TrackSerializedObjectValue(obj: serializedObject, callback: _ => Refresh());
            Refresh();

            return new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .AddChild(reset)
                .AddClass("ability-actions");

            void Refresh()
            {
                var isDefault =
                    ability.ManaCost == AbilityConfig.DefaultManaCost
                    && Mathf.Approximately(a: ability.Cooldown, b: AbilityConfig.DefaultCooldown);

                reset.SetDisplay(isDefault
                    ? DisplayStyle.None
                    : DisplayStyle.Flex);
            }
        }

        private void CreateAsset()
        {
            var ability = CreateInstance<AbilityConfig>();
            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/Ability.asset");

            AssetDatabase.CreateAsset(asset: ability, path: path);
            _search.SetValueWithoutNotify(string.Empty);

            Reload(ability);
        }

        private StyleSheet LoadStyleSheet(string fileName)
        {
            var script = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var root = script[..script.LastIndexOf(value: "/Scripts/", comparisonType: StringComparison.Ordinal)];

            return AssetDatabase.LoadAssetAtPath<StyleSheet>($"{root}/Styles/{fileName}");
        }

        private static VisualElement MakeRow()
        {
            var row = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .AddClass("ability-row");

            var selectAsset = MakeRowButton(icon: "d_Project", tooltip: "Select the asset in the Project window")
                .AddClicked(() =>
                {
                    Selection.activeObject = (AbilityConfig)row.userData;
                    EditorGUIUtility.PingObject(Selection.activeObject);
                });

            var delete = MakeRowButton(icon: "d_clear", tooltip: "Move the asset to the trash")
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

        private static void DeleteAsset(AbilityConfig ability)
        {
            var path = AssetDatabase.GetAssetPath(ability);
            var isDisplay = EditorUtility.DisplayDialog(
                title: "Delete ability",
                message: $"Move {ability.AbilityName} to the trash?\n\n{path}",
                ok: "Delete",
                cancel: "Cancel");

            if (isDisplay)
                AssetDatabase.MoveAssetToTrash(path);
        }

        private static VisualElement MakeIcon(string name) => new VisualElement()
            .SetBackgroundImage(LoadIcon(name))
            .SetPickingMode(PickingMode.Ignore);

        private static Texture2D LoadIcon(string name) => string.IsNullOrWhiteSpace(name)
                ? null
                : EditorGUIUtility.IconContent(name).image as Texture2D;
    }
}
