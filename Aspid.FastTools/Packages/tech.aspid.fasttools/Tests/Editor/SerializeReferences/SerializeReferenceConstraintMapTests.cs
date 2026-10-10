using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceHelpers.BuildConstraintMap"/> on a reference that fields of different
    /// types share: the pickers and checks of the References windows must see the type of every field, not only the
    /// last one walked.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceConstraintMapTests
    {
        private const string ProbeAssetPath = "Assets/__AspidConstraintMapProbe__.asset";

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(ProbeAssetPath);

        [Test]
        public void BuildConstraintMap_RidSharedByUnrelatedFieldTypes_KeepsEveryType()
        {
            var shared = new ConstraintMapBoth();
            var (fileId, rid) = CreateProbe(probe =>
            {
                probe.first = shared;
                probe.second = shared;
            }, nameof(ConstraintMapProbe.first));

            var map = SerializeReferenceHelpers.BuildConstraintMap(ProbeAssetPath);

            CollectionAssert.AreEquivalent(new[] { typeof(IConstraintMapFirst), typeof(IConstraintMapSecond) }, map[(fileId, rid)]);
        }

        [Test]
        public void BuildConstraintMap_RidSharedByBaseAndDerivedFieldTypes_KeepsTheNarrowest()
        {
            var shared = new ConstraintMapDerived();
            var (fileId, rid) = CreateProbe(probe =>
            {
                probe.narrow = shared;
                probe.first = shared;
            }, nameof(ConstraintMapProbe.narrow));

            var map = SerializeReferenceHelpers.BuildConstraintMap(ProbeAssetPath);

            CollectionAssert.AreEqual(new[] { typeof(ConstraintMapBase) }, map[(fileId, rid)]);
        }

        [Test]
        public void FitsConstraints_TypeMissingOneConstraint_DoesNotFit()
        {
            var constraints = new[] { typeof(IConstraintMapFirst), typeof(IConstraintMapSecond) };

            Assert.IsTrue(SerializeReferenceHelpers.FitsConstraints(typeof(ConstraintMapBoth), constraints));
            Assert.IsFalse(SerializeReferenceHelpers.FitsConstraints(typeof(ConstraintMapBase), constraints));
            Assert.IsTrue(SerializeReferenceHelpers.FitsConstraints(typeof(ConstraintMapBase), constraints: null));
        }

        private static (long fileId, long rid) CreateProbe(Action<ConstraintMapProbe> populate, string sharedField)
        {
            var probe = ScriptableObject.CreateInstance<ConstraintMapProbe>();
            populate(probe);
            AssetDatabase.CreateAsset(probe, ProbeAssetPath);

            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId));

            using var serializedObject = new SerializedObject(probe);
            return (fileId, serializedObject.FindProperty(sharedField).managedReferenceId);
        }
    }

    internal interface IConstraintMapFirst { }

    internal interface IConstraintMapSecond { }

    [Serializable]
    internal sealed class ConstraintMapBoth : IConstraintMapFirst, IConstraintMapSecond { }

    [Serializable]
    internal class ConstraintMapBase : IConstraintMapFirst { }

    [Serializable]
    internal sealed class ConstraintMapDerived : ConstraintMapBase { }

    internal sealed class ConstraintMapProbe : ScriptableObject
    {
        [SerializeReference] public IConstraintMapFirst first;
        [SerializeReference] public IConstraintMapSecond second;
        [SerializeReference] public ConstraintMapBase narrow;
    }
}
