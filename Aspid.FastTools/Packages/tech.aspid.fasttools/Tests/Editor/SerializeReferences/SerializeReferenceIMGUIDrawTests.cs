using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Paints the IMGUI drawer and its list in a window, because IMGUI controls need a live OnGUI.
    // An error in the drawer then shows as a logged error or as GUI state left behind.
    // The windows need a graphics device: these tests run in CI, not under -nographics.
    [TestFixture]
    internal sealed class SerializeReferenceIMGUIDrawTests
    {
        private const float FieldWidth = 320f;

        private const string MissingProbePath = "Assets/__AspidIMGUIDrawProbe__.asset";

        private EditorWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window == null) return;

            _window.rootVisualElement.Clear();
            Object.DestroyImmediate(_window);
        }

        [UnityTest]
        public IEnumerator Draw_EmptyField_PaintsAndLeavesTheGuiBalanced()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var property = new SerializedObject(obj).FindProperty("a");
                var report = new PaintReport();

                yield return Paint(report, () => DrawField(property));

                AssertPainted(report);
                Assert.IsNull(obj.a, "Painting must not assign a value.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [UnityTest]
        public IEnumerator Draw_ExpandedNestedValue_PaintsEveryLevel()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var value = new IMGUINestingWeapon { level = 3, inner = new TestSword() };
                obj.a = value;

                var serialized = new SerializedObject(obj);
                var property = serialized.FindProperty("a");
                property.isExpanded = true;
                serialized.FindProperty("a.inner").isExpanded = true;
                var report = new PaintReport();

                yield return Paint(report, () => DrawField(property));

                AssertPainted(report);
                Assert.AreSame(value, obj.a, "Painting must not replace the value.");
                Assert.IsInstanceOf<TestSword>(value.inner);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [UnityTest]
        public IEnumerator Draw_ExpandedValueWithAList_PaintsTheNestedList()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var value = new IMGUIListWeapon { items = { new TestSword(), null } };
                obj.a = value;

                var property = new SerializedObject(obj).FindProperty("a");
                property.isExpanded = true;
                var report = new PaintReport();

                yield return Paint(report, () => DrawField(property));

                AssertPainted(report);
                Assert.AreSame(value, obj.a, "Painting must not replace the value.");
                Assert.AreEqual(2, value.items.Count, "Painting must not change the nested list.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [UnityTest]
        public IEnumerator Draw_RequiredAndEmpty_PaintsTheNotice()
        {
            var obj = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                var property = new SerializedObject(obj).FindProperty("requiredRef");
                var report = new PaintReport();

                yield return Paint(report, () => DrawField(property));

                AssertPainted(report);
                Assert.IsNull(obj.requiredRef);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [UnityTest]
        public IEnumerator Draw_SharedReference_PaintsBothHolders()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);
                serialized.FindProperty("a").managedReferenceValue = new TestSword { damage = 7 };
                serialized.ApplyModifiedProperties();

                Assert.IsTrue(SerializeReferenceLinker.LinkTo(serialized.FindProperty("b"), "a"));
                serialized.Update();
                SerializeReferenceHelpers.InvalidateSharedReferenceCache();

                var holder = serialized.FindProperty("a");
                var alias = serialized.FindProperty("b");
                var report = new PaintReport();

                yield return Paint(report, () =>
                {
                    var y = DrawField(holder);
                    DrawField(alias, y);
                });

                AssertPainted(report);

                serialized.Update();
                Assert.AreEqual(serialized.FindProperty("a").managedReferenceId, serialized.FindProperty("b").managedReferenceId,
                    "Painting must keep the two fields on one shared reference.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [UnityTest]
        public IEnumerator Draw_MissingType_PaintsTheRepairNotice()
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

                var property = new SerializedObject(probe).FindProperty("a");
                Assert.IsTrue(SerializeReferenceHelpers.IsMissingType(property), "The probe must hold a missing type.");
                var report = new PaintReport();

                yield return Paint(report, () => DrawField(property));

                AssertPainted(report);
            }
            finally
            {
                _window.rootVisualElement.Clear();
                AssetDatabase.DeleteAsset(MissingProbePath);
            }
        }

        [UnityTest]
        public IEnumerator Draw_MixedTypes_PaintsTheHint()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                first.a = new TestSword();
                second.a = new IMGUIShield();
                var property = new SerializedObject(new Object[] { first, second }).FindProperty("a");
                Assert.IsTrue(SerializeReferenceHelpers.HasMixedTypes(property), "Precondition: the targets differ.");
                var report = new PaintReport();

                yield return Paint(report, () => DrawField(property));

                AssertPainted(report);
                Assert.IsInstanceOf<TestSword>(first.a, "Painting must not write to a mixed selection.");
                Assert.IsInstanceOf<IMGUIShield>(second.a, "Painting must not write to a mixed selection.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        // Unity routes a [TypeSelector] field to the drawer: the height and the paint both come from it.
        [UnityTest]
        public IEnumerator PropertyField_TypeSelectorManagedReference_PaintsWithTheDrawer()
        {
            var obj = ScriptableObject.CreateInstance<IMGUIDrawerTestObject>();
            try
            {
                obj.weapon = new TestSword();
                var property = new SerializedObject(obj).FindProperty(nameof(IMGUIDrawerTestObject.weapon));
                property.isExpanded = true;
                var report = new PaintReport();
                var heightMatches = false;

                yield return Paint(report, () =>
                {
                    var height = EditorGUI.GetPropertyHeight(property);
                    heightMatches = Mathf.Approximately(height, SerializeReferenceIMGUIPropertyDrawer.GetHeight(property));
                    EditorGUI.PropertyField(new Rect(0f, 0f, FieldWidth, height), property);
                });

                AssertPainted(report);
                Assert.IsTrue(heightMatches, "Unity must reserve the height the drawer measures.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [UnityTest]
        public IEnumerator DrawFieldLayout_PaintsInTheLayoutGroup()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new TestSword();
                var serialized = new SerializedObject(obj);
                var first = serialized.FindProperty("a");
                var second = serialized.FindProperty("b");
                var report = new PaintReport();

                yield return Paint(report, () =>
                {
                    SerializeReferenceEditorGUI.DrawFieldLayout(first, new GUIContent("First"));
                    SerializeReferenceEditorGUI.DrawFieldLayout(second);
                });

                AssertPainted(report);
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        [UnityTest]
        public IEnumerator IMGUIList_Draw_PaintsTheElementsAndGrowsWithThem()
        {
            var filled = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            var empty = ScriptableObject.CreateInstance<ReferenceListTestObject>();
            try
            {
                filled.weapons.Add(new TestSword());
                filled.weapons.Add(item: null);
                filled.weapons.Add(new IMGUINestingWeapon { level = 3 });

                var filledList = new SerializedObject(filled).FindProperty("weapons");
                var emptyList = new SerializedObject(empty).FindProperty("weapons");
                var label = new GUIContent("Weapons");
                var report = new PaintReport();
                var noConstraints = Array.Empty<Type>();
                float filledHeight = 0f, emptyHeight = 0f;

                yield return Paint(report, () =>
                {
                    filledHeight = SerializeReferenceIMGUIList.GetHeight(filledList, label, typeof(ITestWeapon), noConstraints, depth: 0);
                    emptyHeight = SerializeReferenceIMGUIList.GetHeight(emptyList, label, typeof(ITestWeapon), noConstraints, depth: 0);

                    SerializeReferenceIMGUIList.Draw(filledList, label, typeof(ITestWeapon));
                    SerializeReferenceIMGUIList.Draw(emptyList, label, typeof(ITestWeapon));
                });

                AssertPainted(report);
                Assert.Greater(filledHeight, emptyHeight, "Three elements must make the list taller than an empty one.");
                Assert.AreEqual(3, filled.weapons.Count, "Painting must not change the list.");
                Assert.AreEqual(0, empty.weapons.Count, "Painting must not change the list.");
            }
            finally
            {
                Object.DestroyImmediate(filled);
                Object.DestroyImmediate(empty);
            }
        }

        // Draws a field with the height it measures; returns the y where the next field starts.
        private static float DrawField(SerializedProperty property, float y = 0f)
        {
            var height = SerializeReferenceIMGUIPropertyDrawer.GetHeight(property);
            SerializeReferenceIMGUIPropertyDrawer.Draw(new Rect(0f, y, FieldWidth, height), new GUIContent(property.displayName),
                property, Array.Empty<Type>());

            return y + height + EditorGUIUtility.standardVerticalSpacing;
        }

        private static void AssertPainted(PaintReport report)
        {
            Assert.Greater(report.Passes, 0, "Precondition: the IMGUI container must be painted.");
            Assert.IsNull(report.Error, report.Error?.ToString());
            Assert.IsTrue(report.StateBalanced, "Painting must restore the indent level, the mixed-value flag and GUI.enabled.");
        }

        // Runs the draw in an IMGUIContainer and waits until the window has run it, ideally for a layout pass and a
        // paint pass. The container leaves again before the caller disposes what the draw reads.
        // A repaint is not asserted: a batch-mode window may never send one, and then the tests check the layout pass only.
        private IEnumerator Paint(PaintReport report, Action draw)
        {
            _window.rootVisualElement.Add(new IMGUIContainer(() =>
            {
                try
                {
                    var indent = EditorGUI.indentLevel;
                    var mixed = EditorGUI.showMixedValue;
                    var enabled = GUI.enabled;

                    draw();

                    report.StateBalanced &= indent == EditorGUI.indentLevel &&
                                            mixed == EditorGUI.showMixedValue &&
                                            enabled == GUI.enabled;
                }
                catch (Exception exception)
                {
                    report.Error ??= exception;
                }

                report.Passes++;
                if (Event.current.type == EventType.Repaint) report.Repaints++;
            }));

            for (var frame = 0; frame < 10 && (report.Passes < 2 || report.Repaints == 0); frame++)
            {
                _window.Repaint();
                yield return null;
            }

            _window.rootVisualElement.Clear();
        }

        private sealed class PaintReport
        {
            public int Passes;
            public int Repaints;
            public Exception Error;
            public bool StateBalanced = true;
        }
    }
}
