// Docs-media harness: user-facing weapon hierarchy shown in the Types.md pickers.
// Lives in DevTests, but uses a neutral game-like namespace because the picker
// breadcrumbs (and thus the namespace) are visible in the recorded media.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    public abstract class Weapon { }

    public abstract class MeleeWeapon : Weapon { }

    public abstract class RangedWeapon : Weapon { }

    public sealed class Sword : MeleeWeapon { }

    public sealed class Axe : MeleeWeapon { }

    public sealed class Spear : MeleeWeapon { }

    public sealed class Bow : RangedWeapon { }

    public sealed class Crossbow : RangedWeapon { }

    public sealed class Pistol : RangedWeapon { }
}
