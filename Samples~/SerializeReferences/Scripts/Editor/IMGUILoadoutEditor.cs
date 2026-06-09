using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences.Editors
{
    // Forces IMGUI rendering for the IMGUILoadout inspector.
    //
    // Unity picks IMGUI vs UIToolkit at the Editor level: when CreateInspectorGUI is NOT
    // overridden but OnInspectorGUI is, the whole inspector — including every nested
    // PropertyDrawer — falls back to IMGUI. That routes [TypeSelector] fields
    // through SerializeReferenceIMGUIPropertyDrawer.OnGUI instead of CreatePropertyGUI.
    [CustomEditor(typeof(IMGUILoadout))]
    internal sealed class IMGUILoadoutEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
