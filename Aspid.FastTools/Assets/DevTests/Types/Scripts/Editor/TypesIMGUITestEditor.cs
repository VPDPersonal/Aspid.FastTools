using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types.Editors
{
    // Forces IMGUI rendering for the TypesIMGUITest inspector.
    //
    // Unity picks IMGUI vs UIToolkit at the Editor level: when CreateInspectorGUI is NOT
    // overridden but OnInspectorGUI is, the whole inspector — including every nested
    // PropertyDrawer — falls back to IMGUI. That routes SerializableType<T> and [TypeSelector]
    // fields through TypeIMGUIPropertyDrawer.OnGUI instead of CreatePropertyGUI.
    [CustomEditor(typeof(TypesIMGUITest))]
    internal sealed class TypesIMGUITestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
