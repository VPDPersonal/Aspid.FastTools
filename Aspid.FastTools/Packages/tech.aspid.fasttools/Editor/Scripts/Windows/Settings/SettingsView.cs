using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SettingsView : VisualElement
    {
        private readonly ScrollView _scroll;

        private readonly NavRing _ring;

        private bool _openingPicker;

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
                    _ring.Register(
                        dropdown,
                        activate: null,
                        adjust: delta => StepEnum(dropdown, delta, ConfirmStep));
                    return;

                case SliderInt slider:
                    _ring.Register(
                        slider,
                        activate: null,
                        adjust: delta => slider.value = Mathf.Clamp(slider.value + delta, slider.lowValue, slider.highValue));
                    return;

                case ObjectField field:
                    _ring.Register(field, () => OpenPicker(field));
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

        // The field opens its picker on Enter only while it holds the focus, so it gets the focus for that one key.
        private void OpenPicker(ObjectField field)
        {
            // The key sent below bubbles up to the ring of this view, which would open the picker again.
            if (_openingPicker) return;

            var display = field.Q(className: ObjectField.objectUssClassName);
            if (display is null) return;

            _openingPicker = true;
            try
            {
                display.Focus();

                using var evt = KeyDownEvent.GetPooled('\n', KeyCode.Return, EventModifiers.None);
                evt.target = display;
                display.SendEvent(evt);
            }
            finally
            {
                _openingPicker = false;

                // The field would take the Enter meant for the next row.
                Focus();
            }
        }

        // The native popup is outside the keyboard ring, so arrows step the value.
        // Stepping does not wrap, and a value with team-wide effect asks first.
        internal static void StepEnum(EnumField dropdown, int delta, Func<Enum, bool> confirm)
        {
            var values = Enum.GetValues(dropdown.value.GetType());
            var index = Mathf.Clamp(Array.IndexOf(values, dropdown.value) + delta, 0, values.Length - 1);
            var next = (Enum)values.GetValue(index);

            if (Equals(next, dropdown.value)) return;
            if (NeedsConfirmation(next) && !confirm(next)) return;

            dropdown.value = next;
        }

        // Fail breaks the team's builds and Off silently skips the check.
        internal static bool NeedsConfirmation(Enum value) =>
            value is GateSeverity.Fail or GateSeverity.Off;

        private static bool ConfirmStep(Enum next)
        {
            var effect = next is GateSeverity.Fail
                ? "Fail aborts the player build and fails the CI job when the check finds a violation."
                : "Off never checks, so missing types reach the build and the CI job without a warning.";

            return EditorUtility.DisplayDialog(
                title: $"Build / CI gate: {next}",
                message: effect
                    + "\n\nThis edits the committed ProjectSettings asset, so it affects the whole team once committed.",
                ok: $"Set to {next}",
                cancel: "Cancel");
        }

        private static void Submit(Button button)
        {
            using var evt = new NavigationSubmitEvent { target = button };
            button.SendEvent(evt);
        }
    }
}
