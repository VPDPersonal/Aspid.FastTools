using System.Collections.Generic;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceGraphSummary
    {
        public static string BuildOverviewTitle(int broken, int orphans, int migrations, int required)
        {
            var parts = new List<string>(4);
            if (broken > 0) parts.Add(BuildCountText(broken, "missing reference"));
            if (orphans > 0) parts.Add(BuildCountText(orphans, "orphaned reference"));
            if (required > 0) parts.Add(BuildCountText(required, "required violation"));
            if (migrations > 0) parts.Add(BuildCountText(migrations, "pending migration"));

            return parts.Count > 0 ? string.Join(", ", parts) : "No missing references";
        }

        public static string BuildOverviewHint(int total, int missing, int orphans, int empties, int migrations, int required)
        {
            var references = total == 1 ? "1 managed reference" : $"{total} managed references";
            var emptyNote = empties switch
            {
                0 => string.Empty,
                1 => " · 1 unassigned field",
                _ => $" · {empties} unassigned fields"
            };

            if (missing == 0 && orphans == 0 && required == 0)
                return $"{references} mapped{emptyNote} — every [SerializeReference] type resolves.";

            var broken = missing - migrations;

            var parts = new List<string>(5);
            if (broken > 0) parts.Add(broken == 1 ? "1 missing type" : $"{broken} missing types");
            if (migrations > 0) parts.Add(migrations == 1 ? "1 pending [MovedFrom] migration" : $"{migrations} pending [MovedFrom] migrations");
            if (orphans > 0) parts.Add(orphans == 1 ? "1 orphaned rid" : $"{orphans} orphaned rids");
            if (required > 0) parts.Add(required == 1 ? "1 required field unassigned" : $"{required} required fields unassigned");
            if (empties > 0) parts.Add(empties == 1 ? "1 unassigned field" : $"{empties} unassigned fields");

            var action = broken > 0
                ? "Fix a missing type inline from its card."
                : required > 0
                    ? "Assign each required field from its amber card."
                    : migrations > 0
                        ? "Migrate a renamed type from its card — the Inspector already loads it; only the file is stale."
                        : "Clear an orphaned rid from its card.";

            return $"{references} mapped · {string.Join(" · ", parts)}. {action}";
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
