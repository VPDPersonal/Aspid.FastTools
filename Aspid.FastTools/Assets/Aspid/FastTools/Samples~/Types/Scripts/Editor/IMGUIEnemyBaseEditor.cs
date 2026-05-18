using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types.Editors
{
    // Forces IMGUI rendering for IMGUIEnemyBase and every concrete subtype.
    //
    // editorForChildClasses: true makes this editor apply to IMGUIFastEnemy / IMGUITankEnemy
    // too, so swapping the component's m_Script via ComponentTypeSelector keeps the inspector
    // in IMGUI mode after Unity rebuilds the editor for the new subtype. Without that flag,
    // the post-swap subtype would fall back to the default UIToolkit inspector.
    [CustomEditor(typeof(IMGUIEnemyBase), editorForChildClasses: true)]
    internal sealed class IMGUIEnemyBaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
