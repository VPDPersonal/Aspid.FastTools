// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Marker sub-interface used by the tutorial's "narrowing" step.
    //
    // Pistol, Shotgun, Railgun and Crossbow implement it. [TypeSelector(typeof(IRanged))] on an IWeapon
    // field offers only those four; [TypeSelector(typeof(IMelee), typeof(IRanged))] offers both branches
    // (multiple base types are OR-ed), and a bare [TypeSelector] offers every IWeapon.
    public interface IRanged : IWeapon { }
}
