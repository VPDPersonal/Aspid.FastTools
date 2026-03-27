using System;
using UnityEngine;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    /// <summary>
    /// Adds an Inspector dropdown to a field that lets you swap the object's script
    /// to any subtype of the field's declaring class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the user picks a type, the editor locates the corresponding <c>MonoScript</c>
    /// asset and writes it to <c>m_Script</c> on the <c>SerializedObject</c>, effectively
    /// changing the component or ScriptableObject to the chosen subtype.
    /// </para>
    /// <para>
    /// The picker is automatically constrained to subtypes of the class that declares
    /// the attributed field — no extra configuration is needed.
    /// </para>
    /// <para>Only compiled in editor assemblies (<c>UNITY_EDITOR</c>).</para>
    /// </remarks>
    /// <example>
    /// Place the attribute on any field inside the root component class.
    /// The Inspector will render a dropdown listing all subtypes of <c>BaseEnemy</c>:
    /// <code>
    /// public abstract class BaseEnemy : MonoBehaviour
    /// {
    ///     [ComponentTypeSelector]
    ///     [SerializeField] private int _unused; // field type is irrelevant; the drawer reads m_Script
    /// }
    ///
    /// public class FastEnemy : BaseEnemy { }
    /// public class TankEnemy : BaseEnemy { }
    /// </code>
    /// Selecting "TankEnemy" in the Inspector rewrites the object's <c>m_Script</c>
    /// so it becomes a <c>TankEnemy</c> instance.
    /// </example>
    [Conditional(conditionString: "UNITY_EDITOR")]
    [AttributeUsage(validOn: AttributeTargets.Field)]
    public sealed class ComponentTypeSelectorAttribute : PropertyAttribute { }
}