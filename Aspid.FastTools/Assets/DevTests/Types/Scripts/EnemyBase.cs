using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    // Dev-only harness for the IMGUI rendering path of ComponentTypeSelector. The companion
    // EnemyBaseEditor (editorForChildClasses: true) forces IMGUI for this class and every subtype,
    // so the subtype dropdown renders through ComponentTypeSelectorPropertyDrawer.OnGUI. Selecting a
    // subtype rewrites m_Script in place — fields with matching names (_health) persist across the swap.
    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField] private ComponentTypeSelector _enemyType;

        [SerializeField] [Min(0)] private float _health = 100f;

        protected float Health => _health;
    }
}
