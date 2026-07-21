using System.Linq;
using Unity.Pipeline.Commands;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Editors
{
    /// <summary>
    /// Dev-only Unity CLI wrapper over the SerializeReference gate scanner: <c>unity command sr_gate</c> returns the
    /// violations as structured JSON without a batchmode Editor relaunch, unlike <see cref="SerializeReferenceCiGate"/>.
    /// </summary>
    internal static class SerializeReferenceGateCommands
    {
        [CliCommand("sr_gate", "Scan the project for SerializeReference gate violations (missing types / unset Required fields)")]
        internal static object Run(
            [CliArg("scope", "What to scan: missing | required | full")] string scope = "missing",
            [CliArg("warn_only", "Force Warn severity: report but exit code 0")] bool warnOnly = false,
            [CliArg("fail", "Force Fail severity: exit code 1 on violations")] bool fail = false)
        {
            var options = scope?.ToLowerInvariant() switch
            {
                "missing" => GateOptions.MissingOnly,
                "required" => GateOptions.RequiredOnly,
                "full" => GateOptions.Full,
                _ => default,
            };

            if (options.Equals(default(GateOptions)))
                return new { success = false, error = $"Unknown scope '{scope}'. Use: missing | required | full." };

            var severity = SerializeReferenceCiGate.ResolveSeverity(SerializeReferenceSettings.BuildSeverity, warnOnly, fail);
            var violations = SerializeReferenceGateScanner.Scan(options);

            return new
            {
                success = true,
                scope,
                severity = severity.ToString(),
                violationCount = violations.Count,
                exitCode = SerializeReferenceCiGate.ComputeExitCode(violations.Count, severity),
                violations = violations.Select(violation => new
                {
                    kind = violation.Kind.ToString(),
                    assetPath = violation.AssetPath,
                    fieldPath = violation.FieldPath,
                    storedType = violation.StoredType.Class,
                    fileId = violation.FileId,
                    rid = violation.Rid,
                }).ToArray(),
            };
        }
    }
}
