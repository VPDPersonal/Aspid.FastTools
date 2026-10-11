using System;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The baseline of a breakage detector: each asset path maps to the type keys that resolved in it. The detectors
    // change it on every save, so it lives in memory and reaches SessionState once, before a domain reload, which is the
    // only moment the memory copy is lost.
    internal sealed class BreakageBaseline
    {
        private const char EntrySeparator = '\n';
        private const char KeySeparator = '\t';

        private readonly string _sessionKey;

        private bool _changed;
        private Dictionary<string, HashSet<string>> _entries;

        public BreakageBaseline(string sessionKey)
        {
            _sessionKey = sessionKey;
            AssemblyReloadEvents.beforeAssemblyReload += Persist;
        }

        // The live dictionary: a caller that changes it hands it back to Replace.
        public Dictionary<string, HashSet<string>> Entries => _entries ??= Parse(SessionState.GetString(_sessionKey, string.Empty));

        public void Replace(Dictionary<string, HashSet<string>> entries)
        {
            _entries = entries;
            _changed = true;
        }

        public void Clear() => Replace(new Dictionary<string, HashSet<string>>(StringComparer.Ordinal));

        public void Persist()
        {
            if (!_changed) return;

            SessionState.SetString(_sessionKey, Export());
            _changed = false;
        }

        // The persisted form, also for a test that has to restore the session's own baseline afterwards.
        internal string Export()
        {
            var entries = new List<string>(Entries.Count);
            foreach (var (path, keys) in Entries)
                if (keys.Count > 0) entries.Add(path + KeySeparator + string.Join(KeySeparator.ToString(), keys));

            return string.Join(EntrySeparator.ToString(), entries);
        }

        internal void Import(string raw) => Replace(Parse(raw));

        private static Dictionary<string, HashSet<string>> Parse(string raw)
        {
            var entries = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(raw)) return entries;

            foreach (var entry in raw.Split(EntrySeparator))
            {
                var parts = entry.Split(KeySeparator);
                if (parts.Length < 2 || parts[0].Length == 0) continue;

                if (!entries.TryGetValue(parts[0], out var keys))
                {
                    keys = new HashSet<string>(StringComparer.Ordinal);
                    entries.Add(parts[0], keys);
                }

                for (var i = 1; i < parts.Length; i++)
                    if (parts[i].Length > 0) keys.Add(parts[i]);
            }

            return entries;
        }
    }
}
