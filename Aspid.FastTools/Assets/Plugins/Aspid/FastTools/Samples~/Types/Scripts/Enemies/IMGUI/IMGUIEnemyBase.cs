using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Demonstrates the IMGUI rendering path for ComponentTypeSelector.
    //
    // The mechanic mirrors the EnemyBase / Fast/Tank Enemy hierarchy: a single
    // serialized ComponentTypeSelector field surfaces a subtype dropdown in the Inspector,
    // and selecting a subtype rewrites m_Script in place — fields with matching names persist.
    //
    // The companion IMGUIEnemyBaseEditor (with editorForChildClasses: true) overrides
    // OnInspectorGUI without CreateInspectorGUI, so every subclass renders through IMGUI
    // and the picker goes through ComponentTypeSelectorPropertyDrawer.OnGUI instead of
    // CreatePropertyGUI.
    public abstract class IMGUIEnemyBase : MonoBehaviour
    {
        [SerializeField] private ComponentTypeSelector _enemyType;

        [SerializeField] [Min(0)] private float _health = 100f;

        protected float Health => _health;

        public abstract void Attack();
    }
}
