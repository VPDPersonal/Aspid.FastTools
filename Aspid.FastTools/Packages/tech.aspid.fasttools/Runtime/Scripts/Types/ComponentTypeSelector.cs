using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Represents a marker field adding an Inspector dropdown that swaps the object's script to any subtype of the
    /// field's declaring class.
    /// </summary>
    /// <example>
    /// <code>
    /// public abstract class BaseEnemy : MonoBehaviour
    /// {
    ///     [SerializeField] private ComponentTypeSelector _typeSelector;
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public struct ComponentTypeSelector { }
}
