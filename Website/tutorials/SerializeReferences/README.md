# SerializeReferences Sample

A turret whose weapons and their effects are picked in the Inspector, plus assets broken on purpose for you to repair.

![The dummy changes color and shrinks as the configured weapons and effects deal damage.](Images/demo.gif)

The dummy changes color and shrinks as the configured weapons and effects deal damage.

## Open it

1. Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** on **SerializeReferences**.
2. Open `Scenes/SerializeReferences.unity` and enter Play Mode: the primary weapon and the sidearms take turns hitting the dummy, and the Console reports every hit.

The scene's colliders need Unity's built-in **Physics** module; the sample scripts compile without it.

## Try

Exit Play Mode and select **Loadout**.

![Weapons, a nested burn effect, and modifiers in the Loadout Inspector.](Images/weapon-fields.png)

Weapons, a nested burn effect, and modifiers in the Loadout Inspector.

1. **Pick a class.** Open the **Primary** dropdown: a searchable picker lists every concrete <code lang="class-name">IWeapon</code>, grouped under **Weapons/Melee** and **Weapons/Ranged** by <code lang="csharp">[TypeSelectorDisplay]</code>. <code lang="class-name">DebugWeapon</code> is left out with `Hidden`. Pick <code lang="class-name">Shotgun</code>: its fields appear under the dropdown.
2. **Shared data survives a switch.** In **Sidearms**, set the <code lang="class-name">Pistol</code>'s **Damage** to `37`, switch it to <code lang="class-name">Shotgun</code> and back: **Damage** is still `37`, because both classes declare `_damage`.
3. **Lists.** Press **+** on **Sidearms**: the picker opens instead of copying the last element, and the new element gets its own instance. In Play Mode it joins the firing rotation.
4. **Narrowing.** **Melee Backup** is declared <code lang="class-name">IWeapon</code>, but <code lang="csharp">[TypeSelector(typeof(IMelee))]</code> offers only <code lang="class-name">Sword</code>. The **Weapon** field of each **Holster** slot is narrowed the same way one level down, inside a plain <code lang="csharp">[Serializable]</code> class: <code lang="csharp">[TypeSelector(typeof(IRanged))]</code> offers only ranged weapons. Neither field joins the firing rotation.
5. **Nested references.** The <code lang="class-name">Railgun</code> in **Primary** has a **Charge Effect**, a <code lang="csharp">[SerializeReference]</code> of its own with its own dropdown. It holds a <code lang="class-name">BurnEffect</code>, so the dummy catches fire when the railgun hits.
6. **Abstract base.** **On Hit** is a <code lang="class-name">StatusEffect</code>: the picker offers <code lang="class-name">BurnEffect</code> and <code lang="class-name">FreezeEffect</code>, never the abstract class.
7. **Generics.**
   - **Damage Modifier** is a <code lang="class-name">Modifier&lt;float&gt;</code>: <code lang="class-name">DamageModifier</code> and <code lang="class-name">Modifier&lt;Single&gt;</code> are offered and created at once.
   - **Perks** is a <code lang="class-name">List&lt;IModifier&gt;</code>: besides the closed subclasses it offers the open <code lang="class-name">Modifier&lt;T&gt;</code>, which asks for <code lang="class-name">T</code> on a second page.
   - Only <code lang="class-name">DamageModifier</code> changes the damage; **Loadout → Log Loadout** in the component's context menu prints the values of the others.
8. **Required field.** Set **Primary** to `<None>`: a notice appears under the field. Save the scene, and **Project References → Scan Project** and CI runs with `-srGateRequired` report the field as a violation; a player build does not check it ([what each run checks](../../docs/07-serialize-reference-validation.md#what-each-run-checks)).
9. **Header menu.** Right-click a field header for copy and paste, templates, usages and a new script: [every item](../../docs/04-serialize-reference-selector.md#header-menu).

## Repair

The assets in `Presets/` and `Prefabs/` store types that are missing or out of date.

1. **Fix one asset.** Select `Presets/BrokenWeaponPreset.asset`: **Weapon** stores a <code lang="class-name">GhostWeapon</code> that does not exist, and the **Missing type** notice under the field offers **Fix**. Press it and pick <code lang="class-name">Pistol</code>: **Damage** `25` and **Magazine Size** `8` are kept.
2. **Fix a group.** Open **Tools → Aspid 🐍 → FastTools → Project References** and press **Scan Project**: the three <code lang="class-name">GhostWeapon</code> entries of `BrokenArsenalPreset.asset` form one group, and **Fix all** repairs them at once.
3. **A suggested class.** `Presets/MovedWeaponPreset.asset` stores <code lang="class-name">Pistol</code> under an old namespace. Its notice offers **→ Pistol**, and its group in Project References offers **Smart Fix → Pistol**; neither applies itself.
4. **A migration.** `Presets/RenamedWeaponPreset.asset` stores <code lang="class-name">CrossbowLauncher</code>, and the class is now <code lang="class-name">Crossbow</code> with <code lang="csharp">[MovedFrom]</code>: the Inspector already shows <code lang="class-name">Crossbow</code>, only the file is out of date. **Migrate all → Crossbow** in Project References writes the new name into the file.
5. **A prefab.** Select `Prefabs/BrokenLoadout.prefab` in the Project window. `Sidearms[2]` is a missing <code lang="class-name">GhostCrossbow</code> with **Fix**; `Sidearms[0]` and `[1]` point at one <code lang="class-name">Pistol</code> and are marked **Shared reference #N**, and **Make unique** gives an element its own copy. The **Asset References** tab lists every reference of the prefab.

To repeat the repair, import the sample again and overwrite its files.

## IMGUI inspector

<code lang="class-name">WeaponPreset</code> has an IMGUI inspector, `Scripts/Editor/WeaponPresetEditor.cs`: plain <code lang="csharp">EditorGUILayout.PropertyField()</code> calls draw the field and the list, with the picker, **Fix** and the list's **+** working as in UI Toolkit. For fields without <code lang="csharp">[TypeSelector]</code>, see [Custom inspector](../../docs/04-serialize-reference-selector.md#custom-inspector).

## Where to look

| File | Shows |
|---|---|
| `Scripts/Loadout.cs` | Every field shape: single, list, narrowed, container, abstract base, closed and open generics, `Required` |
| `Scripts/Weapons/` | The <code lang="class-name">IWeapon</code> hierarchy, <code lang="csharp">[TypeSelectorDisplay]</code> groups, a `Hidden` class, <code lang="csharp">[MovedFrom]</code> on <code lang="class-name">Crossbow</code>, a nested reference in <code lang="class-name">Railgun</code> |
| `Scripts/Effects/`, `Scripts/Modifiers/` | An abstract base and an open generic class |
| `Scripts/WeaponPreset.cs`, `Presets/`, `Prefabs/` | The repair scenarios |
| `Scripts/Editor/WeaponPresetEditor.cs` | An IMGUI inspector of plain <code lang="class-name">PropertyField</code> calls |

Reference: [SerializeReference Selector](../../docs/04-serialize-reference-selector.md) and [SerializeReference repair](../../docs/06-serialize-reference-tooling.md).
