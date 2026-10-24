# EditorTools Sample

An editor window built in code with the package's helpers.

![The Reset to defaults button returns the cost and the cooldown to their defaults, and Undo restores them.](Images/demo.gif)

The Reset to defaults button returns the cost and the cooldown to their defaults, and Undo restores them.

## Open it

1. Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** on **EditorTools**. It has no scene.
2. Open **Tools → Aspid 🐍 → FastTools → Samples → Ability Catalog**: the abilities are on the left, the selected one on the right.

## Try

1. **A ListView in one chain.** The list is one chain of <code lang="class-name">ListView</code> extensions. Type in the search field: the list filters.
2. **Binding.** The fields are bound with <code lang="csharp">.BindTo(serializedObject)</code>. Edit the name: the list and the title follow.
3. **Property setters.** Select **Fireball** and press **Reset to defaults**: **Mana Cost** and **Cooldown** are applied once, so one Undo reverts both.

   ```csharp
   serializedObject.FindProperty("_cooldown")
       .Update()
       .SetFloat(AbilityConfig.DefaultCooldown);
   serializedObject.FindProperty("_manaCost")
       .SetIntAndApply(AbilityConfig.DefaultManaCost);
   ```

4. **The type picker from code.** Click **Effect**: <code lang="csharp">TypeSelectorWindow.Show()</code> opens the <code lang="csharp">[TypeSelector]</code> picker with the <code lang="class-name">IAbilityEffect</code> classes only.
5. **Open the script.** Double-click the ability title: <code lang="function">AddOpenScriptCommand</code> opens `AbilityConfig.cs`. The title's tooltip names the script with <code lang="csharp">GetDisplayName()</code>.
6. **The FastTools theme.** Pick another **Theme** in **Tools → Aspid 🐍 → FastTools → Settings** → **Appearance**: <code lang="function">AddAspidWindowTheme</code> recolors the catalog at once.

## Where to look

| File | Shows |
|---|---|
| `Scripts/Editor/AbilityCatalogWindow.cs` | Every call above |
| `Styles/` | Colors from the theme's `--aspid-colors-*` tokens |
| `Scripts/Effects/` | <code lang="csharp">[TypeSelectorDisplay]</code> names, a group and icons |

Reference: [VisualElement Extensions](../../docs/10-visual-element-extensions.md), [SerializedProperty Extensions](../../docs/11-serialized-property-extensions.md), [Editor Helpers](../../docs/12-editor-helpers.md) and [TypeSelectorWindow](../../docs/03-type-selector.md#typeselectorwindow).
