using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Headless CI entry point. Invoke with
    /// <c>Unity -batchmode -quit -projectPath . -executeMethod Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceCiGate.RunCheck</c>.
    /// Scans the project, writes a report, logs each violation, and exits non-zero when violations exist so a pipeline
    /// can fail the job. Flags: <c>-srGateReport &lt;path&gt;</c> (report file), <c>-srGateRequired</c> (also scan
    /// unset-required fields), <c>-srGateWarnOnly</c> (always exit 0).
    /// </summary>
    internal static class SerializeReferenceCiGate
    {
        private const string DefaultReportPath = "SerializeReferenceGateReport.txt";

        // ReSharper disable once UnusedMember.Global — invoked via -executeMethod.
        public static void RunCheck()
        {
            if (!Application.isBatchMode)
            {
                Debug.LogWarning("[Aspid FastTools] SerializeReferenceCiGate.RunCheck is intended for -batchmode; ignoring.");
                return;
            }

            int exitCode;
            try
            {
                var args = Environment.GetCommandLineArgs();
                var reportPath = GetArgValue(args, "-srGateReport") ?? DefaultReportPath;
                var scanRequired = HasFlag(args, "-srGateRequired");
                var warnOnly = HasFlag(args, "-srGateWarnOnly");

                var options = scanRequired ? GateOptions.Full : GateOptions.MissingOnly;
                var violations = SerializeReferenceGateScanner.Scan(options);

                File.WriteAllText(reportPath, BuildReport(violations));
                foreach (var violation in violations)
                    Debug.LogError($"[Aspid FastTools] {violation}");

                exitCode = ComputeExitCode(violations.Count, warnOnly);
                Debug.Log($"[Aspid FastTools] Gate check complete: {violations.Count} violation(s), exit code {exitCode}. Report: {reportPath}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Gate check failed: {exception}");
                exitCode = 2; // distinguish an internal failure from a clean violation result
            }

            EditorApplication.Exit(exitCode);
        }

        /// <summary>0 when clean or warn-only; 1 when violations exist. Extracted so it is unit-testable without exiting.</summary>
        internal static int ComputeExitCode(int violationCount, bool warnOnly) =>
            warnOnly || violationCount == 0 ? 0 : 1;

        internal static string BuildReport(IReadOnlyList<GateViolation> violations)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"# SerializeReference Gate Report");
            builder.AppendLine($"# Violations: {violations.Count}");
            builder.AppendLine();

            foreach (var violation in violations)
            {
                // Machine-readable line: KIND<TAB>assetPath<TAB>fileId<TAB>rid<TAB>StoredType<TAB>fieldPath
                builder.Append(violation.Kind).Append('\t')
                    .Append(violation.AssetPath).Append('\t')
                    .Append(violation.FileId).Append('\t')
                    .Append(violation.Rid).Append('\t')
                    .Append(violation.StoredType.Class ?? string.Empty).Append('\t')
                    .Append(violation.FieldPath ?? string.Empty)
                    .AppendLine();
            }

            return builder.ToString();
        }

        private static string GetArgValue(string[] args, string flag)
        {
            for (var i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];

            return null;
        }

        private static bool HasFlag(string[] args, string flag)
        {
            foreach (var arg in args)
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }
    }
}
