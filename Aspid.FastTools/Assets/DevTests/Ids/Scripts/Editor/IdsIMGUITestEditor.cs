using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Ids.Editors
{
    // Forces IMGUI rendering for the IdsIMGUITest inspector.
    //
    // Unity picks IMGUI vs UIToolkit at the Editor level: when CreateInspectorGUI is NOT
    // overridden but OnInspectorGUI is, the whole inspector — including every nested
    // PropertyDrawer — falls back to IMGUI. That routes the DevEnemyId fields through
    // IdStructIMGUIPropertyDrawer.OnGUI instead of CreatePropertyGUI.
    [CustomEditor(typeof(IdsIMGUITest))]
    internal sealed class IdsIMGUITestEditor : Editor
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
