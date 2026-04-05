using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    /// <summary>
    /// Demonstrates <see cref="SerializableType"/> and <see cref="SerializableType{T}"/>:
    /// serializable wrappers around <see cref="Type"/> that are assignable in the Inspector.
    /// </summary>
    public class TypeSelectorTest : MonoBehaviour
    {
        [Header("Any type — resolved at runtime via implicit conversion")]
        [SerializeField] private SerializableType _componentType;

        [Header("Constrained to MonoBehaviour subtypes — safe AddComponent at runtime")]
        [SerializeField] private SerializableType<MonoBehaviour> _behaviourType;

        [Header("Constrained to Enum subtypes — string stored, filtered picker in Inspector")]
        [SerializeField] private SerializableType<Enum> _enumType;

        private void Start()
        {
            // Implicit conversion to System.Type
            Type componentType = _componentType;
            if (componentType is not null)
                Debug.Log($"Selected type: {componentType.FullName}");

            // Safe AddComponent — picker guarantees the type is a MonoBehaviour subtype
            Type behaviourType = _behaviourType;
            if (behaviourType is not null)
                gameObject.AddComponent(behaviourType);

            // Use the resolved enum type for reflection
            Type enumType = _enumType;
            if (enumType is not null)
                Debug.Log($"Enum values: {string.Join(", ", Enum.GetNames(enumType))}");
        }
    }
}
