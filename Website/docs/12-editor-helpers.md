# Editor Helpers

Object labels without “(Script)” and numbering for components of the same type.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <code lang="csharp">ObjectNames.GetInspectorTitle(caster)</code> | <code lang="csharp">caster.GetDisplayName()</code> |
| <code lang="string">Ability Caster (Script)</code> | <code lang="string">Ability Caster</code> |

## GetDisplayName()

Returns a readable type name for any <code lang="class-name">UnityEngine.Object</code>, rather than the value of <code lang="csharp">Object.name</code>.

| <code lang="csharp">[AddComponentMenu]</code> | <code lang="csharp">GetInspectorTitle()</code> | <code lang="csharp">GetDisplayName()</code> |
|---|---|---|
| none | <code lang="string">Ability Caster (Script)</code> | <code lang="string">Ability Caster</code> |
| <code lang="csharp">"Gameplay/Ability"</code> on <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability</code> | <code lang="string">Ability</code> |

## GetDisplayNameWithIndex()

Adds a number when a GameObject has multiple components of the same type. Numbering starts at 1 and follows component order.

| Components on the GameObject | Labels |
|---|---|
| <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster</code> |
| <code lang="class-name">AbilityCaster</code>, <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster (1)</code>, <code lang="string">Ability Caster (2)</code> |
| <code lang="class-name">AbilityCaster</code> and its subclass <code lang="class-name">FireCaster</code> | <code lang="string">Ability Caster</code>, <code lang="string">Fire Caster</code> |

Both methods return <code lang="csharp">string.Empty</code> for a <code lang="csharp">null</code> or destroyed object.

## Package sample

In [EditorTools](../tutorials/EditorTools/README.md), <code lang="csharp">GetDisplayName()</code> titles the custom inspector of the <code lang="class-name">AbilityConfig</code> asset.

![The Ability Catalog window from the EditorTools sample](../tutorials/EditorTools/Images/demo.gif)
