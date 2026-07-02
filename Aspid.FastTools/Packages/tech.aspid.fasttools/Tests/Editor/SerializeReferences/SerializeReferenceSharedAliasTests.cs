using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A plain field plus a list, so alias paths cover both the "field" and the "field.Array.data[i]" shapes.
    internal sealed class SharedAliasTestObject : ScriptableObject
    {
        [SerializeReference] public ITestWeapon primary;
        [SerializeReference] public List<ITestWeapon> sidearms = new();
    }

    /// <summary>
    /// Covers the shared-reference notice's alias listing: <see cref="SerializeReferenceHelpers.GetSharedReferenceAliasPaths"/>
    /// (the OTHER members of a property's shared group, in document order),
    /// <see cref="SerializeReferenceHelpers.GetPropertyDisplayPath"/> (the inspector-style display form those paths are
    /// shown in) and <see cref="SerializeReferenceHelpers.BuildSharedReferenceDetail"/> (the tooltip both drawers share).
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceSharedAliasTests
    {
        [Test]
        public void GetPropertyDisplayPath_NicifiesFieldsAndListElements()
        {
            Assert.AreEqual("Primary", SerializeReferenceHelpers.GetPropertyDisplayPath("primary"));
            Assert.AreEqual("Sidearms › Element 1", SerializeReferenceHelpers.GetPropertyDisplayPath("sidearms.Array.data[1]"));
            Assert.AreEqual("Sidearms › Element 0 › On Hit Effect",
                SerializeReferenceHelpers.GetPropertyDisplayPath("sidearms.Array.data[0].onHitEffect"));
        }

        [Test]
        public void GetSharedReferenceAliasPaths_ListsTheOtherMembers_AndOnlyThem()
        {
            var obj = ScriptableObject.CreateInstance<SharedAliasTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);
                serialized.FindProperty("primary").managedReferenceValue = new TestSword { damage = 7 };
                serialized.FindProperty("sidearms").arraySize = 1;
                serialized.ApplyModifiedProperties();

                // Alias the list element onto the primary's instance (one shared rid across both paths).
                Assert.IsTrue(SerializeReferenceLinker.LinkTo(serialized.FindProperty("sidearms.Array.data[0]"), "primary"));
                serialized.Update();
                SerializeReferenceHelpers.InvalidateSharedReferenceCache();

                var fromPrimary = SerializeReferenceHelpers.GetSharedReferenceAliasPaths(serialized.FindProperty("primary"));
                Assert.AreEqual(new[] { "sidearms.Array.data[0]" }, fromPrimary,
                    "The primary field's alias list must hold exactly the list element, not itself.");

                var fromElement = SerializeReferenceHelpers.GetSharedReferenceAliasPaths(
                    serialized.FindProperty("sidearms.Array.data[0]"));
                Assert.AreEqual(new[] { "primary" }, fromElement,
                    "The list element's alias list must hold exactly the primary field, not itself.");
            }
            finally { Object.DestroyImmediate(obj); }
        }

        [Test]
        public void GetSharedReferenceAliasPaths_EmptyWhenNotShared()
        {
            var obj = ScriptableObject.CreateInstance<SharedAliasTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);
                serialized.FindProperty("primary").managedReferenceValue = new TestSword();
                serialized.ApplyModifiedProperties();
                serialized.Update();
                SerializeReferenceHelpers.InvalidateSharedReferenceCache();

                Assert.IsEmpty(SerializeReferenceHelpers.GetSharedReferenceAliasPaths(serialized.FindProperty("primary")),
                    "A reference used by a single field is not part of any shared group.");
            }
            finally { Object.DestroyImmediate(obj); }
        }

        [Test]
        public void BuildSharedReferenceDetail_ListsAliasesByDisplayPath()
        {
            var obj = ScriptableObject.CreateInstance<SharedAliasTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);
                serialized.FindProperty("primary").managedReferenceValue = new TestSword();
                serialized.FindProperty("sidearms").arraySize = 1;
                serialized.ApplyModifiedProperties();

                Assert.IsTrue(SerializeReferenceLinker.LinkTo(serialized.FindProperty("sidearms.Array.data[0]"), "primary"));
                serialized.Update();
                SerializeReferenceHelpers.InvalidateSharedReferenceCache();

                var detail = SerializeReferenceHelpers.BuildSharedReferenceDetail(serialized.FindProperty("primary"));

                StringAssert.Contains("Also used by:", detail,
                    "A shared reference's tooltip must announce the other fields using the instance.");
                StringAssert.Contains("Sidearms › Element 0", detail,
                    "The alias must be listed by its inspector-style display path, not the raw property path.");
                StringAssert.Contains("Make unique", detail,
                    "The tooltip must explain the notice's Make-unique affordance.");
            }
            finally { Object.DestroyImmediate(obj); }
        }
    }
}
