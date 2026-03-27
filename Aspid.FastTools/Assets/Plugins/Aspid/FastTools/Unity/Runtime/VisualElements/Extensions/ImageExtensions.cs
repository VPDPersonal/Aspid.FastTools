using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Sets the texture displayed by the image element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetImage<T>(this T image, Texture2D texture)
            where T : Image
        {
            image.image = texture;
            return image;
        }

        /// <summary>
        /// Sets the texture displayed by the image element by loading it from <c>Resources</c> at the given path.
        /// </summary>
        /// <param name="image">The image element.</param>
        /// <param name="path">The resource path passed to <see cref="Resources.Load{T}(string)"/>.</param>
        /// <returns>The element for method chaining.</returns>
        public static T SetImageFromResource<T>(this T image, string path)
            where T : Image
        {
            return image.SetImage(Resources.Load<Texture2D>(path));
        }
    }
}