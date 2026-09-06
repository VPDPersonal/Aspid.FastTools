// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    public sealed class HealEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) => $"restores health for {ability.ManaCost} MP";
    }
}
