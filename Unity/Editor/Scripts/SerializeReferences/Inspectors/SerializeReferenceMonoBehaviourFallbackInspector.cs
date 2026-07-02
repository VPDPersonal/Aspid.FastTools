using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The attribute-free dropdown fallback for components; see <see cref="SerializeReferenceFallbackInspector"/>.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true, isFallback = true)]
    internal sealed class SerializeReferenceMonoBehaviourFallbackInspector : SerializeReferenceFallbackInspector { }
}
