using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.EditorTools
{
    [TypeSelectorDisplay(Name = "Damage", Group = "Effects", Icon = "d_ParticleSystem Icon")]
    public sealed class DamageEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) => $"deals damage every {ability.Cooldown:0.#}s";
    }
}
