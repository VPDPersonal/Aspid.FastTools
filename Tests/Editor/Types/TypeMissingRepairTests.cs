using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.Types.Editors.Tests
{
    internal sealed class TypeMissingRepairTestObject : ScriptableObject
    {
        public string typeName;
    }

    internal sealed class TypeMissingRepairTests
    {
        private interface IRepairProbe { }
        private sealed class UniqueRepairProbe : IRepairProbe { }
        private sealed class OtherScope
        {
            internal sealed class DuplicateRepairProbe : IRepairProbe { }
        }
        private sealed class DuplicateRepairProbe : IRepairProbe { }
        [TypeSelectorDisplay(Hidden = true)]
        private sealed class HiddenRepairProbe : IRepairProbe { }
        private abstract class AbstractRepairProbe : IRepairProbe { }

        [Test]
        public void Suggestion_UniqueName_RespectsEveryBaseType()
        {
            Assert.AreEqual(typeof(UniqueRepairProbe), Suggest(nameof(UniqueRepairProbe)));
            Assert.IsNull(TypeMissingRepair.GetSuggestion(storedName: Missing(nameof(UniqueRepairProbe)),
                types: new[] { typeof(IRepairProbe), typeof(IDisposable) }, allow: TypeAllow.All,
                excludeEditorOnly: false));
        }

        [Test]
        public void Suggestion_AmbiguousOrHiddenName_IsNotOffered()
        {
            Assert.IsNull(Suggest(nameof(DuplicateRepairProbe)));
            Assert.IsNull(Suggest(nameof(HiddenRepairProbe)));
        }

        [Test]
        public void Suggestion_RespectsAllowedKindsAndRuntimeAssemblyFilter()
        {
            Assert.IsNull(Suggest(nameof(AbstractRepairProbe)));
            Assert.AreEqual(typeof(AbstractRepairProbe), Suggest(nameof(AbstractRepairProbe), TypeAllow.All));
            Assert.IsNull(TypeMissingRepair.GetSuggestion(storedName: Missing(nameof(UniqueRepairProbe)),
                types: new[] { typeof(IRepairProbe) }, allow: TypeAllow.All, excludeEditorOnly: true));
        }

        [Test]
        public void Suggestion_Predicate_NarrowsCandidatesAndIsPartOfTheCacheKey()
        {
            Assert.AreEqual(typeof(UniqueRepairProbe), Suggest(nameof(UniqueRepairProbe)));
            Assert.IsNull(Suggest(nameof(UniqueRepairProbe), predicate: RejectAll));
            Assert.AreEqual(typeof(UniqueRepairProbe), Suggest(nameof(UniqueRepairProbe)),
                "A filtered lookup must not leak into an unfiltered one.");
            Assert.AreEqual(typeof(UniqueRepairProbe), Suggest(nameof(UniqueRepairProbe), predicate: AcceptAll));
            Assert.IsNull(Suggest(nameof(UniqueRepairProbe), predicate: RejectAll),
                "A cached filtered result stays rejected.");
        }

        [Test]
        public void Suggestion_Predicate_ResolvesAnAmbiguousName()
        {
            Assert.IsNull(Suggest(nameof(DuplicateRepairProbe)));
            Assert.AreEqual(typeof(OtherScope.DuplicateRepairProbe),
                Suggest(nameof(DuplicateRepairProbe), predicate: IsInOtherScope));
        }

        [Test]
        public void Suggestion_ScriptPredicate_OffersOnlyTypesWithAScript()
        {
            // A nested type owns no script asset; a top-level class of the runtime assembly does.
            Assert.IsNull(Suggest(nameof(UniqueRepairProbe), predicate: SerializableMonoScriptUtility.HasScript));
            Assert.AreEqual(typeof(SerializableMonoScript), TypeMissingRepair.GetSuggestion(
                storedName: Missing(nameof(SerializableMonoScript)), types: new[] { typeof(SerializableTypeBase) },
                allow: TypeAllow.None, excludeEditorOnly: false, predicate: SerializableMonoScriptUtility.HasScript));
        }

        [TestCase("UniqueRepairProbe[]")]
        [TestCase("UniqueRepairProbe`1[[System.Int32, mscorlib]]")]
        [TestCase("UnrelatedName")]
        public void Suggestion_DoesNotGuessChangedTypeShapesOrRenamedClasses(string name) =>
            Assert.IsNull(Suggest(name));

        [UnityTest]
        public IEnumerator Notice_RepairAndUndo_RefreshesAndPreservesTheStoredName()
        {
            var target = ScriptableObject.CreateInstance<TypeMissingRepairTestObject>();
            target.typeName = Missing(nameof(UniqueRepairProbe));
            var window = ScriptableObject.CreateInstance<EditorWindow>();
            using var serialized = new SerializedObject(target);
            try
            {
                window.ShowUtility();
                var property = serialized.FindProperty(nameof(TypeMissingRepairTestObject.typeName));
                var root = TypeUIToolkitPropertyDrawer.Draw(label: "Type", property: property,
                    allow: TypeAllow.None, types: new[] { typeof(IRepairProbe) }, out var field);
                window.rootVisualElement.Add(root);
                yield return null;

                Assert.IsTrue(root.ClassListContains("aspid-fasttools-type-property--missing"));
                Assert.IsNotNull(root.Q<Label>(className: "aspid-fasttools-inspector-notice__suggestion--visible"));
                Assert.Greater(TypeIMGUIPropertyDrawer.GetHeight(property), EditorGUIUtility.singleLineHeight);

                var suggestion = root.Q<Label>(className: "aspid-fasttools-inspector-notice__suggestion");
                using (var click = ClickEvent.GetPooled())
                {
                    click.target = suggestion;
                    suggestion.SendEvent(click);
                }
                Undo.FlushUndoRecordObjects();
                Assert.AreEqual(typeof(UniqueRepairProbe).AssemblyQualifiedName, target.typeName);
                Assert.IsFalse(root.ClassListContains("aspid-fasttools-type-property--missing"));
                serialized.Update();
                Assert.AreEqual(EditorGUIUtility.singleLineHeight, TypeIMGUIPropertyDrawer.GetHeight(property));

                Undo.PerformUndo();
                var caption = field.Q<TextElement>(className: EnumField.textUssClassName);
                var deadline = EditorApplication.timeSinceStartup + 2;
                while ((!root.ClassListContains("aspid-fasttools-type-property--missing") ||
                        caption.text != MissingCaption(nameof(UniqueRepairProbe))) &&
                       EditorApplication.timeSinceStartup < deadline)
                    yield return null;
                Assert.AreEqual(Missing(nameof(UniqueRepairProbe)), target.typeName);
                Assert.IsTrue(root.ClassListContains("aspid-fasttools-type-property--missing"));
                Assert.AreEqual(MissingCaption(nameof(UniqueRepairProbe)), caption.text);

                field.ApplyPicked(assemblyQualifiedName: null);
                deadline = EditorApplication.timeSinceStartup + 2;
                while (root.ClassListContains("aspid-fasttools-type-property--missing") &&
                       EditorApplication.timeSinceStartup < deadline)
                    yield return null;
                Assert.AreEqual(string.Empty, target.typeName);
                Assert.IsFalse(root.ClassListContains("aspid-fasttools-type-property--missing"));
            }
            finally
            {
                window.rootVisualElement.Clear();
                Object.DestroyImmediate(window);
                Undo.ClearUndo(target);
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void MixedSelection_DoesNotShowMissingNotice()
        {
            var first = ScriptableObject.CreateInstance<TypeMissingRepairTestObject>();
            var second = ScriptableObject.CreateInstance<TypeMissingRepairTestObject>();
            try
            {
                first.typeName = Missing(nameof(UniqueRepairProbe));
                second.typeName = typeof(UniqueRepairProbe).AssemblyQualifiedName;
                using var serialized = new SerializedObject(new Object[] { first, second });
                var property = serialized.FindProperty(nameof(TypeMissingRepairTestObject.typeName));
                Assert.IsFalse(TypeMissingRepair.IsMissing(property: property));
                Assert.AreEqual(EditorGUIUtility.singleLineHeight, TypeIMGUIPropertyDrawer.GetHeight(property));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        private static string Missing(string name) => $"Old.Namespace.{name}, Missing.Assembly";

        private static string MissingCaption(string name) => $"<Missing Old.Namespace.{name}>";

        private static bool AcceptAll(Type _) => true;

        private static bool RejectAll(Type _) => false;

        private static bool IsInOtherScope(Type type) => type.DeclaringType == typeof(OtherScope);

        private static Type Suggest(string name, TypeAllow allow = TypeAllow.None, Func<Type, bool> predicate = null) =>
            TypeMissingRepair.GetSuggestion(storedName: Missing(name), types: new[] { typeof(IRepairProbe) },
                allow: allow, excludeEditorOnly: false, predicate: predicate);
    }
}
