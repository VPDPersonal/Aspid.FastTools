#if UNITY_EDITOR
using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    public sealed partial class IdRegistry
    {
        [SerializeField] private string[] _names = Array.Empty<string>();

        [TypeSelector(typeof(IId))]
        [SerializeField] private string _targetStructType = string.Empty;

        [SerializeField] private int _nextId = 1;
    }
}
#endif
