using System.Text;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceBreakageNotificationController
    {
        private const string ShownPrefix = "Aspid.FastTools.SerializeReferences.Breakage.Shown.";
        private const double FadeOutSeconds = 5.0;

        // Both detectors run on the same import, so their messages are collected and shown as one toast.
        private static readonly List<(string Message, bool IsWarning)> _pending = new();
        private static bool _flushScheduled;

        [InitializeOnLoadMethod]
        private static void Hook()
        {
            SerializeReferenceBreakageDetector.BreakageDetected += OnBreakageDetected;
            TypeNameBreakageDetector.BreakageDetected += OnTypeNamesBroken;
        }

        private static void OnTypeNamesBroken(TypeNameBreakageReport report)
        {
            if (!report.HasAny || Application.isBatchMode) return;
            if (!SerializeReferenceSettings.BreakageDetectionEnabled) return;

            var shownKey = ShownPrefix + ProjectId() + ".names." + string.Join(";", report.TypeNames).GetHashCode().ToString("X8");
            if (SessionState.GetBool(shownKey, false)) return;
            SessionState.SetBool(shownKey, true);

            var count = report.TypeNames.Count;
            var files = report.FileCount == 1 ? "1 file" : $"{report.FileCount} files";
            var message = count == 1
                ? $"SerializableType name {report.TypeNames[0]} just became missing in {files} — open Project References"
                : $"{count} SerializableType names just became missing in {files} — open Project References";

            Enqueue(message, isWarning: true);
        }

        private static void OnBreakageDetected(BreakageReport report)
        {
            if (!report.HasAny || Application.isBatchMode) return;
            if (!SerializeReferenceSettings.BreakageDetectionEnabled) return;

            var shownKey = ShownPrefix + ProjectId() + "." + ContentHash(report);
            if (SessionState.GetBool(shownKey, false)) return;
            SessionState.SetBool(shownKey, true);

            var count = report.Entries.Count;
            var typeWord = report.TypeCount == 1 ? "type" : "types";

            // A [MovedFrom]-resolvable entry is not really broken — Unity migrates it in memory at load; only the
            // file text is stale. Word it as a calm "migrate" invitation, not a data-loss alarm.
            var migratable = report.Entries.Count(entry => entry.MigrationTarget is not null);

            // The cold-index path emits one TYPE-level entry per broken type (no per-site data without the index),
            // so counting entries as "references" would misreport — those reports speak in types.
            var typeLevel = report.Entries.Count > 0 && report.Entries[0].AssetPath is null;

            var plural = count == 1 ? "" : "s";
            var message = typeLevel
                ? migratable == count
                    ? $"{report.TypeCount} managed-reference {typeWord} carr{(report.TypeCount == 1 ? "ies" : "y")} " +
                      "an outdated name after a [MovedFrom] rename — open Project References to migrate"
                    : $"{report.TypeCount} managed-reference {typeWord} just became missing — open Project References"
                : migratable == count
                    ? $"{count} managed reference{plural} carr{(count == 1 ? "ies" : "y")} an outdated type name " +
                      $"after a [MovedFrom] rename ({report.TypeCount} {typeWord}) — open Project References to migrate"
                    : migratable > 0
                        ? $"{count} managed reference{plural} became missing ({report.TypeCount} {typeWord}; " +
                          $"{migratable} auto-migratable after a [MovedFrom] rename) — open Project References"
                        : $"{count} managed reference{plural} became missing ({report.TypeCount} {typeWord}) " +
                          "— open Project References";

            Enqueue(message, isWarning: migratable != count);
        }

        private static void Enqueue(string message, bool isWarning)
        {
            _pending.Add((message, isWarning));
            if (_flushScheduled) return;

            _flushScheduled = true;
            EditorApplication.delayCall += Flush;
        }

        private static void Flush()
        {
            _flushScheduled = false;
            if (_pending.Count == 0) return;

            ShowToast(string.Join("\n", _pending.Select(entry => entry.Message)));

            foreach (var (message, isWarning) in _pending)
            {
                var console = $"[Aspid FastTools] {message}. Open Tools/Aspid \U0001F40D/FastTools/Project References.";
                if (isWarning) Debug.LogWarning(console);
                else Debug.Log(console);
            }

            _pending.Clear();
        }

        private static void ShowToast(string message)
        {
            var content = new GUIContent(message);

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.ShowNotification(content, FadeOutSeconds);
                sceneView.Repaint();
                return;
            }

            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (window == null) continue;
                window.ShowNotification(content, FadeOutSeconds);
                window.Repaint();
                return;
            }
        }

        private static string ContentHash(BreakageReport report)
        {
            var keys = new SortedSet<string>(System.StringComparer.Ordinal);
            foreach (var entry in report.Entries)
                keys.Add(SerializeReferenceHelpers.StoredTypeKey(entry.StoredType));

            var builder = new StringBuilder();
            foreach (var key in keys) builder.Append(key).Append(';');
            return builder.ToString().GetHashCode().ToString("X8");
        }

        private static string ProjectId() =>
            PlayerSettings.productGUID.ToString();
    }
}
