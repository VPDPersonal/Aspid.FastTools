// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    /// <summary>
    /// <see cref="IAbilityEffect"/> that describes periodic damage.
    /// </summary>
    public sealed class DamageEffect : IAbilityEffect
    {
        /// <inheritdoc/>
        public string Describe(AbilityConfig ability) => $"deals damage every {ability.Cooldown:0.#}s";
    }
}
