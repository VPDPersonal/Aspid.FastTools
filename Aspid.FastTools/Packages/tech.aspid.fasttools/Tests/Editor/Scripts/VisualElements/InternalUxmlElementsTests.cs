using System;
using System.IO;
using System.Linq;
using UnityEditor;
using NUnit.Framework;
using System.Reflection;
using Aspid.FastTools.Types;
using UnityEngine.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.VisualElements
{
    [TestFixture]
    internal sealed class InternalUxmlElementsTests
    {
        [Test]
        public void CloneTree_CreatesCustomControlsAndAppliesTheirUxmlAttributes()
        {
            var path = $"Assets/AspidFastToolsControls-{Guid.NewGuid():N}.uxml";

            try
            {
                File.WriteAllText(path,
                    @"<ui:UXML xmlns:ui=""UnityEngine.UIElements""
                               xmlns:internal=""Aspid.FastTools.UIElements.Editors.Internal""
                               xmlns:types=""Aspid.FastTools.Types.Editors"">
                        <internal:AspidBox theme=""Darkness"">
                            <internal:AspidLabel text=""Hello"" label-status=""Success""/>
                        </internal:AspidBox>
                        <types:TypeField label=""Type"" allow=""Abstract"" hide-none-option=""true""/>
                        <types:InspectorTypeField label=""Inspector"" is-read-only=""true""/>
                    </ui:UXML>");
                AssetDatabase.ImportAsset(path, options: ImportAssetOptions.ForceSynchronousImport);

                var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path).CloneTree();
                var box = root.Q<AspidBox>();
                var label = box.Q<AspidLabel>();
                var typeField = root.Q<TypeField>();
                var inspectorField = root.Q<InspectorTypeField>();

                Assert.AreEqual(ThemeStyle.Type.Darkness, box.Theme);
                Assert.AreEqual("Hello", label.Text);
                Assert.AreEqual(StatusStyle.Type.Success, label.LabelStatus);
                Assert.AreEqual("Type", typeField.label);
                Assert.AreEqual(TypeAllow.Abstract, typeField.Allow);
                Assert.IsTrue(typeField.HideNoneOption);
                Assert.AreEqual("Inspector", inspectorField.label);
                Assert.IsTrue(inspectorField.IsReadOnly);
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

#if ASPID_UNITY_HAS_UXML_LIBRARY
        [Test]
        public void InternalUxmlElements_AreHiddenInLibrary()
        {
            var elements = typeof(AspidBox).Assembly.GetTypes()
                .Where(type => !type.IsVisible && type.GetCustomAttribute<UxmlElementAttribute>(inherit: false) is not null)
                .ToArray();

            Assert.IsNotEmpty(elements, "No internal UXML elements found; update this test.");

            foreach (var element in elements)
            {
                var attribute = element.GetCustomAttribute<UxmlElementAttribute>(inherit: false);
                Assert.AreEqual(LibraryVisibility.Hidden, attribute.visibility,
                    $"{element.Name} must set visibility = LibraryVisibility.Hidden.");
            }
        }
#endif
    }
}
