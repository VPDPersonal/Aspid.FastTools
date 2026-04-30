#nullable enable
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Ids.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.EditorTests
{
    [TestFixture]
    internal sealed class StringIdRegistryAccessorTests
    {
        private StringIdRegistry _registry = null!;
        private IRegistryAccessor _accessor = null!;

        [SetUp]
        public void Setup()
        {
            _registry = ScriptableObject.CreateInstance<StringIdRegistry>();
            _accessor = new StringIdRegistryAccessor(_registry);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_registry);
        }

        [Test]
        public void Add_AssignsNextIdAndIncrements()
        {
            _accessor.NextIdProperty.intValue = 1;
            var id = _accessor.Add("Goblin");
            _accessor.Commit();

            Assert.AreEqual(1, id);
            Assert.AreEqual(2, _accessor.NextIdProperty.intValue);
            Assert.AreEqual("Goblin", _accessor.GetName(0));
            Assert.AreEqual(1, _accessor.GetId(0));
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
        public void RemoveAt_ShrinksArray()
        {
            _accessor.Add("A");
            _accessor.Add("B");
            _accessor.RemoveAt(0);
            _accessor.Commit();

            Assert.AreEqual(1, _accessor.Count);
            Assert.AreEqual("B", _accessor.GetName(0));
        }

        [Test]
        public void HasStructuralDamage_AlwaysReturnsFalse()
        {
            _accessor.Add("A");
            _accessor.Commit();

            Assert.IsFalse(_accessor.HasStructuralDamage(out _));
        }

        [Test]
        public void EnumerateInvalidIndices_ReportsEmptyAndDuplicates()
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
