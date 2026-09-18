# Editor Helpers

Два метода для читаемых подписей компонентов и ScriptableObject. Они убирают повторяющийся код форматирования имён и помогают различать одинаковые компоненты на одном GameObject.

## Быстрый старт

Примеры построены на двух компонентах: один задаёт себе имя через `[AddComponentMenu]`, другой — нет.

```csharp
using UnityEngine;

[AddComponentMenu("Gameplay/Fire Ability")]
public sealed class FireAbility : MonoBehaviour { }

public sealed class AbilityConfig : MonoBehaviour { }
```

```csharp
using Aspid.FastTools.Editors;

fireAbility.GetDisplayName();       // "Fire Ability"
abilityConfig.GetDisplayName();     // "Ability Config"

// Второй AbilityConfig на том же GameObject
abilityConfig.GetDisplayNameWithIndex(); // "Ability Config (2)"
```

> [!NOTE]
> Методы доступны только в редакторе. Размещайте использующий их код в папке `Editor` или в сборке, ограниченной платформой Editor.

## Имя объекта

`GetDisplayName()` работает с `UnityEngine.Object`. Если у типа есть `[AddComponentMenu]`, метод берёт заголовок через `ObjectNames.GetInspectorTitle`. В остальных случаях он преобразует имя типа через `ObjectNames.NicifyVariableName`. Для `null` или уничтоженного объекта метод возвращает `string.Empty`.

| Компонент | `GetDisplayName()` | `ObjectNames.GetInspectorTitle()` |
|---|---|---|
| `FireAbility`, с атрибутом | `Fire Ability` | `Fire Ability` |
| `AbilityConfig`, без атрибута | `Ability Config` | `Ability Config (Script)` |

## Номер компонента

`GetDisplayNameWithIndex()` работает с `Component` и учитывает только компоненты **точно того же типа** на том же GameObject. Суффикс соответствует порядку компонентов, начиная с единицы. Для `null` или уничтоженного компонента метод возвращает `string.Empty`.

| Компоненты на GameObject | Подписи |
|---|---|
| `AbilityConfig` | `Ability Config` |
| `AbilityConfig`, `AbilityConfig` | `Ability Config (1)`, `Ability Config (2)` |

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) метод `GetDisplayName()` формирует заголовок панели выбранной способности.
