using System.Linq;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // Contract plus one offered and one opted-out implementation — the smallest shape that shows a hidden type
    // being dropped while its sibling survives the same scan.
    internal interface IPickerFilterContract { }

    [System.Serializable]
    internal sealed class OfferedPickerType : IPickerFilterContract { }

    [System.Serializable]
    [TypeSelectorDisplay(Hidden = true)]
    internal sealed class HiddenPickerType : IPickerFilterContract { }

    [System.Serializable]
    [TypeSelectorDisplay(Hidden = true)]
    internal sealed class HiddenPickerBox<T> : IPickerFilterContract { }

    // Hidden is not inherited, so a subclass of a hidden type is still offered.
    [System.Serializable]
    internal sealed class OfferedSubtypeOfHidden : IPickerFilterContract { }

    /// <summary>
    /// Coverage for <see cref="TypeSelectorDisplayAttribute.Hidden"/> — a type that opts out must not reach the
    /// picker through either path <see cref="TypeInfo.GetAllTypeInfos"/> feeds it: the domain scan or the
    /// verbatim <c>additionalTypes</c> injection used for open generic definitions.
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorPickerFilterTests
    {
        private static string[] ScanNames(params System.Type[] additionalTypes) =>
            TypeInfo.GetAllTypeInfos(new[] { typeof(IPickerFilterContract) }, TypeAllow.None, additionalTypes: additionalTypes)
                .Select(info => info.Name)
                .ToArray();

        [Test]
        public void GetAllTypeInfos_HiddenType_IsNotOffered()
        {
            var names = ScanNames();

            CollectionAssert.Contains(names, nameof(OfferedPickerType),
                "A plain candidate must still be offered.");
            CollectionAssert.DoesNotContain(names, nameof(HiddenPickerType),
                "A type marked [TypeSelectorDisplay(Hidden = true)] must not be offered.");
        }

        [Test]
        public void GetAllTypeInfos_HiddenAdditionalType_IsNotOffered()
        {
            // Open generic definitions bypass the scan's own checks, so the opt-out has to hold on this path too.
            var names = ScanNames(typeof(HiddenPickerBox<>));

            CollectionAssert.DoesNotContain(names, TypeUtility.FormatGenericName(typeof(HiddenPickerBox<>)),
                "An injected additional type marked Hidden must not be offered either.");
        }

        [Test]
        public void IsHidden_IsNotInherited()
        {
            Assert.IsTrue(TypeInfo.IsHidden(typeof(HiddenPickerType)));
            Assert.IsFalse(TypeInfo.IsHidden(typeof(OfferedSubtypeOfHidden)),
                "Hiding a type must never hide the types meant to be picked instead of it.");
        }
    }
}
