using Aspid.FastTools.Types;

// Docs-media harness for Images/aspid_fasttools_type_selector_display.png in
// Documentation/02-serializable-types.md. DamageModifier mirrors the attribute example in that
// page; KnockbackModifier stays undecorated (group only) to contrast a custom name/icon row with
// a default one. ModifierRack hosts the field the screenshot's picker is opened from.

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
