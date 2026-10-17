using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Enums.Editors
{
    // Forces IMGUI rendering for the EnumValuesIMGUITest inspector.
    //
    // Unity picks IMGUI vs UIToolkit at the Editor level: when CreateInspectorGUI is NOT
    // overridden but OnInspectorGUI is, the whole inspector — including every nested
    // PropertyDrawer — falls back to IMGUI. That routes the EnumValues<TValue>/
    // EnumValues<TEnum,TValue> fields through EnumValuesIMGUIPropertyDrawer.OnGUI instead of
    // CreatePropertyGUI.
    [CustomEditor(typeof(EnumValuesIMGUITest))]
    internal sealed class EnumValuesIMGUITestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(enterChildren: true); // skip m_Script

            while (iterator.NextVisible(enterChildren: false))
                EditorGUILayout.PropertyField(iterator, includeChildren: true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
