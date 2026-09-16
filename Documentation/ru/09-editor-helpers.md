# Editor Helpers

Два метода для читаемых подписей компонентов и ScriptableObject в заголовках инспекторов, списках и окнах редактора. Они убирают повторяющийся код форматирования имён и помогают различать одинаковые компоненты на одном GameObject.

Оба метода — расширения: `GetDisplayName()` вызывается у любого `UnityEngine.Object`, `GetDisplayNameWithIndex()` — у `Component`.

```csharp
using Aspid.FastTools.Editors;

[SerializeField] private AudioSource _audioSource;

var title = _audioSource.GetDisplayName();
// "Audio Source"

var indexedTitle = _audioSource.GetDisplayNameWithIndex();
// "Audio Source (2)", если это второй AudioSource на том же GameObject
```

> [!NOTE]
> Методы доступны только в редакторе. Размещайте использующий их код в папке `Editor` или в сборке, ограниченной платформой Editor.

## Имя объекта

`GetDisplayName()` работает с `UnityEngine.Object`. Если у типа есть `[AddComponentMenu]`, включая унаследованный атрибут, метод берёт заголовок через `ObjectNames.GetInspectorTitle`. В остальных случаях он преобразует имя типа через `ObjectNames.NicifyVariableName`.

Метод описывает тип, а не объект: имя GameObject или ассета на результат не влияет.

## Номер компонента

`GetDisplayNameWithIndex()` работает с `Component` и учитывает только компоненты **точно того же типа** на том же GameObject. Суффикс соответствует порядку компонентов, начиная с единицы.

| Компоненты на GameObject | Подписи |
|---|---|
| `AudioSource` | `Audio Source` |
| `AudioSource`, `AudioSource` | `Audio Source (1)`, `Audio Source (2)` |
| `AudioSource`, `BoxCollider` | `Audio Source`, `Box Collider` |

Для `null` или уничтоженного объекта оба метода возвращают `string.Empty`.

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) метод `GetDisplayName()` формирует заголовок панели выбранной способности. Его можно дополнить [`AddOpenScriptCommand`](07-visual-element-extensions.md#открытие-скрипта-и-окно-владелец): двойной клик по заголовку откроет исходный файл в IDE.
