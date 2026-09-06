using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools.Editors
{
    // A card-style inspector built with the fluent extensions. Every Set*/Add* returns the element, so the
    // whole tree is one expression; the badge and the help box react to Mana Cost edits.
    [CustomEditor(typeof(AbilityConfig))]
    internal sealed class AbilityConfigEditor : Editor
    {
        private static readonly Color _border = new(0.26f, 0.28f, 0.31f);
        private static readonly Color _accent = new(0.42f, 0.69f, 1f);
        private static readonly Color _warning = new(1f, 0.76f, 0.3f);

        public override VisualElement CreateInspectorGUI()
        {
            var config = (AbilityConfig)target;

            var badge = new Label()
                .SetFontSize(10)
                .AddBoldUnityFontStyleAndWeight()
                .SetUnityTextAlign(TextAnchor.MiddleCenter)
                .SetPaddingX(10).SetPaddingY(3)
                .SetBorderRadius(10).SetBorderWidth(1);

            var helpBox = new HelpBox("This ability costs no mana. Intentional?", HelpBoxMessageType.Warning)
                .SetMarginTop(8);

            // GetScriptName honors [AddComponentMenu]; AddOpenScriptCommand opens the script on double-click.
            var title = new Label(target.GetScriptName())
                .SetFlexGrow(1).SetFontSize(14)
                .AddBoldUnityFontStyleAndWeight()
                .SetTooltip("Double-click to open the script")
                .AddOpenScriptCommand(target);

            var header = new VisualElement()
                .SetFlexDirection(FlexDirection.Row).SetAlignItems(Align.Center)
                .SetPaddingX(12).SetPaddingY(10)
                .SetBorderColor(_border).SetBorderWidth(bottom: 1)
                .AddChild(title)
                .AddChild(badge);

            var body = new VisualElement()
                .SetPaddingX(12).SetPaddingY(10)
                .AddChild(new PropertyField(serializedObject.FindProperty("_abilityName")))
                .AddChild(new PropertyField(serializedObject.FindProperty("_description")))
                .AddChild(new PropertyField(serializedObject.FindProperty("_cooldown")))
                .AddChild(new PropertyField(serializedObject.FindProperty("_manaCost")).AddValueChanged(_ => Refresh()))
                .AddChild(new PropertyField(serializedObject.FindProperty("_effectType")))
                .AddChild(helpBox);

            Refresh();
            return new VisualElement()
                .SetBorderColor(_border).SetBorderWidth(1).SetBorderRadius(8)
                .AddChild(header)
                .AddChild(body);

            void Refresh()
            {
                var isFree = config.ManaCost is 0;
                var color = isFree ? _warning : _accent;
                badge.SetText(isFree ? "FREE" : $"{config.ManaCost} MP").SetColor(color).SetBorderColor(color);
                helpBox.SetDisplay(isFree ? DisplayStyle.Flex : DisplayStyle.None);
            }
        }
    }
}
