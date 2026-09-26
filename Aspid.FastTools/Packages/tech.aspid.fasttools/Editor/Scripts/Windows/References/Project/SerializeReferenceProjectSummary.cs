using System.Text;
using System.Collections.Generic;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceProjectSummary
    {
        private const int MaxPreviewedEntries = 8;

        private const string RequiredNotScannedText = "Required fields have not been checked yet — Rescan to include them.";
        private const string RequiredDisabledText =
            "Required fields are not checked while gate severity is Off (Project Settings → Aspid.FastTools → SerializeReference).";

        // Only a sweep that actually ran the required check can call the project clean.
        public static RequiredAuditState GetRequiredAuditState(bool scanned, bool checkDisabled) =>
            !scanned ? RequiredAuditState.NotScanned
            : checkDisabled ? RequiredAuditState.Disabled
            : RequiredAuditState.Checked;

        public static string BuildRequiredNotCheckedText(RequiredAuditState state) =>
            state == RequiredAuditState.Disabled ? RequiredDisabledText : RequiredNotScannedText;

        public static (bool Success, string Title, string Message) BuildNothingFoundState(RequiredAuditState state) =>
            state == RequiredAuditState.Checked
                ? (true, "Project clean", "No missing managed references or unset required fields found anywhere under Assets/.")
                : (false, "No missing references",
                    "No missing managed references found anywhere under Assets/. " + BuildRequiredNotCheckedText(state));

        public static string BuildMissingReferencesCleanHintText(RequiredAuditState state) =>
            state == RequiredAuditState.Checked
                ? "Nothing left to repair. Rescan to sweep the project again and confirm it's clean."
                : BuildRequiredNotCheckedText(state);

        public static string BuildResultsHeaderText(int brokenCount, int migrationCount, int requiredCount)
        {
            var parts = new List<string>(3);
            if (brokenCount > 0) parts.Add(BuildCountText(brokenCount, "missing reference"));
            if (migrationCount > 0) parts.Add(BuildCountText(migrationCount, "pending migration"));
            if (requiredCount > 0) parts.Add(BuildCountText(requiredCount, "required violation"));

            return string.Join(", ", parts);
        }

        public static string BuildResultsHintText(bool hasRequiredViolations, RequiredAuditState state)
        {
            const string hint = "Each group is a broken stored type — Fix all re-points its every entry to one replacement, or to <None>.";

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
