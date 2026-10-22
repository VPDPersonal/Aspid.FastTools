using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools.Effects
{
    [TypeSelectorDisplay(Name = "Shield", Group = "Effects", Icon = "d_SphereCollider Icon")]
    public sealed class ShieldEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) =>
            "absorbs the next hit";
    }
}
