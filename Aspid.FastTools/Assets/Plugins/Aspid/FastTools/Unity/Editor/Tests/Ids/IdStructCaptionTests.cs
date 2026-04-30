#nullable enable
using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Ids.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.EditorTests
{
    [TestFixture]
    internal sealed class IdStructCaptionTests
    {
        private StringIdRegistry _stringRegistry = null!;
        private IdRegistry _intRegistry = null!;
        private StringIdRegistryAccessor _stringAccessor = null!;

        [SetUp]
        public void Setup()
        {
            _stringRegistry = ScriptableObject.CreateInstance<StringIdRegistry>();
            _intRegistry = ScriptableObject.CreateInstance<IdRegistry>();
            _stringAccessor = new StringIdRegistryAccessor(_stringRegistry);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_stringRegistry);
            Object.DestroyImmediate(_intRegistry);
        }

        [Test]
        public void Build_NoRegistry_FallsBackToNoneWhenZero()
        {
            var caption = IdStructCaption.Build(
                id: 0, storedName: string.Empty, registry: null,
                out var resolvedName, out var isMissing);

            Assert.IsFalse(isMissing);
            Assert.AreEqual(string.Empty, resolvedName);
            Assert.AreEqual("<None>", caption);
        }

        [Test]
        public void Build_NoRegistry_PositiveIdShowsMissingId()
        {
            var caption = IdStructCaption.Build(
                id: 7, storedName: string.Empty, registry: null,
                out _, out var isMissing);

            Assert.IsTrue(isMissing);
            Assert.AreEqual("<Missing id 7>", caption);
        }

        [Test]
        public void Build_IntOnlyRegistry_NeverMissing()
        {
            var caption = IdStructCaption.Build(
                id: 42, storedName: "anything", registry: _intRegistry,
                out var resolvedName, out var isMissing);

            Assert.IsFalse(isMissing);
            Assert.AreEqual("anything", resolvedName);
            Assert.AreEqual("anything", caption);
        }

        [Test]
        public void Build_StringIdRegistry_KnownId_UsesRegistryName()
        {
            _stringAccessor.NextIdProperty.intValue = 1;
            _stringAccessor.Add("Goblin");
            _stringAccessor.Commit();

            var caption = IdStructCaption.Build(
                id: 1, storedName: "stale", registry: _stringRegistry,
                out var resolvedName, out var isMissing);

            Assert.IsFalse(isMissing);
            Assert.AreEqual("Goblin", resolvedName);
            Assert.AreEqual("Goblin", caption);
        }

        [Test]
        public void Build_StringIdRegistry_UnknownId_ShowsMissingWithStoredName()
        {
            _stringAccessor.NextIdProperty.intValue = 1;
            _stringAccessor.Add("Goblin");
            _stringAccessor.Commit();

            var caption = IdStructCaption.Build(
                id: 99, storedName: "OldName", registry: _stringRegistry,
                out var resolvedName, out var isMissing);

            Assert.IsTrue(isMissing);
            Assert.AreEqual("OldName", resolvedName);
            Assert.AreEqual("<Missing 'OldName'>", caption);
        }

        [Test]
        public void Build_StringIdRegistry_UnknownIdWithoutStoredName_ShowsMissingId()
        {
            _stringAccessor.NextIdProperty.intValue = 1;
            _stringAccessor.Add("Goblin");
            _stringAccessor.Commit();

            var caption = IdStructCaption.Build(
                id: 99, storedName: string.Empty, registry: _stringRegistry,
                out _, out var isMissing);

            Assert.IsTrue(isMissing);
            Assert.AreEqual("<Missing id 99>", caption);
        }

        [Test]
        public void Build_StringIdRegistry_ZeroId_NotMissing()
        {
            var caption = IdStructCaption.Build(
                id: 0, storedName: string.Empty, registry: _stringRegistry,
                out _, out var isMissing);

            Assert.IsFalse(isMissing);
            Assert.AreEqual("<None>", caption);
        }
    }
}
