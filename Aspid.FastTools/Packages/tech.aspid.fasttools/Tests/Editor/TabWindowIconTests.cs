using UnityEngine;
using NUnit.Framework;
using System.Reflection;

namespace Aspid.FastTools.Editors.Tests
{
    /// <summary>
    /// The tab icon is drawn at 16 pt: a large texture without mipmaps only adds package weight and makes the edges shimmer.
    /// </summary>
    [TestFixture]
    internal sealed class TabWindowIconTests
    {
        private const int MaxIconSize = 64;

        [Test]
        public void TabIcon_IsSmallAndMipmapped()
        {
            var field = typeof(TabWindow).GetField("WindowIconPath", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "TabWindow.WindowIconPath was not found; update this test.");

            var icon = Resources.Load<Texture2D>((string)field.GetRawConstantValue());
            Assert.IsNotNull(icon, "The tab icon does not load from Resources.");

            Assert.LessOrEqual(Mathf.Max(icon.width, icon.height), MaxIconSize);
            Assert.Greater(icon.mipmapCount, 1, "The tab icon has no mipmaps.");
        }
    }
}
