using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Completes a "Create new script" flow across the domain reload the new .cs triggers: the pending (target, path,
    // expected type) is parked in SessionState before the reload and resolved on a later load, once the script has
    // compiled.
    //
    // The assignment can outlive several reloads — a stub may fail to compile, or the new assembly may register only
    // on a later reload — so an unresolved entry is re-persisted and retried rather than dropped. Only the
    // type-not-resolved reason, the one a reload can fix, spends the cross-reload budget; an entry whose target is
    // merely not loaded waits indefinitely, since no reload count can open its scene. Provably dead entries are
    // dropped silently, and a few in-session re-arms catch an assembly that lands a tick late.
    internal static class SerializeReferencePendingAssignment
    {
        public const string Key = "Aspid.FastTools.SerializeReference.PendingAssignment";
        private const char EntrySeparator = '\n';
        private const char FieldSeparator = '|';

        // Cross-reload backstop: a still-unresolved entry is dropped, with a warning, after this many loads.
        public const int MaxResolveAttempts = 32;

        // How many extra passes to arm within one load, for an assembly that registers a tick late.
        public const int MaxInSessionRetries = 3;

        // Re-arms left for the current load; static state does not survive a reload, so Hook resets it.
        private static int _inSessionRetriesLeft;

        public static void Enqueue(UnityEngine.Object target, string propertyPath, string fullTypeName)
        {
            if (target == null || string.IsNullOrEmpty(propertyPath) || string.IsNullOrEmpty(fullTypeName)) return;

            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
            var entry = new Entry(globalId, propertyPath, fullTypeName, attempts: 0);

            var queue = Decode(SessionState.GetString(Key, string.Empty));
            Merge(queue, entry);
            SessionState.SetString(Key, Encode(queue));
        }

        [InitializeOnLoadMethod]
        private static void Hook()
        {
            _inSessionRetriesLeft = MaxInSessionRetries;
            EditorApplication.delayCall += ResolveAfterLoad;
        }

        // The first pass after a load counts one attempt against each pending entry's cross-reload budget.
        private static void ResolveAfterLoad() => Resolve(countAttempt: true);

        // An in-session retry: nothing has reloaded, so it does not spend the cross-reload budget.
        private static void ResolveRetry() => Resolve(countAttempt: false);

        private static void Resolve(bool countAttempt)
        {
            // Re-armed only while something is still pending, in case an assembly lands a tick after this one.
            if (ResolvePass(countAttempt) && _inSessionRetriesLeft > 0)
            {
                _inSessionRetriesLeft--;
                EditorApplication.delayCall += ResolveRetry;
            }
        }

        // Applies what it can, re-persists what is still pending and erases the queue once nothing remains.
        public static bool ResolvePass(bool countAttempt)
        {
            var raw = SessionState.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(raw)) return false;

            var pending = Decode(raw);
            var survivors = new List<Entry>(pending.Count);

            foreach (var entry in pending)
            {
                ApplyOutcome outcome;
                try
                {
                    outcome = TryApply(entry);
                }
                catch (Exception)
                {
                    // A resolved but incompatible type throws on assign; counting it as unresolved lets the give-up
                    // cap bound it instead of stranding the entries queued behind it.
                    outcome = ApplyOutcome.PendingUnresolved;
                }

                switch (outcome)
                {
                    case ApplyOutcome.Applied:
                    case ApplyOutcome.Dead:
                        break; // resolved or provably dead — drop from the queue.

                    case ApplyOutcome.PendingUnloaded:
                        // A reload cannot open the owning scene, so this waits without spending the budget.
                        survivors.Add(entry);
                        break;

                    case ApplyOutcome.PendingUnresolved:
                        // The type has not compiled yet — the case the budget bounds.
                        var next = countAttempt ? entry.WithIncrementedAttempt() : entry;
                        if (next.Attempts >= MaxResolveAttempts) WarnDropped(next);
                        else survivors.Add(next);
                        break;
                }
            }

            if (survivors.Count == 0)
            {
                SessionState.EraseString(Key);
                return false;
            }

            SessionState.SetString(Key, Encode(survivors));
            return true;
        }

        private enum ApplyOutcome
        {
            Applied,
            Dead,
            PendingUnloaded,
            PendingUnresolved,
        }

        private static ApplyOutcome TryApply(Entry entry)
        {
            if (!GlobalObjectId.TryParse(entry.GlobalId, out var globalId)) return ApplyOutcome.Dead;

            var target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
            if (target == null) return ApplyOutcome.PendingUnloaded; // the scene/asset holding the field isn't open yet.

            var type = ResolveType(entry.FullTypeName);
            if (type is null) return ApplyOutcome.PendingUnresolved; // the new assembly has not compiled/loaded yet.

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(entry.PropertyPath);
            if (property is null || property.propertyType != SerializedPropertyType.ManagedReference) return ApplyOutcome.Dead;

            property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstance(type));
            return ApplyOutcome.Applied;
        }

        private static void WarnDropped(Entry entry) =>
            Debug.LogWarning(
                $"[Aspid.FastTools] Dropping the pending \"Create new script\" assignment of '{entry.FullTypeName}' to " +
                $"'{entry.PropertyPath}' after {MaxResolveAttempts} domain reloads — its type never resolved (a compile " +
                "error or unsupported generated stub) or could not be applied to the field. Re-pick the type once it compiles.");

        private static Type ResolveType(string fullName)
        {
            var direct = Type.GetType(fullName, throwOnError: false);
            if (direct is not null) return direct;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, throwOnError: false);
                if (type is not null) return type;
            }

            return null;
        }

        // Wire model: SessionState holds newline-separated entries, each a pipe-separated
        // (globalId | propertyPath | fullTypeName | attempts) record — no field can contain a pipe or a newline.
        // Legacy three-field records, written before retry tracking, decode as attempts = 0.

        public readonly struct Entry : IEquatable<Entry>
        {
            public readonly string GlobalId;
            public readonly string PropertyPath;
            public readonly string FullTypeName;
            public readonly int Attempts;

            public Entry(string globalId, string propertyPath, string fullTypeName, int attempts)
            {
                GlobalId = globalId;
                PropertyPath = propertyPath;
                FullTypeName = fullTypeName;
                Attempts = attempts;
            }

            public Entry WithIncrementedAttempt() => new(GlobalId, PropertyPath, FullTypeName, Attempts + 1);

            // True when both entries target the same field on the same object, whatever their type and attempts.
            public bool SameTarget(Entry other) => GlobalId == other.GlobalId && PropertyPath == other.PropertyPath;

            public string Encode() =>
                $"{GlobalId}{FieldSeparator}{PropertyPath}{FieldSeparator}{FullTypeName}{FieldSeparator}{Attempts}";

            public static bool TryDecode(string line, out Entry entry)
            {
                entry = default;
                if (string.IsNullOrEmpty(line)) return false;

                var parts = line.Split(FieldSeparator);
                if (parts.Length < 3) return false;
                if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]) || string.IsNullOrEmpty(parts[2])) return false;

                var attempts = parts.Length > 3 && int.TryParse(parts[3], out var parsed) && parsed > 0 ? parsed : 0;
                entry = new Entry(parts[0], parts[1], parts[2], attempts);
                return true;
            }

            public bool Equals(Entry other) =>
                GlobalId == other.GlobalId && PropertyPath == other.PropertyPath &&
                FullTypeName == other.FullTypeName && Attempts == other.Attempts;

            public override bool Equals(object obj) => obj is Entry other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(GlobalId, PropertyPath, FullTypeName, Attempts);

            public override string ToString() => Encode();
        }

        public static List<Entry> Decode(string raw)
        {
            var entries = new List<Entry>();
            if (string.IsNullOrEmpty(raw)) return entries;

            foreach (var line in raw.Split(EntrySeparator))
                if (Entry.TryDecode(line, out var entry))
                    entries.Add(entry);

            return entries;
        }

        public static string Encode(IReadOnlyList<Entry> entries)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < entries.Count; i++)
            {
                if (i > 0) builder.Append(EntrySeparator);
                builder.Append(entries[i].Encode());
            }

            return builder.ToString();
        }

        // An earlier entry for the same field is replaced: re-picking a field's new script supersedes the previous
        // pending pick rather than queuing a second, stale assignment.
        public static void Merge(List<Entry> queue, Entry entry)
        {
            queue.RemoveAll(existing => existing.SameTarget(entry));
            queue.Add(entry);
        }
    }
}
