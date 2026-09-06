using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Dropdown window for browsing and selecting a <see cref="System.Type"/> from a filtered list.
    /// </summary>
    /// <remarks>
    /// A thin host around the selector view, which owns the search, navigation and generic-argument flow.
    /// </remarks>
    public sealed class TypeSelectorWindow : EditorWindow
    {
        /// <summary>
        /// Opens the selector as a dropdown anchored to <paramref name="screenRect"/>.
        /// </summary>
        /// <param name="screenRect">Screen-space rectangle the dropdown is anchored to.</param>
        /// <param name="filter">Which types the selector offers.</param>
        /// <param name="currentAqn">Assembly-qualified name of the current type, pre-navigated to; empty starts at
        /// the root.</param>
        /// <param name="onSelected">Receives the assembly-qualified name of the selected type — the constructed
        /// closed type for a resolved open generic — or <see langword="null"/> for <c>&lt;None&gt;</c>.</param>
        public static void Show(
            Rect screenRect,
            TypeSelectorFilter filter = default,
            string currentAqn = "",
            Action<string> onSelected = null)
        {
            var window = CreateInstance<TypeSelectorWindow>();
            var view = new TypeSelectorView(filter, currentAqn, onSelected, onDismiss: window.Close);

            window.rootVisualElement.AddChild(view);

            // 400 keeps the footer's keyboard hint visible beside the settings gear; a longer variant ellipsizes
            // rather than pushing the gear out.
            var size = new Vector2(Mathf.Max(400, screenRect.width), 320);
            window.ShowAsDropDown(screenRect, size);

            view.FocusPicker();
        }
    }
}
