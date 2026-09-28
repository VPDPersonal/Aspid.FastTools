using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Guards the post-repair path of the UI Toolkit <see cref="SerializeReferenceField"/>: a Fix / Smart Fix on a
    /// saved asset reimports it and invalidates the field's live SerializedObject, after which the reference-change
    /// handler must skip that object instead of throwing and still notify the sibling fields.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceFieldStalePropertyTests
    {
        private static readonly MethodInfo ApplyReferenceChange = typeof(SerializeReferenceField)
            .GetMethod("ApplyReferenceChange", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly EventInfo ManagedReferencesChanged = typeof(SerializeReferenceField)
            .GetEvent("ManagedReferencesChanged", BindingFlags.Static | BindingFlags.NonPublic);

        [Test]
        public void ApplyReferenceChange_AfterSerializedObjectInvalidated_NotifiesSiblingsWithoutThrowing()
        {
            Assert.IsNotNull(ApplyReferenceChange, "SerializeReferenceField.ApplyReferenceChange was renamed or removed.");
            Assert.IsNotNull(ManagedReferencesChanged, "SerializeReferenceField.ManagedReferencesChanged was renamed or removed.");

            var notifications = 0;
            Action onChanged = () => notifications++;
            var obj = ScriptableObject.CreateInstance<LinkerTestObject>();

            try
            {
                var serialized = new SerializedObject(obj);
                serialized.FindProperty("a").managedReferenceValue = new TestSword { damage = 7 };
                serialized.ApplyModifiedProperties();

                var field = new SerializeReferenceField("A", serialized.FindProperty("a"));

                // Stands in for the ForceUpdate reimport of a saved-asset repair tearing the live object down.
                serialized.Dispose();

                ManagedReferencesChanged.GetAddMethod(nonPublic: true).Invoke(null, new object[] { onChanged });
                Assert.DoesNotThrow(() => ApplyReferenceChange.Invoke(field, null));
                Assert.AreEqual(1, notifications, "Sibling fields were not notified after the repair.");
            }
            finally
            {
                ManagedReferencesChanged.GetRemoveMethod(nonPublic: true).Invoke(null, new object[] { onChanged });
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
    }
}
