#nullable enable
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    [CreateAssetMenu(fileName = "StringIdRegistry", menuName = "Aspid/FastTools/String Id Registry")]
    public sealed class StringIdRegistry : ScriptableObject
    {
        [TypeSelector]
        [SerializeField] private string _targetStructType = string.Empty;

        [SerializeField] private List<string> _ids = new();

        public string TargetStructType => _targetStructType;
        public IReadOnlyList<string> Ids => _ids;

        public bool Contains(string id) => _ids.Contains(id);

        public void Add(string id)
        {
            if (!Contains(id))
                _ids.Add(id);
        }

        public void Remove(string id) => _ids.Remove(id);
    }
}
