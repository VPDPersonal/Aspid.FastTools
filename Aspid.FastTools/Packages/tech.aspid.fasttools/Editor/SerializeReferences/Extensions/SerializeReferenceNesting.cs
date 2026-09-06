using UnityEditor;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Decides whether a child of an assigned managed reference is drawn by this package or handed back to Unity.
    // Shared by both inspector modes so one asset never behaves differently between them.
    internal static class SerializeReferenceNesting
    {
        // A managed-reference graph may be cyclic and each level draws the next, so an unbounded descent ends in an
        // uncatchable StackOverflowException that takes the Editor and the unsaved scene with it. Past the cap a
        // child falls back to Unity's drawing, which stops at the reference instead of following it.
        internal const int MaxDepth = 8;

        internal static bool DrawsOwnHeader(SerializedProperty child, int depth)
        {
            if (depth >= MaxDepth) return false;
            if (child.propertyType is not SerializedPropertyType.ManagedReference &&
                !SerializeReferenceHelpers.IsManagedReferenceArray(child)) return false;

            return !DrawnByUnity(child);
        }

        // True when Unity already has a body to draw, so drawing it here would discard the author's own UI. Plain
        // decorators like [Header] are deliberately excluded: the caller re-emits them, so one never costs the field
        // its dropdown.
        internal static bool DrawnByUnity(SerializedProperty child)
        {
            var field = child.GetFieldInfo();
            if (field is null) return false;

            return field.IsDefined(typeof(TypeSelectorAttribute), inherit: true) ||
                   CustomDrawerRegistry.HasDrawerFor(field.FieldType) ||
                   CustomDrawerRegistry.DeclaresDrawnAttribute(field);
        }

        // Decides whether the field offers an expand arrow at all.
        internal static bool HasVisibleChildren(SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();

            return iterator.NextVisible(enterChildren: true) && !SerializedProperty.EqualContents(iterator, end);
        }
    }
}
