using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for the <see cref="TypeSelectorHelpers"/> formatting rules that do not depend on a built
    /// hierarchy: the caption for the unresolved / empty states, the tooltip identity, and the normalization
    /// of a <see cref="TypeSelectorDisplayAttribute.Name"/> override (blank and sentinel values, generic suffix).
    /// The attribute's placement inside the picker tree is covered by <see cref="TypeSelectorDisplayTests"/>.
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorHelpersTests
    {
        [TypeSelectorDisplay(Name = "   ")]
        private sealed class BlankNamed { }

        [TypeSelectorDisplay(Name = TypeSelectorHelpers.NoneOption)]
        private sealed class NoneImpersonator { }

        [TypeSelectorDisplay(Name = "Mod")]
        private sealed class NamedWrapper<T> { }

        private sealed class Host<T>
        {
            [TypeSelectorDisplay(Name = "Mod")]
            internal sealed class NamedNested { }

            internal sealed class Nested { }
        }

        [Test]
        public void Title_NoTypeAndNoName_IsTheNoneOption()
        {
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, TypeSelectorHelpers.GetTypeSelectorTitle(null));
            Assert.AreEqual(TypeSelectorHelpers.NoneOption, TypeSelectorHelpers.GetTypeSelectorTitle(null, "   "));
        }

        [Test]
        public void Title_UnresolvedName_IsTheMissingMarker()
        {
            Assert.AreEqual("<Missing Gone.Type, Gone>",
                TypeSelectorHelpers.GetTypeSelectorTitle(null, "Gone.Type, Gone"));
        }

        [TestCase("Gone.Type, Gone", "Gone.Type")]
        [TestCase("Gone.Type, Gone, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Gone.Type")]
        [TestCase("Gone.Type", "Gone.Type")]
        [TestCase("Gone.Outer+Inner, Gone", "Gone.Outer+Inner")]
        [TestCase("Gone.Box`1[[Gone.Item, Gone, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], Gone, Version=1.0.0.0",
            "Gone.Box`1[[Gone.Item, Gone, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]")]
        [TestCase("Gone.Item[], Gone", "Gone.Item[]")]
        public void MissingDisplayName_DropsTheAssemblyPartOutsideBrackets(string stored, string expected) =>
            Assert.AreEqual(expected, TypeSelectorHelpers.GetMissingDisplayName(stored));

        [Test]
        public void MissingDisplayName_EmptyName_IsReturnedAsIs()
        {
            Assert.IsNull(TypeSelectorHelpers.GetMissingDisplayName(null));
            Assert.AreEqual("   ", TypeSelectorHelpers.GetMissingDisplayName("   "));
        }

        [Test]
        public void Title_ResolvedGeneric_SpellsTheArguments() =>
            Assert.AreEqual("List<Int32>", TypeSelectorHelpers.GetTypeSelectorTitle(typeof(List<int>)));

        [Test]
        public void CustomDisplayName_BlankOverride_CountsAsNone() =>
            Assert.IsNull(TypeSelectorHelpers.GetCustomDisplayName(typeof(BlankNamed)));

        [Test]
        public void CustomDisplayName_NoneSentinel_IsRejected() =>
            Assert.IsNull(TypeSelectorHelpers.GetCustomDisplayName(typeof(NoneImpersonator)),
                "A real type must not impersonate the <None> option.");

        [Test]
        public void CustomDisplayName_Generic_KeepsTheFormattedArguments()
        {
            Assert.AreEqual("Mod<Single>", TypeSelectorHelpers.GetCustomDisplayName(typeof(NamedWrapper<float>)),
                "A closed form must stay distinguishable by its arguments.");
            Assert.AreEqual("Mod<T>", TypeSelectorHelpers.GetCustomDisplayName(typeof(NamedWrapper<>)),
                "The open definition must still read as generic.");
        }

        [Test]
        public void CustomDisplayName_NestedInGeneric_HasNoArgumentsOfItsOwn()
        {
            Assert.AreEqual("Mod", TypeSelectorHelpers.GetCustomDisplayName(typeof(Host<>.NamedNested)));
            Assert.AreEqual("Mod", TypeSelectorHelpers.GetCustomDisplayName(typeof(Host<int>.NamedNested)));
        }

        [Test]
        public void Tooltip_NoType_IsNull() =>
            Assert.IsNull(TypeSelectorHelpers.GetTypeSelectorTooltip(null));

        [Test]
        public void Tooltip_SpellsTheFullIdentity()
        {
            var assembly = typeof(List<int>).Assembly.GetName().Name;
            Assert.AreEqual($"System.Collections.Generic.List<Int32>, {assembly}",
                TypeSelectorHelpers.GetTypeSelectorTooltip(typeof(List<int>)));
        }

        [Test]
        public void Tooltip_NestedType_KeepsTheOuterTypesWithTheirArguments()
        {
            var assembly = typeof(Host<>).Assembly.GetName().Name;
            var prefix = $"{typeof(TypeSelectorHelpersTests).Namespace}.{nameof(TypeSelectorHelpersTests)}";

            Assert.AreEqual($"{prefix}.Host<T>.Nested, {assembly}",
                TypeSelectorHelpers.GetTypeSelectorTooltip(typeof(Host<>.Nested)));
            Assert.AreEqual($"{prefix}.Host<Int32>.Nested, {assembly}",
                TypeSelectorHelpers.GetTypeSelectorTooltip(typeof(Host<int>.Nested)));
        }
    }
}
