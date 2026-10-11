using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The prompt is modal and cannot be shown from a test, so its placement is checked on the rect it computes.
    [TestFixture]
    internal sealed class SerializeReferenceNamePromptTests
    {
        private static readonly Vector2 _size = new(340f, 96f);

        [Test]
        public void CenterOn_PlacesThePromptAtTheCenterOfTheOwner()
        {
            var rect = SerializeReferenceNamePrompt.CenterOn(new Rect(100f, 50f, 1000f, 800f), _size);

            Assert.AreEqual(new Rect(430f, 402f, 340f, 96f), rect);
        }

        [Test]
        public void CenterOn_OwnerOnASecondMonitor_StaysInsideTheOwner()
        {
            // A monitor left of the primary one has negative screen coordinates.
            var owner = new Rect(-1800f, 100f, 1200f, 700f);

            var rect = SerializeReferenceNamePrompt.CenterOn(owner, _size);

            Assert.IsTrue(owner.Contains(rect.min) && owner.Contains(rect.max), $"{rect} must lie inside {owner}.");
        }
    }
}
