// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    // The catalog opens TypeSelectorWindow with this interface as its constraint.
    /// <summary>
    /// Defines a description of an ability effect.
    /// </summary>
    public interface IAbilityEffect
    {
        /// <summary>
        /// Returns a description of the configured effect.
        /// </summary>
        /// <param name="ability">Ability settings used in the description.</param>
        /// <returns>Effect description.</returns>
        string Describe(AbilityConfig ability);
    }
}
