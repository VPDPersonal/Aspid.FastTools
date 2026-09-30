using UnityEngine;
using Aspid.FastTools.Types;

// Docs-media harness for Documentation/11-component-type-selector.md: Images/component-type-selector.gif switches a
// FastEnemy with Health 75 and Speed 40 to ArmoredEnemy through _enemyType. Each class sits in a file named after it
// because the switch writes the class's own MonoScript.

// ReSharper disable once CheckNamespace
namespace Game.Enemies
{
    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField] private ComponentTypeSelector _enemyType;
        [SerializeField] private float _health = 100f;
    }
}
