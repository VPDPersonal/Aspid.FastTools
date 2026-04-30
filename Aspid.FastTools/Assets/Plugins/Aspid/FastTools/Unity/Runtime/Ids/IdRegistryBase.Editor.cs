#if UNITY_EDITOR
using UnityEngine;
using Aspid.FastTools.Types;

#pragma warning disable CS0414 // Both fields are read by the editor inspector via SerializedObject.FindProperty.
// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local
// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids
{
    public abstract partial class IdRegistryBase
    {
        [TypeSelector(typeof(IId))]
        [SerializeField] private string _targetStructType = string.Empty;

        [SerializeField] private int _nextId = 1;
    }
}
#pragma warning restore CS0414
#endif
