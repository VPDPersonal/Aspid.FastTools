using Aspid.FastTools.Types;

// Docs-media harness: [TypeSelectorDisplay] demo types shown in the Types.md picker screenshot.
// DamageModifier mirrors the attribute example in the docs; KnockbackModifier stays undecorated
// (group only) to contrast a custom name/icon row with a default one.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    public abstract class CombatModifier { }

    [TypeSelectorDisplay(
        Name = "Damage ×",
        Group = "Combat/Modifiers",
        Tooltip = "Scales incoming damage",
        Icon = "d_ScriptableObject Icon")]
    public sealed class DamageModifier : CombatModifier { }

    [TypeSelectorDisplay(
        Name = "Crit %",
        Group = "Combat/Modifiers",
        Tooltip = "Adds critical-hit chance")]
    public sealed class CritChanceModifier : CombatModifier { }

    [TypeSelectorDisplay(Group = "Combat/Modifiers")]
    public sealed class KnockbackModifier : CombatModifier { }
}
