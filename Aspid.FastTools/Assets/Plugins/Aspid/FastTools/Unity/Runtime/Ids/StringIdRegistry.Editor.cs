#if UNITY_EDITOR
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public sealed partial class StringIdRegistry
    {
        [TypeSelector(typeof(IId))]
        [SerializeField] private string _targetStructType;
        [SerializeField] private int _nextId = 1;
    }
}
#endif
