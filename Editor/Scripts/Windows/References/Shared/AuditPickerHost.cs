using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class AuditPickerHost
    {
        internal readonly struct PickerClasses
        {
            public readonly string Picker;

            public readonly string PickerAttached;

            public readonly string CardPicking;

            public PickerClasses(string picker, string pickerAttached, string cardPicking)
            {
                Picker = picker;
                PickerAttached = pickerAttached;
                CardPicking = cardPicking;
            }
        }

        private const char ChevronCollapsed = '▼';
        private const char ChevronExpanded = '▲';

        private readonly VisualElement _host;
        private readonly VisualElement _fallbackContainer;
        private readonly PickerClasses _classes;

        private VisualElement _picker;
        private AspidGradientButton _anchor;
        private VisualElement _card;

        public AuditPickerHost(VisualElement host, VisualElement fallbackContainer, in PickerClasses classes)
        {
            _host = host;
            _fallbackContainer = fallbackContainer;
            _classes = classes;
        }

        public bool IsOpen => _picker is not null;

        public bool ToggleClosed(AspidGradientButton anchor)
        {
            var wasOpen = _anchor == anchor;
            Close();
            return wasOpen;
        }

        public void Open(AspidGradientButton anchor, TypeSelectorView content)
        {
            _picker = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(_classes.Picker)
                .AddChild(content);

            _anchor = anchor;
            if (anchor is not null) anchor.Text = anchor.Text.Replace(ChevronCollapsed, ChevronExpanded);

            var card = anchor?.parent;
            var container = card ?? _fallbackContainer;
            container.InsertChild(container.IndexOf(anchor) + 1, _picker);

            if (card is not null)
            {
                _card = card;
                _card.AddClass(_classes.CardPicking);
                _picker.AddClass(_classes.PickerAttached);
            }

            content.FocusPicker();
        }

        public void Close()
        {
            _picker?.RemoveFromHierarchy();
            if (_anchor is not null) _anchor.Text = _anchor.Text.Replace(ChevronExpanded, ChevronCollapsed);
            _card?.RemoveClass(_classes.CardPicking);

            _picker = null;
            _anchor = null;
            _card = null;

            // The removed search field retains focus; restore the host only after it has a panel.
            if (_host.panel is not null) _host.Focus();
        }
    }
}
