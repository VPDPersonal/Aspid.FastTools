using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The attribute-free dropdown fallback for assets; see <see cref="SerializeReferenceFallbackInspector"/>.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), editorForChildClasses: true, isFallback = true)]
    internal sealed class SerializeReferenceScriptableObjectFallbackInspector : SerializeReferenceFallbackInspector { }
}
