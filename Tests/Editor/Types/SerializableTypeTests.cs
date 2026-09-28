using System;
using UnityEngine;
using NUnit.Framework;

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
        public void MonoScriptWrapper_IsNotASerializableType() =>
            Assert.IsFalse(typeof(SerializableType).IsAssignableFrom(typeof(SerializableMonoScript)),
                "The two families share only SerializableTypeBase: their serialized layouts differ.");
    }
}
