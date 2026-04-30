#nullable enable
using NUnit.Framework;
using UnityEngine;
using Aspid.FastTools.Ids;
using Aspid.FastTools.Ids.Editors;

namespace Aspid.FastTools.Ids.EditorTests
{
    [TestFixture]
    internal sealed class IdRegistryAccessorTests
    {
        private IdRegistry _registry = null!;
        private IRegistryAccessor _accessor = null!;

        [SetUp]
        public void Setup()
        {
            _registry = ScriptableObject.CreateInstance<IdRegistry>();
            _accessor = new IdRegistryAccessor(_registry);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_registry);
        }

        [Test]
        public void Add_AssignsNextIdAndIncrements()
        {
            _accessor.NextIdProperty.intValue = 5;
            var id = _accessor.Add("Goblin");
            _accessor.Commit();

            Assert.AreEqual(5, id);
            Assert.AreEqual(6, _accessor.NextIdProperty.intValue);
            Assert.AreEqual(1, _accessor.Count);
            Assert.AreEqual("Goblin", _accessor.GetName(0));
        }

        [Test]
        public void SetName_ChangesEntryName()
        {
            _accessor.Add("Goblin");
            _accessor.SetName(0, "Orc");
            _accessor.Commit();

            Assert.AreEqual("Orc", _accessor.GetName(0));
        }

        [Test]
        public void RemoveAt_RemovesFromBothArrays()
        {
            _accessor.Add("A");
            _accessor.Add("B");
            _accessor.Add("C");
            _accessor.Commit();

            _accessor.RemoveAt(1);
            _accessor.Commit();

            Assert.AreEqual(2, _accessor.Count);
            Assert.AreEqual("A", _accessor.GetName(0));
            Assert.AreEqual("C", _accessor.GetName(1));
        }

        [Test]
        public void Contains_FindsExistingName()
        {
            _accessor.Add("Goblin");
            _accessor.Commit();

            Assert.IsTrue(_accessor.Contains("Goblin"));
            Assert.IsFalse(_accessor.Contains("Orc"));
        }

        [Test]
        public void MaxAssignedId_ReturnsHighestId()
        {
            _accessor.NextIdProperty.intValue = 10;
            _accessor.Add("A");
            _accessor.NextIdProperty.intValue = 25;
            _accessor.Add("B");
            _accessor.NextIdProperty.intValue = 17;
            _accessor.Add("C");
            _accessor.Commit();

            Assert.AreEqual(25, _accessor.MaxAssignedId);
        }

        [Test]
        public void MaxAssignedId_EmptyRegistry_ReturnsZero()
        {
            Assert.AreEqual(0, _accessor.MaxAssignedId);
        }

        [Test]
        public void HasStructuralDamage_IntactArrays_ReturnsFalse()
        {
            _accessor.Add("A");
            _accessor.Commit();

            Assert.IsFalse(_accessor.HasStructuralDamage(out var reason));
            Assert.IsEmpty(reason);
        }

        [Test]
        public void HasStructuralDamage_LengthMismatch_ReturnsTrue()
        {
            var idsProp = _accessor.SerializedObject.FindProperty("_ids");
            var namesProp = _accessor.SerializedObject.FindProperty("_names");
            idsProp.arraySize = 3;
            namesProp.arraySize = 2;
            _accessor.SerializedObject.ApplyModifiedPropertiesWithoutUndo();

            Assert.IsTrue(_accessor.HasStructuralDamage(out var reason));
            StringAssert.Contains("Length mismatch", reason);
        }

        [Test]
        public void EnumerateInvalidIndices_ReportsEmptyAndDuplicateNames()
        {
            _accessor.Add("Goblin");
            _accessor.Add(string.Empty);
            _accessor.Add("Goblin");
            _accessor.Commit();

            var invalid = new System.Collections.Generic.List<int>(_accessor.EnumerateInvalidIndices());

            CollectionAssert.AreEquivalent(new[] { 1, 2 }, invalid);
        }
    }
}
