using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// <see cref="EditorWindow"/> for selecting a type from a filtered hierarchy.
    /// </summary>
    public sealed class TypeSelectorWindow : EditorWindow
    {
        /// <summary>
        /// Opens the selector as a dropdown anchored to <paramref name="screenRect"/>.
        /// </summary>
        /// <param name="screenRect">Screen-space rectangle the dropdown is anchored to.</param>
        /// <param name="filter">Which types the selector offers.</param>
        /// <param name="currentAqn">The current type name; empty selects the empty row, while <see langword="null"/> leaves the selection unset.</param>
        /// <param name="onSelected">Receives the assembly-qualified name of the selected type — the constructed
        /// closed type for a resolved open generic — or <see langword="null"/> for <c>&lt;None&gt;</c>; a <see langword="null"/> callback is ignored.</param>
        public static void Show(
            Rect screenRect,
            TypeSelectorFilter filter = default,
            string currentAqn = "",
            Action<string> onSelected = null)
        {
            var window = CreateInstance<TypeSelectorWindow>();
            var view = new TypeSelectorView(filter, currentAqn, onSelected, onDismiss: window.Close);

            window.rootVisualElement.AddChild(view);

            var size = new Vector2(Mathf.Max(400, screenRect.width), 320);
            window.ShowAsDropDown(screenRect, size);

            view.FocusPicker();
        }
    }
}
