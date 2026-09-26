using System.Linq;
using UnityEditor;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceTemplates"/>: a template whose type does not resolve right now is
    /// hidden from the menu but kept in the store, so it comes back once the type is available again.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceTemplatesTests
    {
        private const string UnresolvedName = "Aspid Test Unresolved Template";

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
    }
}
