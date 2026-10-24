# SerializeReferences Sample

A turret whose weapons and their effects are picked in the Inspector.

![The dummy changes color and shrinks as the configured weapons and effects deal damage.](Images/demo.gif)

The dummy changes color and shrinks as the configured weapons and effects deal damage.

## Open it

1. Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** on **SerializeReferences**.
2. Open `Scenes/SerializeReferences.unity` and enter Play Mode: the primary weapon and the sidearms take turns hitting the dummy.

## Try

Exit Play Mode and select **Loadout**.

1. **Pick a class.** Open the **Primary** dropdown: the picker lists every <code lang="class-name">IWeapon</code> class, and the picked class shows its fields under the dropdown.
2. **Values survive a switch.** In **Sidearms**, set the <code lang="class-name">Pistol</code>'s **Damage** to `37`, switch it to <code lang="class-name">Shotgun</code> and back: **Damage** is still `37`, because both classes have `_damage`.
3. **Lists.** Press **+** on **Sidearms**: the picker opens, and the new element gets its own instance.
4. **Nested references.** The <code lang="class-name">Railgun</code> in **Primary** has its own <code lang="csharp">[SerializeReference]</code> field, **Charge Effect**, with a <code lang="class-name">BurnEffect</code>: the dummy catches fire when the railgun hits.
5. **Abstract base.** **On Hit** is a <code lang="class-name">StatusEffect</code>: the picker offers <code lang="class-name">BurnEffect</code> and <code lang="class-name">FreezeEffect</code>, never the abstract class.
6. **Header menu.** Right-click a field header for copy, paste and more: [every item](../../docs/04-serialize-reference-selector.md#header-menu).

## Where to look

| File | Shows |
|---|---|
| `Scripts/Loadout.cs` | <code lang="csharp">[TypeSelector]</code> on a single field, a list and an abstract base |
| `Scripts/Weapons/Railgun.cs` | A nested <code lang="csharp">[SerializeReference]</code> |

Reference: [SerializeReference Selector](../../docs/04-serialize-reference-selector.md).
