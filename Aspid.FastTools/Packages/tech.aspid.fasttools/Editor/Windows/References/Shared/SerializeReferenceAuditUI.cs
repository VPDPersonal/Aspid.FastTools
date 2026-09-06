using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Shared building blocks for the two References audit tabs, so their count wording, selectable labels and accent
    // legend cannot drift apart. Each tab keeps its own USS block, so the legend builder wears the names it is
    // handed.
    internal static class SerializeReferenceAuditUI
    {
        // A naive y-to-ies plural rule, which covers the audit's fixed nouns.
        public static string BuildCountText(int count, string noun) =>
            count == 1 ? $"1 {noun}" : $"{count} {(noun.EndsWith("y") ? noun[..^1] + "ies" : noun + "s")}";

        // The shared severity verdict, tinting both a headline and the canvas wash behind it: anything broken,
        // orphaned or required-unset is amber, a graph whose only findings are pending migrations is info-blue,
        // since a stale file is not a breakage, and anything else is green. broken EXCLUDES those migrations.
        public static StatusStyle.Type ResolveStatus(int broken, int orphans, int required, int migrations) =>
            broken > 0 || orphans > 0 || required > 0
                ? StatusStyle.Type.Warning
                : migrations > 0
                    ? StatusStyle.Type.Info
                    : StatusStyle.Type.Success;

        // Makes a row's text selectable so it can be copied out. A caller that also carries a row click gates it on
        // an empty selection, since a drag-select ends in a click too.
        public static Label MakeSelectable(Label label)
        {
            label.selection.isSelectable = true;
            label.selection.doubleClickSelectsWord = true;
            label.selection.tripleClickSelectsLine = true;
            return label;
        }

        // One dot-and-caption pair of the accent legend: amber for the broken band, info blue for the
        // pending-migration cards.
        public static VisualElement BuildLegendItem(string text, bool info, in LegendClasses classes)
        {
            var dot = new VisualElement().AddClass(classes.Dot);
            if (info) dot.AddClass(classes.DotInfo);

            return new VisualElement()
                .AddClass(classes.Item)
                .AddChild(dot)
                .AddChild(new Label(text).AddClass(classes.Text));
        }

        // The block-specific USS class names the shared legend builder wears; the two tabs use different blocks.
        internal readonly struct LegendClasses
        {
            public readonly string Item;
            public readonly string Dot;
            public readonly string DotInfo;
            public readonly string Text;

            public LegendClasses(string item, string dot, string dotInfo, string text)
            {
                Item = item;
                Dot = dot;
                DotInfo = dotInfo;
                Text = text;
            }
        }
    }
}
