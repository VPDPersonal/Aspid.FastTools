using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceAuditUI
    {
        private const string RescanIconName = "d_Refresh";

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

        // Unity's own refresh glyph, tinted by the view's sheet. The light variant is used on both skins, so one tint
        // reads the same on each.
        public static VisualElement BuildRescanIcon(string iconClass)
        {
            var icon = new VisualElement()
                .AddClass(iconClass)
                .SetPickingMode(PickingMode.Ignore);

            if (EditorGUIUtility.IconContent(RescanIconName).image is Texture2D texture)
                icon.style.backgroundImage = new StyleBackground(texture);

            return icon;
        }

        // The target of a Smart Fix or a migration with its namespace, as Project References and Asset References both
        // name it, so the row says which type it writes whatever the stored name was.
        public static string GetQualifiedTitle(Type type)
        {
            var title = TypeSelectorHelpers.GetTypeSelectorTitle(type);
            return string.IsNullOrEmpty(type.Namespace) ? title : $"{type.Namespace}.{title}";
        }

        // Two types with one class name read the same on a card: "Pistol → Pistol". This qualifies both with the
        // shortest trailing part of their namespaces that differs, as in "Legacy.Pistol → Weapons.Pistol", or with the
        // assembly when the namespaces match too. Returns false, and leaves the titles to the caller, when the names
        // already differ.
        public static bool TryQualifySameNames(ManagedTypeName stored, Type target, out string storedTitle, out string targetTitle)
        {
            storedTitle = stored.Class;
            targetTitle = target is null ? null : TypeSelectorHelpers.GetTypeSelectorTitle(target);
            if (target is null || targetTitle != stored.Class) return false;

            var storedParts = SplitNamespace(stored.Namespace);
            var targetParts = SplitNamespace(target.Namespace);

            for (var count = 1; count <= Math.Max(storedParts.Length, targetParts.Length); count++)
            {
                var storedSuffix = JoinLast(storedParts, count);
                var targetSuffix = JoinLast(targetParts, count);
                if (storedSuffix == targetSuffix) continue;

                storedTitle = Qualify(storedSuffix, storedTitle);
                targetTitle = Qualify(targetSuffix, targetTitle);
                return true;
            }

            storedTitle = $"{storedTitle} ({stored.Assembly})";
            targetTitle = $"{targetTitle} ({target.Assembly.GetName().Name})";
            return true;
        }

        private static string[] SplitNamespace(string @namespace) =>
            string.IsNullOrEmpty(@namespace) ? Array.Empty<string>() : @namespace.Split('.');

        private static string JoinLast(string[] parts, int count)
        {
            var taken = Math.Min(count, parts.Length);
            return string.Join(".", parts, parts.Length - taken, taken);
        }

        private static string Qualify(string @namespace, string name) =>
            @namespace.Length == 0 ? name : $"{@namespace}.{name}";

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
