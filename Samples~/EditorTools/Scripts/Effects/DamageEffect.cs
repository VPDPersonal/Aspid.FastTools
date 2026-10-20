using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    /// <summary>
    /// <see cref="IAbilityEffect"/> that describes periodic damage.
    /// </summary>
    [TypeSelectorDisplay(
        Name = "Damage",
        Group = "Effects",
        Tooltip = "Deals damage every cooldown",
        Icon = "d_ParticleSystem Icon")]
    public sealed class DamageEffect : IAbilityEffect
    {
        /// <inheritdoc/>
        public string Describe(AbilityConfig ability) =>
            $"deals damage every {ability.Cooldown:0.#}s";
    }
}
