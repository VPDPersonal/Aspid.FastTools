using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The IMGUI drawer accepts a script asset dropped on the field line.
    // The handler picks the dragged class and gives each selected object its own instance.
    [TestFixture]
    internal sealed class SerializeReferenceDropHandlerTests
    {
        private MonoScript _script;

        [SetUp]
        public void SetUp()
        {
            var guids = AssetDatabase.FindAssets($"{nameof(IMGUIDropWeapon)} t:MonoScript");
            Assert.IsNotEmpty(guids, "Precondition: the fixture script must be an asset.");

            _script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Assert.AreEqual(typeof(IMGUIDropWeapon), _script.GetClass(), "Precondition: the script reports its class.");
        }

        [TearDown]
        public void TearDown() =>
            DragAndDrop.PrepareStartDrag();

        [Test]
        public void TryResolveDroppedType_AssignableScript_ResolvesItsClass()
        {
            Drag(_script);

            Assert.IsTrue(SerializeReferenceDropHandler.TryResolveDroppedType(typeof(ITestWeapon), Array.Empty<Type>(), out var type));
            Assert.AreEqual(typeof(IMGUIDropWeapon), type);
        }

        [Test]
        public void TryResolveDroppedType_PicksTheFirstAssignableScriptOfSeveral()
        {
            var notAScript = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                Drag(notAScript, _script);

                Assert.IsTrue(SerializeReferenceDropHandler.TryResolveDroppedType(typeof(ITestWeapon), null, out var type));
                Assert.AreEqual(typeof(IMGUIDropWeapon), type, "An object that is not a script must be skipped, not reject the drop.");
            }
            finally
            {
                Object.DestroyImmediate(notAScript);
            }
        }

        [Test]
        public void TryResolveDroppedType_ClassNotAssignableToTheField_Rejects()
        {
            Drag(_script);

            Assert.IsFalse(SerializeReferenceDropHandler.TryResolveDroppedType(typeof(IDisposable), Array.Empty<Type>(), out var type));
            Assert.IsNull(type);
        }

        [Test]
        public void TryResolveDroppedType_ClassOutsideTheTypeSelectorConstraint_Rejects()
        {
            Drag(_script);

            Assert.IsFalse(SerializeReferenceDropHandler.TryResolveDroppedType(typeof(ITestWeapon), new[] { typeof(IMGUIShield) }, out _),
                "A [TypeSelector] base type must narrow what a drop may assign.");
        }

        [Test]
        public void TryResolveDroppedType_NothingDragged_Rejects()
        {
            DragAndDrop.PrepareStartDrag();

            Assert.IsFalse(SerializeReferenceDropHandler.TryResolveDroppedType(typeof(ITestWeapon), Array.Empty<Type>(), out _));
        }

        [Test]
        public void Assign_SingleObject_SetsAnInstanceOfTheDroppedType()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var serialized = new SerializedObject(obj);

                SerializeReferenceDropHandler.Assign(serialized.FindProperty("a"), typeof(IMGUIDropWeapon));

                Assert.IsInstanceOf<IMGUIDropWeapon>(obj.a);
                Assert.IsNull(obj.b, "Only the dropped field changes.");
            }
            finally
            {
                Undo.ClearUndo(obj);
                Object.DestroyImmediate(obj);
            }
        }

        [Test]
        public void Assign_MultipleObjects_GivesEachItsOwnInstance()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                var serialized = new SerializedObject(new Object[] { first, second });

                SerializeReferenceDropHandler.Assign(serialized.FindProperty("a"), typeof(IMGUIDropWeapon));

                Assert.IsInstanceOf<IMGUIDropWeapon>(first.a);
                Assert.IsInstanceOf<IMGUIDropWeapon>(second.a);
                Assert.AreNotSame(first.a, second.a, "A multi-object drop must never alias one reference across objects.");
            }
            finally
            {
                Undo.ClearUndo(first);
                Undo.ClearUndo(second);
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Assign_NullPropertyOrType_ChangesNothing()
        {
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();
            try
            {
                obj.a = new TestSword();
                var original = obj.a;
                var serialized = new SerializedObject(obj);

                Assert.DoesNotThrow(() => SerializeReferenceDropHandler.Assign(null, typeof(IMGUIDropWeapon)));
                SerializeReferenceDropHandler.Assign(serialized.FindProperty("a"), type: null);

                Assert.AreSame(original, obj.a, "A drop without a resolved type must not clear the field.");
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }

        private static void Drag(params Object[] objects)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = objects;
        }
    }
}
