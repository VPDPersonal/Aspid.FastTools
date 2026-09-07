using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards the script-backed wrappers: the code-side contract (base type, implicit conversion), the editor
    /// utility that maps types to their script assets and writes a wrapper property, and the editor-side sync
    /// that re-reads the stored type name from the referenced script.
    /// </summary>
    [TestFixture]
    internal sealed class SerializableMonoScriptTests
    {
        // A type of the package's runtime assembly declared in a file of its own name — exactly the shape
        // MonoScript.GetClass() reports, so it is guaranteed to have a script asset.
        private static readonly Type ScriptedType = typeof(SerializableMonoScript);

        // A scripted type the constrained wrapper accepts.
        private static readonly Type ConstrainedType = typeof(SerializableType);

        private sealed class Holder : ScriptableObject
        {
            // The wrappers have no public constructor: only Unity's serializer creates them.
            [SerializeField] public SerializableMonoScript wrapper;

            [TypeSelector(Required = true)]
            [SerializeField] public SerializableMonoScript<SerializableType> required;
        }

        // Unity's serializer, not a constructor, creates the wrappers, and it only runs once the object is
        // serialized — a freshly created instance still carries null fields.
        private static Holder CreateHolder()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
            new SerializedObject(holder).Update();
            return holder;
        }

        // The script reference is a private editor-only field, so a test reads it the way the drawers do.
        private static MonoScript ScriptOf(SerializedProperty wrapperProperty) =>
            wrapperProperty.FindPropertyRelative(SerializableMonoScriptUtility.ScriptFieldName).objectReferenceValue as MonoScript;

        [Test]
        public void ImplicitConversion_NullWrapper_YieldsNull()
        {
            SerializableMonoScript plain = null;
            SerializableMonoScript<IComparable> constrained = null;

            Assert.IsNull((Type)plain);
            Assert.IsNull((Type)constrained);
        }

        [Test]
        public void ConstrainedWrapper_IsAMonoScriptWrapper()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                SerializableMonoScriptUtility.Assign(serialized.FindProperty(nameof(Holder.required)), ConstrainedType);

                SerializableMonoScript wrapper = holder.required;

                Assert.AreEqual(typeof(SerializableType), wrapper.BaseType, "BaseType must stay virtual through the base reference.");
                Assert.AreEqual(ConstrainedType, (Type)wrapper);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Wrappers_ExposeTheirBaseType()
        {
            var holder = CreateHolder();
            try
            {
                Assert.AreEqual(typeof(object), holder.wrapper.BaseType);
                Assert.AreEqual(typeof(SerializableType), holder.required.BaseType);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }

            Assert.IsTrue(SerializableTypeUtility.TryGetBaseType(typeof(SerializableMonoScript<Exception>[]), out var baseType));
            Assert.AreEqual(typeof(Exception), baseType);
        }

        [Test]
        public void Utility_RecognisesWrapperFields()
        {
            Assert.IsTrue(SerializableMonoScriptUtility.IsMonoScriptWrapperField(typeof(SerializableMonoScript)));
            Assert.IsTrue(SerializableMonoScriptUtility.IsMonoScriptWrapperField(typeof(SerializableMonoScript<Exception>[])));
            Assert.IsFalse(SerializableMonoScriptUtility.IsMonoScriptWrapperField(typeof(SerializableType)));
            Assert.IsTrue(SerializableTypeUtility.IsSerializableTypeField(typeof(SerializableMonoScript)), "The gate treats it as a type wrapper.");
        }

        [Test]
        public void ScriptsByType_ContainsAScriptedRuntimeType_ButNotANestedOne()
        {
            Assert.IsTrue(SerializableMonoScriptUtility.TryGetScript(ScriptedType, out var script));
            Assert.AreEqual(ScriptedType, script.GetClass());
            Assert.IsFalse(SerializableMonoScriptUtility.HasScript(typeof(Holder)), "A nested type owns no script asset.");
        }

        [Test]
        public void Assign_WritesScriptAndName_AndNullClearsBoth()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                var wrapper = serialized.FindProperty(nameof(Holder.wrapper));

                SerializableMonoScriptUtility.Assign(wrapper, ScriptedType);
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
                Assert.AreEqual(ScriptedType, ScriptOf(wrapper)?.GetClass());

                serialized.Update();
                Assert.AreEqual(ScriptedType, SerializableMonoScriptUtility.GetCurrentType(wrapper, out var name));
                Assert.AreEqual(ScriptedType.AssemblyQualifiedName, name);

                SerializableMonoScriptUtility.Assign(wrapper, null);
                Assert.IsNull(holder.wrapper.Type);
                Assert.IsNull(ScriptOf(wrapper));
                Assert.AreEqual(string.Empty, holder.wrapper.AssemblyQualifiedName);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Serialization_ResyncsTheNameFromTheScript()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                SerializableMonoScriptUtility.Assign(serialized.FindProperty(nameof(Holder.wrapper)), ScriptedType);

                // Simulate a stale name (what a class rename leaves behind) while the script reference is intact.
                serialized.Update();
                serialized.FindProperty($"{nameof(Holder.wrapper)}.{SerializableTypeUtility.BackingFieldName}").stringValue = "Old.Name, Old";
                serialized.ApplyModifiedProperties();

                // Building a SerializedObject serializes the target, which runs the wrapper's OnBeforeSerialize.
                using var fresh = new SerializedObject(holder);
                var name = fresh.FindProperty($"{nameof(Holder.wrapper)}.{SerializableTypeUtility.BackingFieldName}").stringValue;

                Assert.AreEqual(ScriptedType.AssemblyQualifiedName, name, "The script asset is the source of truth for the stored name.");
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void SyncScriptFromName_PointsTheScriptAtTheWrittenType()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                var name = serialized.FindProperty($"{nameof(Holder.wrapper)}.{SerializableTypeUtility.BackingFieldName}");
                name.stringValue = ScriptedType.AssemblyQualifiedName;
                serialized.ApplyModifiedProperties();

                SerializableMonoScriptUtility.SyncScriptFromName(name);

                Assert.AreEqual(ScriptedType, ScriptOf(serialized.FindProperty(nameof(Holder.wrapper)))?.GetClass());
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void RequiredGate_CoversTheWrapper()
        {
            var holder = CreateHolder();
            try
            {
                var serialized = new SerializedObject(holder);
                var backing = serialized.FindProperty($"{nameof(Holder.required)}.{SerializableTypeUtility.BackingFieldName}");

                Assert.IsTrue(TypeSelectorRequiredGate.IsViolation(backing), "An empty required wrapper is a violation.");

                SerializableMonoScriptUtility.Assign(serialized.FindProperty(nameof(Holder.required)), ScriptedType);
                serialized.Update();

                Assert.IsFalse(TypeSelectorRequiredGate.IsViolation(
                    serialized.FindProperty($"{nameof(Holder.required)}.{SerializableTypeUtility.BackingFieldName}")));
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }
    }
}
