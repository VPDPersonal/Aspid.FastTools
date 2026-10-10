using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
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

            var exitCode = Execute(
                Environment.GetCommandLineArgs(),
                () => SerializeReferenceSettings.BuildSeverity,
                (options, unscanned) => SerializeReferenceGateScanner.Scan(options, unscanned: unscanned));

            EditorApplication.Exit(exitCode);
        }

        // The whole check without the batch-mode guard and the exit, so tests can run it with their own arguments and scan.
        internal static int Execute(
            string[] args,
            Func<GateSeverity> committedSeverity,
            Func<GateOptions, ICollection<(string AssetPath, AssetFileFormat Format)>, IReadOnlyList<GateViolation>> scan)
        {
            try
            {
                var reportPath = GetArgValue(args, "-srGateReport") ?? DefaultReportPath;
                var scanRequired = HasFlag(args, "-srGateRequired");
                var warnOnly = HasFlag(args, "-srGateWarnOnly");
                var failOverride = HasFlag(args, "-srGateFail");
                var strict = HasFlag(args, "-srGateStrict");

                var severity = ResolveSeverity(committedSeverity(), warnOnly, failOverride);
                if (severity == GateSeverity.Off)
                {
                    Debug.Log("[Aspid FastTools] SerializeReference gate severity is Off; CI check skipped.");
                    return 0;
                }

                var options = scanRequired ? GateOptions.Full : GateOptions.MissingOnly;
                var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();
                var violations = scan(options, unscanned);

                // Logged before the report is written: a report path that cannot be written still leaves them in the log.
                LogViolations(violations, severity);

                var serializationMode = EditorSettings.serializationMode;
                var notice = SerializeReferenceGateScanner.DescribeUnscanned(unscanned, serializationMode);
                if (notice is not null) Debug.LogWarning(notice);

                var uncheckedFiles = SerializeReferenceGateScanner.CountUnchecked(unscanned, serializationMode);
                if (strict && uncheckedFiles > 0)
                    Log(severity, $"[Aspid FastTools] -srGateStrict: {uncheckedFiles} file(s) were not checked.");

                File.WriteAllText(reportPath, BuildReport(violations, unscanned));

                var exitCode = ComputeExitCode(violations.Count, severity, uncheckedFiles, strict);
                Debug.Log($"[Aspid FastTools] Gate check complete: {violations.Count} violation(s), {unscanned.Count} file(s) not scanned, severity {severity}, exit code {exitCode}. Report: {reportPath}");
                return exitCode;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Aspid FastTools] Gate check failed: {exception}");
                return 2; // distinguish an internal failure from a clean violation result
            }
        }

        // Error lines only under Fail, where the exit code is 1: under Warn the exit code is 0, and a wrapper that
        // scans the log for errors would take them for a failure. The report lists every violation, the log a part.
        internal static void LogViolations(IReadOnlyList<GateViolation> violations, GateSeverity severity)
        {
            var listed = Math.Min(violations.Count, SerializeReferenceGateScanner.MaxListedViolations);
            for (var i = 0; i < listed; i++)
                Log(severity, $"[Aspid FastTools] {violations[i]}");

            if (violations.Count > listed)
                Log(severity, $"[Aspid FastTools] … and {violations.Count - listed} more violation(s); the report lists all of them.");
        }

        // No stack trace: every line of a long CI log would otherwise carry the same three frames.
        private static void Log(GateSeverity severity, string message)
        {
            var type = severity == GateSeverity.Fail ? LogType.Error : LogType.Warning;
            Debug.LogFormat(type, LogOption.NoStacktrace, context: null, "{0}", message);
        }

        public static GateSeverity ResolveSeverity(GateSeverity committed, bool warnOnly, bool failOverride)
        {
            if (warnOnly) return GateSeverity.Warn;
            return failOverride ? GateSeverity.Fail : committed;
        }

        // Under -srGateStrict, files the run could not check count as violations.
        public static int ComputeExitCode(int violationCount, GateSeverity severity, int uncheckedFiles = 0, bool strict = false)
        {
            var failures = violationCount + (strict ? uncheckedFiles : 0);
            return failures > 0 && severity == GateSeverity.Fail ? 1 : 0;
        }

        public static string BuildReport(
            IReadOnlyList<GateViolation> violations,
            IReadOnlyCollection<(string AssetPath, AssetFileFormat Format)> unscanned = null)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"# SerializeReference Gate Report");
            builder.AppendLine($"# Violations: {violations.Count}");
            builder.AppendLine($"# Not scanned (not text YAML): {unscanned?.Count ?? 0}");

            // Comment lines, so a parser of the violation lines below is unaffected.
            if (unscanned is not null)
            {
                foreach (var (assetPath, format) in unscanned)
                    builder.Append("#   ").Append(format).Append('\t').Append(assetPath).AppendLine();
            }

            builder.AppendLine();

            foreach (var violation in violations)
            {
                // Machine-readable line: KIND<TAB>assetPath<TAB>fileId<TAB>rid<TAB>StoredType<TAB>fieldPath<TAB>origin<TAB>ns<TAB>asm.
                // "override" marks a type set by a prefab instance override, whose fileId is the PrefabInstance. A
                // MissingTypeName row stores the whole type name, and its rid is the managed reference that holds the
                // field, or 0. ns and asm are the stored namespace and assembly of a MissingType row, empty otherwise.
                builder.Append(violation.Kind).Append('\t')
                    .Append(violation.AssetPath).Append('\t')
                    .Append(violation.FileId).Append('\t')
                    .Append(violation.Rid).Append('\t')
                    .Append(violation.StoredName).Append('\t')
                    .Append(violation.FieldPath ?? string.Empty).Append('\t')
                    .Append(violation.IsOverride ? "override" : string.Empty).Append('\t')
                    .Append(violation.StoredNamespace).Append('\t')
                    .Append(violation.StoredAssembly)
                    .AppendLine();
            }

            return builder.ToString();
        }

        private static string GetArgValue(string[] args, string flag)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }

            return null;
        }

        private static bool HasFlag(string[] args, string flag) =>
            args.Any(arg => string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase));
    }
}
