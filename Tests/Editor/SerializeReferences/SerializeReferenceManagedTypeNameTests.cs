using System.Collections.Generic;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Top-level (namespace-scoped) types so the nested-identity test sees exactly "NestingOuter/NestingInner" rather than
    // the test-fixture class prepended to the chain.
    internal sealed class NestingOuter
    {
        internal sealed class NestingInner { }
    }

    /// <summary>
    /// Coverage for <see cref="ManagedTypeName"/> — the YAML type-identity builder used by every repair write. Pins the
    /// closed-generic <c>Name`N[[arg, asm]]</c> shape and the single-quote escaping Unity requires for class identities
    /// containing reserved characters, the write-side half of the generic round-trip (risk register #1).
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceManagedTypeNameTests
    {
        [Test]
        public void FromType_ClosedGeneric_BuildsBacktickName_AndQuotesYaml()
        {
            var name = ManagedTypeName.FromType(typeof(List<float>));

            StringAssert.Contains("List`1[[", name.Class, "A closed generic must use Unity's Name`N[[arg]] class shape.");
            StringAssert.Contains("System.Single", name.Class);
            StringAssert.Contains("{class: '", name.ToYamlType(),
                "A class identity with reserved chars ([ ] , { }) must be single-quoted in the inline mapping.");
        }

        [Test]
        public void ToYamlType_NonGeneric_IsNotQuoted()
        {
            var name = new ManagedTypeName("Asm", "Ns", "Pistol");
            Assert.AreEqual("{class: Pistol, ns: Ns, asm: Asm}", name.ToYamlType());
        }

        [Test]
        public void FromType_Null_IsEmpty()
        {
            Assert.IsTrue(ManagedTypeName.FromType(null).IsEmpty);
        }

        [Test]
        public void FromType_NestedType_JoinsDeclaringChainWithSlash()
        {
            // Unity stores nested managed-reference types as "Outer/Inner"; reflection's Type.Name is only the leaf, so
            // FromType must rebuild the declaring-type prefix or a repair to a nested type writes an unresolvable class.
            var name = ManagedTypeName.FromType(typeof(NestingOuter.NestingInner));
            Assert.AreEqual("NestingOuter/NestingInner", name.Class);
        }
    }
}
