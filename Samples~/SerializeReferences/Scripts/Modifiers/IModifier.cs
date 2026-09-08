// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Selecting Modifier<T> through this non-generic interface opens a type-argument page.
    /// <summary>
    /// Defines a loadout modifier with a description and damage transformation.
    /// </summary>
    public interface IModifier
    {
        /// <summary>
        /// Returns a description of the modifier.
        /// </summary>
        /// <returns>Modifier description.</returns>
        string Describe();

        /// <summary>
        /// Returns damage after applying the modifier.
        /// </summary>
        /// <param name="damage">Incoming attack damage.</param>
        /// <returns>Modified attack damage.</returns>
        int ModifyDamage(int damage);
    }
}
