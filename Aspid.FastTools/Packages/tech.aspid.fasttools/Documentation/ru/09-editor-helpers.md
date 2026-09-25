# Editor Helpers

Для подписи объекта в своём окне редактора у Unity есть `ObjectNames.GetInspectorTitle`, но он дописывает «(Script)» к скриптам без `[AddComponentMenu]` и не различает два одинаковых компонента на одном GameObject. `GetDisplayName()` возвращает читаемое имя типа (`AbilityConfig` → «Ability Config»), а `GetDisplayNameWithIndex()` нумерует дубликаты: «Ability Config (1)», «Ability Config (2)».

## Быстрый старт

Примеры на этой странице работают с компонентом `AbilityConfig`:

```csharp
public sealed class AbilityConfig : MonoBehaviour { }
```

```csharp
using Aspid.FastTools.Editors;

var title = new Label(config.GetDisplayNameWithIndex());
```

> [!NOTE]
> Методы доступны только в редакторе: вызывайте их из папки `Editor` или из Assembly Definition только для Editor, который ссылается на `Aspid.FastTools.Editor`.

## GetDisplayName()

Расширяет `UnityEngine.Object`. Если `[AddComponentMenu]` объявлен на самом типе, возвращает последний сегмент пути меню; иначе — имя типа, разбитое на слова. Для `null` или уничтоженного объекта возвращает `string.Empty`.

| `[AddComponentMenu]` на `AbilityConfig` | `GetInspectorTitle()` | `GetDisplayName()` |
|---|---|---|
| Нет | `Ability Config (Script)` | `Ability Config` |
| `"Gameplay/Ability"` | `Ability` | `Ability` |

## GetDisplayNameWithIndex()

Расширяет `Component`. Считает компоненты **точно того же типа** на GameObject и добавляет позицию компонента среди них, начиная с единицы. Номер вычисляется при каждом вызове, поэтому после удаления или перестановки компонентов вызовите метод заново. Для `null` или уничтоженного компонента возвращает `string.Empty`.

| Компоненты на GameObject | Подписи |
|---|---|
| `AbilityConfig` | `Ability Config` |
| `AbilityConfig`, `AbilityConfig` | `Ability Config (1)`, `Ability Config (2)` |

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) `GetDisplayName()` задаёт заголовок кастомного инспектора ассета `AbilityConfig`.
