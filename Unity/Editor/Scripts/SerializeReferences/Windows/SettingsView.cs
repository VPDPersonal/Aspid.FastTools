using System;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The Settings tab (the rightmost square tab): the package's settings surfaced inside the window so the toolset is
    /// self-contained. The one surface showing BOTH storage scopes — the Unity-native pages each render only their
    /// own scope (Preferences the per-user controls, Project Settings the shared ones) from the same definitions.
    /// A thin host over the shared settings surface — <see cref="AspidSettingsUI.BuildSurfaceContent"/> composes the
    /// scope legend and every package area's section from one definition per area, and
    /// <see cref="AspidSettingsUI.BuildResetFooter"/> pins the per-scope reset under the scroll. The window's dotted
    /// canvas already sits behind this tab, so unlike the Unity-native pages it brings no canvas of its own.
    /// On top of the surface the tab runs the window's keyboard ring (the Project/Asset References idiom): ↑/↓ walk
    /// one flat focus ring over every actionable element, Enter activates the highlighted one (toggles a switch,
    /// cycles the gate, opens the add-folder picker, presses a button), ←/→ nudge the highlighted slider, Escape
    /// drops the highlight.
    /// </summary>
    internal sealed class SettingsView : VisualElement
    {
        private readonly ScrollView _scroll;

        // Keyboard navigation: one flat focus ring in visual order. Activate fires on Enter; Adjust (sliders only)
        // on ←/→ with the step sign; Remove (excluded-folder rows only) on Delete/Backspace.
        private readonly List<(VisualElement Element, Action Activate, Action<int> Adjust, Action Remove)> _navTargets = new();
        private int _navIndex = -1;

        public SettingsView()
        {
            // Repaint the plain Unity fields into the window's dark palette so they don't float bright over the canvas.
            this.AsSurface();

            _scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            AspidSettingsUI.BuildSurfaceContent(_scroll.contentContainer);
            Add(_scroll);

            // Pinned under the scroll, so the reset affordance stays reachable however long the tab grows.
            Add(AspidSettingsUI.BuildResetFooter());

            // Arrow-key navigation: the root holds keyboard focus (grabbed on attach) so key events reach
            // OnNavKeyDown even before anything is highlighted; events from focused descendants bubble here too.
            focusable = true;
            RegisterCallback<KeyDownEvent>(OnNavKeyDown);
            RegisterCallback<AttachToPanelEvent>(_ => schedule.Execute(() => Focus()));

            // The surface's controls are stable for the tab's lifetime (the excluded-folders panel rebuilds only its
            // internal rows), so the ring is collected once, in tree (= visual) order.
            CollectNavTargets(this);
        }

        // ---------------------------------------------------------------------------------------------------------
        // Keyboard navigation
        // ---------------------------------------------------------------------------------------------------------

        private void OnNavKeyDown(KeyDownEvent evt)
        {
            // A control being edited owns the keyboard: arrows/Enter inside the slider (or its inline value box)
            // adjust and commit there — stay out of the way. Escape still clears the ring below because an editing
            // control never keeps focus past Escape.
            if (IsEditingControlFocused()) return;

            // FunctionKey rides along with arrows on some platforms; any real modifier means the key is not ours.
            if ((evt.modifiers & ~EventModifiers.FunctionKey) != 0) return;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveNavFocus(+1);
                    evt.StopPropagation();
                    break;

                case KeyCode.UpArrow:
                    MoveNavFocus(-1);
                    evt.StopPropagation();
                    break;

                case KeyCode.LeftArrow when _navIndex >= 0 && _navTargets[_navIndex].Adjust is { } decrease:
                    decrease(-1);
                    evt.StopPropagation();
                    break;

                case KeyCode.RightArrow when _navIndex >= 0 && _navTargets[_navIndex].Adjust is { } increase:
                    increase(+1);
                    evt.StopPropagation();
                    break;

                case KeyCode.Return or KeyCode.KeypadEnter when _navIndex >= 0 && _navTargets[_navIndex].Activate is { } activate:
                    activate();
                    evt.StopPropagation();
                    break;

                case KeyCode.Delete or KeyCode.Backspace when _navIndex >= 0 && _navTargets[_navIndex].Remove is { } remove:
                    remove();
                    evt.StopPropagation();
                    break;

                case KeyCode.Escape when _navIndex >= 0:
                    ClearNavFocus();
                    evt.StopPropagation();
                    break;
            }
        }

        private bool IsEditingControlFocused()
        {
            if (focusController?.focusedElement is not VisualElement focused) return false;

            for (var element = focused; element != null; element = element.parent)
                if (element is ITextEdition or TextElement or SliderInt)
                    return true;

            return false;
        }

        // First press (nothing highlighted) lands on the first row whichever arrow is hit; after that the ring
        // clamps at both ends instead of wrapping.
        private void MoveNavFocus(int delta)
        {
            if (_navTargets.Count == 0) return;
            SetNavFocus(_navIndex < 0 ? 0 : Mathf.Clamp(_navIndex + delta, 0, _navTargets.Count - 1));
        }

        private void SetNavFocus(int index)
        {
            if (_navIndex == index) return;

            ClearNavFocus();
            _navIndex = index;

            var element = _navTargets[index].Element;
            element.AddToClassList(AspidSettingsUI.NavTargetFocusedClass);

            // The ring also covers the reset footer pinned OUTSIDE the scroll; ScrollTo throws on non-descendants.
            if (IsInScrollContent(element))
                _scroll.ScrollTo(element);
        }

        private bool IsInScrollContent(VisualElement element)
        {
            for (var parent = element.parent; parent != null; parent = parent.parent)
                if (parent == _scroll.contentContainer)
                    return true;

            return false;
        }

        private void ClearNavFocus()
        {
            if (_navIndex >= 0 && _navIndex < _navTargets.Count)
                _navTargets[_navIndex].Element.RemoveFromClassList(AspidSettingsUI.NavTargetFocusedClass);

            _navIndex = -1;
        }

        // Walks the tree in order and registers every actionable control by type, stopping the descent at each one so
        // its internals (a switch's thumb, a button's label) never become ring members of their own. Enter mirrors
        // what a click on the element does; the slider instead takes ←/→ (its natural axis). The excluded-folders
        // control contributes several members (its add header and each folder row) and replaces its row elements on
        // every list change, so the whole ring re-collects on its RowsRebuilt.
        private void CollectNavTargets(VisualElement element)
        {
            switch (element)
            {
                case AspidSwitch toggle:
                    RegisterNavTarget(toggle, () => toggle.value = !toggle.value);
                    return;

                case EnumField dropdown:
                    RegisterNavTarget(dropdown, () => CycleEnum(dropdown));
                    return;

                case SliderInt slider:
                    RegisterNavTarget(
                        slider,
                        activate: null,
                        adjust: delta => slider.value = Mathf.Clamp(slider.value + delta, slider.lowValue, slider.highValue));
                    return;

                case SerializeReferenceExcludedFoldersField folders:
                    foreach (var (target, activate, remove) in folders.GetNavTargets())
                        RegisterNavTarget(target, activate, remove: remove);

                    // -= then += keeps the subscription single across re-collections (this case re-runs on every
                    // rebuild of the ring).
                    folders.RowsRebuilt -= RebuildNavTargets;
                    folders.RowsRebuilt += RebuildNavTargets;
                    return;

                case Button button when button.ClassListContains(AspidSettingsUI.ActionClass):
                    RegisterNavTarget(button, () => Submit(button));
                    return;
            }

            foreach (var child in element.Children())
                CollectNavTargets(child);
        }

        private void RegisterNavTarget(VisualElement element, Action activate, Action<int> adjust = null, Action remove = null)
        {
            // EnableInClassList (not AddToClassList): stable elements are re-registered on every ring rebuild and
            // must not stack copies.
            element.EnableInClassList(AspidSettingsUI.NavTargetClass, true);
            _navTargets.Add((element, activate, adjust, remove));
        }

        // Re-collects the whole ring after the excluded-folders rows are replaced. A highlight is restored to the
        // same position, clamped — so deleting a folder row with the keyboard lands the highlight on the next row
        // instead of dropping it.
        private void RebuildNavTargets()
        {
            var restore = _navIndex;
            ClearNavFocus();
            _navTargets.Clear();
            CollectNavTargets(this);

            if (restore >= 0 && _navTargets.Count > 0)
                SetNavFocus(Mathf.Min(restore, _navTargets.Count - 1));
        }

        // Enter on the gate dropdown steps to the next value (wrapping) instead of opening the popup — the popup is
        // a native menu the ring can't reach into, while cycling keeps the whole interaction on the keyboard.
        private static void CycleEnum(EnumField dropdown)
        {
            var values = Enum.GetValues(dropdown.value.GetType());
            var index = Array.IndexOf(values, dropdown.value);
            dropdown.value = (Enum)values.GetValue((index + 1) % values.Length);
        }

        // Presses a Button exactly as the keyboard would: Clickable listens for the submit navigation event.
        private static void Submit(Button button)
        {
            using var evt = new NavigationSubmitEvent { target = button };
            button.SendEvent(evt);
        }
    }
}
