using UnityEditor;
using UnityEngine;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences.Editors
{
    // Forces IMGUI rendering for the IMGUILoadout inspector.
    //
    // Unity picks IMGUI vs UIToolkit at the Editor level: when CreateInspectorGUI is NOT
    // overridden but OnInspectorGUI is, the whole inspector — including every nested
    // PropertyDrawer — falls back to IMGUI. That routes [TypeSelector] fields
    // through SerializeReferenceIMGUIPropertyDrawer.OnGUI instead of CreatePropertyGUI.
    //
    // The single-reference fields (_primaryWeapon, _onHitEffect, _modifier, _floatModifier)
    // render straight through PropertyField. The two [SerializeReference] LISTS are drawn via
    // SerializeReferenceIMGUIList instead: Unity applies a [TypeSelector] drawer per element in
    // IMGUI, so it can never reach the list's "+" — the helper restores the picker-backed,
    // de-aliased add the UIToolkit ListView gets automatically.
    [CustomEditor(typeof(IMGUILoadout))]
    internal sealed class IMGUILoadoutEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(enterChildren: true); // skip m_Script

            while (iterator.NextVisible(enterChildren: false))
            {
                switch (iterator.name)
                {
                    case "_sidearms":
                        SerializeReferenceIMGUIList.Draw(iterator.Copy(),
                            new GUIContent(iterator.displayName), typeof(IWeapon));
                        break;

                    case "_modifiers":
                        SerializeReferenceIMGUIList.Draw(iterator.Copy(),
                            new GUIContent(iterator.displayName), typeof(IModifier));
                        break;

                    default:
                        EditorGUILayout.PropertyField(iterator, includeChildren: true);
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
