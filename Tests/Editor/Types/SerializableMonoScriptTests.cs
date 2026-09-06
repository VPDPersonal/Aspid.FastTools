using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards the script-backed wrappers: the code-side contract (constructors, constraint, implicit conversion),
    /// the editor utility that maps types to their script assets and writes a wrapper property, and the
    /// editor-side sync that re-reads the stored type name from the referenced script.
    /// </summary>
    [TestFixture]
    internal sealed class SerializableMonoScriptTests
    {
        // A type of the package's runtime assembly declared in a file of its own name — exactly the shape
        // MonoScript.GetClass() reports, so it is guaranteed to have a script asset.
        private static readonly Type ScriptedType = typeof(SerializableMonoScript);

        private sealed class Holder : ScriptableObject
        {
            [SerializeField] public SerializableMonoScript wrapper = new();

            [TypeSelector(Required = true)]
            [SerializeField] public SerializableMonoScript<SerializableType> required = new();
        }

        [Test]
        public void ImplicitConversion_NullWrapper_YieldsNull()
        {
            SerializableMonoScript plain = null;
            SerializableMonoScript<IComparable> constrained = null;

            Assert.IsNull((Type)plain);
            Assert.IsNull((Type)constrained);
        }

        [Test]
        public void Constructor_StoresTheTypeByNameOnly()
        {
            var wrapper = new SerializableMonoScript(typeof(Exception));

            Assert.AreEqual(typeof(Exception), wrapper.Type);
            Assert.AreEqual(typeof(Exception).AssemblyQualifiedName, wrapper.AssemblyQualifiedName);
            Assert.IsNull(wrapper.Script, "A code-constructed wrapper carries the name only.");
        }

        [Test]
        public void GenericConstructor_RejectsAnUnrelatedType() =>
            Assert.Throws<ArgumentException>(() => new SerializableMonoScript<Exception>(typeof(string)));

        [Test]
        public void ConstrainedWrapper_IsAMonoScriptWrapper()
        {
            SerializableMonoScript wrapper = new SerializableMonoScript<Exception>(typeof(ArgumentException));

            Assert.AreEqual(typeof(Exception), wrapper.BaseType, "BaseType must stay virtual through the base reference.");
            Assert.AreEqual(typeof(ArgumentException), (Type)wrapper);
        }

        [Test]
        public void Wrappers_ExposeTheirBaseType()
        {
            Assert.AreEqual(typeof(object), new SerializableMonoScript().BaseType);
            Assert.AreEqual(typeof(Exception), new SerializableMonoScript<Exception>().BaseType);
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
            var holder = ScriptableObject.CreateInstance<Holder>();
            try
            {
                var serialized = new SerializedObject(holder);
                var wrapper = serialized.FindProperty(nameof(Holder.wrapper));

                SerializableMonoScriptUtility.Assign(wrapper, ScriptedType);
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
                Assert.IsInstanceOf<MonoScript>(holder.wrapper.Script);
                Assert.AreEqual(ScriptedType, ((MonoScript)holder.wrapper.Script).GetClass());

                serialized.Update();
                Assert.AreEqual(ScriptedType, SerializableMonoScriptUtility.GetCurrentType(wrapper, out var name));
                Assert.AreEqual(ScriptedType.AssemblyQualifiedName, name);

                SerializableMonoScriptUtility.Assign(wrapper, null);
                Assert.IsNull(holder.wrapper.Type);
                Assert.IsNull(holder.wrapper.Script);
                Assert.AreEqual(string.Empty, holder.wrapper.AssemblyQualifiedName);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Serialization_ResyncsTheNameFromTheScript()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
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
            var holder = ScriptableObject.CreateInstance<Holder>();
            try
            {
                var serialized = new SerializedObject(holder);
                var name = serialized.FindProperty($"{nameof(Holder.wrapper)}.{SerializableTypeUtility.BackingFieldName}");
                name.stringValue = ScriptedType.AssemblyQualifiedName;
                serialized.ApplyModifiedProperties();

                SerializableMonoScriptUtility.SyncScriptFromName(name);

                Assert.IsInstanceOf<MonoScript>(holder.wrapper.Script);
                Assert.AreEqual(ScriptedType, holder.wrapper.Type);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void RequiredGate_CoversTheWrapper()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
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
