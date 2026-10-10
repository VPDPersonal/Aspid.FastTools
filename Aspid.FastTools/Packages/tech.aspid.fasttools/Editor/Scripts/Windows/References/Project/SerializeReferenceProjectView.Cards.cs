using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Aspid.FastTools.Types.Editors;
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
        private const string GroupMoreClass = RootClass + "__group-more";
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

            AddPagedRows(card, "missing|" + SerializeReferenceHelpers.StoredTypeKey(group.StoredType), group.Entries,
                BuildGroupEntryRow);

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
            fixAll.tooltip = mixed
                ? $"{group.DisplayName}\nStored type name. Field types differ or cannot be read — the picker is unconstrained."
                : $"{group.DisplayName}\nStored type name. Constrained to {string.Join(", ", constraint.Types.Select(type => type.FullName))}.";

            fixAll.AddLeadingContent(BuildGroupHeaderRow(
                group.ShortName,
                SerializeReferenceProjectSummary.BuildTypeNameCountText(group),
                StatusStyle.Type.Warning,
                isStatic: false));
            card.AddChild(fixAll);

            AddGroupDivider(card, withSweep: true);

            if (group.TryGetSuggestion(constraint, out var suggestion))
            {
                card.AddChild(BuildGroupActionRow(
                    $"Smart Fix → {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion)}",
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
            var path = MakeSelectable(new Label(entry.AssetPath).AddClass(GroupEntryPathClass));
            path.tooltip = entry.AssetPath;

            var field = MakeSelectable(new Label(SerializeReferenceProjectSummary.DescribeTypeNameField(entry.Entry))
                .AddClass(GroupEntryFieldClass));
            field.tooltip = entry.Entry.TypeName;

            return BuildEntryRow(entry.AssetPath, path, field);
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

            // A click leaves the keyboard focus alone. Enter on the row moves it to the first row the page adds, so
            // the next arrow key continues from there instead of from the Scan button.
            void ShowMore(bool focusFirstNewRow)
            {
                _shownRows[key] = SerializeReferenceProjectSummary.GetNextShownRows(shown);
                if (focusFirstNewRow) _focusRow = (key, shown);

                _picker.Close();
                RerenderList();
                _focusRow = default;
            }

            var more = new Label(SerializeReferenceProjectSummary.BuildShowMoreText(shown, entries.Count))
                .AddClass(GroupMoreClass);
            more.tooltip = SerializeReferenceProjectSummary.BuildShowMoreTooltip(shown, entries.Count);
            more.RegisterCallback<ClickEvent>(_ => ShowMore(focusFirstNewRow: false));
            RegisterNavTarget(more, () => ShowMore(focusFirstNewRow: true));

            card.AddChild(more);
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

            AddPagedRows(card, "required", violations, BuildRequiredViolationRow);

            return card;
        }

        // Report-only: Fix all edits RefIds entries, and an override type lives in the PrefabInstance document instead.
        private VisualElement BuildOverrideGroupCard(IReadOnlyList<MissingReferenceLocation> overrides)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var files = overrides.Select(entry => entry.AssetPath).Distinct(StringComparer.Ordinal).Count();
            var allPending = overrides.All(entry => MissingReferenceGroup.OverrideMigrationTarget(entry) is not null);

            card.AddChild(BuildGroupHeaderRow(
                "Prefab instance overrides",
                $"{BuildCountText(overrides.Count, "entry")} · {(files == 1 ? "1 file" : $"{files} files")}",
                allPending ? StatusStyle.Type.Info : StatusStyle.Type.Warning,
                isStatic: true));

            AddGroupDivider(card, withSweep: false);

            AddPagedRows(card, "overrides", overrides, BuildOverrideEntryRow);

            return card;
        }

        private VisualElement BuildOverrideEntryRow(MissingReferenceLocation entry)
        {
            var path = MakeSelectable(new Label(entry.AssetPath).AddClass(GroupEntryPathClass));
            path.tooltip = entry.AssetPath;

            var stored = entry.Entry.StoredType;
            var target = MissingReferenceGroup.OverrideMigrationTarget(entry);

            var type = MakeSelectable(new Label(target is null
                    ? $"{stored.Class} · rid {entry.Entry.Rid}"
                    : $"{stored.Class} → {target.Name} · rid {entry.Entry.Rid}")
                .AddClass(GroupEntryFieldClass));
            type.tooltip = target is null
                ? $"{stored.DisplayName}\nSet by a prefab instance override. Select the instance and pick a new type " +
                  "in its Inspector, or Revert the override."
                : $"{stored.DisplayName}\nPending migration to {target.FullName}: Unity migrates it at load through " +
                  "[MovedFrom], but Migrate all does not rewrite prefab instance overrides. Keep the attribute until " +
                  "the instance is saved with the new name, or Revert the override.";

            return BuildEntryRow(entry.AssetPath, path, type);
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

        private VisualElement BuildRequiredViolationRow(GateViolation violation)
        {
            var path = MakeSelectable(new Label(violation.AssetPath).AddClass(GroupEntryPathClass));
            path.tooltip = violation.AssetPath;

            var field = MakeSelectable(new Label(violation.FieldLabel).AddClass(GroupEntryFieldClass));

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
