using System;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SettingsView : VisualElement
    {
        private readonly ScrollView _scroll;

        private readonly NavRing _ring;

        public SettingsView()
        {
            this.AsSurface();

            _scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            AspidSettingsUI.BuildSurfaceContent(_scroll.contentContainer);
            Add(_scroll);

            Add(AspidSettingsUI.BuildResetFooter());

            // ScrollTo rejects the reset footer because it is outside the scroll content.
            _ring = new NavRing(
                host: this,
                navTargetClass: AspidSettingsUI.NavTargetClass,
                focusedClass: AspidSettingsUI.NavTargetFocusedClass,
                scrollTo: element => { if (IsInScrollContent(element)) _scroll.ScrollTo(element); });

            CollectNavTargets(this);
        }

        private bool IsInScrollContent(VisualElement element)
        {
            for (var parent = element.parent; parent != null; parent = parent.parent)
            {
                if (parent == _scroll.contentContainer)
                    return true;
            }

            return false;
        }

        private void CollectNavTargets(VisualElement element)
        {
            switch (element)
            {
                case AspidSwitch toggle:
                    _ring.Register(toggle, () => toggle.value = !toggle.value);
                    return;

                case EnumField dropdown:
                    _ring.Register(dropdown, () => CycleEnum(dropdown));
                    return;

                case SliderInt slider:
                    _ring.Register(
                        slider,
                        activate: null,
                        adjust: delta => slider.value = Mathf.Clamp(slider.value + delta, slider.lowValue, slider.highValue));
                    return;

                case SerializeReferenceExcludedFoldersField folders:
                    foreach (var (target, activate, remove) in folders.GetNavTargets())
                        _ring.Register(target, activate, remove: remove);

                    folders.RowsRebuilt -= RebuildNavTargets;
                    folders.RowsRebuilt += RebuildNavTargets;
                    return;

                case Button button when button.ClassListContains(AspidSettingsUI.ActionClass):
                    _ring.Register(button, () => Submit(button));
                    return;
            }

            foreach (var child in element.Children())
                CollectNavTargets(child);
        }

        private void RebuildNavTargets() =>
            _ring.Rebuild(() => CollectNavTargets(this));

        // Cycle values because the native popup is outside the keyboard ring.
        private static void CycleEnum(EnumField dropdown)
        {
            var values = Enum.GetValues(dropdown.value.GetType());
            var index = Array.IndexOf(values, dropdown.value);
            dropdown.value = (Enum)values.GetValue((index + 1) % values.Length);
        }

        private static void Submit(Button button)
        {
            using var evt = new NavigationSubmitEvent { target = button };
            button.SendEvent(evt);
        }
    }
}
