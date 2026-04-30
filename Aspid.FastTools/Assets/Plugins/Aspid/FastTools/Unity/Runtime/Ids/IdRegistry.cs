#nullable enable
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    public class IdRegistry<T> : IdRegistry
        where T : struct, IId
    {
        public bool Contains(T id) =>
            base.Contains(id.Id);
    }

    /// <summary>
    /// A ScriptableObject that holds a stable set of integer IDs for a given struct type.
    /// Names are stored and edited in the inspector but stripped from player builds.
    /// Use <see cref="Aspid.FastTools.StringIdRegistry"/> when name lookups are needed at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "IdRegistry", menuName = "Aspid/FastTools/Id Registry")]
    public partial class IdRegistry : IdRegistryBase, IEnumerable<int>
    {
        [SerializeField] private int[] _ids = Array.Empty<int>();

        [NonSerialized] private HashSet<int> _idSet = new();

        public override int Count => _ids.Length;

        public override bool Contains(int id)
        {
            EnsureCache();
            return _idSet.Contains(id);
        }

        public IEnumerator<int> GetEnumerator() =>
            ((IEnumerable<int>)_ids).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        protected override void RebuildCache()
        {
            _idSet = new HashSet<int>(_ids.Length);
            foreach (var id in _ids)
                _idSet.Add(id);
        }
    }
}
