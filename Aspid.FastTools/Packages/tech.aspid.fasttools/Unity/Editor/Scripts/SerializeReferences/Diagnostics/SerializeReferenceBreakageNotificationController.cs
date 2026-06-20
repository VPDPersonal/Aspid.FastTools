using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Surfaces a breakage report non-intrusively: a fade-out <see cref="EditorWindow.ShowNotification"/> toast that
    /// steals no focus, plus a single clickable warning in the console pointing at the Repair window. The same breakage
    /// set is shown at most once per session (a content hash in <see cref="SessionState"/>), so a recompile that
    /// re-detects the identical set does not nag.
    /// </summary>
    internal static class SerializeReferenceBreakageNotificationController
    {
        private const string ShownPrefix = "Aspid.FastTools.SerializeReferences.Breakage.Shown.";
        private const double FadeOutSeconds = 5.0;

        [InitializeOnLoadMethod]
        private static void Hook() => SerializeReferenceBreakageDetector.BreakageDetected += OnBreakageDetected;

        private static void OnBreakageDetected(BreakageReport report)
        {
            if (!report.HasAny || Application.isBatchMode) return;

            var shownKey = ShownPrefix + ProjectId() + "." + ContentHash(report);
            if (SessionState.GetBool(shownKey, false)) return;
            SessionState.SetBool(shownKey, true);

            var count = report.Entries.Count;
            var typeWord = report.TypeCount == 1 ? "type" : "types";
            var message = $"{count} managed reference{(count == 1 ? "" : "s")} became missing " +
                          $"({report.TypeCount} {typeWord}) — open Repair";

            ShowToast(message);
            Debug.LogWarning($"[Aspid FastTools] {message}. Open Tools/Aspid \U0001F40D/FastTools/Project References.");
        }

        /// <summary>Public deep-link the user can wire to a button/menu — opens Repair straight into project-scan mode.</summary>
        public static void OpenRepair() => SerializeReferenceWindow.OpenProjectScan();

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
            // No editor window open (rare) — the console warning above is the fallback signal.
        }

        // A stable identity for the breakage set: its sorted, distinct stored-type keys. Two events affecting the same
        // types are the "same" set and are not re-toasted.
        private static string ContentHash(BreakageReport report)
        {
            var keys = new SortedSet<string>(System.StringComparer.Ordinal);
            foreach (var entry in report.Entries)
                keys.Add(SerializeReferenceHelpers.StoredTypeKey(entry.StoredType));

            var builder = new StringBuilder();
            foreach (var key in keys) builder.Append(key).Append(';');
            return builder.ToString().GetHashCode().ToString("X8");
        }

        private static string ProjectId() => PlayerSettings.productGUID.ToString();
    }
}
