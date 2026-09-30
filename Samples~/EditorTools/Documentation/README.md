# EditorTools Sample

An editor window and an Inspector built in code with the package's helpers.

![The Halve cooldown, +5 MP button changes the cost and the cooldown, and Undo restores them.](Images/demo.gif)

The Halve cooldown, +5 MP button changes the cost and the cooldown, and Undo restores them.

## Open it

1. Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** on **EditorTools**. It has no scene.
2. Open **Tools → Aspid 🐍 → FastTools → Samples → Ability Catalog**: the left pane lists the project's abilities, the four assets from `Data/` among them, and the right pane shows the selected one.

## Try

1. **A ListView in one chain.** The list on the left is one chain of <code lang="class-name">ListView</code> extensions, <code lang="function">SetMakeItem</code>, <code lang="function">SetBindItem</code> and <code lang="function">AddSelectionChanged</code> included. Type in the search field: <code lang="function">AddValueChanged</code> filters the source, and <code lang="function">RefreshItems</code> redraws the list.
2. **Binding.** The name, description, cost and cooldown fields sit in one container bound with <code lang="csharp">.BindTo(serializedObject)</code>. Edit the name: the list and the title follow, and Undo restores it.
3. **Typed property setters.** Press **Halve cooldown, +5 MP**. The handler writes two properties and applies them once, so a single Undo reverts both:

   ```csharp
   serializedObject.Update();
   var cooldown = serializedObject.FindProperty("_cooldown");
   var manaCost = serializedObject.FindProperty("_manaCost");
   cooldown.SetFloat(cooldown.floatValue * 0.5f);
   manaCost.SetIntAndApply(manaCost.intValue + 5);
   ```

4. **The type picker from code.** Press **Change…** next to **Effect**: <code lang="csharp">TypeSelectorWindow.Show()</code> opens the picker of <code lang="csharp">[TypeSelector]</code> at the button, limited to <code lang="class-name">IAbilityEffect</code> implementations, and writes the pick into a string property. Pick <code lang="class-name">HealEffect</code> and change **Mana Cost**: the effect description follows the value, through Undo and Redo too.
5. **Open the script.** Double-click the ability title in the right pane: <code lang="function">AddOpenScriptCommand</code> opens `AbilityConfig.cs` in your IDE.
6. **The inspector.** Select `Data/Sprint.asset` in the Project window. <code lang="class-name">AbilityConfigEditor</code> draws a card titled with <code lang="csharp">GetDisplayName()</code>, “Ability Config”, a cost badge and a warning shown while **Mana Cost** is `0`; <code lang="csharp">PropertyField.AddValueChanged()</code> drives both. Set the cost to `10` and back.
7. **Create.** Press **Create**: a new asset appears next to the selected one, already selected in the list.

## Where to look

| File | Shows |
|---|---|
| `Scripts/Editor/AbilityCatalogWindow.cs` | <code lang="class-name">ListView</code> extensions, <code lang="function">BindTo</code>, <code lang="function">SetFloat</code> and <code lang="function">SetIntAndApply</code>, <code lang="csharp">TypeSelectorWindow.Show()</code> with a <code lang="class-name">TypeSelectorFilter</code>, <code lang="function">AddOpenScriptCommand</code> |
| `Scripts/Editor/AbilityConfigEditor.cs` | A custom inspector on the style and layout setters, <code lang="function">GetDisplayName</code> |
| `Scripts/AbilityConfig.cs` | The data; <code lang="csharp">[TypeSelector]</code> on the effect string gives the asset's Inspector the same picker |
| `Scripts/Effects/` | The effect classes the picker offers |

Reference: [VisualElement Extensions](../../../Documentation/07-visual-element-extensions.md), [SerializedProperty Extensions](../../../Documentation/08-serialized-property-extensions.md), [Editor Helpers](../../../Documentation/09-editor-helpers.md) and [TypeSelectorWindow](../../../Documentation/02-serializable-types.md#typeselectorwindow).
