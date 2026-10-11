using System;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards the code-side contract of the wrappers: the implicit <c>SerializableType → Type</c> conversions
    /// (a <see langword="null"/> wrapper converts to <see langword="null"/> instead of throwing), the
    /// type-taking constructors, the stored-name accessor and the cached lookup.
    /// </summary>
    [TestFixture]
    internal sealed class SerializableTypeTests
    {
        private const string MissingAssembly = "Aspid.FastTools.Tests.MissingAssembly";

        private sealed class Holder : ScriptableObject
        {
            [SerializeField] public SerializableType wrapper;
        }

        // FromJsonOverwrite deserializes like loading an asset, so it runs the wrapper's OnAfterDeserialize.
        private static void LoadName(Holder holder, string assemblyQualifiedName) =>
            JsonUtility.FromJsonOverwrite(
                $"{{\"{nameof(Holder.wrapper)}\":{{\"{SerializableTypeUtility.BackingFieldName}\":\"{assemblyQualifiedName}\"}}}}",
                holder);

        [Test]
        public void MissingType_IsLookedUpOnce_UntilTheNameChanges()
        {
            // Type.GetType raises AssemblyResolve on every attempt to load the missing assembly.
            var probes = 0;
            ResolveEventHandler onResolve = (_, args) =>
            {
                if (args.Name.StartsWith(MissingAssembly, StringComparison.Ordinal)) probes++;
                return null;
            };

            var holder = ScriptableObject.CreateInstance<Holder>();
            AppDomain.CurrentDomain.AssemblyResolve += onResolve;
            try
            {
                LoadName(holder, $"Missing.Type, {MissingAssembly}");

                Assert.IsNull(holder.wrapper.Type);
                Assert.IsNull(holder.wrapper.Type);
                Assert.AreEqual($"Missing.Type, {MissingAssembly}", holder.wrapper.ToString());
                Assert.AreEqual(1, probes, "A failed lookup must be cached.");

                LoadName(holder, typeof(Exception).AssemblyQualifiedName);
                Assert.AreEqual(typeof(Exception), holder.wrapper.Type, "A new stored name must reset the cache.");
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= onResolve;
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void ImplicitConversion_NullWrapper_YieldsNull()
        {
            SerializableType wrapper = null;
            Type type = wrapper;

            Assert.IsNull(type);
        }

        [Test]
        public void ImplicitConversion_NullGenericWrapper_YieldsNull()
        {
            SerializableType<IComparable> wrapper = null;
            Type type = wrapper;

            Assert.IsNull(type);
        }

        [Test]
        public void EmptyWrapper_HasNoTypeAndAnEmptyName()
        {
            var wrapper = new SerializableType(null);

            Assert.IsNull(wrapper.Type);
            Assert.AreEqual(string.Empty, wrapper.AssemblyQualifiedName);
            Assert.AreEqual(string.Empty, wrapper.ToString());
        }

        [Test]
        public void EmptyWrapper_IsEmpty_AndNotMissing()
        {
            var wrapper = new SerializableType(null);

            Assert.IsTrue(wrapper.IsEmpty);
            Assert.IsFalse(wrapper.IsMissing);
            Assert.IsTrue(new SerializableType<Exception>(null).IsEmpty);
        }

        [Test]
        public void ResolvedWrapper_IsNeitherEmptyNorMissing()
        {
            var wrapper = new SerializableType(typeof(Exception));

            Assert.IsFalse(wrapper.IsEmpty);
            Assert.IsFalse(wrapper.IsMissing);
            Assert.IsNotNull(wrapper.Type);
        }

        [Test]
        public void UnresolvedName_IsMissing_AndNotEmpty()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
            try
            {
                LoadName(holder, $"Missing.Type, {MissingAssembly}");

                Assert.IsTrue(holder.wrapper.IsMissing);
                Assert.IsFalse(holder.wrapper.IsEmpty);
                Assert.IsNull(holder.wrapper.Type);
                Assert.AreEqual($"Missing.Type, {MissingAssembly}", holder.wrapper.AssemblyQualifiedName);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void BlankName_IsEmpty_AndNotMissing()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
            try
            {
                LoadName(holder, "  ");

                Assert.IsTrue(holder.wrapper.IsEmpty);
                Assert.IsFalse(holder.wrapper.IsMissing);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void State_FollowsTheStoredName_WhenItChanges()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
            try
            {
                LoadName(holder, $"Missing.Type, {MissingAssembly}");
                Assert.IsTrue(holder.wrapper.IsMissing, "Precondition: the name is unresolved and cached.");

                LoadName(holder, typeof(Exception).AssemblyQualifiedName);
                Assert.IsFalse(holder.wrapper.IsMissing);
                Assert.IsFalse(holder.wrapper.IsEmpty);

                LoadName(holder, string.Empty);
                Assert.IsTrue(holder.wrapper.IsEmpty);
                Assert.IsFalse(holder.wrapper.IsMissing);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void UnsetField_IsNotNull_ButIsEmpty()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
            try
            {
                // Unity creates the wrapper when it serializes the object, never earlier.
                new UnityEditor.SerializedObject(holder).Update();

                Assert.IsFalse(holder.wrapper == null, "Once Unity has serialized the object it fills the field, so a null check does not see it empty.");
                Assert.IsTrue(holder.wrapper.IsEmpty);
            }
            finally { UnityEngine.Object.DestroyImmediate(holder); }
        }

        [Test]
        public void Constructor_StoresTheTypeAndItsAssemblyQualifiedName()
        {
            var wrapper = new SerializableType(typeof(Exception));

            Assert.AreEqual(typeof(Exception), wrapper.Type);
            Assert.AreEqual(typeof(Exception).AssemblyQualifiedName, wrapper.AssemblyQualifiedName);
            Assert.AreEqual("Exception", wrapper.ToString());
        }

        [Test]
        public void Constructor_NullType_IsAnEmptyWrapper()
        {
            Assert.IsNull(new SerializableType(null).Type);
            Assert.IsNull(new SerializableType<Exception>(null).Type);
        }

        [Test]
        public void GenericConstructor_AcceptsAnAssignableType() =>
            Assert.AreEqual(typeof(ArgumentException), new SerializableType<Exception>(typeof(ArgumentException)).Type);

        [Test]
        public void GenericConstructor_RejectsAnUnrelatedType() =>
            Assert.Throws<ArgumentException>(() => new SerializableType<Exception>(typeof(string)));

        [Test]
        public void ConstrainedWrapper_IsASerializableType()
        {
            SerializableType wrapper = new SerializableType<IComparable>(typeof(int));

            Assert.AreEqual(typeof(IComparable), wrapper.BaseType, "BaseType must stay virtual through the base reference.");
            Assert.AreEqual(typeof(int), wrapper.Type);
            Assert.AreEqual(typeof(int), (Type)wrapper);
        }

        [Test]
        public void Equals_SameStoredType_IsEqualWhateverTheConstraint()
        {
            var plain = new SerializableType(typeof(ArgumentException));
            var constrained = new SerializableType<Exception>(typeof(ArgumentException));

            Assert.IsTrue(plain.Equals(constrained));
            Assert.IsTrue(constrained.Equals((object)plain));
            Assert.AreEqual(plain.GetHashCode(), constrained.GetHashCode());
        }

        [Test]
        public void Equals_DifferentTypes_IsNotEqual()
        {
            var wrapper = new SerializableType(typeof(ArgumentException));

            Assert.IsFalse(wrapper.Equals(new SerializableType(typeof(Exception))));
            Assert.IsFalse(wrapper.Equals(new SerializableType(null)));
        }

        [Test]
        public void Equals_Null_IsNotEqual()
        {
            var wrapper = new SerializableType(typeof(Exception));

            Assert.IsFalse(wrapper.Equals((SerializableTypeBase)null));
            Assert.IsFalse(wrapper.Equals((object)null));
            Assert.IsFalse(wrapper.Equals("not a wrapper"));
        }

        [Test]
        public void Equals_EmptyWrappers_AreEqual()
        {
            var first = new SerializableType(null);
            var second = new SerializableType<Exception>(null);

            Assert.IsTrue(first.Equals(second));
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Test]
        public void Equals_LoadedName_MatchesTheConstructedWrapper()
        {
            var holder = ScriptableObject.CreateInstance<Holder>();
            try
            {
                LoadName(holder, typeof(Exception).AssemblyQualifiedName);

                Assert.IsTrue(holder.wrapper.Equals(new SerializableType(typeof(Exception))));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void Equals_UnresolvedNames_ComparesTheStoredName()
        {
            var first = ScriptableObject.CreateInstance<Holder>();
            var second = ScriptableObject.CreateInstance<Holder>();
            try
            {
                LoadName(first, $"Missing.Type, {MissingAssembly}");
                LoadName(second, $"Other.Type, {MissingAssembly}");

                Assert.IsFalse(first.wrapper.Equals(second.wrapper));

                LoadName(second, $"Missing.Type, {MissingAssembly}");

                Assert.IsTrue(first.wrapper.Equals(second.wrapper), "Equal names are equal even when no type resolves.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Wrapper_WorksAsDictionaryAndHashSetKey()
        {
            var counts = new Dictionary<SerializableType, int> { [new SerializableType(typeof(Exception))] = 3 };
            var unique = new HashSet<SerializableType>
            {
                new(typeof(Exception)),
                new SerializableType<Exception>(typeof(Exception)),
                new(typeof(ArgumentException)),
            };

            Assert.AreEqual(3, counts[new SerializableType<Exception>(typeof(Exception))]);
            Assert.IsFalse(counts.ContainsKey(new SerializableType(typeof(ArgumentException))));
            Assert.AreEqual(2, unique.Count);
        }

        [Test]
        public void MonoScriptWrapper_IsNotASerializableType() =>
            Assert.IsFalse(typeof(SerializableType).IsAssignableFrom(typeof(SerializableMonoScript)),
                "The two families share only SerializableTypeBase: their serialized layouts differ.");
    }
}
