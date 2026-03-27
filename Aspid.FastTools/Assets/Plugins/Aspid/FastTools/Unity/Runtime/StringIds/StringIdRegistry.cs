#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    [CreateAssetMenu(fileName = "StringIdRegistry", menuName = "Aspid/FastTools/String Id Registry")]
    public sealed class StringIdRegistry : ScriptableObject
    {
        [Serializable]
        public struct IdEntry
        {
            public string Name;
            public int Id;
        }

        [TypeSelector]
        [SerializeField] private string _targetStructType = string.Empty;

        [SerializeField] private List<IdEntry> _entries = new();
        [SerializeField] private int _nextId = 1;

        public string TargetStructType => _targetStructType;

        public IReadOnlyList<IdEntry> Entries => _entries;

        // Backward compat: IdDropdownDrawer still uses Ids
        public IReadOnlyList<string> Ids => _entries.Select(e => e.Name).ToList();

        public bool Contains(string name) => _entries.Exists(e => e.Name == name);

        public int Add(string name)
        {
            if (Contains(name))
                return GetId(name);

            var id = _nextId++;
            _entries.Add(new IdEntry { Name = name, Id = id });
            return id;
        }

        public void Remove(string name) => _entries.RemoveAll(e => e.Name == name);

        public void Rename(string oldName, string newName)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Name == oldName)
                {
                    var entry = _entries[i];
                    entry.Name = newName;
                    _entries[i] = entry;
                    return;
                }
            }
        }

        public int GetId(string name)
        {
            foreach (var e in _entries)
                if (e.Name == name) return e.Id;
            return 0;
        }

        public string? GetName(int id)
        {
            foreach (var e in _entries)
                if (e.Id == id) return e.Name;
            return null;
        }
    }
}
