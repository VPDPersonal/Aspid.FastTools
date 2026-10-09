using System.Text;
using System.Linq;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceProjectSummary
    {
        private const int MaxPreviewedEntries = 8;

        private const string RequiredNotScannedText = "Required fields have not been checked yet — Rescan to include them.";
        private const string RequiredDisabledText =
            "Required fields are not checked while gate severity is Off — set it to Warn or Fail in Project Settings → " +
            "Aspid.FastTools → SerializeReference, then Rescan.";
        private const string TypeNamesNotScannedText = "Type names and required fields have not been checked yet — Rescan to include them.";
        private const string StaleScanDataText = "Type names and required violations are as of the last scan — Rescan to refresh them.";

        // Only a sweep that actually ran the required check can call the project clean. A sweep skipped because
        // severity was Off has not checked anything once severity is on again.
        public static RequiredAuditState GetRequiredAuditState(bool scanned, bool checkDisabled, bool severityOff) =>
            !scanned || (checkDisabled && !severityOff) ? RequiredAuditState.NotScanned
            : checkDisabled ? RequiredAuditState.Disabled
            : RequiredAuditState.Checked;

        public static string BuildRequiredNotCheckedText(RequiredAuditState state) =>
            state == RequiredAuditState.Disabled ? RequiredDisabledText : RequiredNotScannedText;

        // Type names are read only by Scan Project, so a window that shows the index of another feature (Unity Search,
        // Asset References) has not checked them.
        public static string BuildNotCheckedText(RequiredAuditState state, bool typeNamesScanned) =>
            typeNamesScanned ? BuildRequiredNotCheckedText(state) : TypeNamesNotScannedText;

        // Binary files and Git LFS pointers are not text YAML, so no scan reads them.
        public static string BuildUnreadFilesText(int count) =>
            $"{(count == 1 ? "1 binary or Git LFS file was" : $"{count} binary or Git LFS files were")} not checked — see the Console.";

        // A read-only or locked file logs its reason to the Console; an entry that changed since the scan is left
        // alone without a log, so the text names both causes.
        public static string BuildNotRewrittenText(int count, string verb = "rewritten") =>
            $"{count} could not be {verb} — a file is read-only or locked (see the Console), or the entry changed since the scan.";

        // The parts of a summary body that apply, in one paragraph; a part that is null or empty is left out.
        public static string JoinSentences(params string[] sentences) =>
            string.Join(" ", sentences.Where(sentence => !string.IsNullOrEmpty(sentence)));

        // Only a sweep that read every file and checked every kind of problem can call the project clean.
        private static bool IsFullyChecked(RequiredAuditState state, bool typeNamesScanned, int unreadFileCount) =>
            state == RequiredAuditState.Checked && typeNamesScanned && unreadFileCount == 0;

        public static (bool Success, string Title, string Message) BuildNothingFoundState(
            RequiredAuditState state, bool typeNamesScanned = true, int unreadFileCount = 0)
        {
            if (IsFullyChecked(state, typeNamesScanned, unreadFileCount))
                return (true, "Project clean", "No missing managed references, type names or unset required fields found anywhere under Assets/.");

            var found = state == RequiredAuditState.Checked && typeNamesScanned
                ? "No missing managed references, type names or unset required fields"
                : typeNamesScanned
                    ? "No missing managed references or type names"
                    : "No missing managed references";

            // Without Scan Project the number of unread files is unknown, so "anywhere under Assets/" would claim too much.
            var wherever = unreadFileCount > 0 || !typeNamesScanned ? "in the files that could be read" : "anywhere under Assets/";
            var message = $"{found} found {wherever}.";

            if (state != RequiredAuditState.Checked || !typeNamesScanned)
                message += " " + BuildNotCheckedText(state, typeNamesScanned);

            if (unreadFileCount > 0)
                message += " " + BuildUnreadFilesText(unreadFileCount);

            return (false, "No missing references", message);
        }

        public static StatusStyle.Type GetMissingReferencesCleanStatus(
            RequiredAuditState state, bool hasRequiredViolations, bool typeNamesScanned = true, int unreadFileCount = 0) =>
            hasRequiredViolations ? StatusStyle.Type.Warning
            : IsFullyChecked(state, typeNamesScanned, unreadFileCount) ? StatusStyle.Type.Success
            : StatusStyle.Type.Info;

        public static string BuildMissingReferencesCleanHintText(
            RequiredAuditState state, bool hasRequiredViolations, bool typeNamesScanned = true, int unreadFileCount = 0)
        {
            var hint =
                state != RequiredAuditState.Checked || !typeNamesScanned ? BuildNotCheckedText(state, typeNamesScanned)
                : hasRequiredViolations ? "Click a required-violation row to jump to its asset."
                : unreadFileCount > 0 ? "Nothing left to repair in the files that were read."
                : "Nothing left to repair. Rescan to sweep the project again and confirm it's clean.";

            if (typeNamesScanned && hasRequiredViolations)
                hint += " " + StaleScanDataText;

            return unreadFileCount > 0 ? hint + " " + BuildUnreadFilesText(unreadFileCount) : hint;
        }

        public static string BuildResultsHeaderText(int brokenCount, int migrationCount, int requiredCount, int typeNameCount = 0)
        {
            var parts = new List<string>(4);
            if (brokenCount > 0) parts.Add(BuildCountText(brokenCount, "missing reference"));
            if (typeNameCount > 0) parts.Add(BuildCountText(typeNameCount, "missing type name"));
            if (migrationCount > 0) parts.Add(BuildCountText(migrationCount, "pending migration"));
            if (requiredCount > 0) parts.Add(BuildCountText(requiredCount, "required violation"));

            return string.Join(", ", parts);
        }

        // Type names and required violations are read by Scan Project and refreshed only by this window's own edits,
        // so a hint that follows them says they may be out of date.
        public static string BuildResultsHintText(
            bool hasRequiredViolations,
            RequiredAuditState state,
            bool hasOverrides = false,
            bool hasBrokenGroups = true,
            bool hasTypeNames = false,
            bool typeNamesScanned = true,
            int unreadFileCount = 0)
        {
            var parts = new List<string>(5);

            if (hasBrokenGroups)
                parts.Add("Each group is a broken stored type — Fix all re-points its every entry to one replacement, or to <None>.");

            if (hasOverrides)
                parts.Add("Prefab instance overrides are fixed on the instance: pick a new type or Revert the override.");

            if (state != RequiredAuditState.Checked || !typeNamesScanned)
                parts.Add(BuildNotCheckedText(state, typeNamesScanned));
            else if (hasRequiredViolations)
                parts.Add("Click a required-violation row to jump to its asset.");

            if (typeNamesScanned && (hasTypeNames || hasRequiredViolations))
                parts.Add(StaleScanDataText);

            if (unreadFileCount > 0)
                parts.Add(BuildUnreadFilesText(unreadFileCount));

            return string.Join(" ", parts);
        }

        public static string BuildGroupCountText(MissingReferenceGroup group)
        {
            var entries = group.Entries.Count;
            var files = group.FileCount;
            var entryText = entries == 1 ? "1 entry" : $"{entries} entries";
            var fileText = files == 1 ? "1 file" : $"{files} files";
            return $"{entryText} · {fileText}";
        }

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
