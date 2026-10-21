using Aspid.FastTools.Types;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools.Effects
{
    [TypeSelectorDisplay(Name = "Damage", Group = "Effects", Icon = "d_ParticleSystem Icon")]
    public sealed class DamageEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) =>
            $"deals damage every {ability.Cooldown:0.#}s";
    }
}
