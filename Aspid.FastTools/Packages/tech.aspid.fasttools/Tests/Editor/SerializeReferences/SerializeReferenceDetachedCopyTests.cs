using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using Aspid.FastTools.Editors;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Covers the copies of <see cref="SerializedObject"/> the drawers open for menu, picker and drop callbacks:
    /// <see cref="DetachedProperty"/> disposes its copy and skips a destroyed target, and
    /// <see cref="SerializeReferenceHelpers.MakeReferenceUnique"/> and <see cref="SerializeReferenceDropHandler.Assign"/>
    /// skip a path that no longer exists.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceDetachedCopyTests
    {
        private SharedAliasTestObject _target;
        private SerializedObject _serialized;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<SharedAliasTestObject>();
            _target.primary = new TestSword { damage = 1 };
            _target.sidearms.Add(new TestSword());
            _target.sidearms.Add(new TestSword());
            _serialized = new SerializedObject(_target);
        }

        [TearDown]
        public void TearDown()
        {
            _serialized.Dispose();
            Object.DestroyImmediate(_target);
        }

        // Unity gives no public disposed flag; Dispose() clears the native pointer.
        private static bool IsDisposed(SerializedObject serializedObject)
        {
            var field = typeof(SerializedObject).GetField("m_NativeObjectPtr", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field is null) Assert.Inconclusive("This Unity version has no SerializedObject.m_NativeObjectPtr.");

            return (IntPtr)field.GetValue(serializedObject) == IntPtr.Zero;
        }

        [Test]
        public void Use_RunsOnAnIndependentCopyAndDisposesItAfterwards()
        {
            var detached = new DetachedProperty(_serialized.FindProperty("primary"));
            SerializedObject copy = null;

            detached.Use(property =>
            {
                copy = property.serializedObject;
                Assert.AreNotSame(_serialized, copy);
                Assert.AreEqual("primary", property.propertyPath);
                Assert.IsFalse(IsDisposed(copy), "The copy must stay open while the action runs.");
            });

            Assert.IsNotNull(copy);
            Assert.IsTrue(IsDisposed(copy));
        }

        [Test]
        public void Use_DisposesTheCopyWhenTheActionThrows()
        {
            var detached = new DetachedProperty(_serialized.FindProperty("primary"));
            SerializedObject copy = null;

            Assert.Throws<InvalidOperationException>(() => detached.Use(property =>
            {
                copy = property.serializedObject;
                throw new InvalidOperationException();
            }));

            Assert.IsTrue(IsDisposed(copy));
        }

        [Test]
        public void Use_ReadsTheTargetsWhenCalledNotWhenCreated()
        {
            var detached = new DetachedProperty(_serialized.FindProperty("primary"));
            _target.primary = new TestSword { damage = 5 };

            var damage = 0;
            detached.Use(property => damage = ((TestSword)property.managedReferenceValue).damage);

            Assert.AreEqual(5, damage, "A callback that runs later must see the targets as they are then.");
        }

        [Test]
        public void Use_SkipsTheActionWhenThePathNoLongerExists()
        {
            var detached = new DetachedProperty(_serialized.FindProperty("sidearms.Array.data[1]"));
            _target.sidearms.RemoveAt(1);

            var called = false;
            detached.Use(_ => called = true);

            Assert.IsFalse(called);
        }

        [Test]
        public void Use_SkipsTheActionWhenATargetIsDestroyed()
        {
            var other = ScriptableObject.CreateInstance<SharedAliasTestObject>();
            DetachedProperty detached;

            using (var source = new SerializedObject(other))
                detached = new DetachedProperty(source.FindProperty("primary"));

            Object.DestroyImmediate(other);

            var called = false;
            Assert.DoesNotThrow(() => detached.Use(_ => called = true));
            Assert.IsFalse(called);
        }

        [Test]
        public void Remembers_TheTargetsAndThePathOfTheSource()
        {
            var detached = new DetachedProperty(_serialized.FindProperty("sidearms.Array.data[1]"));

            Assert.AreEqual("sidearms.Array.data[1]", detached.Path);
            Assert.AreEqual(new Object[] { _target }, detached.Targets);
        }

        [Test]
        public void MakeReferenceUnique_UnsharesTheReference()
        {
            Assert.IsTrue(SerializeReferenceLinker.LinkTo(_serialized.FindProperty("sidearms.Array.data[0]"), "primary"));
            _serialized.Update();
            SerializeReferenceHelpers.InvalidateSharedReferenceCache();
            Assert.IsTrue(SerializeReferenceHelpers.HasSharedReference(_serialized.FindProperty("primary")));

            SerializeReferenceHelpers.MakeReferenceUnique(_serialized.FindProperty("sidearms.Array.data[0]"));

            _serialized.Update();
            Assert.IsFalse(SerializeReferenceHelpers.HasSharedReference(_serialized.FindProperty("primary")));
        }

        [Test]
        public void MakeReferenceUnique_DoesNothingWhenThePathNoLongerExists()
        {
            var property = _serialized.FindProperty("sidearms.Array.data[1]");
            _target.sidearms.RemoveAt(1);

            Assert.DoesNotThrow(() => SerializeReferenceHelpers.MakeReferenceUnique(property));
        }

        [Test]
        public void Assign_SetsAnInstanceOfTheType()
        {
            SerializeReferenceDropHandler.Assign(_serialized.FindProperty("sidearms.Array.data[0]"), typeof(TestSword));

            _serialized.Update();
            Assert.IsInstanceOf<TestSword>(_serialized.FindProperty("sidearms.Array.data[0]").managedReferenceValue);
        }

        [Test]
        public void Assign_DoesNothingWhenThePathNoLongerExists()
        {
            var property = _serialized.FindProperty("sidearms.Array.data[1]");
            _target.sidearms.RemoveAt(1);

            Assert.DoesNotThrow(() => SerializeReferenceDropHandler.Assign(property, typeof(TestSword)));
        }
    }
}
