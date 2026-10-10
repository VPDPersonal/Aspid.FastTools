using System;
using System.Text;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceProjectSummary
    {
        // A card builds this many rows, then offers Show more, so thousands of entries stay cheap to build.
        public const int RowsPerPage = 200;

        private const int MaxPreviewedEntries = 8;

        private const string RequiredNotScannedText = "Required fields have not been checked yet — Rescan to include them.";
        private const string RequiredDisabledText =
            "Required fields are not checked while gate severity is Off — set it to Warn or Fail in Project Settings → " +
            "Aspid.FastTools → SerializeReference, then Rescan.";

        // Only a sweep that actually ran the required check can call the project clean. A sweep skipped because
        // severity was Off has not checked anything once severity is on again.
        public static RequiredAuditState GetRequiredAuditState(bool scanned, bool checkDisabled, bool severityOff) =>
            !scanned || (checkDisabled && !severityOff) ? RequiredAuditState.NotScanned
            : checkDisabled ? RequiredAuditState.Disabled
            : RequiredAuditState.Checked;

        public static string BuildRequiredNotCheckedText(RequiredAuditState state) =>
            state == RequiredAuditState.Disabled ? RequiredDisabledText : RequiredNotScannedText;

        public static (bool Success, string Title, string Message) BuildNothingFoundState(RequiredAuditState state) =>
            state == RequiredAuditState.Checked
                ? (true, "Project clean", "No missing managed references, type names or unset required fields found anywhere under Assets/.")
                : (false, "No missing references",
                    "No missing managed references or type names found anywhere under Assets/. " + BuildRequiredNotCheckedText(state));

        public static StatusStyle.Type GetMissingReferencesCleanStatus(RequiredAuditState state, bool hasRequiredViolations) =>
            hasRequiredViolations ? StatusStyle.Type.Warning
            : state == RequiredAuditState.Checked ? StatusStyle.Type.Success
            : StatusStyle.Type.Info;

        public static string BuildMissingReferencesCleanHintText(RequiredAuditState state, bool hasRequiredViolations) =>
            state != RequiredAuditState.Checked ? BuildRequiredNotCheckedText(state)
            : hasRequiredViolations ? "Click a required-violation row to jump to its asset."
            : "Nothing left to repair. Rescan to sweep the project again and confirm it's clean.";

        public static string BuildResultsHeaderText(int brokenCount, int migrationCount, int requiredCount, int typeNameCount = 0)
        {
            var parts = new List<string>(4);
            if (brokenCount > 0) parts.Add(BuildCountText(brokenCount, "missing reference"));
            if (typeNameCount > 0) parts.Add(BuildCountText(typeNameCount, "missing type name"));
            if (migrationCount > 0) parts.Add(BuildCountText(migrationCount, "pending migration"));
            if (requiredCount > 0) parts.Add(BuildCountText(requiredCount, "required violation"));

            return string.Join(", ", parts);
        }

        public static string BuildResultsHintText(bool hasRequiredViolations, RequiredAuditState state, bool hasOverrides = false)
        {
            var hint = "Each group is a broken stored type — Fix all re-points its every entry to one replacement, or to <None>.";

            if (hasOverrides)
                hint += " Prefab instance overrides are fixed on the instance: pick a new type or Revert the override.";

            if (state != RequiredAuditState.Checked) return hint + " " + BuildRequiredNotCheckedText(state);

            return hasRequiredViolations
                ? hint + " Click a required-violation row to jump to its asset."
                : hint;
        }

        public static string BuildGroupCountText(MissingReferenceGroup group)
        {
            var entries = group.Entries.Count;
            var files = group.FileCount;
            var entryText = entries == 1 ? "1 entry" : $"{entries} entries";
            var fileText = files == 1 ? "1 file" : $"{files} files";
            return $"{entryText} · {fileText}";
        }

        // The rows a card builds when `requested` rows were asked for: the first page at least, every entry at most.
        public static int GetShownRows(int requested, int total) =>
            Math.Min(total, Math.Max(requested, RowsPerPage));

        public static int GetNextShownRows(int shown) => shown + RowsPerPage;

        public static string BuildShowMoreText(int shown, int total) =>
            $"Show more ({shown} of {total} shown)";

        public static string BuildShowMoreTooltip(int shown, int total) =>
            $"Show the next {BuildCountText(Math.Min(RowsPerPage, total - shown), "row")}.";

        public static string BuildTypeNameCountText(MissingTypeNameGroup group)
        {
            var entries = group.Entries.Count;
            var files = group.FileCount;
            var entryText = entries == 1 ? "1 entry" : $"{entries} entries";
            var fileText = files == 1 ? "1 file" : $"{files} files";
            return $"{entryText} · {fileText} · type name";
        }

        public static string BuildTypeNameDiffPreview(IReadOnlyList<MissingTypeNameLocation> entries, string newName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Changes:");

            for (var i = 0; i < entries.Count && i < MaxPreviewedEntries; i++)
            {
                var entry = entries[i];
                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} ({DescribeTypeNameField(entry.Entry)}):");
                builder.AppendLine($"    - {entry.Entry.TypeName}");
                builder.AppendLine($"    + {newName}");
            }

            if (entries.Count > MaxPreviewedEntries)
                builder.AppendLine($"  …and {entries.Count - MaxPreviewedEntries} more");

            builder.AppendLine();
            return builder.ToString();
        }

        // "_weapon", "rid 1001 · _arrow", "_weapon · override".
        public static string DescribeTypeNameField(StoredTypeNameEntry entry)
        {
            var field = string.IsNullOrEmpty(entry.FieldPath) ? SerializeReferenceYamlEditor.TypeNameKey : entry.FieldPath;
            if (entry.Rid != 0) field = $"rid {entry.Rid} · {field}";
            return entry.IsOverride ? field + " · override" : field;
        }

        public static string BuildFixAllLabel(MissingReferenceGroup group, bool isMigration) =>
            $"{(isMigration ? "Reassign all" : "Fix all")} ({group.Entries.Count})  ▼";

        public static string BuildDiffPreview(IReadOnlyList<MissingReferenceLocation> entries, ManagedTypeName newType)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Changes:");

            // Failed preview computations must not inflate the hidden-entry remainder.
            var edits = new List<(MissingReferenceLocation entry, RewriteEdit edit)>(entries.Count);
            foreach (var entry in entries)
            {
                if (SerializeReferenceYamlEditor.TryComputeRewrite(entry.AssetPath, entry.Entry.FileId, entry.Entry.Rid, newType, out var edit))
                    edits.Add((entry, edit));
            }

            for (var i = 0; i < edits.Count && i < MaxPreviewedEntries; i++)
            {
                var (entry, edit) = edits[i];
                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} (rid {entry.Entry.Rid}):");
                builder.AppendLine($"    - {edit.OldLine.Trim()}");
                builder.AppendLine($"    + {edit.NewLine.Trim()}");
            }

            if (edits.Count > MaxPreviewedEntries)
                builder.AppendLine($"  …and {edits.Count - MaxPreviewedEntries} more");

            var uncomputable = entries.Count - edits.Count;
            if (uncomputable > 0)
                builder.AppendLine($"  ({uncomputable} entr{(uncomputable == 1 ? "y" : "ies")} could not be previewed)");

            builder.AppendLine();
            return builder.ToString();
        }

        public static string BuildClearPreview(IReadOnlyList<MissingReferenceLocation> entries)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Clears:");

            var shown = 0;
            foreach (var entry in entries)
            {
                if (shown >= MaxPreviewedEntries)
                {
                    builder.AppendLine($"  …and {entries.Count - shown} more");
                    break;
                }

                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} (rid {entry.Entry.Rid})");
                shown++;
            }

            builder.AppendLine();
            return builder.ToString();
        }
    }

    internal enum RequiredAuditState
    {
        NotScanned,
        Disabled,
        Checked,
    }
}
