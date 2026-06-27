using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Editor window that displays a hierarchical type selector dropdown, allowing the user to browse and select a <see cref="System.Type"/> from a filtered list.
    /// </summary>
    /// <remarks>
    /// A thin dropdown host around <see cref="TypeSelectorView"/>, which owns the search, navigation and
    /// generic-argument flow. Embedding hosts (e.g. the Repair References window) use the view directly.
    /// </remarks>
    public sealed class TypeSelectorWindow : EditorWindow
    {
        /// <summary>
        /// Opens the type selector window as a dropdown anchored to <paramref name="screenRect"/>.
        /// </summary>
        /// <param name="screenRect">The screen-space rectangle the dropdown is anchored to.</param>
        /// <param name="types">Base types used to filter which concrete types are shown. Only types assignable to all entries are listed.</param>
        /// <param name="currentAqn">Assembly-qualified name of the currently selected type, used to pre-navigate to that type's location. Pass <c>null</c> or empty to start at the root.</param>
        /// <param name="allow">Which type kinds are included in the list. Defaults to <c>TypeAllow.None</c>.</param>
        /// <param name="onSelected">Callback invoked with the assembly-qualified name of the selected type, or <c>null</c> if the user chose <c>&lt;None&gt;</c>. When an open generic is resolved, the assembly-qualified name of the constructed closed type is passed.</param>
        /// <param name="filter">Optional predicate applied to each candidate type after the base-type and <paramref name="allow"/> checks. Return <c>false</c> to hide a type. Pass <c>null</c> to keep every matching type.</param>
        /// <param name="additionalTypes">Optional extra types appended to the list verbatim, bypassing the base-type and <paramref name="allow"/> checks — used to inject entries the assignability scan cannot match, such as open generic definitions.</param>
        /// <param name="argumentFilter">Optional predicate applied to candidate types offered for an open generic's type arguments (in addition to the parameter's own constraints). Used to restrict arguments to, e.g., Unity-serializable types. Pass <c>null</c> to accept any constraint-satisfying type.</param>
        public static void Show(
            Rect screenRect,
            Type[] types = null,
            string currentAqn = "",
            TypeAllow allow = TypeAllow.None,
            Action<string> onSelected = null,
            Func<Type, bool> filter = null,
            IEnumerable<Type> additionalTypes = null,
            Func<Type, bool> argumentFilter = null)
        {
            var window = CreateInstance<TypeSelectorWindow>();

            var view = new TypeSelectorView(
                types,
                currentAqn,
                allow,
                onSelected,
                filter,
                additionalTypes,
                argumentFilter,
                onDismiss: window.Close);

            window.rootVisualElement.AddChild(view);

            var size = new Vector2(Mathf.Max(350, screenRect.width), 320);
            window.ShowAsDropDown(screenRect, size);

            view.FocusPicker();
        }
    }
}
