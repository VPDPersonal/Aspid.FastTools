using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // Search matching, the candidate caches and the editor-only exclusion of the picker's candidate list.
    [TestFixture]
    internal sealed class TypeSelectorCandidateTests
    {
        private interface ISearchProbe { }

        private sealed class PlainSearchProbe : ISearchProbe { }

        [TypeSelectorDisplay(Group = "Probes/Search")]
        private sealed class GroupedSearchProbe : ISearchProbe { }

        private static TreeNode BuildProbeHierarchy() =>
            HierarchyBuilder.Build(new[] { typeof(ISearchProbe) }, TypeAllow.None, includeNoneOption: false);

        [TestCase("Token")]
        [TestCase("PublicKey")]
        [TestCase("Culture")]
        [TestCase("neutral")]
        [TestCase("Version")]
        [TestCase("null")]
        [TestCase("Editor.Tests,")]
        public void Search_DoesNotMatchTheAssemblyPartOfTheName(string query)
        {
            var leaves = Leaves(BuildProbeHierarchy()).ToList();
            Assert.IsNotEmpty(leaves);

            foreach (var leaf in leaves)
                Assert.IsFalse(leaf.MatchesFilter(query), $"'{query}' must not match {leaf.Caption}.");
        }

        [Test]
        public void Search_StillMatchesTheNamespace_OfAGroupedType()
        {
            var leaf = FindLeaf(BuildProbeHierarchy(), typeof(GroupedSearchProbe));

            Assert.IsTrue(leaf.MatchesFilter("Types.Editors.Tests.GroupedSearch"),
                "A type placed in a group must stay findable by its namespace.");
        }

        [Test]
        public void Build_WithoutPredicate_ReusesTheHierarchy()
        {
            var first = HierarchyBuilder.Build(new[] { typeof(ISearchProbe) }, TypeAllow.All);

            Assert.AreSame(first, HierarchyBuilder.Build(new[] { typeof(ISearchProbe) }, TypeAllow.All),
                "An unfiltered picker must not rebuild the tree on every opening.");
            Assert.AreNotSame(first, HierarchyBuilder.Build(new[] { typeof(ISearchProbe) }, TypeAllow.None),
                "A different filter needs its own tree.");
            Assert.AreNotSame(first, HierarchyBuilder.Build(new[] { typeof(ISearchProbe) }, TypeAllow.All, _ => true),
                "A predicate cannot be compared between openings, so its tree is never reused.");
        }

        [Test]
        public void GetAllTypeInfos_ReusesTheInfoOfAType()
        {
            var first = Scan(excludeEditorOnly: false).Single(info => info.Name == nameof(PlainSearchProbe));
            var second = Scan(excludeEditorOnly: false).Single(info => info.Name == nameof(PlainSearchProbe));

            Assert.AreSame(first, second);
        }

        [Test]
        public void IsEditorOnlyAssembly_SeparatesEditorAndRuntimeAssemblies()
        {
            Assert.IsTrue(TypeUtility.IsEditorOnlyAssembly(typeof(EditorWindow).Assembly), "UnityEditor");
            Assert.IsTrue(TypeUtility.IsEditorOnlyAssembly(typeof(TypeField).Assembly), "Editor asmdef");
            Assert.IsTrue(TypeUtility.IsEditorOnlyAssembly(typeof(TypeSelectorCandidateTests).Assembly), "Editor test asmdef");

            Assert.IsFalse(TypeUtility.IsEditorOnlyAssembly(typeof(GameObject).Assembly), "UnityEngine module");
            Assert.IsFalse(TypeUtility.IsEditorOnlyAssembly(typeof(UnityEngine.UIElements.VisualElement).Assembly), "UnityEngine UI Toolkit");
            Assert.IsFalse(TypeUtility.IsEditorOnlyAssembly(typeof(SerializableType).Assembly), "Runtime asmdef");
            Assert.IsFalse(TypeUtility.IsEditorOnlyAssembly(typeof(string).Assembly), "System library");
        }

        [Test]
        public void GetAllTypeInfos_ExcludeEditorOnly_LeavesOutEditorAssemblies()
        {
            var runtime = new HashSet<string>(TypeInfo
                .GetAllTypeInfos(new[] { typeof(object) }, TypeAllow.All, excludeEditorOnly: true)
                .Select(info => info.AssemblyQualifiedName));

            Assert.IsFalse(runtime.Contains(typeof(EditorWindow).AssemblyQualifiedName));
            Assert.IsFalse(runtime.Contains(typeof(TypeField).AssemblyQualifiedName));
            Assert.IsFalse(runtime.Contains(typeof(PlainSearchProbe).AssemblyQualifiedName));
            Assert.IsTrue(runtime.Contains(typeof(GameObject).AssemblyQualifiedName));
            Assert.IsTrue(runtime.Contains(typeof(SerializableType).AssemblyQualifiedName));

            Assert.IsTrue(Scan(excludeEditorOnly: false).Any(info => info.Name == nameof(PlainSearchProbe)),
                "Without the flag, editor-only types stay offered.");
        }

        private static List<TypeInfo> Scan(bool excludeEditorOnly) =>
            TypeInfo.GetAllTypeInfos(new[] { typeof(ISearchProbe) }, TypeAllow.None, excludeEditorOnly: excludeEditorOnly);

        private static IEnumerable<TreeNode> Leaves(TreeNode node)
        {
            if (node.IsType) yield return node;

            foreach (var leaf in node.Children.SelectMany(Leaves))
                yield return leaf;
        }

        private static TreeNode FindLeaf(TreeNode root, Type type) =>
            Leaves(root).Single(node => node.AssemblyQualifiedName == type.AssemblyQualifiedName);
    }
}
