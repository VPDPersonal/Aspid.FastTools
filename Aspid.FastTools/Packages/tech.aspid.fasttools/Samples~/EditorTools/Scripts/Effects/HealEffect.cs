using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    /// <summary>
    /// <see cref="IAbilityEffect"/> that describes healing with a mana cost.
    /// </summary>
    [TypeSelectorDisplay(
        Name = "Heal",
        Group = "Effects",
        Tooltip = "Restores health for the mana it costs",
        Icon = "d_Light Icon")]
    public sealed class HealEffect : IAbilityEffect
    {
        /// <inheritdoc/>
        public string Describe(AbilityConfig ability) => $"restores health for {ability.ManaCost} MP";
    }
}
