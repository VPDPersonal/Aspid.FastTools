using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="Image"/>.
    /// </summary>
    public static class ImageExtensions
    {
        #region Image
        /// <summary>
        /// Sets <see cref="Image.image"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The texture to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetImage<T>(this T element, Texture value)
            where T : Image
        {
            element.image = value;
            return element;
        }

        /// <summary>
        /// Loads a <see cref="Texture"/> from Resources and sets the <see cref="Image.image"/> property.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources path of the texture to load.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetImageFromResources<T>(this T element, string path)
            where T : Image
        {
            var texture = Resources.Load<Texture>(path);
            if (texture == null)
            {
                Debug.LogWarning($"Failed to load Texture from Resources path: '{path}'");
                return element;
            }

            return element.SetImage(texture);
        }
        #endregion

        #region Sprite
        /// <summary>
        /// Sets <see cref="Image.sprite"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The sprite to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSprite<T>(this T element, Sprite value)
            where T : Image
        {
            element.sprite = value;
            return element;
        }

        /// <summary>
        /// Loads a <see cref="Sprite"/> from Resources and sets the <see cref="Image.sprite"/> property.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources path of the sprite to load.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSpriteFromResources<T>(this T element, string path)
            where T : Image
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"Failed to load Sprite from Resources path: '{path}'");
                return element;
            }

            return element.SetSprite(sprite);
        }
        #endregion

        #region VectorImage
        /// <summary>
        /// Sets <see cref="Image.vectorImage"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The vector image to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVectorImage<T>(this T element, VectorImage value)
            where T : Image
        {
            element.vectorImage = value;
            return element;
        }

        /// <summary>
        /// Loads a <see cref="VectorImage"/> from Resources and sets the <see cref="Image.vectorImage"/> property.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources path of the vector image to load.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVectorImageFromResources<T>(this T element, string path)
            where T : Image
        {
            var vectorImage = Resources.Load<VectorImage>(path);
            if (vectorImage == null)
            {
                Debug.LogWarning($"Failed to load VectorImage from Resources path: '{path}'");
                return element;
            }

            return element.SetVectorImage(vectorImage);
        }
        #endregion

        /// <summary>
        /// Sets <see cref="Image.uv"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The UV rect to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUv<T>(this T element, Rect value)
            where T : Image
        {
            element.uv = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Image.sourceRect"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The source rect to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSourceRect<T>(this T element, Rect value)
            where T : Image
        {
            element.sourceRect = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Image.tintColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The tint color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTintColor<T>(this T element, Color value)
            where T : Image
        {
            element.tintColor = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="Image.scaleMode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The scale mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetScaleMode<T>(this T element, ScaleMode value)
            where T : Image
        {
            element.scaleMode = value;
            return element;
        }
    }
}
