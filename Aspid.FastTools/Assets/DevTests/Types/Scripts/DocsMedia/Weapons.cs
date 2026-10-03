// Docs-media harness: the weapon hierarchy shown in the Documentation/02-serializable-types.md pickers.
// Lives in DevTests, but uses a neutral game-like namespace because the picker
// breadcrumbs (and thus the namespace) are visible in the recorded media.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    public interface ITwoHanded { }

    public abstract class Weapon { }

    public abstract class MeleeWeapon : Weapon { }

    public abstract class RangedWeapon : Weapon { }

    public sealed class Sword : MeleeWeapon { }

    public sealed class Axe : MeleeWeapon, ITwoHanded { }

    public sealed class Bow : RangedWeapon, ITwoHanded { }
}
