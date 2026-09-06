using UnityEditor;
using UnityEngine;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences.Editors
{
    // An IMGUI inspector. Overriding OnInspectorGUI without CreateInspectorGUI routes every nested drawer,
    // the [TypeSelector] ones included, through IMGUI. Single fields need nothing special. A list is
    // different: Unity applies the drawer per element, so the list's + would clone the last element;
    // SerializeReferenceIMGUIList.Draw restores the picker-backed add.
    [CustomEditor(typeof(WeaponPreset))]
    internal sealed class WeaponPresetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_weapon"), includeChildren: true);

            var alternates = serializedObject.FindProperty("_alternates");
            SerializeReferenceIMGUIList.Draw(alternates, new GUIContent(alternates.displayName), typeof(IWeapon));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
