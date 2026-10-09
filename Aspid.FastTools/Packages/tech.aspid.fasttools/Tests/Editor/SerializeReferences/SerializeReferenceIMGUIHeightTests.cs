using System.IO;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The IMGUI drawer must reserve the rows Draw paints: the field line, a row per notice and a row per child.
    // A mismatch overlaps rows or leaves a gap, and only an IMGUI inspector shows it.
    // Measuring needs no GUI context, so these tests run without a window.
    [TestFixture]
    internal sealed class SerializeReferenceIMGUIHeightTests
    {
        private const string MissingProbePath = "Assets/__AspidIMGUIHeightProbe__.asset";

        private static float Line => EditorGUIUtility.singleLineHeight;

        private static float Spacing => EditorGUIUtility.standardVerticalSpacing;

        [Test]
        public void GetHeight_EmptyField_IsOneLine()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);

                Assert.AreEqual(Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(serialized.FindProperty("a")));
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void GetHeight_CollapsedValue_IsOneLine()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new TestSword();
                var property = new SerializedObject(obj).FindProperty("a");
                property.isExpanded = false;

                Assert.AreEqual(Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "A collapsed value must not reserve the rows of its children.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void GetHeight_ExpandedValue_AddsOneRowPerChildField()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new TestSword();
                var property = new SerializedObject(obj).FindProperty("a");
                property.isExpanded = true;

                Assert.AreEqual(Line + Line + Spacing, SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "TestSword has one child field, which takes one row.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void GetHeight_ExpandedValue_MeasuresANestedReferenceWithItsOwnChildren()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new IMGUINestingWeapon { level = 3 };
                var serialized = new SerializedObject(obj);
                var property = serialized.FindProperty("a");
                property.isExpanded = true;

                // The nested header is one line while the nested field is empty.
                Assert.AreEqual(Line + (Line + Spacing) + (Line + Spacing),
                    SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "The level field and the empty nested header take one row each.");

                obj.a = new IMGUINestingWeapon { level = 3, inner = new TestSword() };
                serialized.Update();
                property = serialized.FindProperty("a");
                property.isExpanded = true;
                serialized.FindProperty("a.inner").isExpanded = true;

                Assert.AreEqual(Line + (Line + Spacing) + (Line + (Line + Spacing) + Spacing),
                    SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "An expanded nested value must add its own child rows inside the outer field.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void GetHeight_RequiredAndEmpty_ReservesTheNoticeRow()
        {
            var obj = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);
                var property = serialized.FindProperty("requiredRef");

                Assert.AreEqual(Line + Spacing + Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "A required field without a value shows a notice row under the field line.");

                property.managedReferenceValue = new TestSword();
                serialized.ApplyModifiedProperties();
                serialized.Update();

                property = serialized.FindProperty("requiredRef");
                property.isExpanded = false;

                Assert.AreEqual(Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "A required field with a value shows no notice.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void GetHeight_SharedReference_ReservesTheNoticeRow()
        {
            var obj = CreateSharedPair(out var serialized);
            try
            {
                var holder = serialized.FindProperty("a");
                var alias = serialized.FindProperty("b");
                holder.isExpanded = false;
                alias.isExpanded = false;

                Assert.AreEqual(Line + Spacing + Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(holder),
                    "Both holders of a shared reference show the notice row.");
                Assert.AreEqual(Line + Spacing + Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(alias));
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void GetHeight_SharedReferenceExpanded_AddsTheChildRowsAndTheNoticeRow()
        {
            var obj = CreateSharedPair(out var serialized);
            try
            {
                var property = serialized.FindProperty("a");
                property.isExpanded = true;

                Assert.AreEqual(Line + (Spacing + Line) + (Line + Spacing),
                    SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "The notice row and the child rows are both reserved.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void GetHeight_MissingType_ReservesTheNoticeRow()
        {
            var probe = ScriptableObject.CreateInstance<LinkerTestObject>();
            probe.a = new TestSword();
            try
            {
                AssetDatabase.CreateAsset(probe, MissingProbePath);
                AssetDatabase.SaveAssets();

                // The saved type no longer resolves and the live value is null, as Unity loads a missing type.
                var yaml = File.ReadAllText(MissingProbePath);
                StringAssert.Contains("class: TestSword,", yaml);
                File.WriteAllText(MissingProbePath, yaml.Replace("class: TestSword,", "class: GoneTestSword,"));
                probe.a = null;

                var missing = new SerializedObject(probe).FindProperty("a");
                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(missing), "The probe must hold a missing type.");

                Assert.AreEqual(Line + Spacing + Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(missing),
                    "A missing type shows a notice row under the field line.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(MissingProbePath);
            }
        }

        [Test]
        public void GetHeight_MixedTypes_ReservesTheHintRowAndHidesTheChildren()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                first.a = new TestSword();
                second.a = new IMGUIShield();
                var property = new SerializedObject(new Object[] { first, second }).FindProperty("a");
                property.isExpanded = true;

                Assert.IsTrue(SerializeReferenceHelpers.HasMixedTypes(property), "Precondition: the targets differ.");
                Assert.AreEqual(Line + Spacing + Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(property),
                    "Different types show one hint row and no children, even when expanded.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void GetHeight_MultipleObjects_ReservesNoNoticeRow()
        {
            var first = ScriptableObject.CreateInstance<RequiredTestObject>();
            var second = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                var property = new SerializedObject(new Object[] { first, second }).FindProperty("requiredRef");

                // Each target alone would show the required notice; a repair notice cannot describe a selection.
                Assert.AreEqual(Line, SerializeReferenceIMGUIPropertyDrawer.GetHeight(property));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void GetPropertyHeight_TypeSelectorField_UsesTheDrawerHeight()
        {
            var obj = ScriptableObject.CreateInstance<IMGUIDrawerTestObject>();
            try
            {
                obj.weapon = new TestSword();
                var property = new SerializedObject(obj).FindProperty(nameof(IMGUIDrawerTestObject.weapon));
                property.isExpanded = false;

                Assert.AreEqual(Line, EditorGUI.GetPropertyHeight(property),
                    "Unity must size a collapsed [TypeSelector] managed reference with the drawer.");

                property.isExpanded = true;

                Assert.AreEqual(SerializeReferenceIMGUIPropertyDrawer.GetHeight(property), EditorGUI.GetPropertyHeight(property),
                    "Unity must size an expanded [TypeSelector] managed reference with the drawer, children included.");
                Assert.Greater(EditorGUI.GetPropertyHeight(property), Line);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        // Two fields on one shared rid, the shared-reference cache invalidated. The caller destroys the object.
        private static LinkerTestObject CreateSharedPair(out SerializedObject serialized)
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            serialized = new SerializedObject(obj);
            serialized.FindProperty("a").managedReferenceValue = new TestSword { damage = 7 };
            serialized.ApplyModifiedProperties();

            Assert.IsTrue(SerializeReferenceLinker.LinkTo(serialized.FindProperty("b"), "a"));
            serialized.Update();
            SerializeReferenceHelpers.InvalidateSharedReferenceCache();
            return obj;
        }
    }
}
