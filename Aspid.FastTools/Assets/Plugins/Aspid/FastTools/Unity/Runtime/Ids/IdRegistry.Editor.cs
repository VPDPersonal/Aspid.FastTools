#if UNITY_EDITOR
using System;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    public partial class IdRegistry
    {
        [SerializeField] private string[] _names = Array.Empty<string>();
    }
}
#endif
