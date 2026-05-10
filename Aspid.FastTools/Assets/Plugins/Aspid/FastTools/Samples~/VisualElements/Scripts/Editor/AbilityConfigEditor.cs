using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.VisualElements.Editors
{
    [CustomEditor(typeof(AbilityConfig))]
    internal sealed class AbilityConfigEditor : Editor
    {
        private static readonly Color _cardBackground = new(0.16f, 0.17f, 0.19f);
        private static readonly Color _cardBorder = new(0.26f, 0.28f, 0.31f);
        private static readonly Color _dividerColor = new(0.22f, 0.24f, 0.27f);
        private static readonly Color _titleColor = new(0.93f, 0.93f, 0.94f);
        private static readonly Color _subtleColor = new(0.58f, 0.61f, 0.66f);
        private static readonly Color _accent = new(0.42f, 0.69f, 1.00f);
        private static readonly Color _warning = new(1.00f, 0.76f, 0.30f);

        public override VisualElement CreateInspectorGUI()
        {
            var config = (AbilityConfig)target;

            var statusBadge = new Label()
                .SetFontSize(10)
                .SetUnityFontStyleAndWeight(FontStyle.Bold)
                .SetUnityTextAlign(TextAnchor.MiddleCenter)
                .SetLetterSpacing(1)
                .SetPadding(top: 3, bottom: 3, left: 10, right: 10)
                .SetBorderRadius(10)
                .SetBorderWidth(1);

            var titles = new VisualElement()
                .SetFlexGrow(1)
                .AddChild(new Label(target.GetScriptName())
                    .SetColor(_titleColor)
                    .SetFontSize(15)
                    .SetUnityFontStyleAndWeight(FontStyle.Bold))
                .AddChild(new Label("ABILITY CONFIGURATION")
                    .SetColor(_subtleColor)
                    .SetFontSize(10)
                    .SetLetterSpacing(2)
                    .SetMarginTop(3));

            var header = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetAlignItems(Align.Center)
                .SetPadding(top: 12, bottom: 12, left: 14, right: 14)
                .AddChild(titles)
                .AddChild(statusBadge);

            var divider = new VisualElement()
                .SetHeight(1)
                .SetBackgroundColor(_dividerColor);

            var helpBox = new HelpBox()
                .SetText("This ability costs no mana — is that intentional?")
                .SetMessageType(HelpBoxMessageType.Warning)
                .SetMarginTop(8)
                .SetBorderRadius(6);

            var manaCostField = new PropertyField(serializedObject.FindProperty("_manaCost"))
                .AddValueChanged(_ => UpdateState());

            var body = new VisualElement()
                .SetPadding(top: 12, bottom: 12, left: 14, right: 14)
                .AddChild(new PropertyField(serializedObject.FindProperty("_abilityName")))
                .AddChild(new PropertyField(serializedObject.FindProperty("_description")))
                .AddChild(new PropertyField(serializedObject.FindProperty("_cooldown")))
                .AddChild(manaCostField)
                .AddChild(helpBox);

            var card = new VisualElement()
                .SetBackgroundColor(_cardBackground)
                .SetBorderColor(_cardBorder)
                .SetBorderWidth(1)
                .SetBorderRadius(10)
                .AddChild(header)
                .AddChild(divider)
                .AddChild(body);

            UpdateState();
            return card;

            void UpdateState()
            {
                var manaCost = config.ManaCost;
                var isFree = manaCost is 0;
                var color = isFree ? _warning : _accent;

                statusBadge
                    .SetText(isFree ? "FREE" : $"{manaCost} MP")
                    .SetColor(color)
                    .SetBorderColor(color);

                helpBox.SetDisplay(isFree ? DisplayStyle.Flex : DisplayStyle.None);
            }
        }
    }
}
