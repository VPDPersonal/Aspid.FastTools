using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A stored type name that resolved when its file was read and no longer does.
    internal readonly struct TypeNameBreakageReport
    {
        public readonly IReadOnlyList<string> TypeNames;
        public readonly int FileCount;

        public TypeNameBreakageReport(IReadOnlyList<string> typeNames, int fileCount)
        {
            TypeNames = typeNames;
            FileCount = fileCount;
        }

        public bool HasAny => TypeNames is { Count: > 0 };
    }

    // Breakage detection for the names stored by SerializableType and SerializableMonoScript wrappers, the counterpart of
    // SerializeReferenceBreakageDetector's cold path: a baseline of the keys that resolved in each asset, re-read for the
    // changed assets and re-resolved after every import.
    internal static class TypeNameBreakageDetector
    {
        internal const string EstablishedKey = "Aspid.FastTools.TypeNames.Breakage.BaselineEstablished";
        internal const string BaselineKey = "Aspid.FastTools.TypeNames.Breakage.Baseline";
        private const char EntrySeparator = '\n';
        private const char KeySeparator = '\t';

        // A key is the stored name and the script that also resolves a SerializableMonoScript: "<name>\u001f<guid>:<fileId>".
        private const char KeyPartSeparator = '\u001f';
        private const double SweepBudgetMilliseconds = 8;

        private static readonly Queue<string> _pending = new();
        private static readonly Dictionary<string, HashSet<string>> _swept = new(StringComparer.Ordinal);
        private static bool _establishing;
        private static bool _pumping;
        private static bool _reportPending;

        public static event Action<TypeNameBreakageReport> BreakageDetected;

        internal static bool IsEstablished => SessionState.GetBool(EstablishedKey, false);

        [InitializeOnLoadMethod]
        private static void EstablishBaselineOnce() => EditorApplication.delayCall += () =>
        {
            if (Application.isBatchMode || IsEstablished) return;
            Scan(changedAssets: null);
        };

        // changedAssets are the candidate assets imported, deleted or moved since the last scan.
        public static void Scan(IReadOnlyCollection<string> changedAssets)
        {
            if (!SerializeReferenceSettings.BreakageDetectionEnabled) return;

            // Type resolution flaps while scripts compile, so defer (never drop) until the editor settles.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += () => Scan(changedAssets);
                return;
            }

            // A class compiled since the last scan must be seen.
            MissingTypeNames.ClearCache();

            if (!IsEstablished)
            {
                if (_establishing) Enqueue(changedAssets);
                else BeginEstablishing();
                return;
            }

            // The changed assets are re-read first: a rename whose assets were saved with the new name must not alarm
            // on the names they held before.
            Enqueue(changedAssets);

            if (_pending.Count == 0) ReportBrokenKeys();
            else _reportPending = true;
        }

        // Finishes the sweep in one call instead of across editor updates.
        internal static void CompleteSweep() => ProcessPending(double.PositiveInfinity);

        // Tests start from no sweep in flight, whatever the session's own sweep was doing.
        internal static void ResetForTests() => CancelSweep();

        internal static IReadOnlyCollection<string> GetBaselineKeys(string assetPath) =>
            LoadBaseline().TryGetValue(assetPath, out var keys) ? keys : Array.Empty<string>();

        internal static string GetKey(StoredTypeNameEntry entry) =>
            $"{entry.TypeName}{KeyPartSeparator}{entry.ScriptGuid}:{entry.ScriptFileId}";

        private static void BeginEstablishing()
        {
            CancelSweep();
            _establishing = true;

            var paths = new List<string>();
            foreach (var path in AssetDatabase.GetAllAssetPaths())
                if (SerializeReferenceHelpers.IsScanCandidate(path)) paths.Add(path);

            if (paths.Count == 0) CompleteSweep();
            else Enqueue(paths);
        }

        private static void Enqueue(IEnumerable<string> paths)
        {
            if (paths is null) return;

            foreach (var path in paths)
                if (!string.IsNullOrEmpty(path)) _pending.Enqueue(path);

            if (_pending.Count == 0 || _pumping) return;

            _pumping = true;
            EditorApplication.update += Pump;
        }

        private static void Pump()
        {
            if (!SerializeReferenceSettings.BreakageDetectionEnabled)
            {
                CancelSweep();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            ProcessPending(SweepBudgetMilliseconds);
        }

        private static void ProcessPending(double budgetMilliseconds)
        {
            var stopwatch = Stopwatch.StartNew();

            while (_pending.Count > 0 && stopwatch.Elapsed.TotalMilliseconds < budgetMilliseconds)
            {
                var path = _pending.Dequeue();
                _swept[path] = CollectResolvableKeys(path);
            }

            if (_pending.Count > 0) return;

            StopPump();
            if (!_establishing && !IsEstablished)
            {
                _swept.Clear();
                _reportPending = false;
                return;
            }

            var baseline = _establishing
                ? new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
                : LoadBaseline();

            foreach (var (path, keys) in _swept)
            {
                if (keys.Count == 0) baseline.Remove(path);
                else baseline[path] = keys;
            }

            _swept.Clear();
            SaveBaseline(baseline);

            if (_establishing)
            {
                _establishing = false;
                SessionState.SetBool(EstablishedKey, true);
                return;
            }

            if (!_reportPending) return;

            _reportPending = false;
            ReportBrokenKeys();
        }

        private static void ReportBrokenKeys()
        {
            var baseline = LoadBaseline();
            if (baseline.Count == 0) return;

            MissingTypeNames.ClearCache();

            var broken = new HashSet<string>(StringComparer.Ordinal);
            var names = new SortedSet<string>(StringComparer.Ordinal);
            var files = 0;

            foreach (var keys in baseline.Values)
            {
                var brokenInFile = false;

                foreach (var key in keys)
                {
                    if (!broken.Contains(key) && KeyResolves(key, out _)) continue;

                    broken.Add(key);
                    brokenInFile = true;
                    if (TryParseKey(key, out var typeName, out _, out _)) names.Add(MissingTypeNames.FullName(typeName));
                }

                if (brokenInFile) files++;
            }

            if (broken.Count == 0) return;

            // Reported once: a broken key leaves the baseline.
            foreach (var keys in baseline.Values) keys.ExceptWith(broken);
            SaveBaseline(baseline);

            BreakageDetected?.Invoke(new TypeNameBreakageReport(new List<string>(names), files));
        }

        // A deleted, moved-away or no longer scanned asset yields an empty set, which drops its entry.
        private static HashSet<string> CollectResolvableKeys(string path)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            if (!SerializeReferenceHelpers.IsScanCandidate(path)) return keys;

            var lines = SerializeReferenceYaml.ReadLines(path);
            if (lines is null || !Array.Exists(lines, SerializeReferenceYamlEditor.MayHoldTypeNames)) return keys;

            foreach (var entry in SerializeReferenceYamlEditor.FindStoredTypeNames(lines))
            {
                if (entry.TypeName.Trim().Length == 0 || !MissingTypeNames.Resolves(entry)) continue;
                keys.Add(GetKey(entry));
            }

            return keys;
        }

        private static bool KeyResolves(string key, out string typeName)
        {
            if (!TryParseKey(key, out typeName, out var guid, out var fileId)) return true;

            return MissingTypeNames.NameResolves(typeName) || MissingTypeNames.GetScriptClass(guid, fileId) is not null;
        }

        private static bool TryParseKey(string key, out string typeName, out string guid, out long fileId)
        {
            typeName = string.Empty;
            guid = string.Empty;
            fileId = 0;

            var separator = key.LastIndexOf(KeyPartSeparator);
            if (separator <= 0) return false;

            typeName = key[..separator];
            var script = key[(separator + 1)..];
            var colon = script.LastIndexOf(':');
            if (colon < 0) return false;

            guid = script[..colon];
            return long.TryParse(script[(colon + 1)..], out fileId);
        }

        private static void CancelSweep()
        {
            StopPump();
            _pending.Clear();
            _swept.Clear();
            _establishing = false;
            _reportPending = false;
        }

        private static void StopPump()
        {
            if (!_pumping) return;

            _pumping = false;
            EditorApplication.update -= Pump;
        }

        private static Dictionary<string, HashSet<string>> LoadBaseline()
        {
            var raw = SessionState.GetString(BaselineKey, string.Empty);
            var baseline = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(raw)) return baseline;

            foreach (var entry in raw.Split(EntrySeparator))
            {
                var parts = entry.Split(KeySeparator);
                if (parts.Length < 2 || parts[0].Length == 0) continue;

                if (!baseline.TryGetValue(parts[0], out var keys))
                {
                    keys = new HashSet<string>(StringComparer.Ordinal);
                    baseline.Add(parts[0], keys);
                }

                for (var i = 1; i < parts.Length; i++)
                    if (parts[i].Length > 0) keys.Add(parts[i]);
            }

            return baseline;
        }

        private static void SaveBaseline(Dictionary<string, HashSet<string>> baseline)
        {
            var entries = new List<string>(baseline.Count);
            foreach (var (path, keys) in baseline)
                if (keys.Count > 0) entries.Add(path + KeySeparator + string.Join(KeySeparator.ToString(), keys));

            SessionState.SetString(BaselineKey, string.Join(EntrySeparator.ToString(), entries));
        }
    }
}
