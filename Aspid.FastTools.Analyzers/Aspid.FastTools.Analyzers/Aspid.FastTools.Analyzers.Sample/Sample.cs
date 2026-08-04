// Demonstrates [TypeSelector] usages the analyzer accepts. The analyzer is referenced as an Analyzer, so building
// this project runs AFT0001–AFT0005 against the code below — it stays clean because every usage is valid.
//
// Minimal stand-ins for the real attributes (this sample references neither Unity nor the package).
namespace UnityEngine
{
    public class Object { }
    // Unity declares SerializeReference without the "Attribute" suffix — the analyzer matches by that display name.
    public sealed class SerializeReference : System.Attribute { }
}

namespace Aspid.FastTools.Types
{
    [System.Flags] public enum TypeAllow { None = 0, Abstract = 1, Interface = 2, All = 3 }

    public sealed class TypeSelectorAttribute : System.Attribute
    {
        public TypeSelectorAttribute() { }
        public TypeSelectorAttribute(System.Type type) { }
        public TypeSelectorAttribute(params System.Type[] types) { }
        public TypeAllow Allow { get; set; }
    }
}

namespace Aspid.FastTools.Analyzers.Sample
{
    using UnityEngine;
    using Aspid.FastTools.Types;
    using System.Collections.Generic;

    public interface IWeapon { }
    public interface IMelee : IWeapon { }

    // Concrete implementations so AFT0005 does not fire — the picker has at least one candidate for IWeapon/IMelee.
    public class Sword : IMelee { }
    public class Bow : IWeapon { }

    public sealed class Loadout
    {
        // String type-name picker: Allow may opt in abstract/interface types because a Type is named, not instantiated.
        [TypeSelector(typeof(IWeapon), Allow = TypeAllow.Interface)]
        private string _weaponTypeName;

        // Managed reference: candidates default to the field's declared type.
        [SerializeReference, TypeSelector]
        private IWeapon _primary;

        // Managed reference narrowed by a base type assignable to the field type.
        [SerializeReference, TypeSelector(typeof(IMelee))]
        private IWeapon _sidearm;

        // Collections of managed references are supported too.
        [SerializeReference, TypeSelector]
        private List<IWeapon> _stash;
    }
}
