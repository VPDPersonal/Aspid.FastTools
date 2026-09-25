# Editor Helpers

Readable object labels for your own editor windows — without “(Script)”, and numbered for identical components.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>new Label(ObjectNames&#10;    .GetInspectorTitle(caster));</code></pre> | <pre lang="csharp"><code>new Label(caster&#10;    .GetDisplayNameWithIndex());</code></pre> |

## GetDisplayName()

Works on any <code lang="class-name">UnityEngine.Object</code>. <code lang="csharp">[AddComponentMenu]</code> counts only when declared on the type itself, not on a base class; a null or destroyed object returns <code lang="csharp">string.Empty</code>.

| <code lang="csharp">[AddComponentMenu]</code> on <code lang="class-name">AbilityCaster</code> | <code lang="csharp">GetInspectorTitle()</code> | <code lang="csharp">GetDisplayName()</code> |
|---|---|---|
| None | <code lang="string">Ability Caster (Script)</code> | <code lang="string">Ability Caster</code> |
| <code lang="csharp">"Gameplay/Ability"</code> | <code lang="string">Ability</code> | <code lang="string">Ability</code> |

## GetDisplayNameWithIndex()

Works on <code lang="class-name">Component</code>. The number is the position among components of exactly the same type on the GameObject — subclasses do not count; a null or destroyed component returns <code lang="csharp">string.Empty</code>.

| Components on the GameObject | Labels |
|---|---|
| <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster</code> |
| <code lang="class-name">AbilityCaster</code>, <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster (1)</code>, <code lang="string">Ability Caster (2)</code> |

## Package sample

In [EditorTools](../Samples~/EditorTools/Documentation/README.md), <code lang="csharp">GetDisplayName()</code> titles the custom inspector of the <code lang="class-name">AbilityConfig</code> asset.
