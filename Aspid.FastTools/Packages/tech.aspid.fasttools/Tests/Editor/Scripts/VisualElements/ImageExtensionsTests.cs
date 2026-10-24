using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.VisualElements
{
    [TestFixture]
    internal sealed class ImageExtensionsTests
    {
        private const string MissingPath = "Icons/aspid_icon_missing";
        private const string ExistingTexturePath = "Icons/aspid_icon_home";

        private readonly List<Object> _created = new();

        // Created objects are destroyed after each test, also when an assert fails.
        private T Track<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.DestroyImmediate(created);

            _created.Clear();
        }

        [Test]
        public void Setters_ReturnImageAndSetProperties()
        {
            var texture = Track(new Texture2D(width: 2, height: 2));
            var source = new Image();

            Image image = source
                .SetImage(texture)
                .SetTintColor(Color.red)
                .SetScaleMode(ScaleMode.ScaleAndCrop);

            Assert.AreSame(source, image);
            Assert.AreSame(texture, image.image);
            Assert.AreEqual(Color.red, image.tintColor);
            Assert.AreEqual(ScaleMode.ScaleAndCrop, image.scaleMode);
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
            var texture = Track(new Texture2D(width: 4, height: 4));
            var sourceRect = new Rect(1f, 1f, 2f, 2f);

            var image = new Image().SetImage(texture).SetSourceRect(sourceRect);

            Assert.AreEqual(sourceRect, image.sourceRect);
        }

        [Test]
        public void SetSprite_SetsSprite()
        {
            var texture = Track(new Texture2D(width: 2, height: 2));
            var sprite = Track(Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), pivot: Vector2.zero));

            var image = new Image().SetSprite(sprite);

            Assert.AreSame(sprite, image.sprite);
        }

        [Test]
        public void SetVectorImage_SetsVectorImage()
        {
            var vectorImage = Track(ScriptableObject.CreateInstance<VectorImage>());

            var image = new Image().SetVectorImage(vectorImage);

            Assert.AreSame(vectorImage, image.vectorImage);
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
