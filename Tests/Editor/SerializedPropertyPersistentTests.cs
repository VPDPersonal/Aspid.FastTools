using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Editors.Tests
{
    internal sealed class SerializedPropertyPersistentTests
    {
        private PersistentTarget _target;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<PersistentTarget>();
            _target.Items = new[] { 1, 2 };
            _serializedObject = new SerializedObject(_target);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject.Dispose();
            Object.DestroyImmediate(_target);
        }

        [Test]
        public void Persistent_ReturnsSamePathOnIndependentObject()
        {
            var property = _serializedObject.FindProperty(nameof(PersistentTarget.Items)).GetArrayElementAtIndex(1);

            var persistent = property.Persistent();
            try
            {
                Assert.IsNotNull(persistent);
                Assert.AreNotSame(_serializedObject, persistent.serializedObject);
                Assert.AreEqual(property.propertyPath, persistent.propertyPath);
                Assert.AreEqual(2, persistent.intValue);
            }
            finally
            {
                persistent?.serializedObject.Dispose();
            }
        }

        [Test]
        public void Persistent_ReturnsNullWhenPathNoLongerExists()
        {
            var property = _serializedObject.FindProperty(nameof(PersistentTarget.Items)).GetArrayElementAtIndex(1);
            _target.Items = new[] { 1 };

            Assert.IsNull(property.Persistent());
        }

        private sealed class PersistentTarget : ScriptableObject
        {
            public int[] Items = System.Array.Empty<int>();
        }
    }
}
