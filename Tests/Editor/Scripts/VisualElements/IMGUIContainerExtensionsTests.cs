using System;
using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.VisualElements
{
    internal sealed class IMGUIContainerExtensionsTests
    {
        [Test]
        public void MarkDirtyLayoutSelf_ReturnsContainerForChaining()
        {
            var container = new IMGUIContainer();

            var result = container
                .MarkDirtyLayoutSelf()
                .SetCullingEnabled(true);

            Assert.AreSame(container, result);
            Assert.IsTrue(result.cullingEnabled);
        }

        [Test]
        public void SetOnGUIHandler_ReplacesTheHandler()
        {
            var calls = new List<string>();
            var container = new IMGUIContainer(() => calls.Add("old"));

            container.SetOnGUIHandler(() => calls.Add("new")).onGUIHandler();

            CollectionAssert.AreEqual(new[] { "new" }, calls);
        }

        [Test]
        public void AddAndRemoveOnGUIHandler_ChangeOnlyTheGivenHandler()
        {
            var calls = new List<string>();
            Action first = () => calls.Add("first");
            Action second = () => calls.Add("second");
            var container = new IMGUIContainer(first).AddOnGUIHandler(second);

            container.onGUIHandler();
            container.RemoveOnGUIHandler(first).onGUIHandler();

            CollectionAssert.AreEqual(new[] { "first", "second", "second" }, calls);
        }
    }
}
