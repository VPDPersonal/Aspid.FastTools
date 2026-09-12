// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // IMelee and IRanged let a field restrict the picker to one branch of the hierarchy.
    /// <summary>
    /// Defines a named weapon that returns damage for each attack.
    /// </summary>
    public interface IWeapon
    {
        /// <summary>
        /// Gets the display name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns the damage dealt by one attack.
        /// </summary>
        /// <returns>Damage before loadout modifiers and status effects.</returns>
        int Fire();
    }
}
