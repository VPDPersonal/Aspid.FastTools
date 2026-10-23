using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// <see cref="EditorWindow"/> for selecting a type from a filtered hierarchy.
    /// </summary>
    public sealed class TypeSelectorWindow : EditorWindow
    {
        // The narrowest dropdown; a caller that places the window by its width reads it from here.
        internal const float MinWidth = 400f;

        /// <summary>
        /// Opens the selector as a dropdown anchored to <paramref name="screenRect"/>.
        /// </summary>
        /// <param name="screenRect">Screen-space rectangle the dropdown is anchored to.</param>
        /// <param name="filter">Which types the selector offers.</param>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the current type; empty selects the empty row, while <see langword="null"/> leaves the selection unset.</param>
        /// <param name="onSelected">Receives the assembly-qualified name of the selected type — the constructed
        /// closed type for a resolved open generic — or <see langword="null"/> for <c>&lt;None&gt;</c>; a <see langword="null"/> callback is ignored.</param>
        public static void Show(
            Rect screenRect,
            TypeSelectorFilter filter = default,
            string assemblyQualifiedName = "",
            Action<string> onSelected = null) =>
            Open(screenRect, filter, assemblyQualifiedName, onSelected, styleSheet: null);

        /// <summary>
        /// Opens the selector as a dropdown anchored to <paramref name="screenRect"/> and styled by
        /// <paramref name="styleSheet"/>.
        /// </summary>
        /// <remarks>
        /// The selector takes the Aspid.FastTools palette and theme, as
        /// <see cref="UIElements.Editors.VisualElementExtensions.AddAspidWindowTheme{T}"/> gives them, in place of the
        /// colors of Unity's popups. Its root has the <c>aspid-fasttools-type-selector</c> class.
        /// </remarks>
        /// <param name="screenRect">Screen-space rectangle the dropdown is anchored to.</param>
        /// <param name="filter">Which types the selector offers.</param>
        /// <param name="assemblyQualifiedName">The assembly-qualified name of the current type; empty selects the empty row, while <see langword="null"/> leaves the selection unset.</param>
        /// <param name="onSelected">Receives the assembly-qualified name of the selected type — the constructed
        /// closed type for a resolved open generic — or <see langword="null"/> for <c>&lt;None&gt;</c>; a <see langword="null"/> callback is ignored.</param>
        /// <param name="styleSheet">The style sheet added after the selector's own; <see langword="null"/> keeps the
        /// look of Unity's popups.</param>
        public static void Show(
            Rect screenRect,
            TypeSelectorFilter filter,
            string assemblyQualifiedName,
            Action<string> onSelected,
            StyleSheet styleSheet) =>
            Open(screenRect, filter, assemblyQualifiedName, onSelected, styleSheet);

        private static void Open(
            Rect screenRect,
            TypeSelectorFilter filter,
            string assemblyQualifiedName,
            Action<string> onSelected,
            StyleSheet styleSheet)
        {
            var window = CreateInstance<TypeSelectorWindow>();
            var view = new TypeSelectorView(filter: filter, currentAqn: assemblyQualifiedName, onSelected: onSelected, onDismiss: window.Close);

            if (styleSheet == null) view.UseDropdownStyle();
            else
            {
                window.rootVisualElement.AddClass(AspidStyles.WindowClass);
                view.AddStyleSheet(styleSheet);
            }

            window.rootVisualElement.AddChild(view);

            var size = new Vector2(Mathf.Max(MinWidth, screenRect.width), 320);
            window.ShowAsDropDown(screenRect, size);

            view.FocusPicker();
        }
    }
}
