using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // The window's shared keyboard focus ring: one flat list of actionable elements walked with the arrows,
    // activated with Enter and dropped with Escape. Every window view drives this one ring, so their keyboard
    // behavior stays identical; each supplies only how to scroll a member into view, which class marks a focused
    // plain row and when the ring is suspended. The ring never moves real keyboard focus onto its members — the host
    // keeps focus and the ring only paints a highlight — so a member hidden by a collapsed container is skipped
    // while walking and never activated.
    internal sealed class NavRing
    {
        // One ring member: the element plus what Enter (Activate), ←/→ (Adjust, sliders) and Delete/Backspace (Remove,
        // removable rows) do to it. Adjust / Remove are null for members that don't take them. HoverCard / HoverClass
        // carry the card-level sweep modifier of a header member (see RegisterHeader).
        private readonly struct Target
        {
            public readonly VisualElement Element;
            public readonly Action Activate;
            public readonly Action<int> Adjust;
            public readonly Action Remove;
            public readonly VisualElement HoverCard;
            public readonly string HoverClass;

            public Target(
                VisualElement element,
                Action activate,
                Action<int> adjust,
                Action remove,
                VisualElement hoverCard,
                string hoverClass)
            {
                Element = element;
                Activate = activate;
                Adjust = adjust;
                Remove = remove;
                HoverCard = hoverCard;
                HoverClass = hoverClass;
            }
        }

        private readonly List<Target> _targets = new();
        private int _index = -1;

        // Set by Clear(keepFocusedElement: true) and consumed by the member's own re-registration — see Add.
        private VisualElement _restore;

        private readonly VisualElement _host;
        private readonly string _navTargetClass;
        private readonly string _focusedClass;
        private readonly Action<VisualElement> _scrollTo;
        private readonly Func<bool> _isSuspended;

        public NavRing(
            VisualElement host,
            string navTargetClass,
            string focusedClass = null,
            Action<VisualElement> scrollTo = null,
            Func<bool> isSuspended = null)
        {
            _host = host;
            _navTargetClass = navTargetClass;
            _focusedClass = focusedClass;
            _scrollTo = scrollTo;
            _isSuspended = isSuspended;

            host.focusable = true;
            host.RegisterCallback<KeyDownEvent>(OnKeyDown);
            host.RegisterCallback<AttachToPanelEvent>(_ => host.schedule.Execute(() => host.Focus()));
        }

        public void Register(VisualElement element, Action activate, Action<int> adjust = null, Action remove = null) =>
            Add(new Target(element, activate, adjust, remove, hoverCard: null, hoverClass: null));

        public void RegisterHeader(VisualElement header, VisualElement card, string hoverClass, Action activate)
        {
            Add(new Target(header, activate, adjust: null, remove: null, card, hoverClass));

            header.RegisterCallback<MouseEnterEvent>(_ => card.EnableInClassList(hoverClass, true));
            header.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                // The highlighted card keeps its sweep lit, so a mouse pass over it never half-extinguishes it.
                if (!IsFocused(header)) card.EnableInClassList(hoverClass, false);
            });
        }

        public void Rebuild(Action register)
        {
            var slot = _index;

            Clear();
            register();

            if (slot >= 0 && _targets.Count > 0)
                Focus(Mathf.Min(slot, _targets.Count - 1), scrollTo: false);
        }

        public void Clear(bool keepFocusedElement = false)
        {
            _restore = keepFocusedElement && _index >= 0 && _index < _targets.Count
                ? _targets[_index].Element
                : null;

            ClearFocus();
            _targets.Clear();
        }

        // EnableInClassList (not Add), so a member re-registered on a ring rebuild never stacks the class twice.
        private void Add(in Target target)
        {
            target.Element.EnableInClassList(_navTargetClass, true);
            _targets.Add(target);

            if (_restore != target.Element) return;

            _restore = null;
            Focus(_targets.Count - 1, scrollTo: false);
        }

        // Gradient buttons paint their hover in code (accent overlay + tinted labels); the focused class's flat fill
        // would just show through their fading gradient as a gray pill, so they take ONLY the programmatic hover and
        // the plain rows take ONLY the class. A header member also lights its card's sweep modifier — the same class
        // its mouse hover mirrors (see RegisterHeader).
        private void Paint(in Target target, bool on)
        {
            if (target.Element is AspidGradientButton button) button.Highlighted = on;
            else if (_focusedClass is not null) target.Element.EnableInClassList(_focusedClass, on);

            target.HoverCard?.EnableInClassList(target.HoverClass, on);
        }

        private bool IsFocused(VisualElement element) =>
            _index >= 0 && _index < _targets.Count && _targets[_index].Element == element;

        private void ClearFocus()
        {
            if (_index >= 0 && _index < _targets.Count)
                Paint(_targets[_index], false);

            _index = -1;
        }

        private void Focus(int index, bool scrollTo = true)
        {
            if (_index == index) return;

            ClearFocus();
            _index = index;

            var target = _targets[index];
            Paint(target, true);
            if (scrollTo) _scrollTo?.Invoke(target.Element);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            // A view whose own picker (or any modal child) owns the keyboard suspends the ring entirely.
            if (_isSuspended is not null && _isSuspended()) return;

            // A control being edited owns the keyboard: arrows/Enter inside a slider or text field adjust and commit
            // there. Escape still clears below — an editing control never keeps focus past Escape.
            if (IsEditingControlFocused()) return;

            // FunctionKey rides along with arrows on some platforms; any real modifier means the key is not ours.
            if ((evt.modifiers & ~EventModifiers.FunctionKey) != 0) return;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    Move(+1);
                    evt.StopPropagation();
                    break;

                case KeyCode.UpArrow:
                    Move(-1);
                    evt.StopPropagation();
                    break;

                case KeyCode.LeftArrow when _index >= 0 && _targets[_index].Adjust is { } decrease:
                    decrease(-1);
                    evt.StopPropagation();
                    break;

                case KeyCode.RightArrow when _index >= 0 && _targets[_index].Adjust is { } increase:
                    increase(+1);
                    evt.StopPropagation();
                    break;

                // Guarded on visibility too: a member hidden inside a collapsed container must not be activatable even
                // if the highlight was left on it (e.g. the card was collapsed by mouse after the keyboard focused it).
                case KeyCode.Return or KeyCode.KeypadEnter
                    when _index >= 0 && _targets[_index].Activate is { } activate && IsVisible(_targets[_index].Element):
                    activate();
                    evt.StopPropagation();
                    break;

                case KeyCode.Delete or KeyCode.Backspace when _index >= 0 && _targets[_index].Remove is { } remove:
                    remove();
                    evt.StopPropagation();
                    break;

                case KeyCode.Escape when _index >= 0:
                    ClearFocus();
                    evt.StopPropagation();
                    break;
            }
        }

        // First press (nothing highlighted) lands on the first visible member whichever arrow is hit; after that the
        // ring steps to the next visible member in the arrow's direction, skipping members hidden inside a collapsed
        // container, and clamps (does not wrap) when no visible member remains that way.
        private void Move(int delta)
        {
            if (_targets.Count == 0) return;

            var start = _index < 0 ? 0 : _index + delta;
            var step = _index < 0 ? +1 : delta;

            for (var i = start; i >= 0 && i < _targets.Count; i += step)
            {
                if (!IsVisible(_targets[i].Element)) continue;
                Focus(i);
                return;
            }
        }

        private bool IsEditingControlFocused()
        {
            if (_host.focusController?.focusedElement is not VisualElement focused) return false;

            for (var element = focused; element is not null; element = element.parent)
            {
                if (element is ITextEdition or SliderInt) return true;
                // Button derives from TextElement, so a focused ring button (e.g. a Settings reset button) must not be
                // mistaken for a text-editing control — that would silently kill the ring while the button holds focus.
                if (element is TextElement and not Button) return true;
            }

            return false;
        }

        // A member is navigable only while it is actually on screen: a collapsed document band sets its body's
        // display to None, so its descendant members fail this ancestor walk and drop out of the ring.
        private static bool IsVisible(VisualElement element)
        {
            for (var e = element; e is not null; e = e.parent)
                if (e.resolvedStyle.display == DisplayStyle.None)
                    return false;

            return true;
        }
    }
}
