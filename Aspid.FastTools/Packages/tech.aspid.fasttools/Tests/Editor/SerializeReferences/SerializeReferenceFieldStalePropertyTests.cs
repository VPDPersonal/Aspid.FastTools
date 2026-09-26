using System.Reflection;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Guards the post-repair path of the UI Toolkit <see cref="SerializeReferenceField"/>: a Fix / Smart Fix on a
    /// saved asset reimports it and invalidates the field's live SerializedObject, after which the reference-change
    /// handler must no-op on that object instead of throwing.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceFieldStalePropertyTests
    {
        private static readonly MethodInfo ApplyReferenceChange = typeof(SerializeReferenceField)
            .GetMethod("ApplyReferenceChange", BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void ApplyReferenceChange_AfterSerializedObjectInvalidated_DoesNotThrow()
        {
            Assert.IsNotNull(ApplyReferenceChange, "SerializeReferenceField.ApplyReferenceChange was renamed or removed.");

            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();

            try
            {
                var serialized = new SerializedObject(obj);
                serialized.FindProperty("a").managedReferenceValue = new TestSword { damage = 7 };
                serialized.ApplyModifiedProperties();

                var field = new SerializeReferenceField("A", serialized.FindProperty("a"));

                // Stands in for the ForceUpdate reimport of a saved-asset repair tearing the live object down.
                serialized.Dispose();

                Assert.DoesNotThrow(() => ApplyReferenceChange.Invoke(field, null));
            }
            finally
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
