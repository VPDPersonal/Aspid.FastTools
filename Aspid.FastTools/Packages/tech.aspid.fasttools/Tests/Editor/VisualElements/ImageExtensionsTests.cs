using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class ImageExtensionsTests
    {
        private const string MissingPath = "Icons/aspid_icon_missing";
        private const string ExistingTexturePath = "Icons/aspid_icon_home";

        [Test]
        public void Setters_ReturnImageAndSetProperties()
        {
            var texture = new Texture2D(2, 2);
            var source = new Image();

            Image image = source
                .SetImage(texture)
                .SetTintColor(Color.red)
                .SetScaleMode(ScaleMode.ScaleAndCrop);

            Assert.AreSame(source, image);
            Assert.AreSame(texture, image.image);
            Assert.AreEqual(Color.red, image.tintColor);
            Assert.AreEqual(ScaleMode.ScaleAndCrop, image.scaleMode);

            Object.DestroyImmediate(texture);
        }

        // Image keeps uv and sourceRect in sync, so each is checked on its own.
        [Test]
        public void SetUv_SetsUv()
        {
            var uv = new Rect(0.25f, 0.25f, 0.5f, 0.5f);

            var image = new Image().SetUv(uv);

            Assert.AreEqual(uv, image.uv);
        }

        [Test]
        public void SetSourceRect_SetsSourceRect()
        {
            var texture = new Texture2D(4, 4);
            var sourceRect = new Rect(1f, 1f, 2f, 2f);

            var image = new Image().SetImage(texture).SetSourceRect(sourceRect);

            Assert.AreEqual(sourceRect, image.sourceRect);

            Object.DestroyImmediate(texture);
        }

        [Test]
        public void SetSprite_SetsSprite()
        {
            var texture = new Texture2D(2, 2);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), pivot: Vector2.zero);

            var image = new Image().SetSprite(sprite);

            Assert.AreSame(sprite, image.sprite);

            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(texture);
        }

        [Test]
        public void SetVectorImage_SetsVectorImage()
        {
            var vectorImage = ScriptableObject.CreateInstance<VectorImage>();

            var image = new Image().SetVectorImage(vectorImage);

            Assert.AreSame(vectorImage, image.vectorImage);

            Object.DestroyImmediate(vectorImage);
        }

        [Test]
        public void SetImageFromResources_ExistingPath_SetsTexture()
        {
            var expected = Resources.Load<Texture>(ExistingTexturePath);
            Assert.IsNotNull(expected, "The test texture must be loadable from Resources.");

            var image = new Image().SetImageFromResources(ExistingTexturePath);

            Assert.AreSame(expected, image.image);
        }

        [Test]
        public void SetImageFromResources_MissingPath_LogsWarningAndKeepsImage()
        {
            LogAssert.Expect(LogType.Warning, $"Failed to load Texture from Resources path: '{MissingPath}'");

            var image = new Image().SetImageFromResources(MissingPath);

            Assert.IsNull(image.image);
        }

        [Test]
        public void SetSpriteFromResources_MissingPath_LogsWarningAndKeepsImage()
        {
            LogAssert.Expect(LogType.Warning, $"Failed to load Sprite from Resources path: '{MissingPath}'");

            var image = new Image().SetSpriteFromResources(MissingPath);

            Assert.IsNull(image.sprite);
        }

        [Test]
        public void SetVectorImageFromResources_MissingPath_LogsWarningAndKeepsImage()
        {
            LogAssert.Expect(LogType.Warning, $"Failed to load VectorImage from Resources path: '{MissingPath}'");

            var image = new Image().SetVectorImageFromResources(MissingPath);

            Assert.IsNull(image.vectorImage);
        }
    }
}
