#nullable enable
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    /// <summary>
    /// A strongly-typed wrapper around <see cref="IdRegistry"/> that exposes <see cref="IId"/>-aware membership checks.
    /// </summary>
    /// <typeparam name="T">The id struct type bound to this registry.</typeparam>
    public class IdRegistry<T> : IdRegistry
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
    /// A ScriptableObject that holds a stable set of integer IDs for a given struct type.
    /// Names are stored and edited in the inspector but stripped from player builds.
    /// Use <see cref="StringIdRegistry"/> when name lookups are needed at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "IdRegistry", menuName = "Aspid/Id Registry/Id Registry")]
    public partial class IdRegistry : IdRegistryBase, IEnumerable<int>
    {
        [SerializeField] private int[] _ids = Array.Empty<int>();
        [NonSerialized] private HashSet<int> _idSet = new();

        /// <inheritdoc/>
        public override int Count => _ids.Length;

        /// <inheritdoc/>
        public override bool Contains(int id)
        {
            EnsureCache();
            return _idSet.Contains(id);
        }

        /// <summary>
        /// Returns an enumerator over the registered integer ids in serialized order.
        /// </summary>
        public IEnumerator<int> GetEnumerator() =>
            ((IEnumerable<int>)_ids).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        /// <inheritdoc/>
        protected override void RebuildCache()
        {
            _idSet = new HashSet<int>(_ids.Length);
            
            foreach (var id in _ids)
                _idSet.Add(id);
        }
    }
}
