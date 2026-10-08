using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class VisualElementClassesExtensionsTests
    {
        private static readonly string[] _classNames = { "a", "b", "c" };

        private static VisualElement CreateElement()
        {
            var element = new VisualElement();
            foreach (var className in _classNames)
                element.AddToClassList(className);

            return element;
        }

        [Test]
        public void AddClasses_Array_AddsEveryClass()
        {
            var element = new VisualElement();

            element.AddClasses("a", "b", "c");

            CollectionAssert.AreEqual(_classNames, element.GetClasses().ToArray());
        }

        [Test]
        public void AddClasses_List_AddsEveryClass()
        {
            var element = new VisualElement();

            element.AddClasses(new List<string>(_classNames));

            CollectionAssert.AreEqual(_classNames, element.GetClasses().ToArray());
        }

        [Test]
        public void AddClasses_ReadOnlySpan_AddsEveryClass()
        {
            var element = new VisualElement();

            element.AddClasses(new ReadOnlySpan<string>(_classNames));

            CollectionAssert.AreEqual(_classNames, element.GetClasses().ToArray());
        }

        [Test]
        public void AddClasses_SkipsNullEntries()
        {
            var element = new VisualElement();

            element.AddClasses("a", null, "b");

            CollectionAssert.AreEqual(new[] { "a", "b" }, element.GetClasses().ToArray());
        }

        [Test]
        public void AddClasses_ReadsQueryOverOwnClasses()
        {
            var element = CreateElement();

            element.AddClasses(element.GetClasses().Select(className => className + "-x"));

            CollectionAssert.AreEqual(new[] { "a", "b", "c", "a-x", "b-x", "c-x" }, element.GetClasses().ToArray());
        }

        [Test]
        public void AddClasses_CopiesClassesOfAnotherElement()
        {
            var source = CreateElement();
            var target = new VisualElement();

            target.AddClasses(source.GetClasses());

            CollectionAssert.AreEqual(_classNames, target.GetClasses().ToArray());
        }

        [Test]
        public void AddClassesIf_False_LeavesElementUnchanged()
        {
            var element = new VisualElement();

            element.AddClassesIf(condition: false, "a", "b", "c");

            CollectionAssert.IsEmpty(element.GetClasses());
        }

        [Test]
        public void RemoveClasses_RemovesEveryOwnClass()
        {
            var element = CreateElement();

            element.RemoveClasses(element.GetClasses());

            CollectionAssert.IsEmpty(element.GetClasses());
        }

        [Test]
        public void RemoveClassesIf_RemovesEveryOwnClass()
        {
            var element = CreateElement();

            element.RemoveClassesIf(condition: true, element.GetClasses());

            CollectionAssert.IsEmpty(element.GetClasses());
        }

        [Test]
        public void ToggleClasses_SwapsTwoStates()
        {
            var element = new VisualElement().AddClass("expanded");

            element.ToggleClasses("expanded", "collapsed");

            CollectionAssert.AreEqual(new[] { "collapsed" }, element.GetClasses().ToArray());
        }

        [Test]
        public void ToggleClasses_RemovesEveryOwnClass()
        {
            var element = CreateElement();

            element.ToggleClasses(element.GetClasses());

            CollectionAssert.IsEmpty(element.GetClasses());
        }

        [Test]
        public void EnableClasses_False_RemovesEveryOwnClass()
        {
            var element = CreateElement();

            element.EnableClasses(enable: false, element.GetClasses());

            CollectionAssert.IsEmpty(element.GetClasses());
        }

        [Test]
        public void EnableClassesIf_True_AddsEveryClass()
        {
            var element = new VisualElement();

            element.EnableClassesIf(condition: true, enable: true, "a", "b", "c");

            CollectionAssert.AreEqual(_classNames, element.GetClasses().ToArray());
        }

        [Test]
        public void EnableClassIf_False_LeavesElementUnchanged()
        {
            var element = new VisualElement();

            element.EnableClassIf(condition: false, "a", enable: true);

            CollectionAssert.IsEmpty(element.GetClasses());
        }

        [Test]
        public void ClearClassesIf_True_RemovesEveryClass()
        {
            var element = CreateElement();

            element.ClearClassesIf(condition: true);

            CollectionAssert.IsEmpty(element.GetClasses());
        }
    }
}
