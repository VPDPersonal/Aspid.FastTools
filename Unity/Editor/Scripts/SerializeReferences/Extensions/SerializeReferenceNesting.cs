using UnityEditor;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The single rule deciding whether a child property of an assigned managed reference is drawn by this package
    /// (its own header with a type dropdown) or handed back to Unity. Shared by the UIToolkit field and the IMGUI
    /// drawer so the same asset never behaves differently between the two inspector modes.
    /// </summary>
    internal static class SerializeReferenceNesting
    {
        /// <summary>
        /// How deep the nested dropdown follows a managed-reference graph.
        /// </summary>
        /// <remarks>
        /// A managed-reference graph may be cyclic (<c>a.Next = a</c> — the reason every other walk in this feature
        /// carries a visited guard), and each level draws the next, so an unbounded descent ends in a
        /// <see cref="System.StackOverflowException"/>: uncatchable, and fatal to the Editor along with the unsaved
        /// scene. Past the cap a child falls back to Unity's own drawing, which stops at the reference instead of
        /// following it.
        /// </remarks>
        internal const int MaxDepth = 8;

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="child"/> is a managed reference (or an array/list of
        /// them) this package should draw its own header for.
        /// </summary>
        internal static bool DrawsOwnHeader(SerializedProperty child, int depth)
        {
            if (depth >= MaxDepth) return false;
            if (child.propertyType is not SerializedPropertyType.ManagedReference &&
                !SerializeReferenceHelpers.IsManagedReferenceArray(child)) return false;

            return !DrawnByUnity(child);
        }

        /// <summary>
        /// Returns <see langword="true"/> when Unity already has a body to draw for <paramref name="child"/>, so
        /// drawing it here would silently discard the author's own UI.
        /// </summary>
        /// <remarks>
        /// Three ways that happens: a <c>[TypeSelector]</c> on the child (whose drawer draws this very dropdown and
        /// narrows the candidates besides), a <c>[CustomPropertyDrawer]</c> registered for the child's declared
        /// type, or a property attribute that brings a drawer of its own. Plain decorators —
        /// <see cref="UnityEngine.HeaderAttribute"/> and friends — are deliberately not on that list: they are
        /// re-emitted by the caller, so adding one never costs the field its dropdown.
        /// </remarks>
        internal static bool DrawnByUnity(SerializedProperty child)
        {
            var field = child.GetFieldInfo();
            if (field is null) return false;

            return field.IsDefined(typeof(TypeSelectorAttribute), inherit: true) ||
                   CustomDrawerRegistry.HasDrawerFor(field.FieldType) ||
                   CustomDrawerRegistry.DeclaresDrawnAttribute(field);
        }

        /// <summary>
        /// Returns <see langword="true"/> when the assigned instance has at least one visible serialized child —
        /// what decides whether the field offers an expand arrow at all.
        /// </summary>
        internal static bool HasVisibleChildren(SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();

            return iterator.NextVisible(enterChildren: true) && !SerializedProperty.EqualContents(iterator, end);
        }
    }
}
