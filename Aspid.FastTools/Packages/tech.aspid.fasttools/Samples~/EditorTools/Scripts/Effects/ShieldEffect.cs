using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EditorTools
{
    /// <summary>
    /// <see cref="IAbilityEffect"/> that describes protection from the next hit.
    /// </summary>
    [TypeSelectorDisplay(
        Name = "Shield",
        Group = "Effects",
        Tooltip = "Absorbs the next hit",
        Icon = "d_SphereCollider Icon")]
    public sealed class ShieldEffect : IAbilityEffect
    {
        /// <inheritdoc/>
        public string Describe(AbilityConfig ability) => "absorbs the next hit";
    }
}
