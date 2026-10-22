using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools.Effects
{
    [TypeSelectorDisplay(Name = "Heal", Group = "Effects", Icon = "d_Light Icon")]
    public sealed class HealEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) =>
            $"restores health for {ability.ManaCost} MP";
    }
}
