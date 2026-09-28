# Editor Helpers

Читаемые подписи объектов в своих окнах редактора — без «(Script)» и с номерами у одинаковых компонентов.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>new Label(ObjectNames&#10;    .GetInspectorTitle(caster));</code></pre> | <pre lang="csharp"><code>new Label(caster&#10;    .GetDisplayNameWithIndex());</code></pre> |

## GetDisplayName()

Работает с любым <code lang="class-name">UnityEngine.Object</code>; для <code lang="csharp">null</code> или уничтоженного объекта возвращает <code lang="csharp">string.Empty</code>.

| <code lang="csharp">[AddComponentMenu]</code> | <code lang="csharp">GetInspectorTitle()</code> | <code lang="csharp">GetDisplayName()</code> |
|---|---|---|
| нет | <code lang="string">Ability Caster (Script)</code> | <code lang="string">Ability Caster</code> |
| <code lang="csharp">"Gameplay/Ability"</code> на <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability</code> | <code lang="string">Ability</code> |
| <code lang="csharp">"Gameplay/Ability"</code> на базовом классе | <code lang="string">Ability Caster (Script)</code> | <code lang="string">Ability Caster</code> |

## GetDisplayNameWithIndex()

Работает с <code lang="class-name">Component</code>. Номер — позиция среди компонентов того же типа на GameObject; для <code lang="csharp">null</code> или уничтоженного компонента возвращает <code lang="csharp">string.Empty</code>.

| Компоненты на GameObject | Подписи |
|---|---|
| <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster</code> |
| <code lang="class-name">AbilityCaster</code>, <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster (1)</code>, <code lang="string">Ability Caster (2)</code> |
| <code lang="class-name">AbilityCaster</code>, <code lang="csharp">class FireCaster : AbilityCaster</code> | <code lang="string">Ability Caster</code>, <code lang="string">Fire Caster</code> |

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) <code lang="csharp">GetDisplayName()</code> задаёт заголовок кастомного инспектора ассета <code lang="class-name">AbilityConfig</code>.
