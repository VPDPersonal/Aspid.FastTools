using System;
using NUnit.Framework;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    [Serializable]
    internal sealed class UsagePistol { }

    [Serializable]
    internal sealed class UsageHolster<T> { }

    [Serializable]
    [TypeSelectorDisplay(Name = "Side/Arm")]
    internal sealed class UsageSidearm<T> { }

    /// <summary>
    /// Locks <b>Find Usages of &lt;Class&gt;</b> to the exact stored identity: the query
    /// <see cref="SerializeReferenceUsageSearchProvider.QueryFor"/> writes matches the type's own
    /// <see cref="ManagedTypeName"/> and nothing that merely shares its class name, while a free-typed <c>sr:</c>
    /// query keeps matching class names by substring. Its label names the type as the picker does.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceUsageSearchTests
    {
        private static readonly ManagedTypeName Pistol = ManagedTypeName.FromType(typeof(UsagePistol));

        [Test]
        public void MenuQuery_MatchesTheExactType() =>
            Assert.IsTrue(MenuQueryMatches(typeof(UsagePistol), Pistol));

        [Test]
        public void MenuQuery_SkipsLongerClassNames() =>
            Assert.IsFalse(MenuQueryMatches(typeof(UsagePistol), WithClass($"{Pistol.Class}Mk2")));

        [Test]
        public void MenuQuery_SkipsNamesakesFromOtherNamespaces() =>
            Assert.IsFalse(MenuQueryMatches(typeof(UsagePistol), new ManagedTypeName(
                assembly: Pistol.Assembly,
                @namespace: "Other.Weapons",
                className: Pistol.Class)));

        [Test]
        public void MenuQuery_SkipsNamesakesFromOtherAssemblies() =>
            Assert.IsFalse(MenuQueryMatches(typeof(UsagePistol), new ManagedTypeName(
                assembly: "Other.Assembly",
                @namespace: Pistol.Namespace,
                className: Pistol.Class)));

        [Test]
        public void MenuQuery_SeparatesGenericInstantiations()
        {
            var intHolster = ManagedTypeName.FromType(typeof(UsageHolster<int>));
            var longHolster = ManagedTypeName.FromType(typeof(UsageHolster<long>));

            Assert.IsTrue(MenuQueryMatches(typeof(UsageHolster<int>), intHolster));
            Assert.IsFalse(MenuQueryMatches(typeof(UsageHolster<int>), longHolster));
        }

        [Test]
        public void FreeTypedQuery_MatchesClassNamesBySubstring()
        {
            var token = SerializeReferenceUsageSearchProvider.Token("sr:usagepist");

            Assert.IsTrue(SerializeReferenceUsageSearchProvider.Matches(token, Pistol));
            Assert.IsTrue(SerializeReferenceUsageSearchProvider.Matches(token, WithClass($"{Pistol.Class}Mk2")));
        }

        [Test]
        public void EmptyQuery_MatchesNothing() =>
            Assert.IsFalse(SerializeReferenceUsageSearchProvider.Matches(
                SerializeReferenceUsageSearchProvider.Token("sr:  "), Pistol));

        [Test]
        public void MenuLabel_FormatsGenericArguments() =>
            Assert.AreEqual("Find Usages of UsageHolster<Int32>",
                SerializeReferenceHelpers.GetFindUsagesMenuLabel(typeof(UsageHolster<int>)));

        [Test]
        public void MenuLabel_KeepsDisplayNameSlashOutOfSubmenus() =>
            Assert.AreEqual("Find Usages of Side\u2215Arm<Int32>",
                SerializeReferenceHelpers.GetFindUsagesMenuLabel(typeof(UsageSidearm<int>)));

        private static bool MenuQueryMatches(Type type, ManagedTypeName storedType)
        {
            var query = SerializeReferenceUsageSearchProvider.QueryFor(type);
            return SerializeReferenceUsageSearchProvider.Matches(SerializeReferenceUsageSearchProvider.Token(query), storedType);
        }

        private static ManagedTypeName WithClass(string className) =>
            new(assembly: Pistol.Assembly, @namespace: Pistol.Namespace, className: className);
    }
}
