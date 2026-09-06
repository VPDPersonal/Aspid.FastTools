using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Represents a marker field adding an Inspector dropdown that swaps the object's script to any subtype of the
    /// field's declaring class.
    /// </summary>
    /// <remarks>
    /// Picking a type writes the matching <c>MonoScript</c> asset to <c>m_Script</c>, turning the object into that
    /// subtype. The picker is constrained to the declaring class automatically.
    /// </remarks>
    /// <example>
    /// Place a field of this type in the root class; the Inspector lists all subtypes of <c>BaseEnemy</c>:
    /// <code>
    /// public abstract class BaseEnemy : MonoBehaviour
    /// {
    ///     [SerializeField] private ComponentTypeSelector _typeSelector;
    /// }
    ///
    /// public class FastEnemy : BaseEnemy { }
    /// public class TankEnemy : BaseEnemy { }
    /// </code>
    /// </example>
    [Serializable]
    public struct ComponentTypeSelector { }
}
