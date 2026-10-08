using System;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class VisualElementStyleSheetsExtensionsTests
    {
        private const string ExistingPath = "UI/Aspid-FastTools-Default-Dark";
        private const string MissingPath = "UI/Aspid-FastTools-Missing";

        private StyleSheet _first;
        private StyleSheet _second;
        private StyleSheet _third;

        [SetUp]
        public void SetUp()
        {
            _first = ScriptableObject.CreateInstance<StyleSheet>();
            _second = ScriptableObject.CreateInstance<StyleSheet>();
            _third = ScriptableObject.CreateInstance<StyleSheet>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
            Object.DestroyImmediate(_third);
        }

        private static StyleSheet[] GetStyleSheets(VisualElement element) =>
            Enumerable.Range(0, element.styleSheets.count).Select(index => element.styleSheets[index]).ToArray();

        [Test]
        public void AddStyleSheets_Array_KeepsOrder()
        {
            var element = new VisualElement();

            element.AddStyleSheets(_first, _second);

            CollectionAssert.AreEqual(new[] { _first, _second }, GetStyleSheets(element));
        }

        [Test]
        public void AddStyleSheets_List_KeepsOrder()
        {
            var element = new VisualElement();

            element.AddStyleSheets(new List<StyleSheet> { _first, _second });

            CollectionAssert.AreEqual(new[] { _first, _second }, GetStyleSheets(element));
        }

        [Test]
        public void AddStyleSheets_ReadOnlySpan_KeepsOrder()
        {
            var element = new VisualElement();

            element.AddStyleSheets(new ReadOnlySpan<StyleSheet>(new[] { _first, _second }));

            CollectionAssert.AreEqual(new[] { _first, _second }, GetStyleSheets(element));
        }

        [Test]
        public void AddStyleSheets_SkipsNullEntries()
        {
            var element = new VisualElement();

            element.AddStyleSheets(_first, null, _second);

            CollectionAssert.AreEqual(new[] { _first, _second }, GetStyleSheets(element));
        }

        [Test]
        public void AddStyleSheet_Null_LeavesElementUnchanged()
        {
            var element = new VisualElement();

            element.AddStyleSheet(null).RemoveStyleSheet(null);

            Assert.AreEqual(0, element.styleSheets.count);
        }

        [Test]
        public void AddStyleSheetsIf_False_LeavesElementUnchanged()
        {
            var element = new VisualElement();

            element.AddStyleSheetsIf(condition: false, _first, _second);

            Assert.AreEqual(0, element.styleSheets.count);
        }

        [Test]
        public void InsertStyleSheet_InsertsAtIndex()
        {
            var element = new VisualElement().AddStyleSheet(_first);

            element.InsertStyleSheet(0, _second);

            CollectionAssert.AreEqual(new[] { _second, _first }, GetStyleSheets(element));
        }

        [Test]
        public void InsertStyleSheets_SkipsNullEntries()
        {
            var element = new VisualElement().AddStyleSheet(_third);

            element.InsertStyleSheets(0, _first, null, _second);

            CollectionAssert.AreEqual(new[] { _first, _second, _third }, GetStyleSheets(element));
        }

        [Test]
        public void InsertStyleSheets_KeepsPositionOfPresentStyleSheet()
        {
            var element = new VisualElement().AddStyleSheet(_first);

            element.InsertStyleSheets(0, _second, _first, _third);

            CollectionAssert.AreEqual(new[] { _second, _third, _first }, GetStyleSheets(element));
        }

        [Test]
        public void RemoveStyleSheets_RemovesEveryStyleSheet()
        {
            var element = new VisualElement().AddStyleSheets(_first, _second, _third);

            element.RemoveStyleSheets(_first, null, _third);

            CollectionAssert.AreEqual(new[] { _second }, GetStyleSheets(element));
        }

        [Test]
        public void EnableStyleSheet_AddsAndRemoves()
        {
            var element = new VisualElement();

            element.EnableStyleSheet(_first, enable: true);
            Assert.IsTrue(element.styleSheets.Contains(_first));

            element.EnableStyleSheet(_first, enable: false);
            Assert.AreEqual(0, element.styleSheets.count);
        }

        [Test]
        public void EnableStyleSheets_False_RemovesEveryStyleSheet()
        {
            var element = new VisualElement().AddStyleSheets(_first, _second);

            element.EnableStyleSheets(enable: false, _first, _second);

            Assert.AreEqual(0, element.styleSheets.count);
        }

        [Test]
        public void AddStyleSheetFromResources_AddsStyleSheet()
        {
            var element = new VisualElement();

            element.AddStyleSheetFromResources(ExistingPath);

            Assert.AreEqual(1, element.styleSheets.count);
        }

        [Test]
        public void AddStyleSheetsFromResources_SkipsMissingPath()
        {
            var element = new VisualElement();
            LogAssert.Expect(LogType.Warning, $"Failed to load StyleSheet from Resources path: '{MissingPath}'");

            element.AddStyleSheetsFromResources(MissingPath, ExistingPath);

            Assert.AreEqual(1, element.styleSheets.count);
        }

        [Test]
        public void RemoveStyleSheetsFromResources_RemovesStyleSheet()
        {
            var element = new VisualElement().AddStyleSheetFromResources(ExistingPath);

            element.RemoveStyleSheetsFromResources(ExistingPath);

            Assert.AreEqual(0, element.styleSheets.count);
        }

        [Test]
        public void ClearStyleSheets_RemovesEveryStyleSheet()
        {
            var element = new VisualElement().AddStyleSheets(_first, _second);

            element.ClearStyleSheets();

            Assert.AreEqual(0, element.styleSheets.count);
        }

        [Test]
        public void ClearStyleSheetsIf_False_LeavesElementUnchanged()
        {
            var element = new VisualElement().AddStyleSheets(_first, _second);

            element.ClearStyleSheetsIf(condition: false);

            Assert.AreEqual(2, element.styleSheets.count);
        }
    }
}
