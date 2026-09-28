using System;
using System.Linq;
using UnityEditor;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    [Serializable]
    internal sealed class ResolvedTemplateValue { }

    // A template whose type does not resolve right now is hidden from the menu but kept in the store, so it comes
    // back once the type is available again; only an explicit Remove Missing deletes it.
    [TestFixture]
    internal sealed class SerializeReferenceTemplatesTests
    {
        private const string UnresolvedName = "Aspid Test Unresolved Template";
        private const string ResolvedName = "Aspid Test Resolved Template";

        private bool _hadStore;
        private string _store;

        [SetUp]
        public void SetUp()
        {
            // Snapshot the project's real templates so the test can replace them and restore on teardown.
            _hadStore = EditorPrefs.HasKey(SerializeReferenceTemplates.Key);
            _store = EditorPrefs.GetString(SerializeReferenceTemplates.Key, string.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            if (_hadStore) EditorPrefs.SetString(SerializeReferenceTemplates.Key, _store);
            else EditorPrefs.DeleteKey(SerializeReferenceTemplates.Key);
        }

        [Test]
        public void LoadResolved_UnresolvedType_IsHiddenButKept()
        {
            EditorPrefs.SetString(SerializeReferenceTemplates.Key,
                "{\"entries\":[{\"name\":\"" + UnresolvedName +
                "\",\"aqn\":\"Missing.Namespace.Gone, Missing.Assembly\",\"json\":\"{}\"}]}");

            var resolved = SerializeReferenceTemplates.LoadResolved();

            Assert.IsFalse(resolved.Any(template => template.Name == UnresolvedName),
                "A template whose type does not resolve must not be offered.");
            Assert.IsTrue(SerializeReferenceTemplates.Contains(UnresolvedName),
                "A template whose type does not resolve must stay in the store.");
        }

        [Test]
        public void RemoveUnresolved_RemovesOnlyUnresolved()
        {
            EditorPrefs.SetString(SerializeReferenceTemplates.Key,
                "{\"entries\":[{\"name\":\"" + UnresolvedName +
                "\",\"aqn\":\"Missing.Namespace.Gone, Missing.Assembly\",\"json\":\"{}\"}]}");
            SerializeReferenceTemplates.Save(ResolvedName, new ResolvedTemplateValue());

            CollectionAssert.AreEqual(new[] { UnresolvedName }, SerializeReferenceTemplates.UnresolvedNames());
            Assert.AreEqual(1, SerializeReferenceTemplates.RemoveUnresolved());
            Assert.IsFalse(SerializeReferenceTemplates.Contains(UnresolvedName), "Remove Missing must delete the unresolved template.");
            Assert.IsTrue(SerializeReferenceTemplates.Contains(ResolvedName), "Remove Missing must keep a template whose type loads.");
            Assert.AreEqual(0, SerializeReferenceTemplates.UnresolvedNames().Count);
        }
    }
}
