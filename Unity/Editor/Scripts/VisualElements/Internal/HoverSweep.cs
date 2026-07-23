using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    /// <summary>
    /// Wires the window cards' hover-sweep idiom: a flat header button whose accent underline sweep is a sibling (so
    /// USS <c>:hover</c> can't reach it) mirrors its hover onto an ancestor card modifier class the sweep rule
    /// listens to. Composed with <see cref="NavRing"/> — while the button is the nav-focused ring member the sweep
    /// stays lit after the mouse leaves, matching the programmatic hover the ring paints — so keyboard focus and mouse
    /// hover render identically. Shared by the Welcome sample cards and both References audit tabs so the composition
    /// (which ancestor carries the modifier, and the keep-lit-while-focused guard) can't drift between them.
    /// </summary>
    internal static class HoverSweep
    {
        /// <summary>
        /// Mirrors <paramref name="button"/>'s mouse hover onto the modifier <paramref name="hoverClass"/> on the card
        /// resolved by <paramref name="resolveCard"/> — resolved at event time, so it works whether the card is a
        /// captured local or the button's live parent. The modifier is kept lit while <paramref name="isNavFocused"/>
        /// reports the button as the focus-ring member, so a mouse pass over a keyboard-focused card never
        /// half-extinguishes its sweep.
        /// </summary>
        public static void MirrorHover(
            VisualElement button,
            Func<VisualElement> resolveCard,
            string hoverClass,
            Func<bool> isNavFocused)
        {
            button.RegisterCallback<MouseEnterEvent>(_ => resolveCard()?.AddToClassList(hoverClass));
            button.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (isNavFocused()) return;
                resolveCard()?.RemoveFromClassList(hoverClass);
            });
        }
    }
}
