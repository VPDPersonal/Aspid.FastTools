using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types.Editors
{
    // Forces IMGUI rendering for the IMGUIAbilitySelector inspector.
    //
    // Unity decides between IMGUI and UIToolkit at the Editor level: when
    // CreateInspectorGUI is NOT overridden but OnInspectorGUI is, the entire inspector —
    // including every nested PropertyDrawer — falls back to the IMGUI path. That routes
    // SerializableType<T> and [TypeSelector] fields through TypeIMGUIPropertyDrawer.OnGUI
    // instead of CreatePropertyGUI, demonstrating the IMGUI rendering of the picker.
    [CustomEditor(typeof(IMGUIAbilitySelector))]
    internal sealed class IMGUIAbilitySelectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
