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
    /// A ScriptableObject that maps string names to stable integer IDs for a given struct type.
    /// Used by the <c>IdStruct</c> system to persist and resolve string/int ID pairs.
    /// </summary>
    [CreateAssetMenu(fileName = "StringIdRegistry", menuName = "Aspid/FastTools/String Id Registry")]
    public partial class StringIdRegistry : ScriptableObject, IEnumerable<KeyValuePair<int, string>>
    {
        [SerializeField] private IdEntry[] _entries = Array.Empty<IdEntry>();
        
        public IEnumerable<int> Ids  =>
            this.Select(entry => entry.Key);
            
        public IEnumerable<string> IdNames =>
            this.Select(entry => entry.Value);
        
        public int GetId(string nameId)
        {
            foreach (var e in _entries)
                if (e.Name == nameId) return e.Id;

            return -1;
        }

        public string? GetNameId(int id)
        {
            foreach (var e in _entries)
                if (e.Id == id) return e.Name;

            return null;
        }

        public bool Contains(string nameId)
        {
            foreach (var e in _entries)
                if (e.Name == nameId) return true;

            return false;
        }
        
        public IEnumerator<KeyValuePair<int, string>> GetEnumerator() =>
            _entries.Select(entry => new KeyValuePair<int, string>(entry.Id, entry.Name)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
        
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
