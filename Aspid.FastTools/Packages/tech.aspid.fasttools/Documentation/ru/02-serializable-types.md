# Serializable Types

Тип как обычное поле: Unity его сохраняет, а в инспекторе его выбирают из списка.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

![Выбор сериализуемого типа в инспекторе](../Images/serializable-type-quick-start.gif)

## Что выбрать

| Задача | Поле |
|---|---|
| Хранить тип, в том числе из DLL, вложенный или generic | <code lang="class-name">SerializableType</code> |
| Сохранить выбор при переименовании собственного скрипта | <code lang="class-name">SerializableMonoScript</code> |

Обе обёртки имеют вариант с <code lang="class-name">T</code>, ограничивающий выбор совместимыми типами. Атрибут <code lang="csharp">[TypeSelector]</code> добавляет [настройки выбора](03-type-selector.md); в примере <code lang="csharp">Allow = TypeAllow.None</code> оставляет только конкретные классы.

Чтобы хранить экземпляр выбранного класса, используйте [SerializeReference Selector](04-serialize-reference-selector.md). Для смены класса существующего компонента — [ComponentTypeSelector](05-component-type-selector.md).

## SerializableType

<code lang="class-name">SerializableType</code> хранит assembly-qualified name — имя типа вместе со сборкой.

Значение по умолчанию задаётся в коде: <code lang="csharp">= new(typeof(Sword))</code>; тип, несовместимый с <code lang="class-name">T</code>, вызывает <code lang="class-name">ArgumentException</code>.

<code lang="csharp">ToString()</code> возвращает <code lang="csharp">Type.Name</code>: для <code lang="class-name">Enchanted&lt;Fire&gt;</code> это <code lang="string">Enchanted`1</code>.

### Потерянный тип

После переименования класса, namespace или сборки сохранённое имя больше не находится, и инспектор показывает `<Missing …>`.

![Потерянный тип Game.Combat.Spear в поле инспектора](../Images/serializable-type-missing.png)

- <code lang="csharp">.Type</code> возвращает <code lang="csharp">null</code>.
- <code lang="csharp">AssemblyQualifiedName</code> и <code lang="csharp">ToString()</code> сохраняют прежнее имя.

Чтобы исправить поле, выберите тип заново или верните классу прежнее имя.

## SerializableMonoScript

То же поле, но выбор переживает переименование класса: поле помнит сам ассет скрипта. Тип выбирают в инспекторе или перетаскивают на поле `.cs` из **Project**.

| После переименования `Sword.cs` → `Blade.cs` | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| <code lang="csharp">.Type</code> | <code lang="csharp">null</code> — [потерянный тип](#потерянный-тип) | <code lang="class-name">Blade</code>, в Play Mode тоже |
| Сохранённое имя | <code lang="class-name">Sword</code> | <code lang="class-name">Blade</code>, как только ассет пересохранят |

Ограничения:

- выбрать можно только класс верхнего уровня, не generic, объявленный в `.cs` с тем же именем, а из DLL — только <code lang="class-name">MonoBehaviour</code> и <code lang="class-name">ScriptableObject</code>;
- публичного конструктора нет, из кода поле не создать;
- если переименовать класс без файла или файл вне Unity без `.meta`, связь теряется и поле показывает потерянный тип.

## Типы в плеере

> [!WARNING]
> В плеере обе обёртки находят тип по сохранённому имени. Если класс используется только через такой выбор, при **Managed Stripping Level** Low и выше Unity может вырезать его из билда: <code lang="csharp">.Type</code> вернёт <code lang="csharp">null</code>, хотя в редакторе тип находится. Сохраните класс через <code lang="csharp">[Preserve]</code> (<code lang="csharp">UnityEngine.Scripting</code>) или `link.xml`. Это относится и к строковым полям с <code lang="csharp">[TypeSelector]</code>.

## Пример в пакете

Выбор типов врагов и паттерна расстановки показан в примере [Types](../../Samples~/Types/Documentation/README.ru.md).

![Волна обычных и элитных врагов в сцене Types](../../Samples~/Types/Documentation/Images/demo.gif)
