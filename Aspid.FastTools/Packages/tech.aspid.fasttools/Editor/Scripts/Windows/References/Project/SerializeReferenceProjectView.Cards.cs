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
        private const string GroupHeaderClass = RootClass + "__group-header";
        private const string GroupCountClass = RootClass + "__group-count";
        private const string GroupFixAllClass = RootClass + "__group-fix-all";
        private const string GroupFixAllMigrateClass = GroupFixAllClass + "--migrate";
        private const string GroupActionClass = RootClass + "__group-action";
        private const string GroupActionInfoClass = GroupActionClass + "--info";
        private const string GroupMoreClass = RootClass + "__group-more";
        private const string FilesMoreClass = RootClass + "__files-more";
        private const string GroupEntryClass = RootClass + "__group-entry";
        private const string GroupEntryPathClass = RootClass + "__group-entry-path";
        private const string GroupEntryFolderClass = RootClass + "__group-entry-folder";
        private const string GroupEntryFileClass = RootClass + "__group-entry-file";
        private const string GroupEntryRidClass = RootClass + "__group-entry-rid";
        private const string GroupEntryFieldClass = RootClass + "__group-entry-field";
        private const string GroupEntryNameClass = RootClass + "__group-entry-name";
        private const string FileHeaderClass = RootClass + "__file-header";
        private const string SectionHeadingClass = RootClass + "__section-heading";
        private const string SectionHeadingFirstClass = SectionHeadingClass + "--first";

        private const string BrokenSectionTitle = "Broken";
        private const string BrokenSectionTooltip = "Each group is a stored type that no longer exists: pick a replacement with Fix all.";
        private const string RenamedSectionTitle = "Renamed";
        private const string RenamedSectionTooltip =
            "Each type was renamed or moved with [MovedFrom]: Migrate all rewrites the stored name in one click.";
        private const string RequiredSectionTitle = "Required";
        private const string RequiredSectionTooltip =
            "Fields marked [Required] that hold nothing. Click a row to open its asset and assign the field.\n"
            + SerializeReferenceProjectSummary.ScanTimeNote;
        private const string OverridesSectionTitle = "Prefab instance overrides";
        private const string OverridesSectionTooltip =
            "Types set by an override on a prefab instance. Fix all does not rewrite them: pick a new type on the instance, "
            + "or Revert the override.";

        private static VisualElement BuildSectionHeading(string title, StatusStyle.Type status, string tooltip, bool isFirst)
        {
            var heading = new ReferenceSectionHeading(title, status)
                .AddClass(SectionHeadingClass);
            if (isFirst) heading.AddClass(SectionHeadingFirstClass);

            heading.tooltip = tooltip;
            return heading;
        }

        private VisualElement BuildGroupCard(MissingReferenceGroup group, MissingReferenceMigration migration)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var constraint = migration.Constraint;
            var isMigration = migration.IsMigration;

            if (isMigration) card.AddClass(GroupMigrateClass);

            SerializeReferenceRepairSuggestions.RepairCandidate suggestion = default;
            var hasSuggestion = !isMigration && group.TryGetSuggestion(constraint, out suggestion);

            var title = group.StoredType.Class;
            var suggestionTitle = hasSuggestion ? GetQualifiedTitle(suggestion.Type) : null;
            if (hasSuggestion && TryQualifySameNames(group.StoredType, suggestion.Type, out var storedTitle, out _))
                title = storedTitle;

            AspidGradientButton fixAll = null;
            fixAll = new AspidGradientButton(SerializeReferenceProjectSummary.BuildFixAllLabel(group, isMigration),
                    _ => ToggleGroupPicker(group, constraint, fixAll))
                .AddClass(GroupFixAllClass);
            if (isMigration) fixAll.AddClass(GroupFixAllMigrateClass);
            _ring.RegisterHeader(fixAll, card, GroupHeaderHoverClass, () => ToggleGroupPicker(group, constraint, fixAll));
            fixAll.tooltip = constraint is null
                ? $"{group.DisplayName}\nMixed or unresolvable field types — the picker is unconstrained (any managed-reference type)."
                : $"{group.DisplayName}\nConstrained to {string.Join(", ", constraint.Select(type => type.FullName))}.";

            fixAll.AddLeadingContent(BuildGroupHeaderRow(
                title,
                SerializeReferenceProjectSummary.BuildGroupCountText(group),
                isMigration ? StatusStyle.Type.Info : StatusStyle.Type.Warning));
            card.AddChild(fixAll);

            AddGroupDivider(card, withSweep: true, isMigration ? GroupSweepMigrateClass : null);

            var action = isMigration
                ? BuildMigrationRow(group, migration.Target)
                : hasSuggestion
                    ? BuildGroupActionRow(
                        $"Smart Fix → {suggestionTitle}",
                        SerializeReferenceHelpers.GetSuggestionDetail(suggestion),
                        info: false,
                        () => ApplyGroupFix(group, suggestion.Type))
                    : null;
            if (action is not null) card.AddChild(action);

            AddPagedRows(card, "missing|" + SerializeReferenceHelpers.StoredTypeKey(group.StoredType),
                SerializeReferenceProjectSummary.GroupByFile(group.Entries), BuildGroupFileRow);

            return card;
        }

        // A missing SerializableType or SerializableMonoScript name: Fix all and Smart Fix rewrite the stored name in
        // every listed file, prefab instance overrides included.
        private VisualElement BuildTypeNameGroupCard(MissingTypeNameGroup group)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var constraint = group.ResolveConstraint(out var mixed);

            AspidGradientButton fixAll = null;
            fixAll = new AspidGradientButton($"Fix all ({group.Entries.Count})  ▼", _ => ToggleTypeNamePicker(group, constraint, fixAll))
                .AddClass(GroupFixAllClass);
            _ring.RegisterHeader(fixAll, card, GroupHeaderHoverClass, () => ToggleTypeNamePicker(group, constraint, fixAll));
            fixAll.tooltip = (mixed
                ? $"{group.DisplayName}\nStored type name. Field types differ or cannot be read — the picker is unconstrained."
                : $"{group.DisplayName}\nStored type name. Constrained to {string.Join(", ", constraint.Types.Select(type => type.FullName))}.")
                + $"\n{SerializeReferenceProjectSummary.ScanTimeNote}";

            fixAll.AddLeadingContent(BuildGroupHeaderRow(
                group.ShortName,
                SerializeReferenceProjectSummary.BuildTypeNameCountText(group),
                StatusStyle.Type.Warning));
            card.AddChild(fixAll);

            AddGroupDivider(card, withSweep: true);

            if (group.TryGetSuggestion(constraint, out var suggestion))
            {
                card.AddChild(BuildGroupActionRow(
                    $"Smart Fix → {GetQualifiedTitle(suggestion)}",
                    $"The only compatible type named {suggestion.Name}: replace the stored type name with " +
                    $"{suggestion.AssemblyQualifiedName}.",
                    info: false,
                    () => ApplyTypeNameFix(group, suggestion)));
            }

            AddPagedRows(card, "name|" + MissingTypeNames.GroupKey(group.TypeName), group.Entries, BuildTypeNameEntryRow);

            return card;
        }

        private VisualElement BuildTypeNameEntryRow(MissingTypeNameLocation entry)
        {
            var path = BuildPathLabel(entry.AssetPath);

            var field = MakeSelectable(new Label(SerializeReferenceProjectSummary.DescribeTypeNameField(entry.Entry))
                .AddClass(GroupEntryFieldClass));
            field.tooltip = entry.Entry.TypeName;

            return BuildEntryRow(entry.AssetPath, path, field);
        }

        private VisualElement BuildMigrationRow(MissingReferenceGroup group, Type target) =>
            BuildGroupActionRow(
                $"Migrate all ({group.Entries.Count}) → {GetQualifiedTitle(target)}",
                $"Every entry resolves to {target.FullName} via its declared [MovedFrom] — Unity already " +
                "migrates them in memory when the asset loads. Migrating rewrites the stored type name in the " +
                "files so they match the code; the attribute can be removed once no file stores the old name.",
                info: true,
                () => ApplyGroupFix(group, target));

        private VisualElement BuildGroupActionRow(string text, string tooltipText, bool info, Action onClick)
        {
            var row = new Label(text).AddClass(GroupActionClass);
            if (info) row.AddClass(GroupActionInfoClass);
            row.tooltip = tooltipText;
            row.RegisterCallback<ClickEvent>(_ => onClick());
            RegisterNavTarget(row, onClick);
            return row;
        }

        // The first rows of a card, then a Show more row for the rest. Show more rebuilds the list instead of
        // appending, so keyboard navigation keeps the order of the rows on screen.
        private void AddPagedRows<T>(
            VisualElement card, string key, IReadOnlyList<T> entries, Func<T, VisualElement> buildRow)
        {
            var shown = SerializeReferenceProjectSummary.GetShownRows(_shownRows.GetValueOrDefault(key), entries.Count);
            var focusIndex = _focusRow.Key == key ? _focusRow.Index : -1;

            for (var i = 0; i < shown; i++)
            {
                var row = buildRow(entries[i]);
                card.AddChild(row);

                if (i == focusIndex) _ring.Focus(row, scrollTo: false);
            }

            if (shown == entries.Count) return;

            card.AddChild(BuildShowMore(
                key,
                shown,
                SerializeReferenceProjectSummary.GetNextShownRows(shown),
                SerializeReferenceProjectSummary.BuildShowMoreText(shown, entries.Count),
                SerializeReferenceProjectSummary.BuildShowMoreTooltip(shown, entries.Count)));
        }

        // Enter on the row moves the keyboard focus to the first item the page adds, so the next arrow key continues
        // from there. A click rebuilds the list without restoring a focused item, so the next arrow key starts again
        // from the Scan button.
        private Label BuildShowMore(string key, int shown, int next, string text, string tooltip)
        {
            void ShowMore(bool focusFirstNewItem)
            {
                _shownRows[key] = next;
                if (focusFirstNewItem) _focusRow = (key, shown);

                _picker.Close();
                RerenderList();
                _focusRow = default;
            }

            var more = new Label(text).AddClass(GroupMoreClass);
            more.tooltip = tooltip;
            more.RegisterCallback<ClickEvent>(_ => ShowMore(focusFirstNewItem: false));
            RegisterNavTarget(more, () => ShowMore(focusFirstNewItem: true));

            return more;
        }

        // One card per file under the section heading, headed by the file's path and its entry count, so a row names
        // only what differs: the field of a required violation, the type of an override. The files keep the order they
        // first appear in. The cards are paged like the rows of a card, then a Show more files row offers the rest.
        private void AddFileCards<T>(VisualElement container, string key, IReadOnlyList<T> entries, Func<T, string> pathOf,
            Func<T, VisualElement> buildRow)
        {
            var files = entries.GroupBy(pathOf, StringComparer.Ordinal).ToList();
            var filesKey = key + "|files";
            var shown = SerializeReferenceProjectSummary.GetShownFileCards(_shownRows.GetValueOrDefault(filesKey), files.Count);
            var focusIndex = _focusRow.Key == filesKey ? _focusRow.Index : -1;

            for (var i = 0; i < shown; i++)
            {
                var file = files[i];
                var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                    .AddClass(GroupClass);

                var rows = file.ToList();
                var count = new Label(BuildCountText(rows.Count, "entry"))
                    .AddClass(GroupCountClass)
                    .SetPickingMode(PickingMode.Ignore);

                var header = BuildEntryRow(file.Key, BuildPathLabel(file.Key), count).AddClass(FileHeaderClass);
                card.AddChild(header);
                AddGroupDivider(card, withSweep: false);
                AddPagedRows(card, $"{key}|{file.Key}", rows, buildRow);

                container.AddChild(card);

                if (i == focusIndex) _ring.Focus(header, scrollTo: false);
            }

            if (shown == files.Count) return;

            container.AddChild(BuildShowMore(
                    filesKey,
                    shown,
                    SerializeReferenceProjectSummary.GetNextShownFileCards(shown),
                    SerializeReferenceProjectSummary.BuildShowMoreFilesText(shown, files.Count),
                    SerializeReferenceProjectSummary.BuildShowMoreFilesTooltip(shown, files.Count))
                .AddClass(FilesMoreClass));
        }

        private void AddRequiredFileCards(IReadOnlyList<GateViolation> violations) =>
            AddFileCards(_list, "required", violations, violation => violation.AssetPath, BuildRequiredViolationRow);

        // Report-only: Fix all edits RefIds entries, and an override type lives in the PrefabInstance document instead.
        private void AddOverrideFileCards(IReadOnlyList<MissingReferenceLocation> overrides) =>
            AddFileCards(_list, "overrides", overrides, entry => entry.AssetPath, BuildOverrideEntryRow);

        private VisualElement BuildOverrideEntryRow(MissingReferenceLocation entry)
        {
            var stored = entry.Entry.StoredType;
            var target = MissingReferenceGroup.OverrideMigrationTarget(entry);

            var type = MakeSelectable(new Label(target is null
                    ? stored.Class
                    : $"{stored.Class} → {GetQualifiedTitle(target)}")
                .AddClass(GroupEntryNameClass));
            type.tooltip = target is null
                ? $"{stored.DisplayName}\nSet by a prefab instance override. Select the instance and pick a new type " +
                  "in its Inspector, or Revert the override."
                : $"{stored.DisplayName}\nPending migration to {target.FullName}: Unity migrates it at load through " +
                  "[MovedFrom], but Migrate all does not rewrite prefab instance overrides. Keep the attribute until " +
                  "the instance is saved with the new name, or Revert the override.";

            var rid = MakeSelectable(new Label($"rid {entry.Entry.Rid}").AddClass(GroupEntryRidClass));

            return BuildEntryRow(entry.AssetPath, type, rid);
        }

        private static VisualElement BuildGroupHeaderRow(string title, string countText, StatusStyle.Type status)
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

        // One row per file: the rids of a file share its row, so a file with several broken entries is listed once.
        private VisualElement BuildGroupFileRow(SerializeReferenceProjectSummary.FileRids file)
        {
            var rids = MakeSelectable(new Label(SerializeReferenceProjectSummary.DescribeRids(file.Rids))
                .AddClass(GroupEntryRidClass));
            if (file.Rids.Count > 1) rids.tooltip = string.Join("\n", file.Rids.Select(rid => $"rid {rid}"));

            return BuildEntryRow(file.AssetPath, BuildPathLabel(file.AssetPath), rids);
        }

        // The folder is dimmed and gives way first, so the file name stays readable in a narrow window; the tooltip and
        // Copy Path keep the whole path.
        private static VisualElement BuildPathLabel(string assetPath)
        {
            var slash = assetPath.LastIndexOf('/');

            var path = new VisualElement().AddClass(GroupEntryPathClass);
            path.tooltip = assetPath;

            if (slash >= 0)
                path.AddChild(MakeSelectable(new Label(assetPath[..(slash + 1)]).AddClass(GroupEntryFolderClass)));

            return path.AddChild(MakeSelectable(new Label(assetPath[(slash + 1)..]).AddClass(GroupEntryFileClass)));
        }

        private VisualElement BuildRequiredViolationRow(GateViolation violation)
        {
            var field = MakeSelectable(new Label(violation.FieldLabel).AddClass(GroupEntryNameClass));

            return BuildEntryRow(violation.AssetPath, field, right: null);
        }

        private VisualElement BuildEntryRow(string assetPath, VisualElement left, Label right)
        {
            var row = new VisualElement().AddClass(GroupEntryClass);
            row.AddChild(left);
            if (right is not null) row.AddChild(right);

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

            evt.menu.AppendAction("Copy Path", _ => EditorGUIUtility.systemCopyBuffer = assetPath);

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
