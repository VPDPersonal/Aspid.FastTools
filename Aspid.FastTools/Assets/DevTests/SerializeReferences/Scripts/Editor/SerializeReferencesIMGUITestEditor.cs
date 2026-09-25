using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.SerializeReferences.Editors
{
    // Forces IMGUI rendering for the SerializeReferencesIMGUITest inspector.
    //
    // Unity picks IMGUI vs UIToolkit at the Editor level: when CreateInspectorGUI is NOT overridden but
    // OnInspectorGUI is, the whole inspector — including every nested PropertyDrawer — falls back to
    // IMGUI. That routes the managed-reference fields through SerializeReferenceIMGUIPropertyDrawer.OnGUI
    // instead of CreatePropertyGUI. The list is drawn as a plain PropertyField on purpose: its
    // picker-backed + button belongs to SerializeReferenceIMGUIList, which the sample's WeaponPresetEditor
    // already covers.
    [CustomEditor(typeof(SerializeReferencesIMGUITest))]
    internal sealed class SerializeReferencesIMGUITestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
