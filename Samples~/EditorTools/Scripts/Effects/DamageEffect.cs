// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    public sealed class DamageEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) => $"deals damage every {ability.Cooldown:0.#}s";
    }
}
