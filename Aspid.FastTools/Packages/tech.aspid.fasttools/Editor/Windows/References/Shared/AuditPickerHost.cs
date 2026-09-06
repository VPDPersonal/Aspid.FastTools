using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The inline type picker both References tabs dock under a clicked card header: one panel at a time, dropped
    // directly below its anchor inside that anchor's own card, so header, selector and rows read as one active card.
    //
    // The two tabs keep their own USS blocks, so the host wears the class names it is handed. An anchor is expected
    // to end in a chevron, which the host swaps in place rather than rewriting the label, so every band verb keeps
    // its own wording.
    internal sealed class AuditPickerHost
    {
        // The block-specific USS class names the shared picker host wears.
        internal readonly struct PickerClasses
        {
            public readonly string Picker;

            // Welds the panel to the header above it; applied only when the anchor sits inside a card.
            public readonly string PickerAttached;

            // Marks the hosting card as picking, so its divider and hover sweep stand down.
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

        // The host reclaims keyboard focus when the picker closes; fallbackContainer catches an anchor that is ever
        // hosted outside a card.
        public AuditPickerHost(VisualElement host, VisualElement fallbackContainer, in PickerClasses classes)
        {
            _host = host;
            _fallbackContainer = fallbackContainer;
            _classes = classes;
        }

        // The views suspend their keyboard ring while a picker is docked.
        public bool IsOpen => _picker is not null;

        // The close half of a toggle: closes whatever is open and reports whether that was this anchor's own picker,
        // meaning the click was a collapse and the caller should stop.
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

            // The anchor is a direct child of its card, so the panel drops right below it inside the card; the ??
            // fallback keeps a sane target if the anchor is ever hosted outside one.
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

        // Undocks the panel, restores its anchor's chevron and hands keyboard focus back to the host.
        public void Close()
        {
            _picker?.RemoveFromHierarchy();
            if (_anchor is not null) _anchor.Text = _anchor.Text.Replace(ChevronExpanded, ChevronCollapsed);
            _card?.RemoveClass(_classes.CardPicking);

            _picker = null;
            _anchor = null;
            _card = null;

            // The dismissed picker leaves keyboard focus dangling on its (removed) search field; reclaim it so the
            // arrow-key ring keeps working. Guarded — Close also runs from render paths before attach.
            if (_host.panel is not null) _host.Focus();
        }
    }
}
