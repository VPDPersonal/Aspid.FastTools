using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences.Editors
{
    // An IMGUI inspector. Overriding OnInspectorGUI without CreateInspectorGUI routes every nested drawer,
    // the [TypeSelector] ones included, through IMGUI. A plain PropertyField is enough for the list too:
    // [TypeSelector] reaches the whole list, so its + opens the picker.
    [CustomEditor(typeof(WeaponPreset))]
    internal sealed class WeaponPresetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_weapon"), includeChildren: true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_alternates"), includeChildren: true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
