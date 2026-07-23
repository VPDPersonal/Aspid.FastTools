using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    /// <summary>
    /// The window's shared keyboard focus ring: one flat list of actionable elements walked with ↑/↓, activated with
    /// Enter and dropped with Escape; sliders take ←/→ (Adjust) and removable rows take Delete/Backspace (Remove). The
    /// window views (Welcome, Asset / Project References, Settings) all drive from this one ring so their keyboard
    /// behaviour stays identical — each supplies only how a focused member is painted, how to scroll it into view,
    /// and (optionally) when the ring is suspended because its own picker owns the keyboard.
    /// </summary>
    /// <remarks>
    /// The ring never moves real keyboard focus onto its members — the host element keeps focus and the ring only
    /// paints a highlight — so a member hidden by a collapsed container is skipped while walking and never activated,
    /// keeping an off-screen member out of keyboard reach.
    /// </remarks>
    internal sealed class NavRing
    {
        /// <summary>
        /// One ring member: the element plus what Enter (<see cref="Activate"/>), ←/→ (<see cref="Adjust"/>, sliders)
        /// and Delete/Backspace (<see cref="Remove"/>, removable rows) do to it. <see cref="Adjust"/> / <see cref="Remove"/>
        /// are <see langword="null"/> for members that don't take them.
        /// </summary>
        internal readonly struct Target
        {
            public readonly VisualElement Element;
            public readonly Action Activate;
            public readonly Action<int> Adjust;
            public readonly Action Remove;

            public Target(VisualElement element, Action activate, Action<int> adjust, Action remove)
            {
                Element = element;
                Activate = activate;
                Adjust = adjust;
                Remove = remove;
            }
        }

        private readonly List<Target> _targets = new();
        private int _index = -1;

        private readonly VisualElement _host;
        private readonly string _navTargetClass;
        private readonly Action<VisualElement, bool> _paint;
        private readonly Action<VisualElement> _scrollTo;
        private readonly Func<bool> _isSuspended;

        /// <summary>
        /// Wires the ring onto <paramref name="host"/>: the host grabs keyboard focus on attach (so keys reach the ring
        /// before anything is highlighted) and every <see cref="KeyDownEvent"/> bubbling to it is handled here.
        /// </summary>
        /// <param name="host">The element that holds keyboard focus and receives the ring's key events.</param>
        /// <param name="navTargetClass">USS class applied to every registered member (the view's <c>__nav-target</c>).</param>
        /// <param name="paint">Lights (<see langword="true"/>) or clears (<see langword="false"/>) a member's focus treatment.</param>
        /// <param name="scrollTo">Scrolls a member into view when focus lands on it; may be <see langword="null"/>.</param>
        /// <param name="isSuspended">When it returns <see langword="true"/> the ring ignores all keys (e.g. an open picker owns them); may be <see langword="null"/>.</param>
        public NavRing(
            VisualElement host,
            string navTargetClass,
            Action<VisualElement, bool> paint,
            Action<VisualElement> scrollTo = null,
            Func<bool> isSuspended = null)
        {
            _host = host;
            _navTargetClass = navTargetClass;
            _paint = paint;
            _scrollTo = scrollTo;
            _isSuspended = isSuspended;

            host.focusable = true;
            host.RegisterCallback<KeyDownEvent>(OnKeyDown);
            host.RegisterCallback<AttachToPanelEvent>(_ => host.schedule.Execute(() => host.Focus()));
        }

        /// <summary>The highlighted member's index, or -1 when nothing is highlighted.</summary>
        public int Index => _index;

        /// <summary>The number of registered members.</summary>
        public int Count => _targets.Count;

        /// <summary>Whether <paramref name="element"/> is the currently highlighted member.</summary>
        public bool IsFocused(VisualElement element) =>
            _index >= 0 && _index < _targets.Count && _targets[_index].Element == element;

        /// <summary>
        /// Appends a member in visual order. <paramref name="adjust"/> handles ←/→ (sliders); <paramref name="remove"/>
        /// handles Delete/Backspace (removable rows). EnableInClassList (not Add), so a member re-registered on a ring
        /// rebuild never stacks the class twice.
        /// </summary>
        public void Register(VisualElement element, Action activate, Action<int> adjust = null, Action remove = null)
        {
            element.EnableInClassList(_navTargetClass, true);
            _targets.Add(new Target(element, activate, adjust, remove));
        }

        /// <summary>Clears the highlight and drops every member — a full ring rebuild follows with fresh <see cref="Register"/> calls.</summary>
        public void Clear()
        {
            ClearFocus();
            _targets.Clear();
        }

        /// <summary>Drops the highlight without touching the member list.</summary>
        public void ClearFocus()
        {
            if (_index >= 0 && _index < _targets.Count)
                _paint(_targets[_index].Element, false);

            _index = -1;
        }

        /// <summary>
        /// Highlights the member at <paramref name="index"/>, scrolling it into view unless <paramref name="scrollTo"/>
        /// is <see langword="false"/> — used when restoring a highlight after a rebuild, where a scroll would be jarring.
        /// </summary>
        public void Focus(int index, bool scrollTo = true)
        {
            if (_index == index) return;

            ClearFocus();
            _index = index;

            var element = _targets[index].Element;
            _paint(element, true);
            if (scrollTo) _scrollTo?.Invoke(element);
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
