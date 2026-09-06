// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Root of the polymorphic hierarchy. A [SerializeReference] [TypeSelector] field of this type offers every
    // concrete implementation below; IMelee / IRanged exist so a field can be narrowed to one branch.
    public interface IWeapon
    {
        string Name { get; }

        // Damage dealt by one shot.
        int Fire();
    }
}
