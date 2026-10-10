using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class BackgroundImageExtensionsTests
    {
        private Texture2D _texture;
        private Sprite _sprite;
        private VectorImage _vectorImage;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(2, 2) { hideFlags = HideFlags.HideAndDontSave };
            _sprite = Sprite.Create(_texture, new Rect(0, 0, 2, 2), Vector2.zero);
            _vectorImage = ScriptableObject.CreateInstance<VectorImage>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_vectorImage);
            Object.DestroyImmediate(_sprite);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void SetBackgroundImage_Sprite_SetsBackgroundOfElement()
        {
            var element = new VisualElement();

            var result = element
                .SetBackgroundImage(_sprite)
                .SetTooltip("Icon");

            Assert.AreSame(element, result);
            Assert.AreSame(_sprite, element.style.backgroundImage.value.sprite);
        }

        [Test]
        public void SetBackgroundImage_VectorImage_SetsBackgroundOfElement()
        {
            var element = new VisualElement();

            var result = element
                .SetBackgroundImage(_vectorImage)
                .SetTooltip("Icon");

            Assert.AreSame(element, result);
            Assert.AreSame(_vectorImage, element.style.backgroundImage.value.vectorImage);
        }

        [Test]
        public void SetBackgroundImage_Sprite_SetsBackgroundOfStyle()
        {
            var element = new VisualElement();

            var result = element.style.SetBackgroundImage(_sprite);

            Assert.AreSame(element.style, result);
            Assert.AreSame(_sprite, element.style.backgroundImage.value.sprite);
        }

        [Test]
        public void SetBackgroundImage_VectorImage_SetsBackgroundOfStyle()
        {
            var element = new VisualElement();

            var result = element.style.SetBackgroundImage(_vectorImage);

            Assert.AreSame(element.style, result);
            Assert.AreSame(_vectorImage, element.style.backgroundImage.value.vectorImage);
        }

        [Test]
        public void SetBackgroundImage_NullSprite_DoesNotThrow()
        {
            var element = new VisualElement();

            Assert.DoesNotThrow(() => element.SetBackgroundImage((Sprite)null));
            Assert.IsNull(element.style.backgroundImage.value.sprite);
        }
    }
}
