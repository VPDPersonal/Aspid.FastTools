# Editor Helpers

Editor Helpers — набор небольших утилит для повседневных задач в редакторе Unity. Со временем он будет пополняться новыми инструментами.

Сейчас здесь доступны два метода для читаемых подписей компонентов и ScriptableObject в заголовках инспекторов, списках и окнах редактора. Они убирают повторяющийся код форматирования имён и помогают различать одинаковые компоненты на одном GameObject.

```csharp
using Aspid.FastTools.Editors;

var title = target.GetDisplayName();
// Для AudioSource: "Audio Source"

var indexedTitle = component.GetDisplayNameWithIndex();
// Для второго AudioSource на том же GameObject: "Audio Source (2)"
```

Методы доступны только в редакторе. Размещайте использующий их код в папке `Editor` или в сборке, ограниченной платформой Editor.

## Имя объекта

`GetDisplayName()` работает с `UnityEngine.Object`. Если у типа есть `[AddComponentMenu]`, включая унаследованный атрибут, метод берёт заголовок через `ObjectNames.GetInspectorTitle`. В остальных случаях он преобразует имя типа через `ObjectNames.NicifyVariableName`.

Это подпись типа: переименование GameObject или ассета не превращает её в `object.name`.

## Номер компонента

`GetDisplayNameWithIndex()` работает с `Component` и учитывает только компоненты **точно того же типа** на том же GameObject. Суффикс соответствует порядку компонентов, начиная с единицы.

| Компоненты на GameObject | Подписи |
|---|---|
| Один `AudioSource` | `Audio Source` |
| Два `AudioSource` | `Audio Source (1)`, `Audio Source (2)` |
| Компоненты разных типов | У каждого своё имя без числового суффикса |

Для `null` или уничтоженного объекта оба метода возвращают `string.Empty`.

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) метод `GetDisplayName()` формирует заголовок панели выбранной способности. Его можно дополнить [командой открытия скрипта](07-visual-element-extensions.md#расширения-редактора): двойной клик по заголовку откроет исходный файл в IDE.
