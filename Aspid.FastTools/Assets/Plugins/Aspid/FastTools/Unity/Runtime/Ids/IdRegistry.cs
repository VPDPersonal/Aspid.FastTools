#nullable enable
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    /// <summary>
    /// A ScriptableObject that holds a stable set of integer IDs for a given struct type.
    /// Names are stored and edited in the inspector but stripped from player builds.
    /// Use <see cref="Aspid.FastTools.StringIdRegistry"/> when name lookups are needed at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "IdRegistry", menuName = "Aspid/FastTools/Id Registry")]
    public sealed partial class IdRegistry : ScriptableObject, IEnumerable<int>
    {
        [SerializeField] private int[] _ids = Array.Empty<int>();

        public int Count => _ids.Length;

        public bool Contains(int id)
        {
            for (var i = 0; i < _ids.Length; i++)
                if (_ids[i] == id) return true;
            return false;
        }

        public IEnumerator<int> GetEnumerator() =>
            ((IEnumerable<int>)_ids).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
