using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed class SerializeReferenceBuildGate : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // A pull may have changed the committed severity and excluded folders since the Editor loaded them.
            SerializeReferenceSettings.ReloadShared();

            var severity = SerializeReferenceSettings.BuildSeverity;
            if (severity == GateSeverity.Off) return;

            var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();
            var violations = SerializeReferenceGateScanner.Scan(GateOptions.MissingOnly, unscanned: unscanned);

            var notice = SerializeReferenceGateScanner.DescribeUnscanned(unscanned, EditorSettings.serializationMode);
            if (notice is not null) Debug.LogWarning(notice);

            if (violations.Count is 0) return;

            var summary = BuildSummary(violations);

            if (severity == GateSeverity.Fail)
                throw new BuildFailedException(summary);

            Debug.LogWarning(summary);
        }

        private static string BuildSummary(IReadOnlyList<GateViolation> violations)
        {
            var files = new HashSet<string>();
            var types = new HashSet<string>();
            var references = 0;

            foreach (var violation in violations)
            {
                files.Add(violation.AssetPath);

                if (violation.Kind == GateViolationKind.MissingTypeName)
                {
                    types.Add("name|" + MissingTypeNames.GroupKey(violation.TypeName));
                    continue;
                }

                references++;
                types.Add(SerializeReferenceHelpers.StoredTypeKey(violation.StoredType));
            }

            var names = violations.Count - references;
            var counts = names == 0
                ? $"{references} missing managed reference(s)"
                : references == 0
                    ? $"{names} missing type name(s)"
                    : $"{references} missing managed reference(s) and {names} missing type name(s)";

            var builder = new StringBuilder();
            builder.AppendLine($"[Aspid FastTools] {counts} across {files.Count} file(s), {types.Count} broken type(s):");

            foreach (var violation in violations)
                builder.AppendLine($"  {violation}");

            return builder.ToString();
        }
    }
}
