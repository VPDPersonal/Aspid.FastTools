#if !UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidLabel
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidLabel, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _text = new() { name = "text" };
            private readonly UxmlBoolAttributeDescription _selectable = new() { name = "selectable" };
            private readonly UxmlEnumAttributeDescription<ThemeStyle.Type> _lineTheme = new() { name = "line-theme" };
            private readonly UxmlEnumAttributeDescription<ThemeStyle.Type> _labelTheme = new() { name = "label-theme" };
            private readonly UxmlEnumAttributeDescription<StatusStyle.Type> _lineStatus = new() { name = "line-status" };
            private readonly UxmlEnumAttributeDescription<StatusStyle.Type> _labelStatus = new() { name = "label-status" };
            private readonly UxmlEnumAttributeDescription<FontStyle> _labelFontStyle = new() { name = "label-font-style" };
            private readonly UxmlEnumAttributeDescription<AspidLabelSizeStyle.Type> _labelSize = new() { name = "label-size" };
            private readonly UxmlEnumAttributeDescription<AspidDividingLineSizeStyle.Type> _lineSize = new() { name = "line-size" };
            private readonly UxmlEnumAttributeDescription<AspidDividingLineDirectionStyle.Type> _lineDirection = new() { name = "line-direction" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidLabel)ve;

                var text = element.Text;
                if (_text.TryGetValueFromBag(bag, cc: cc, value: ref text))
                    element.Text = text;

                var selectable = element.Selectable;
                if (_selectable.TryGetValueFromBag(bag, cc: cc, value: ref selectable))
                    element.Selectable = selectable;

                var labelTheme = element.LabelTheme;
                if (_labelTheme.TryGetValueFromBag(bag, cc: cc, value: ref labelTheme))
                    element.LabelTheme = labelTheme;

                var labelStatus = element.LabelStatus;
                if (_labelStatus.TryGetValueFromBag(bag, cc: cc, value: ref labelStatus))
                    element.LabelStatus = labelStatus;

                var labelSize = element.LabelSize;
                if (_labelSize.TryGetValueFromBag(bag, cc: cc, value: ref labelSize))
                    element.LabelSize = labelSize;

                var labelFontStyle = element.LabelFontStyle;
                if (_labelFontStyle.TryGetValueFromBag(bag, cc: cc, value: ref labelFontStyle))
                    element.LabelFontStyle = labelFontStyle;

                var lineTheme = element.LineTheme;
                if (_lineTheme.TryGetValueFromBag(bag, cc: cc, value: ref lineTheme))
                    element.LineTheme = lineTheme;

                var lineStatus = element.LineStatus;
                if (_lineStatus.TryGetValueFromBag(bag, cc: cc, value: ref lineStatus))
                    element.LineStatus = lineStatus;

                var lineSize = element.LineSize;
                if (_lineSize.TryGetValueFromBag(bag, cc: cc, value: ref lineSize))
                    element.LineSize = lineSize;

                var lineDirection = element.LineDirection;
                if (_lineDirection.TryGetValueFromBag(bag, cc: cc, value: ref lineDirection))
                    element.LineDirection = lineDirection;
            }
        }
    }
}
#endif
