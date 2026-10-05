# Editor Helpers

Подписи объектов без «(Script)» и нумерация компонентов одного типа.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <code lang="csharp">ObjectNames.GetInspectorTitle(caster)</code> | <code lang="csharp">caster.GetDisplayName()</code> |
| <code lang="string">Ability Caster (Script)</code> | <code lang="string">Ability Caster</code> |

## GetDisplayName()

Возвращает читаемое имя типа для любого <code lang="class-name">UnityEngine.Object</code>, а не значение <code lang="csharp">Object.name</code>.

| <code lang="csharp">[AddComponentMenu]</code> | <code lang="csharp">GetInspectorTitle()</code> | <code lang="csharp">GetDisplayName()</code> |
|---|---|---|
| нет | <code lang="string">Ability Caster (Script)</code> | <code lang="string">Ability Caster</code> |
| <code lang="csharp">"Gameplay/Ability"</code> на <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability</code> | <code lang="string">Ability</code> |

## GetDisplayNameWithIndex()

Добавляет номер, если на GameObject несколько компонентов одного типа. Нумерация начинается с 1 и следует порядку компонентов.

| Компоненты на GameObject | Подписи |
|---|---|
| <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster</code> |
| <code lang="class-name">AbilityCaster</code>, <code lang="class-name">AbilityCaster</code> | <code lang="string">Ability Caster (1)</code>, <code lang="string">Ability Caster (2)</code> |
| <code lang="class-name">AbilityCaster</code> и его наследник <code lang="class-name">FireCaster</code> | <code lang="string">Ability Caster</code>, <code lang="string">Fire Caster</code> |

Оба метода возвращают <code lang="csharp">string.Empty</code> для <code lang="csharp">null</code> или уничтоженного объекта.

## Пример в пакете

В [EditorTools](../../docusaurus-plugin-content-docs-tutorials/current/EditorTools/README.md) <code lang="csharp">GetDisplayName()</code> задаёт заголовок кастомного инспектора ассета <code lang="class-name">AbilityConfig</code>.

![Окно Ability Catalog из примера EditorTools](../../../../tutorials/EditorTools/Images/demo.gif)
