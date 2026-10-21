using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors;

namespace Aspid.FastTools.Samples.EditorTools.Editors
{
    internal sealed class AbilityCatalogWindow : EditorWindow
    {
        private ListView _list;
        private VisualElement _details;
        private SerializedObject _ability;

        [MenuItem("Tools/Aspid 🐍/FastTools/Samples/Ability Catalog")]
        private static void Open() => GetWindow<AbilityCatalogWindow>("Ability Catalog");

        private void CreateGUI()
        {
            var abilities = AssetDatabase.FindAssets($"t:{nameof(AbilityConfig)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<AbilityConfig>)
                .ToList();

            _list = new ListView()
                .SetItemsSource(abilities)
                .SetMakeItem(() => new Label())
                .SetBindItem((row, index) => ((Label)row).SetTextSelf(Describe(abilities[index])))
                .AddSelectionChanged(selection => ShowDetails(selection.FirstOrDefault() as AbilityConfig))
                .SetWidth(220)
                .AddClass("ability-list");

            _details = new VisualElement()
                .SetFlexGrow(1)
                .AddClass("ability-details");

            rootVisualElement
                .AddAspidWindowTheme()
                .AddStyleSheetFromResources("AbilityCatalog")
                .AddClass("ability-catalog")
                .SetFlexDirection(FlexDirection.Row)
                .AddChild(_list)
                .AddChild(_details);

            _list.SetSelection(index: 0);
        }

        private void OnEnable() => Undo.undoRedoPerformed += RefreshRows;

        private void OnDisable() => Undo.undoRedoPerformed -= RefreshRows;

        private void OnDestroy() => _ability?.Dispose();

        private void RefreshRows() => _list?.RefreshItems();

        private static string Describe(AbilityConfig ability) =>
            $"{ability.AbilityName}  ·  {ability.ManaCost} MP  ·  {ability.Cooldown:0.#} s";

        private void ShowDetails(AbilityConfig ability)
        {
            _details.ClearChildren();
            _ability?.Dispose();
            _ability = null;

            if (ability == null)
                return;

            _ability = new SerializedObject(ability);
            var manaCost = _ability.FindProperty("_manaCost");
            var cooldown = _ability.FindProperty("_cooldown");

            var form = new VisualElement()
                .AddChild(new Label()
                    .SetBindingPath("_abilityName")
                    .SetTooltip($"Double-click to open the {ability.GetDisplayName()} script")
                    .AddOpenScriptCommand(ability)
                    .AddClass("ability-title"))
                .AddChild(new PropertyField(_ability.FindProperty("_abilityName")))
                .AddChild(new PropertyField(manaCost))
                .AddChild(new PropertyField(cooldown))
                .AddChild(new TypeField(_ability.FindProperty("_effectType")) { Types = new[] { typeof(IAbilityEffect) } })
                .AddChild(new Button()
                    .SetTextSelf("Reset to defaults")
                    .AddClicked(() =>
                    {
                        cooldown.Update().SetFloat(AbilityConfig.DefaultCooldown);
                        manaCost.SetIntAndApply(AbilityConfig.DefaultManaCost);
                    }))
                .BindTo(_ability);

            form.TrackSerializedObjectValue(obj: _ability, callback: _ => RefreshRows());
            _details.AddChild(form);
        }
    }
}
