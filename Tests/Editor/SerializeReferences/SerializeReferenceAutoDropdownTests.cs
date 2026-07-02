using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Behavioural coverage for the attribute-free dropdown opt-in ("Dropdown without [TypeSelector]"):
    /// <list type="bullet">
    /// <item>the <see cref="SerializeReferenceAutoDropdown"/> decision matrix — only a bare managed reference (or a
    /// bare list of them) is substituted, only under the opt-in, and an attributed field always keeps its drawer —
    /// at the top level and on the nested fields of an assigned instance alike;</item>
    /// <item>the fallback inspector contract — it is the editor Unity selects for a plain component, it hands the
    /// component back to the default inspector (null) whenever it has nothing to substitute, and when it does build,
    /// exactly the bare reference fields come out as dropdown fields;</item>
    /// <item>the public <see cref="SerializeReferenceEditorGUI"/> facade's shape validation;</item>
    /// <item>the setting's persistence contract — Changed fires, and the per-user reset restores the default (off).</item>
    /// </list>
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceAutoDropdownTests
    {
        private bool _dropdownWithoutAttribute;
        private bool _breakageDetection;

        [SetUp]
        public void SetUp()
        {
            // Snapshot every per-user setting the tests (and ResetUserToDefaults) touch, restoring on teardown.
            _dropdownWithoutAttribute = SerializeReferenceSettings.DropdownWithoutAttributeEnabled;
            _breakageDetection = SerializeReferenceSettings.BreakageDetectionEnabled;
        }

        [TearDown]
        public void TearDown()
        {
            SerializeReferenceSettings.DropdownWithoutAttributeEnabled = _dropdownWithoutAttribute;
            SerializeReferenceSettings.BreakageDetectionEnabled = _breakageDetection;
        }

        // -----------------------------------------------------------------------------------------------------
        // A — the ShouldDraw decision matrix
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void ShouldDraw_BareReference_FollowsTheOptIn()
        {
            WithTarget((_, serialized) =>
            {
                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = false;
                Assert.IsFalse(SerializeReferenceAutoDropdown.ShouldDraw(serialized.FindProperty("plain")),
                    "With the opt-in off, nothing is ever substituted.");

                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;
                Assert.IsTrue(SerializeReferenceAutoDropdown.ShouldDraw(serialized.FindProperty("plain")),
                    "With the opt-in on, a bare managed reference gets the dropdown.");
            });
        }

        [Test]
        public void ShouldDraw_AttributedReference_False_TheAttributeAlwaysWins()
        {
            WithTarget((_, serialized) =>
            {
                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;
                Assert.IsFalse(SerializeReferenceAutoDropdown.ShouldDraw(serialized.FindProperty("marked")),
                    "A [TypeSelector] field keeps its drawer (and its base-type narrowing) — never substituted.");
            });
        }

        [Test]
        public void ShouldDraw_BareList_True_AndRecognisedAsManagedReferenceArray()
        {
            WithTarget((_, serialized) =>
            {
                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;
                var list = serialized.FindProperty("list");

                Assert.IsTrue(SerializeReferenceAutoDropdown.IsManagedReferenceArray(list),
                    "A [SerializeReference] List<T> must be recognised as a managed-reference array.");
                Assert.IsTrue(SerializeReferenceAutoDropdown.ShouldDraw(list),
                    "A bare managed-reference list gets the dropdown list.");
            });
        }

        [Test]
        public void ShouldDraw_PlainValueField_False()
        {
            WithTarget((_, serialized) =>
            {
                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;
                Assert.IsFalse(SerializeReferenceAutoDropdown.ShouldDraw(serialized.FindProperty("number")),
                    "A plain value field is never substituted.");
            });
        }

        [Test]
        public void ShouldDraw_NestedChildren_FollowTheirOwnAttribute()
        {
            WithTarget((_, serialized) =>
            {
                // The nested fields live on the CONCRETE assigned type (TestWeaponHolder), not the declared
                // interface — exactly what the runtime-instance attribute walk must cross.
                serialized.FindProperty("plain").managedReferenceValue = new TestWeaponHolder();
                serialized.ApplyModifiedProperties();
                serialized.Update();

                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;

                Assert.IsTrue(SerializeReferenceAutoDropdown.ShouldDraw(serialized.FindProperty("plain.inner")),
                    "A bare nested reference on the assigned instance gets the dropdown.");
                Assert.IsFalse(SerializeReferenceAutoDropdown.ShouldDraw(serialized.FindProperty("plain.innerMarked")),
                    "An attributed nested reference keeps its drawer.");
            });
        }

        // -----------------------------------------------------------------------------------------------------
        // B — element-type resolution and the factory dispatch
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void GetElementType_BareList_ReturnsTheDeclaredElementType()
        {
            WithTarget((_, serialized) =>
                Assert.AreEqual(typeof(ITestWeapon),
                    SerializeReferenceAutoDropdown.GetElementType(serialized.FindProperty("list")),
                    "The add-picker constraint must be the list's declared element type, even while the list is empty."));
        }

        [Test]
        public void CreateField_DispatchesOnTheShape()
        {
            WithTarget((_, serialized) =>
            {
                Assert.IsInstanceOf<SerializeReferenceField>(
                    SerializeReferenceAutoDropdown.CreateField(serialized.FindProperty("plain")),
                    "A single managed reference builds the dropdown field.");
                Assert.IsInstanceOf<SerializeReferenceListField>(
                    SerializeReferenceAutoDropdown.CreateField(serialized.FindProperty("list")),
                    "A managed-reference list builds the list field.");
            });
        }

        // -----------------------------------------------------------------------------------------------------
        // C — the fallback inspector contract
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void FallbackInspector_IsTheSelectedEditor_ForAPlainScriptableObject()
        {
            WithTarget((target, _) => WithEditor(target, editor =>
                Assert.IsInstanceOf<SerializeReferenceScriptableObjectFallbackInspector>(editor,
                    "With no custom editor declared, Unity must select the package's fallback inspector.")));
        }

        [Test]
        public void FallbackInspector_ReturnsNull_WhenTheOptInIsOff()
        {
            WithTarget((target, _) => WithEditor(target, editor =>
            {
                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = false;
                Assert.IsNull(editor.CreateInspectorGUI(),
                    "With the opt-in off the fallback inspector must hand the component to Unity's default inspector.");
            }));
        }

        [Test]
        public void FallbackInspector_ReturnsNull_WithoutEligibleFields()
        {
            var target = ScriptableObject.CreateInstance<PlainValueTestObject>();
            try
            {
                WithEditor(target, editor =>
                {
                    SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;
                    Assert.IsNull(editor.CreateInspectorGUI(),
                        "A component with no bare managed reference must be left to Unity's default inspector.");
                });
            }
            finally { Object.DestroyImmediate(target); }
        }

        [Test]
        public void FallbackInspector_SubstitutesExactlyTheBareReferences()
        {
            WithTarget((target, _) => WithEditor(target, editor =>
            {
                SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;

                var root = editor.CreateInspectorGUI();
                Assert.IsNotNull(root, "An eligible component must get the substituted inspector.");

                Assert.AreEqual(1, root.Query<SerializeReferenceField>().ToList().Count,
                    "Exactly the one bare reference ('plain') becomes a dropdown field — 'marked' keeps its drawer.");
                Assert.AreEqual(1, root.Query<SerializeReferenceListField>().ToList().Count,
                    "Exactly the one bare list ('list') becomes a dropdown list.");

                // m_Script, 'marked' and 'number' all stay plain PropertyFields (the drawer resolution stays Unity's).
                Assert.AreEqual(3, root.Query<PropertyField>().ToList().Count,
                    "Every non-substituted property must remain a plain PropertyField.");
            }));
        }

        // -----------------------------------------------------------------------------------------------------
        // D — the public custom-editor facade validates the property shape
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void EditorGUIFacade_BuildsForTheRightShapes_AndThrowsForTheWrongOnes()
        {
            WithTarget((_, serialized) =>
            {
                Assert.IsInstanceOf<SerializeReferenceField>(
                    SerializeReferenceEditorGUI.CreateField(serialized.FindProperty("plain")));
                Assert.IsInstanceOf<SerializeReferenceListField>(
                    SerializeReferenceEditorGUI.CreateList(serialized.FindProperty("list")));

                Assert.Throws<ArgumentException>(
                    () => SerializeReferenceEditorGUI.CreateField(serialized.FindProperty("number")),
                    "CreateField must reject a non-managed-reference property.");
                Assert.Throws<ArgumentException>(
                    () => SerializeReferenceEditorGUI.CreateList(serialized.FindProperty("plain")),
                    "CreateList must reject a non-list property.");
            });
        }

        // -----------------------------------------------------------------------------------------------------
        // E — the setting's persistence contract
        // -----------------------------------------------------------------------------------------------------

        [Test]
        public void SettingToggle_RaisesChanged()
        {
            var fired = 0;
            void Handler() => fired++;
            SerializeReferenceSettings.Changed += Handler;
            try
            {
                SerializeReferenceSettings.DropdownWithoutAttributeEnabled =
                    !SerializeReferenceSettings.DropdownWithoutAttributeEnabled;
                Assert.GreaterOrEqual(fired, 1, "The setter must raise Changed for repaint and live-sync.");
            }
            finally { SerializeReferenceSettings.Changed -= Handler; }
        }

        [Test]
        public void ResetUserToDefaults_TurnsTheOptInOff()
        {
            SerializeReferenceSettings.DropdownWithoutAttributeEnabled = true;
            SerializeReferenceSettings.ResetUserToDefaults();
            Assert.IsFalse(SerializeReferenceSettings.DropdownWithoutAttributeEnabled,
                "The attribute-free dropdown is a deliberate opt-in — its default (and reset value) is off.");
        }

        // -----------------------------------------------------------------------------------------------------

        private static void WithTarget(Action<AutoDropdownTestObject, SerializedObject> assert)
        {
            var target = ScriptableObject.CreateInstance<AutoDropdownTestObject>();
            try { assert(target, new SerializedObject(target)); }
            finally { Object.DestroyImmediate(target); }
        }

        private static void WithEditor(Object target, Action<Editor> assert)
        {
            var editor = Editor.CreateEditor(target);
            try { assert(editor); }
            finally { Object.DestroyImmediate(editor); }
        }
    }
}
