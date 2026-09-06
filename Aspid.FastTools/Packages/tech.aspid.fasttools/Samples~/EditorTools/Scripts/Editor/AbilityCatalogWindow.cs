using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools.Editors
{
    // A two-pane editor window: every AbilityConfig in the project on the left, the selected one on the right.
    // Built entirely in code with the fluent extensions; edits go through SerializedProperty setters and
    // UI Toolkit binding, so Undo and dirty tracking work as in the Inspector.
    internal sealed class AbilityCatalogWindow : EditorWindow
    {
        private readonly List<AbilityConfig> _all = new();
        private readonly List<AbilityConfig> _filtered = new();

        private ListView _list;
        private VisualElement _details;
        private string _filter = string.Empty;

        [MenuItem("Tools/Aspid 🐍/FastTools/Samples/Ability Catalog")]
        private static void Open() =>
            GetWindow<AbilityCatalogWindow>("Ability Catalog").minSize = new Vector2(560, 320);

        private void CreateGUI()
        {
            Reload();

            var search = new TextField()
                .SetFlexGrow(1)
                .AddValueChanged<TextField, string>(evt => ApplyFilter(evt.newValue));
            // TextField implements ITextEdition explicitly, so the edition setters go through textEdition.
            search.textEdition.SetPlaceholder("Search abilities…");

            var create = new Button()
                .SetText("Create")
                .SetTooltip("Creates a new AbilityConfig asset next to the selected one")
                .AddClicked(CreateAsset);

            var toolbar = new VisualElement()
                .SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                .SetPaddingX(6).SetPaddingY(4)
                .AddChild(search)
                .AddChild(create);

            _list = new ListView()
                .SetItemsSource(_filtered)
                .SetFixedItemHeight(22)
                .SetSelectionType(SelectionType.Single)
                .SetShowAlternatingRowBackgrounds(AlternatingRowBackground.ContentOnly)
                .SetMakeItem(() => new Label().SetPaddingX(6).SetUnityTextAlign(TextAnchor.MiddleLeft))
                .SetBindItem((element, index) => ((Label)element).SetText(_filtered[index].AbilityName))
                .AddSelectionChanged(selection => ShowDetails(selection.FirstOrDefault() as AbilityConfig))
                .SetFlexGrow(1);

            var left = new VisualElement()
                .SetWidth(200)
                .SetBorderColor(new Color(0.2f, 0.2f, 0.2f)).SetBorderWidth(right: 1)
                .AddChild(toolbar)
                .AddChild(_list);

            _details = new VisualElement().SetFlexGrow(1).SetPaddingX(12).SetPaddingY(8);

            rootVisualElement
                .SetFlexDirection(FlexDirection.Row)
                .AddChild(left)
                .AddChild(_details);

            ShowDetails(null);
        }

        private void Reload()
        {
            _all.Clear();
            _all.AddRange(AssetDatabase.FindAssets($"t:{nameof(AbilityConfig)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AbilityConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .OrderBy(config => config.AbilityName));
            ApplyFilter(_filter);
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

            if (config is null)
            {
                _details.AddChild(new HelpBox("Select an ability, or press Create.", HelpBoxMessageType.Info));
                return;
            }

            var serializedObject = new SerializedObject(config);
            var effectType = serializedObject.FindProperty("_effectType");

            var effectLabel = new Label().SetFlexGrow(1);
            var effectButton = new Button().SetText("Change…");

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
                .SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center).SetMarginTop(8)
                .AddChild(new Label("Effect").SetWidth(120))
                .AddChild(effectLabel)
                .AddChild(effectButton);

            // A one-click balance pass: chainable typed setters, Undo included, applied once at the end.
            var halveCooldown = new Button()
                .SetText("Halve cooldown, +5 MP")
                .AddClicked(() =>
                {
                    serializedObject.Update();
                    var cooldown = serializedObject.FindProperty("_cooldown");
                    var manaCost = serializedObject.FindProperty("_manaCost");
                    cooldown.SetFloat(cooldown.floatValue * 0.5f);
                    manaCost.SetIntAndApply(manaCost.intValue + 5);
                });

            _details
                .AddChild(new Label(config.GetScriptName())
                    .SetFontSize(15).AddBoldUnityFontStyleAndWeight().SetMarginBottom(6)
                    .SetTooltip("Double-click to open the script")
                    .AddOpenScriptCommand(config))
                // BindTo(SerializedObject) binds every PropertyField below in one call.
                .AddChild(new VisualElement()
                    .AddChild(new PropertyField(serializedObject.FindProperty("_abilityName")).AddValueChanged(_ => _list.RefreshItems()))
                    .AddChild(new PropertyField(serializedObject.FindProperty("_description")))
                    .AddChild(new PropertyField(serializedObject.FindProperty("_manaCost")))
                    .AddChild(new PropertyField(serializedObject.FindProperty("_cooldown")))
                    .BindTo(serializedObject))
                .AddChild(effectRow)
                .AddChild(halveCooldown.SetMarginTop(12).SetAlignSelf(Align.FlexStart))
                .AddChild(new Button().SetText("Select asset").SetAlignSelf(Align.FlexStart)
                    .AddClicked(() => Selection.activeObject = config));

            RefreshEffect();

            void RefreshEffect()
            {
                var type = config.EffectType;
                var description = type is null ? "none" : ((IAbilityEffect)Activator.CreateInstance(type)).Describe(config);
                effectLabel.SetText(type is null ? "<None>" : $"{type.Name} — {description}");
            }
        }

        private void CreateAsset()
        {
            var selected = _list.selectedItem as AbilityConfig;
            var folder = selected is null ? "Assets" : System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(selected));
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/Ability.asset");

            var config = CreateInstance<AbilityConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            Reload();
            _list.SetSelection(_filtered.IndexOf(config));
        }
    }
}
