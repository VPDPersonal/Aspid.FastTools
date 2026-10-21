using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.EditorTools
{
    [TypeSelectorDisplay(Name = "Heal", Group = "Effects", Icon = "d_Light Icon")]
    public sealed class HealEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) => $"restores health for {ability.ManaCost} MP";
    }
}
