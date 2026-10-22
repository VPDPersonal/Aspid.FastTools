using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

namespace Aspid.FastTools.Types.Editors.Tests
{
    [Category("Graphics")]
    internal sealed class TypeSelectorWindowTests
    {
        private const string ProbePath = "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements/AspidThemeProbe.uss";
        private const string DropdownClass = "aspid-fasttools-type-selector--dropdown";

        private static readonly Rect _anchor = new(x: 100, y: 100, width: 200, height: 20);

        [TearDown]
        public void TearDown()
        {
            foreach (var selector in Resources.FindObjectsOfTypeAll<TypeSelectorWindow>())
                selector.Close();
        }

        [Test]
        public void Show_WithStyleSheet_StylesTheSelectorAsAnAspidWindow()
        {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ProbePath);
            Assert.IsNotNull(sheet, "The probe sheet must load.");

            TypeSelectorWindow.Show(
                screenRect: _anchor,
                filter: default,
                assemblyQualifiedName: string.Empty,
                onSelected: null,
                styleSheet: sheet);

            var view = OpenedView(out var root);
            Assert.IsTrue(root.ClassListContains(AspidStyles.WindowClass), "A styled selector must read the window palette.");
            Assert.IsTrue(view.styleSheets.Contains(sheet), "The selector must carry the passed sheet.");
            Assert.IsFalse(view.ClassListContains(DropdownClass), "A styled selector must drop the popup look.");
            Assert.AreEqual(!EditorGUIUtility.isProSkin, view.ClassListContains(AspidStyles.PaletteLightClass),
                "The palette must follow the editor skin.");
        }

        [Test]
        public void Show_WithoutStyleSheet_KeepsThePopupLook()
        {
            TypeSelectorWindow.Show(screenRect: _anchor);

            var view = OpenedView(out var root);
            Assert.IsFalse(root.ClassListContains(AspidStyles.WindowClass), "A plain selector must not read the window palette.");
            Assert.IsTrue(view.ClassListContains(DropdownClass), "A plain selector must keep the popup look.");
        }

        private static TypeSelectorView OpenedView(out VisualElement root)
        {
            var selectors = Resources.FindObjectsOfTypeAll<TypeSelectorWindow>();
            Assert.AreEqual(1, selectors.Length, "One selector must be open.");

            root = selectors[0].rootVisualElement;
            return root.Q<TypeSelectorView>();
        }
    }
}
