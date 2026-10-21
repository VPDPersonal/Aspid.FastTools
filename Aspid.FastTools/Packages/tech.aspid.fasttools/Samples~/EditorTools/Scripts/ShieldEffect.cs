using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.EditorTools
{
    [TypeSelectorDisplay(Name = "Shield", Group = "Effects", Icon = "d_SphereCollider Icon")]
    public sealed class ShieldEffect : IAbilityEffect
    {
        public string Describe(AbilityConfig ability) => "absorbs the next hit";
    }
}
