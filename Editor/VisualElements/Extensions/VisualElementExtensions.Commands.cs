using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors
{
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Registers a double-click handler that opens <paramref name="obj"/>'s script in the IDE.
        /// </summary>
        /// <remarks>
        /// Supports <see cref="MonoBehaviour"/> and <see cref="ScriptableObject"/>; a resolved script is required,
        /// so it has no effect otherwise.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to register the command on.</param>
        /// <param name="obj">The object whose script is opened.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOpenScriptCommand<T>(this T element, Object obj)
            where T : VisualElement
        {
            var script = obj switch
            {
                MonoBehaviour mono => MonoScript.FromMonoBehaviour(mono),
                ScriptableObject scriptable => MonoScript.FromScriptableObject(scriptable),
                _ => null
            };

            if (!script) return element;

            var doubleClick = new DoubleClickTracker();
            element.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != (int)MouseButton.LeftMouse) return;
                if (doubleClick.Detect()) AssetDatabase.OpenAsset(script);
            });

            return element;
        }
    }
}
