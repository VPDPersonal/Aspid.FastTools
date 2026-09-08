using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed class NavRing
    {
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

        private void Add(in Target target)
        {
            target.Element.EnableInClassList(_navTargetClass, true);
            _targets.Add(target);

            if (_restore != target.Element) return;

            _restore = null;
            Focus(_targets.Count - 1, scrollTo: false);
        }

        // Gradient buttons need their animated hover instead of the flat focus background.
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
            if (_isSuspended is not null && _isSuspended()) return;

            if (IsEditingControlFocused()) return;

            // Some platforms include FunctionKey on arrow events; ignore it when checking modifiers.
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

                case KeyCode.LeftArrow when _index >= 0 && _targets[_index].Adjust is { } decrease && IsVisible(_targets[_index].Element):
                    decrease(-1);
                    evt.StopPropagation();
                    break;

                case KeyCode.RightArrow when _index >= 0 && _targets[_index].Adjust is { } increase && IsVisible(_targets[_index].Element):
                    increase(+1);
                    evt.StopPropagation();
                    break;

                case KeyCode.Return or KeyCode.KeypadEnter
                    when _index >= 0 && _targets[_index].Activate is { } activate && IsVisible(_targets[_index].Element):
                    activate();
                    evt.StopPropagation();
                    break;

                case KeyCode.Delete or KeyCode.Backspace when _index >= 0 && _targets[_index].Remove is { } remove && IsVisible(_targets[_index].Element):
                    remove();
                    evt.StopPropagation();
                    break;

                case KeyCode.Escape when _index >= 0:
                    ClearFocus();
                    evt.StopPropagation();
                    break;
            }
        }

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
                // Button inherits TextElement but does not edit text.
                if (element is TextElement and not Button) return true;
            }

            return false;
        }

        private static bool IsVisible(VisualElement element)
        {
            for (var e = element; e is not null; e = e.parent)
                if (e.resolvedStyle.display == DisplayStyle.None)
                    return false;

            return true;
        }
    }
}
