using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using Aspid.FastTools.Types;
using Object = UnityEngine.Object;
using Aspid.FastTools.Types.Editors;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    internal interface ILinkWeapon { }

    internal interface ILinkMelee { }

    [Serializable]
    internal sealed class LinkSword : ILinkWeapon, ILinkMelee { }

    [Serializable]
    internal sealed class LinkPistol : ILinkWeapon { }

    internal sealed class LinkFilterTestObject : ScriptableObject
    {
        // Read only through reflection by the constraint resolver.
#pragma warning disable CS0414
        private Type _backupType = typeof(ILinkMelee);
#pragma warning restore CS0414

        [SerializeReference] public ILinkWeapon sword;
        [SerializeReference] public ILinkWeapon pistol;
        [SerializeReference] public ILinkWeapon spare;
        [TypeSelector(typeof(ILinkMelee)), SerializeReference] public ILinkWeapon meleeBackup;
        [TypeSelector(nameof(_backupType)), SerializeReference] public ILinkWeapon memberBackup;
    }

    /// <summary>
    /// Locks <b>Link to Existing</b> to the <c>[TypeSelector]</c> narrowing the field's picker enforces:
    /// <see cref="SerializeReferenceLinker.CollectLinkCandidates"/> with the drawer's filter must drop an instance the
    /// dropdown would never offer, for literal and member-referenced constraints alike.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceLinkCandidateFilterTests
    {
        private LinkFilterTestObject _target;
        private SerializedObject _serialized;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<LinkFilterTestObject>();
            _serialized = new SerializedObject(_target);
            _serialized.FindProperty(nameof(LinkFilterTestObject.sword)).managedReferenceValue = new LinkSword();
            _serialized.FindProperty(nameof(LinkFilterTestObject.pistol)).managedReferenceValue = new LinkPistol();
            _serialized.ApplyModifiedProperties();
        }

        [TearDown]
        public void TearDown()
        {
            _serialized.Dispose();
            Object.DestroyImmediate(_target);
        }

        [Test]
        public void LiteralConstraint_DropsCandidatesThePickerHides() =>
            AssertCandidates(nameof(LinkFilterTestObject.meleeBackup), nameof(LinkFilterTestObject.sword));

        [Test]
        public void MemberReferencedConstraint_DropsCandidatesThePickerHides() =>
            AssertCandidates(nameof(LinkFilterTestObject.memberBackup), nameof(LinkFilterTestObject.sword));

        [Test]
        public void NoConstraint_OffersEveryAssignableCandidate() =>
            AssertCandidates(nameof(LinkFilterTestObject.spare),
                nameof(LinkFilterTestObject.sword), nameof(LinkFilterTestObject.pistol));

        private void AssertCandidates(string fieldName, params string[] expectedPaths)
        {
            var property = _serialized.FindProperty(fieldName);
            var candidates = SerializeReferenceLinker.CollectLinkCandidates(property, filter: PickerFilter(property));

            CollectionAssert.AreEquivalent(expectedPaths, candidates.Select(candidate => candidate.Path));
        }

        // The filter both drawers build from the field's effective base types.
        private static Func<Type, bool> PickerFilter(SerializedProperty property)
        {
            var field = typeof(LinkFilterTestObject).GetField(property.name);
            var attribute = field.GetCustomAttribute<TypeSelectorAttribute>();
            if (attribute is null) return SerializeReferenceHelpers.BuildAssignableFilter(baseTypes: null);

            var resolution = TypeSelectorConstraintResolver.Resolve(property, attribute.AssemblyQualifiedNames);
            return SerializeReferenceHelpers.BuildAssignableFilter(resolution.Types);
        }
    }
}
