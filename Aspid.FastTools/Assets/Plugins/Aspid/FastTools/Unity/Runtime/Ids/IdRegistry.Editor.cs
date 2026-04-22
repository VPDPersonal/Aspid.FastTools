#if UNITY_EDITOR
using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools
{
    public sealed partial class IdRegistry
    {
        [TypeSelector(typeof(IId))]
        [SerializeField] private string _targetStructType;
        [SerializeField] private int _nextId = 1;
    }
}
#endif
