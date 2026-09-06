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

    // Hidden is not inherited: the subclass really derives from the hidden type, so the scan below proves the
    // opt-out stops at the type that declared it rather than passing down the hierarchy.
    [System.Serializable]
    [TypeSelectorDisplay(Hidden = true)]
    internal class HiddenPickerBase : IPickerFilterContract { }

    [System.Serializable]
    internal sealed class OfferedSubtypeOfHidden : HiddenPickerBase { }

    internal abstract class AbstractPickerType : IPickerFilterContract { }

    internal static class StaticPickerHolder
    {
        // Nested so the static class sits next to the contract it would otherwise never implement.
        internal static class StaticPickerType { }
    }

    /// <summary>
    /// Coverage for <see cref="TypeSelectorDisplayAttribute.Hidden"/> — a type that opts out must not reach an
    /// authoring picker through either path <see cref="TypeInfo.GetAllTypeInfos"/> feeds it (the domain scan or the
    /// verbatim <c>additionalTypes</c> injection used for open generic definitions), while a repair picker asking
    /// for <c>includeHidden</c> still sees it, or data already holding that type could never be re-pointed.
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorPickerFilterTests
    {
        private static string[] ScanNames(params System.Type[] additionalTypes) =>
            Scan(includeHidden: false, additionalTypes);

        private static string[] Scan(bool includeHidden, params System.Type[] additionalTypes) =>
            TypeInfo.GetAllTypeInfos(new[] { typeof(IPickerFilterContract) }, TypeAllow.None,
                    additionalTypes: additionalTypes, includeHidden: includeHidden)
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
        public void GetAllTypeInfos_IncludeHidden_OffersHiddenTypesOnBothPaths()
        {
            // What a repair picker sees: the same scan, minus the opt-out, on the scan and the injected path alike.
            var names = Scan(includeHidden: true, typeof(HiddenPickerBox<>));

            CollectionAssert.Contains(names, nameof(HiddenPickerType),
                "A repair picker must be able to re-point a reference to the hidden type it already holds.");
            CollectionAssert.Contains(names, TypeUtility.FormatGenericName(typeof(HiddenPickerBox<>)),
                "The injected path must lift the opt-out with the scan, or the two disagree on the same picker.");
        }

        [Test]
        public void GetAllTypeInfos_HiddenBase_StillOffersItsSubtype()
        {
            var names = ScanNames();

            CollectionAssert.DoesNotContain(names, nameof(HiddenPickerBase));
            CollectionAssert.Contains(names, nameof(OfferedSubtypeOfHidden),
                "Hiding a type must never hide the types meant to be picked instead of it.");
        }

        [Test]
        public void IsHiddenFromPicker_IsNotInherited()
        {
            Assert.IsTrue(TypeSelectorHelpers.IsHiddenFromPicker(typeof(HiddenPickerBase)));
            Assert.IsFalse(TypeSelectorHelpers.IsHiddenFromPicker(typeof(OfferedSubtypeOfHidden)),
                "The attribute is declared Inherited = false, so a subclass must read as visible.");
        }

        [Test]
        public void HiddenAttribute_AppliesToInterfaces()
        {
            // TypeAllow.Interface exists to offer interfaces, so the opt-out has to be expressible on one —
            // AttributeTargets.Class does not cover interfaces, and omitting the flag makes this a compile error.
            Assert.IsTrue(TypeSelectorHelpers.IsHiddenFromPicker(typeof(IHiddenPickerContract)));
        }
    }

    [TypeSelectorDisplay(Hidden = true)]
    internal interface IHiddenPickerContract {

        [Test]
        public void GetAllTypeInfos_StaticClass_IsNeverOffered_EvenWithAbstractAllowed()
        {
            var names = TypeInfo.GetAllTypeInfos(System.Array.Empty<System.Type>(), TypeAllow.All)
                .Select(info => info.Name)
                .ToArray();

            CollectionAssert.DoesNotContain(names, nameof(StaticPickerHolder));
            CollectionAssert.DoesNotContain(names, nameof(StaticPickerHolder.StaticPickerType));
            CollectionAssert.Contains(names, nameof(AbstractPickerType), "A real abstract class must survive with TypeAllow.Abstract.");
        }
    }
}
