using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The icon of an empty Asset References or Project References tab sits at the middle of the tab's viewport, whatever
    // the header card above it and the text below it measure, so switching tabs does not move it. The title and the
    // message follow the icon; in a window too short for that the top padding falls back to its minimum and the view
    // scrolls.
    internal static class ReferenceEmptyStateLayout
    {
        private const float MinTopPadding = 20f;

        // The icon must be the first child of the empty state.
        public static void PinIcon(VisualElement empty, ScrollView scroll)
        {
            empty.RegisterCallback<GeometryChangedEvent>(_ => Update(empty, scroll));
            scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => Update(empty, scroll));
        }

        private static void Update(VisualElement empty, ScrollView scroll)
        {
            if (empty.panel is null || empty.childCount == 0) return;

            var iconHeight = empty[0].resolvedStyle.height;
            if (float.IsNaN(iconHeight)) return;

            // Both points are measured in the scrolled content, so the scroll offset cancels out and cannot feed back.
            var top = empty.ChangeCoordinatesTo(scroll.contentContainer, Vector2.zero).y;
            var center = scroll.contentViewport.layout.height / 2f;
            var padding = Mathf.Max(MinTopPadding, Mathf.Round(center - top - iconHeight / 2f));

            if (Mathf.Abs(empty.resolvedStyle.paddingTop - padding) > 0.5f)
                empty.style.paddingTop = padding;
        }
    }
}
