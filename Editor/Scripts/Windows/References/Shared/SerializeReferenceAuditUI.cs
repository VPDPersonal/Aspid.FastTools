using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceAuditUI
    {
        public static string BuildCountText(int count, string noun) =>
            count == 1 ? $"1 {noun}" : $"{count} {(noun.EndsWith("y") ? noun[..^1] + "ies" : noun + "s")}";

        public static StatusStyle.Type ResolveStatus(int broken, int orphans, int required, int migrations) =>
            broken > 0 || orphans > 0 || required > 0
                ? StatusStyle.Type.Warning
                : migrations > 0
                    ? StatusStyle.Type.Info
                    : StatusStyle.Type.Success;

        public static Label MakeSelectable(Label label)
        {
            label.selection.isSelectable = true;
            label.selection.doubleClickSelectsWord = true;
            label.selection.tripleClickSelectsLine = true;
            return label;
        }

        public static VisualElement BuildLegendItem(string text, bool info, in LegendClasses classes)
        {
            var dot = new VisualElement().AddClass(classes.Dot);
            if (info) dot.AddClass(classes.DotInfo);

            return new VisualElement()
                .AddClass(classes.Item)
                .AddChild(dot)
                .AddChild(new Label(text).AddClass(classes.Text));
        }

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
