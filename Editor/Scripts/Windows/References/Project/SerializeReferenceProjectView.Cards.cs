using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Aspid.FastTools.UIElements.Editors.Internal;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed partial class SerializeReferenceProjectView
    {
        private const string GroupClass = RootClass + "__group";
        private const string GroupMigrateClass = GroupClass + "--migrate";
        private const string GroupHeaderHoverClass = GroupClass + "--header-hover";
        private const string GroupDividerClass = RootClass + "__group-divider";
        private const string GroupSweepClass = RootClass + "__group-sweep";
        private const string GroupSweepMigrateClass = GroupSweepClass + "--migrate";
        private const string GroupHeaderRowClass = RootClass + "__group-header-row";
        private const string GroupHeaderRowStaticClass = GroupHeaderRowClass + "--static";
        private const string GroupHeaderClass = RootClass + "__group-header";
        private const string GroupCountClass = RootClass + "__group-count";
        private const string GroupFixAllClass = RootClass + "__group-fix-all";
        private const string GroupFixAllMigrateClass = GroupFixAllClass + "--migrate";
        private const string GroupActionClass = RootClass + "__group-action";
        private const string GroupActionInfoClass = GroupActionClass + "--info";
        private const string GroupEntryClass = RootClass + "__group-entry";
        private const string GroupEntryPathClass = RootClass + "__group-entry-path";
        private const string GroupEntryRidClass = RootClass + "__group-entry-rid";
        private const string GroupEntryFieldClass = RootClass + "__group-entry-field";

        private VisualElement BuildGroupCard(MissingReferenceGroup group, MissingReferenceMigration migration)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var constraint = migration.Constraint;
            var isMigration = migration.IsMigration;

            if (isMigration) card.AddClass(GroupMigrateClass);

            AspidGradientButton fixAll = null;
            fixAll = new AspidGradientButton(SerializeReferenceProjectSummary.BuildFixAllLabel(group, isMigration),
                    _ => ToggleGroupPicker(group, constraint, fixAll))
                .AddClass(GroupFixAllClass);
            if (isMigration) fixAll.AddClass(GroupFixAllMigrateClass);
            _ring.RegisterHeader(fixAll, card, GroupHeaderHoverClass, () => ToggleGroupPicker(group, constraint, fixAll));
            fixAll.tooltip = constraint == typeof(object)
                ? $"{group.DisplayName}\nMixed or unresolvable field types — the picker is unconstrained (any managed-reference type)."
                : $"{group.DisplayName}\nConstrained to {constraint.FullName}.";

            fixAll.AddLeadingContent(BuildGroupHeaderRow(
                group.StoredType.Class,
                SerializeReferenceProjectSummary.BuildGroupCountText(group),
                isMigration ? StatusStyle.Type.Info : StatusStyle.Type.Warning,
                isStatic: false));
            card.AddChild(fixAll);

            AddGroupDivider(card, withSweep: true, isMigration ? GroupSweepMigrateClass : null);

            var action = BuildBulkActionRow(group, migration);
            if (action is not null) card.AddChild(action);

            foreach (var entry in group.Entries)
                card.AddChild(BuildGroupEntryRow(entry));

            return card;
        }

        private VisualElement BuildBulkActionRow(MissingReferenceGroup group, MissingReferenceMigration migration)
        {
            if (migration.IsMigration)
            {
                var target = migration.Target;

                return BuildGroupActionRow(
                    $"Migrate all ({group.Entries.Count}) → {target.Name}",
                    $"Every entry resolves to {target.FullName} via its declared [MovedFrom] — Unity already " +
                    "migrates them in memory when the asset loads. Migrating rewrites the stored type name in the " +
                    "files so they match the code; the attribute can be removed once no file stores the old name.",
                    info: true,
                    () => ApplyGroupFix(group, target));
            }

            if (!group.TryGetSuggestion(migration.Constraint, out var suggestion)) return null;

            return BuildGroupActionRow(
                $"Smart Fix {SerializeReferenceHelpers.GetSuggestionLabel(suggestion)}",
                SerializeReferenceHelpers.GetSuggestionDetail(suggestion),
                info: false,
                () => ApplyGroupFix(group, suggestion.Type));
        }

        private VisualElement BuildGroupActionRow(string text, string tooltipText, bool info, Action onClick)
        {
            var row = new Label(text).AddClass(GroupActionClass);
            if (info) row.AddClass(GroupActionInfoClass);
            row.tooltip = tooltipText;
            row.RegisterCallback<ClickEvent>(_ => onClick());
            RegisterNavTarget(row, onClick);
            return row;
        }

        private VisualElement BuildRequiredGroupCard(IReadOnlyList<GateViolation> violations)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var files = violations.Select(violation => violation.AssetPath).Distinct(StringComparer.Ordinal).Count();

            card.AddChild(BuildGroupHeaderRow(
                "Required violations",
                $"{BuildCountText(violations.Count, "entry")} · {(files == 1 ? "1 file" : $"{files} files")}",
                StatusStyle.Type.Warning,
                isStatic: true));

            AddGroupDivider(card, withSweep: false);

            var labels = new ViolationFieldLabels();
            foreach (var violation in violations)
                card.AddChild(BuildRequiredViolationRow(violation, labels));

            return card;
        }

        private static VisualElement BuildGroupHeaderRow(string title, string countText, StatusStyle.Type status, bool isStatic)
        {
            var header = new AspidLabel(title, AspidLabelPreset.Default
                    .SetLabelStatus(status)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(GroupHeaderClass)
                .SetPickingMode(PickingMode.Ignore);

            var count = new Label(countText)
                .AddClass(GroupCountClass)
                .SetPickingMode(PickingMode.Ignore);

            var row = new VisualElement()
                .AddClass(GroupHeaderRowClass)
                .AddChild(header)
                .AddChild(count);
            if (isStatic) row.AddClass(GroupHeaderRowStaticClass);
            row.pickingMode = PickingMode.Ignore;

            return row;
        }

        // The sweep is a sibling of the button, so hover is propagated through the card class.
        private static void AddGroupDivider(VisualElement card, bool withSweep, string sweepModifier = null)
        {
            card.AddChild(new AspidDividingLine(AspidDividingLinePreset.Default
                    .SetTheme(ThemeStyle.Type.Light)
                    .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                .AddClass(GroupDividerClass));

            if (!withSweep) return;

            var sweep = new VisualElement()
                .AddClass(GroupSweepClass)
                .SetPickingMode(PickingMode.Ignore);
            if (sweepModifier is not null) sweep.AddClass(sweepModifier);
            card.AddChild(sweep);
        }

        private VisualElement BuildGroupEntryRow(MissingReferenceLocation entry)
        {
            var path = MakeSelectable(new Label(entry.AssetPath).AddClass(GroupEntryPathClass));
            path.tooltip = entry.AssetPath;

            var rid = MakeSelectable(new Label($"rid {entry.Entry.Rid}").AddClass(GroupEntryRidClass));

            return BuildEntryRow(entry.AssetPath, path, rid);
        }

        private VisualElement BuildRequiredViolationRow(GateViolation violation, ViolationFieldLabels labels)
        {
            var path = MakeSelectable(new Label(violation.AssetPath).AddClass(GroupEntryPathClass));
            path.tooltip = violation.AssetPath;

            var field = MakeSelectable(new Label(labels.Describe(violation)).AddClass(GroupEntryFieldClass));

            return BuildEntryRow(violation.AssetPath, path, field);
        }

        private VisualElement BuildEntryRow(string assetPath, Label left, Label right)
        {
            var row = new VisualElement().AddClass(GroupEntryClass);
            row.AddChild(left).AddChild(right);

            row.RegisterCallback<ClickEvent>(evt =>
            {
                // A drag selection also produces a click; keep the selected text available for copying.
                if (evt.target is TextElement text && text.selection.HasSelection()) return;
                JumpToAsset(assetPath);
            });

            RegisterNavTarget(row, () => JumpToAsset(assetPath));
            row.AddManipulator(new ContextualMenuManipulator(evt => PopulateEntryContextMenu(evt, assetPath)));

            return row;
        }

        // Selectable labels add their own context items before this bubbling callback; replace those items here.
        private void PopulateEntryContextMenu(ContextualMenuPopulateEvent evt, string assetPath)
        {
            for (var i = evt.menu.MenuItems().Count - 1; i >= 0; i--)
                evt.menu.RemoveItemAt(i);

            evt.menu.AppendAction("Open in Asset References", _ => JumpToAsset(assetPath));

            evt.menu.AppendAction(
                "Open in Prefab Mode",
                _ => PrefabStageUtility.OpenPrefab(assetPath),
                assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Select in Project", _ =>
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset is null) return;

                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            });
        }

        private void JumpToAsset(string assetPath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset is null) return;

            if (OnInspectAsset is not null) OnInspectAsset(asset);
            else EditorGUIUtility.PingObject(asset);
        }
    }
}
