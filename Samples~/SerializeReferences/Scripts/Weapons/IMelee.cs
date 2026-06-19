// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Marker sub-interface used by the tutorial's "narrowing" step.
    //
    // A field declared as IWeapon normally lists every IWeapon implementation. Annotating it with
    // [TypeSelector(typeof(IMelee))] narrows that same field to the melee branch only (Sword) — the
    // attribute's base type is applied as an extra filter BELOW the declared field type.
    public interface IMelee : IWeapon { }
}
