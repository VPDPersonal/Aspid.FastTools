using System;
using UnityEditor;
using System.Reflection;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The one decision point for attribute-free dropdowns: whether a given property should be drawn with the
    /// SerializeReference type dropdown even though its declared field carries no <c>[TypeSelector]</c>, and the
    /// factory that builds the matching element (single field or list). Consulted by the fallback inspectors (the
    /// top-level fields of a component without its own custom editor) and by
    /// <see cref="SerializeReferenceField"/>'s content rebuild (the nested fields of an already-drawn reference), so
    /// both levels follow the same opt-in setting and the same "the attribute always wins" rule — a field that does
    /// carry <c>[TypeSelector]</c> keeps its drawer-driven rendering, base-type narrowing included.
    /// </summary>
    internal static class SerializeReferenceAutoDropdown
    {
        // SerializedProperty.arrayElementType for a [SerializeReference] array/list — the only array shape whose
        // elements are managed references.
        private const string ManagedReferenceElementPrefix = "managedReference<";

        /// <summary>
        /// The per-user opt-in gating every attribute-free substitution. Off by default.
        /// </summary>
        public static bool Enabled => SerializeReferenceSettings.DropdownWithoutAttributeEnabled;

        /// <summary>
        /// True when <paramref name="property"/> should be drawn with the attribute-free dropdown: the opt-in is on,
        /// the property is a managed reference (or a list/array of them), and its declared field does NOT carry
        /// <c>[TypeSelector]</c> — an attributed field keeps its drawer, whose base types narrow the picker.
        /// </summary>
        public static bool ShouldDraw(SerializedProperty property)
        {
            if (!Enabled || property is null) return false;

            var isReference = property.propertyType == SerializedPropertyType.ManagedReference;
            if (!isReference && !IsManagedReferenceArray(property)) return false;

            return !HasTypeSelector(property);
        }

        /// <summary>
        /// Builds the attribute-free element for <paramref name="property"/>: a <see cref="SerializeReferenceField"/>
        /// for a single managed reference, a <see cref="SerializeReferenceListField"/> for a list/array of them. The
        /// candidate pool is the field's declared type — there is no attribute to narrow it further.
        /// </summary>
        public static VisualElement CreateField(SerializedProperty property, string label = null)
        {
            label ??= property.displayName;

            return IsManagedReferenceArray(property)
                ? new SerializeReferenceListField(label, property, GetElementType(property))
                : new SerializeReferenceField(label, property);
        }

        /// <summary>
        /// True when <paramref name="property"/> is an array/list whose elements are managed references — the list
        /// shape the attribute-free dropdown must build itself (without <c>[TypeSelector]</c> there is no element
        /// drawer for Unity's default list to route through).
        /// </summary>
        public static bool IsManagedReferenceArray(SerializedProperty property) =>
            property.isArray &&
            property.arrayElementType.StartsWith(ManagedReferenceElementPrefix, StringComparison.Ordinal);

        /// <summary>
        /// True when the declared field behind <paramref name="property"/> carries <c>[TypeSelector]</c>. Resolved
        /// through the runtime-instance walk of <see cref="SerializePropertyExtensions.GetMemberInfo"/>, which — unlike
        /// a static-type walk — crosses managed-reference boundaries, so a nested field declared on the assigned
        /// instance's concrete type reports its attribute too. An unresolvable member counts as un-attributed.
        /// </summary>
        public static bool HasTypeSelector(SerializedProperty property) =>
            property.GetMemberInfo() is FieldInfo field && field.IsDefined(typeof(TypeSelectorAttribute), inherit: false);

        /// <summary>
        /// The declared element type of a managed-reference list/array — what constrains the add-picker on a list
        /// that may currently be empty (a non-empty list's elements resolve their own field type). Read from the
        /// reflected field's array/List&lt;T&gt; shape; falls back to the first element's declared typename, then to
        /// <see cref="object"/>.
        /// </summary>
        public static Type GetElementType(SerializedProperty property)
        {
            if (property.GetMemberInfo() is FieldInfo field)
            {
                var fieldType = field.FieldType;
                if (fieldType.IsArray) return fieldType.GetElementType();
                if (fieldType.IsGenericType && fieldType.GetGenericArguments() is { Length: 1 } arguments)
                    return arguments[0];
            }

            return property.arraySize > 0
                ? SerializeReferenceHelpers.GetFieldType(property.GetArrayElementAtIndex(0))
                : typeof(object);
        }
    }
}
