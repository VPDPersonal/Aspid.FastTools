using System;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.Scripting.APIUpdating;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A namespace move recorded by [MovedFrom]: the stored identity keeps the class name, only the namespace differs.
    [Serializable]
    [MovedFrom(true, sourceNamespace: "Aspid.FastTools.SerializeReferences.Editors.Tests.Legacy")]
    internal sealed class MovedNamespacePistol
    {
        [SerializeField] private int _damage;
    }

    // A type that moved out of the global namespace: the stored identity has an empty namespace.
    [Serializable]
    [MovedFrom("")]
    internal sealed class MovedFromGlobalShield { }

    // A stand-in for a Unity version that moved the rename data: it has a "data" field, but the field type has no parts.
    internal sealed class MovedFromWithoutParts
    {
        public readonly string data = string.Empty;
    }

    // Two types claiming the same recorded old class name: neither claim is authoritative.
    [Serializable]
    [MovedFrom(false, null, null, "AmbiguousOldSword")]
    internal sealed class AmbiguousNewSwordA { }

    [Serializable]
    [MovedFrom(false, null, null, "AmbiguousOldSword")]
    internal sealed class AmbiguousNewSwordB { }

    /// <summary>
    /// Coverage for <see cref="SerializeReferenceMovedFromResolver"/> — the authoritative-rename resolver behind the
    /// migration classification (breakage report entries and the Project References bulk <b>Migrate all</b>):
    /// a recorded namespace move and a recorded class rename each resolve to their single declaring type, while an
    /// ambiguous claim, an unknown identity or a mismatched namespace or assembly resolve to nothing.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceMovedFromResolverTests
    {
        private static string Assembly => typeof(MovedNamespacePistol).Assembly.GetName().Name;

        private static string Namespace => typeof(MovedNamespacePistol).Namespace;

        [Test]
        public void TryResolve_RecordedNamespaceMove_FindsTheSingleTarget()
        {
            var stored = new ManagedTypeName(Assembly, Namespace + ".Legacy", nameof(MovedNamespacePistol));

            Assert.IsTrue(SerializeReferenceMovedFromResolver.TryResolve(stored, out var target),
                "A stored identity matching a recorded [MovedFrom] namespace move must resolve.");
            Assert.AreEqual(typeof(MovedNamespacePistol), target);
        }

        [Test]
        public void TryResolve_RecordedClassRename_FindsTheSingleTarget()
        {
            // Reuses the RenamedRanged fixture declared for the ranking tests: [MovedFrom(..., "OldRenamedRanged")].
            var stored = new ManagedTypeName(Assembly, Namespace, "OldRenamedRanged");

            Assert.IsTrue(SerializeReferenceMovedFromResolver.TryResolve(stored, out var target),
                "A stored identity matching a recorded [MovedFrom] class rename must resolve.");
            Assert.AreEqual(typeof(RenamedRanged), target);
        }

        [Test]
        public void TryResolve_AmbiguousClaims_RefusesToPick()
        {
            var stored = new ManagedTypeName(Assembly, Namespace, "AmbiguousOldSword");

            Assert.IsFalse(SerializeReferenceMovedFromResolver.TryResolve(stored, out var target),
                "Two types claiming the same old identity make the rename non-authoritative.");
            Assert.IsNull(target);
        }

        [Test]
        public void TryResolve_RecordedMoveFromGlobalNamespace_FindsTheSingleTarget()
        {
            var stored = new ManagedTypeName(Assembly, string.Empty, nameof(MovedFromGlobalShield));

            Assert.IsTrue(SerializeReferenceMovedFromResolver.TryResolve(stored, out var target),
                "An empty stored namespace must match a [MovedFrom] that records the global namespace.");
            Assert.AreEqual(typeof(MovedFromGlobalShield), target);
        }

        [Test]
        public void TryResolve_GlobalStoredIdentity_DoesNotMatchAForeignClassRename()
        {
            // RenamedRanged records only a class rename, so its old namespace is its own. A stored type with the
            // same class name in the global namespace is a different type, not that rename.
            var stored = new ManagedTypeName(Assembly, string.Empty, "OldRenamedRanged");

            Assert.IsFalse(SerializeReferenceMovedFromResolver.TryResolve(stored, out var target),
                "A stored global type must not be taken for a renamed class from another namespace.");
            Assert.IsNull(target);
        }

        [Test]
        public void TryResolve_NamespaceMismatch_ReturnsFalse()
        {
            var stored = new ManagedTypeName(Assembly, "Some.Other.Namespace", "OldRenamedRanged");

            Assert.IsFalse(SerializeReferenceMovedFromResolver.TryResolve(stored, out _));
        }

        [Test]
        public void TryResolve_UnknownIdentity_ReturnsFalse()
        {
            var stored = new ManagedTypeName(Assembly, Namespace, "NoSuchOldTypeAnywhere");

            Assert.IsFalse(SerializeReferenceMovedFromResolver.TryResolve(stored, out _));
        }

        [Test]
        public void TryResolve_ClosedGenericIdentity_NeverMigrates()
        {
            // TypeCache yields definitions and the eligibility filter excludes generic parameters, so the only
            // possible claimant for a stored closed-generic identity is an arity-stripped name collision — a guess
            // the resolver must leave to the scored Smart Fix path.
            var stored = new ManagedTypeName(Assembly, Namespace, "OldRenamedRanged`1[[System.Single, mscorlib]]");

            Assert.IsFalse(SerializeReferenceMovedFromResolver.TryResolve(stored, out _));
        }

        [Test]
        public void TryResolve_AssemblyMismatch_ReturnsFalse()
        {
            // The fixture's [MovedFrom] records no assembly move, so the old assembly is the declaring one — a stored
            // identity from a different assembly must not match.
            var stored = new ManagedTypeName("Some.Other.Assembly", Namespace, "OldRenamedRanged");

            Assert.IsFalse(SerializeReferenceMovedFromResolver.TryResolve(stored, out _));
        }

        [Test]
        public void MovedFromFields_UnityAttribute_HasEveryField()
        {
            var fields = new SerializeReferenceMovedFromResolver.MovedFromFields(typeof(MovedFromAttribute));

            Assert.IsNull(fields.Missing, "The resolver would warn and match nothing.");
        }

        [Test]
        public void MovedFromFields_AttributeWithoutData_NamesTheDataField()
        {
            var fields = new SerializeReferenceMovedFromResolver.MovedFromFields(typeof(object));

            Assert.AreEqual("data", fields.Missing);
        }

        [Test]
        public void MovedFromFields_DataWithoutParts_NamesTheFirstMissingPart()
        {
            var fields = new SerializeReferenceMovedFromResolver.MovedFromFields(typeof(MovedFromWithoutParts));

            Assert.AreEqual("className", fields.Missing);
        }
    }
}
