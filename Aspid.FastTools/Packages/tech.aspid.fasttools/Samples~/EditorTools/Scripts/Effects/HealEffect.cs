// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    /// <summary>
    /// <see cref="IAbilityEffect"/> that describes healing with a mana cost.
    /// </summary>
    public sealed class HealEffect : IAbilityEffect
    {
        /// <inheritdoc/>
        public string Describe(AbilityConfig ability) => $"restores health for {ability.ManaCost} MP";
    }
}
