using System.Collections.Generic;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceGraphSummary
    {
        // A binary file, an unfetched LFS pointer and a file that cannot be opened are never read, which is not the same
        // as holding no references. Each has its own cause and way out: Force Text helps only a binary file.
        public static string BuildUnreadAssetMessage(AssetFileFormat format) => format switch
        {
            AssetFileFormat.LfsPointer =>
                "This asset is a Git LFS pointer, so its managed references were not read. Fetch the LFS objects to map them.",
            AssetFileFormat.Unreadable =>
                "This asset could not be opened, so its managed references were not read. Another program may have it " +
                "locked, or you may lack permission to read it. Close that program and select the asset again.",
            _ =>
                "This asset is not stored as text YAML, so its managed references were not read. Set Asset " +
                "Serialization → Mode to Force Text and save it again.",
        };

        public static string BuildOverviewTitle(int broken, int orphans, int migrations, int required)
        {
            var parts = new List<string>(4);
            if (broken > 0) parts.Add(BuildCountText(broken, "missing reference"));
            if (orphans > 0) parts.Add(BuildCountText(orphans, "orphaned reference"));
            if (required > 0) parts.Add(BuildCountText(required, "required violation"));
            if (migrations > 0) parts.Add(BuildCountText(migrations, "pending migration"));

            return parts.Count > 0 ? string.Join(", ", parts) : "No missing references";
        }

        // How to act on what the title counts; the view puts it in the title's tooltip.
        public static string BuildOverviewGuide(int missing, int orphans, int migrations, int required)
        {
            var broken = missing - migrations;

            return broken > 0 ? "Fix a missing type inline from its card."
                : required > 0 ? "Assign each required field from its amber card."
                : migrations > 0 ? "Migrate a renamed type from its card — the Inspector already loads it; only the file is stale."
                : orphans > 0 ? "Clear an orphaned rid from its card."
                : string.Empty;
        }

        public static string BuildDocumentCountText(ReferenceGraphDocument document, int broken, int migrations)
        {
            var total = document.Nodes.Count;
            var orphans = document.Orphans.Count;

            var text = total == 1 ? "1 reference" : $"{total} references";
            if (broken > 0) text += $" · {broken} missing";
            if (migrations > 0) text += migrations == 1 ? " · 1 migration" : $" · {migrations} migrations";
            if (orphans > 0) text += orphans == 1 ? " · 1 orphaned" : $" · {orphans} orphaned";
            return text;
        }
    }
}
