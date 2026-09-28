using System;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    internal interface IInteractable { }

    internal interface IEffect<T> { }

    // ReSharper disable once InconsistentNaming
    internal interface Idle { }

    internal class ItemBase { }

    internal class GenericBase<T> { }

    /// <summary>
    /// Coverage for the class name that <b>Create New Script…</b> suggests: a single interface prefix and the generic
    /// arity suffix are dropped, so accepting the default gives a valid, readable class name.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceScriptCreatorTests
    {
        [TestCase(typeof(IInteractable), ExpectedResult = "NewInteractable")]
        [TestCase(typeof(IEffect<int>), ExpectedResult = "NewEffect")]
        [TestCase(typeof(IEffect<>), ExpectedResult = "NewEffect")]
        [TestCase(typeof(Idle), ExpectedResult = "NewIdle")]
        [TestCase(typeof(ItemBase), ExpectedResult = "NewItemBase")]
        [TestCase(typeof(GenericBase<int>), ExpectedResult = "NewGenericBase")]
        public string SuggestClassName_StripsOneInterfacePrefixAndArity(Type baseType) =>
            SerializeReferenceScriptCreator.SuggestClassName(baseType);
    }
}
