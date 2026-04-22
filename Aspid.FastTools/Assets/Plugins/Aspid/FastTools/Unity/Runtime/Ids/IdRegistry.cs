using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    public partial class IdRegistry : ScriptableObject, IEnumerable<int>
    {
        [SerializeField] private IdEntry[] _entries = Array.Empty<IdEntry>();
        
        public IEnumerator<int> GetEnumerator() =>
            _entries.Select(entry => entry.Id).GetEnumerator();

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
