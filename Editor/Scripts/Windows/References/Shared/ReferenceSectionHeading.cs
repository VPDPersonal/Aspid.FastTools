using System;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The heading of a group of cards in Asset References and Project References: a title in the colour of its cards
    // and a thin line under it. The rules are in the theme sheet, so both views draw it the same. An object heading sits
    // a level above the status sections inside it, so it is larger, neutral and carries the object's icon.
    internal sealed class ReferenceSectionHeading : VisualElement
    {
        private const string BlockClass = "aspid-fasttools-section-heading";
        private const string ClickableClass = BlockClass + "--clickable";
        private const string ObjectClass = BlockClass + "--object";
        private const string IconClass = BlockClass + "__icon";
        private const string RowClass = BlockClass + "__row";
        private const string TitleClass = BlockClass + "__title";
        private const string TrailingClass = BlockClass + "__trailing";
        private const string LineClass = BlockClass + "__line";

        private readonly Label _trailing;

        // Text at the right end of the row, such as the chevron of a heading that folds its cards.
        public string TrailingText
        {
            get => _trailing.text;
            set => _trailing.text = value;
        }

        public ReferenceSectionHeading(string title, StatusStyle.Type status)
            : this(title, status, AspidLabelSizeStyle.Type.H5) { }

        private ReferenceSectionHeading(string title, StatusStyle.Type status, AspidLabelSizeStyle.Type size)
        {
            AddToClassList(BlockClass);

            var preset = AspidLabelPreset.Default
                .SetLabelSize(size)
                .SetLineSize(AspidDividingLineSizeStyle.Type.None);
            preset = status == StatusStyle.Type.None
                ? preset.SetLabelTheme(ThemeStyle.Type.Lightness)
                : preset.SetLabelStatus(status);

            var row = new VisualElement()
                .AddClass(RowClass)
                .SetPickingMode(PickingMode.Ignore)
                .AddChild(new AspidLabel(title, preset)
                    .AddClass(TitleClass)
                    .SetPickingMode(PickingMode.Ignore));

            _trailing = new Label(string.Empty)
                .AddClass(TrailingClass)
                .SetPickingMode(PickingMode.Ignore);

            this.AddChild(row.AddChild(_trailing))
                .AddChild(new AspidDividingLine(AspidDividingLinePreset.Default
                        .SetTheme(ThemeStyle.Type.Dark)
                        .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                    .AddClass(LineClass)
                    .SetPickingMode(PickingMode.Ignore));
        }

        // The heading of an object: neutral and one size up, with the object's icon before its name.
        public static ReferenceSectionHeading ForObject(string title, Texture2D icon)
        {
            var heading = new ReferenceSectionHeading(title, StatusStyle.Type.None, AspidLabelSizeStyle.Type.H4);
            heading.AddToClassList(ObjectClass);

            if (icon != null)
            {
                var iconElement = new VisualElement()
                    .AddClass(IconClass)
                    .SetPickingMode(PickingMode.Ignore);
                iconElement.style.backgroundImage = new StyleBackground(icon);

                var row = heading.Q(className: RowClass);
                row.Insert(0, iconElement);
            }

            return heading;
        }

        // Makes the whole heading a button, for a heading that folds or opens its section.
        public ReferenceSectionHeading SetOnClick(Action onClick)
        {
            AddToClassList(ClickableClass);
            this.AddManipulator(new Clickable(onClick));
            return this;
        }
    }
}
