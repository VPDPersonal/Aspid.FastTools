#nullable enable
using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    /// <summary>
    /// A strongly-typed wrapper around <see cref="StringIdRegistry"/> that exposes <see cref="IId"/>-aware membership checks.
    /// </summary>
    /// <typeparam name="T">The id struct type bound to this registry.</typeparam>
    public class StringIdRegistry<T> : StringIdRegistry
        where T : struct, IId
    {
        /// <summary>
        /// Determines whether the registry contains the integer value of the specified id struct.
        /// </summary>
        /// <param name="id">The id struct whose <see cref="IId.Id"/> is checked.</param>
        /// <returns><c>true</c> if the underlying integer is registered; otherwise <c>false</c>.</returns>
        public bool Contains(T id) =>
            base.Contains(id.Id);
    }

    /// <summary>
    /// A ScriptableObject that maps string names to stable integer IDs for a given struct type.
    /// Used by the <c>IdStruct</c> system to persist and resolve string/int ID pairs.
    /// </summary>
    [CreateAssetMenu(fileName = "StringIdRegistry", menuName = "Aspid/FastTools/String Id Registry")]
    public partial class StringIdRegistry : IdRegistryBase, IEnumerable<KeyValuePair<int, string>>
    {
        [SerializeField] private IdEntry[] _entries = Array.Empty<IdEntry>();

        [NonSerialized] private Dictionary<string, int> _idByName = new();
        [NonSerialized] private Dictionary<int, string> _nameById = new();

        /// <inheritdoc/>
        public override int Count => _entries.Length;

        public IEnumerable<int> Ids =>
            this.Select(entry => entry.Key);

        public IEnumerable<string> IdNames =>
            this.Select(entry => entry.Value);

        /// <summary>
        /// Attempts to resolve the integer id for the given name.
        /// </summary>
        /// <param name="nameId">The string name to look up.</param>
        /// <param name="id">When this method returns, contains the resolved integer id if found; otherwise zero.</param>
        /// <returns><c>true</c> if a name-to-id mapping exists; otherwise <c>false</c>.</returns>
        public bool TryGetId(string nameId, out int id)
        {
            EnsureCache();
            return _idByName.TryGetValue(nameId, out id);
        }

        /// <summary>
        /// Attempts to resolve the string name for the given integer id.
        /// </summary>
        /// <param name="id">The integer id to look up.</param>
        /// <param name="name">When this method returns, contains the resolved name if found; otherwise <see cref="string.Empty"/>.</param>
        /// <returns><c>true</c> if an id-to-name mapping exists; otherwise <c>false</c>.</returns>
        public bool TryGetName(int id, out string name)
        {
            EnsureCache();
            if (_nameById.TryGetValue(id, out var found))
            {
                name = found;
                return true;
            }
            name = string.Empty;
            return false;
        }

        /// <inheritdoc/>
        public override bool Contains(int id)
        {
            EnsureCache();
            return _nameById.ContainsKey(id);
        }

        /// <summary>
        /// Determines whether the registry contains an entry with the specified name.
        /// </summary>
        /// <param name="nameId">The string name to look up.</param>
        /// <returns><c>true</c> if the name is registered; otherwise <c>false</c>.</returns>
        public bool Contains(string nameId)
        {
            EnsureCache();
            return _idByName.ContainsKey(nameId);
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator() =>
            _entries.Select(entry => new KeyValuePair<int, string>(entry.Id, entry.Name)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        protected override void RebuildCache()
        {
            _idByName = new Dictionary<string, int>(_entries.Length);
            _nameById = new Dictionary<int, string>(_entries.Length);

            foreach (var entry in _entries)
            {
                if (!string.IsNullOrEmpty(entry.Name))
                    _idByName[entry.Name] = entry.Id;
                _nameById[entry.Id] = entry.Name ?? string.Empty;
            }
        }

        /// <summary>
        /// A single name-to-id mapping entry.
        /// </summary>
        [Serializable]
        private struct IdEntry
        {
            public int Id;
            public string Name;
        }
    }
}
