using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools
{
    public static class FoldoutExtensions
    {
        /// <summary>
        /// Sets the header text of the foldout.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetText<T>(this T foldout, string text)
            where T : Foldout
        {
            foldout.text = text;
            return foldout;
        }

        /// <summary>
        /// Sets the expanded state of the foldout.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetValue<T>(this T foldout, bool value)
            where T : Foldout
        {
            foldout.value = value;
            return foldout;
        }
    }
}