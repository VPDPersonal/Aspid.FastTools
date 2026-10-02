# Serializable Types

Тип класса как обычное поле: Unity его сохраняет, а в инспекторе он выбирается из списка.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private string _primaryWeaponName;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    string.IsNullOrEmpty(&#10;        _primaryWeaponName)&#10;        ? null&#10;        : System.Type.GetType(&#10;            _primaryWeaponName, false);</code></pre> | <pre lang="csharp"><code>[TypeSelector(Allow = TypeAllow.None)]&#10;[SerializeField]&#10;private SerializableType&lt;Weapon&gt;&#10;    _primaryWeapon;&#10;&#10;public System.Type PrimaryWeapon =&gt;&#10;    _primaryWeapon;</code></pre> |

![Выбор сериализуемого типа в инспекторе](../Images/serializable-type-quick-start.gif)

## Что выбрать

| Задача | Поле |
|---|---|
| Хранить тип, в том числе из DLL, вложенный или generic | <code lang="class-name">SerializableType</code> |
| Сохранить выбор при переименовании собственного скрипта | <code lang="class-name">SerializableMonoScript</code> |

Обе обёртки имеют вариант с <code lang="class-name">T</code>, ограничивающий выбор совместимыми типами. Атрибут <code lang="csharp">[TypeSelector]</code> добавляет [настройки выбора](12-type-selector.md); в примере <code lang="csharp">Allow = TypeAllow.None</code> оставляет только конкретные классы.

Чтобы хранить экземпляр выбранного класса, используйте [SerializeReference Selector](03-serialize-reference-selector.md). Для смены класса существующего компонента — [ComponentTypeSelector](11-component-type-selector.md).

## SerializableType

<code lang="class-name">SerializableType</code> хранит assembly-qualified name — имя типа вместе со сборкой.

Из кода обёртку создаёт конструктор; тип, несовместимый с <code lang="class-name">T</code>, вызывает <code lang="class-name">ArgumentException</code>:

```csharp
var primary = new SerializableType<Weapon>(typeof(Sword));
System.Type type = primary;

var empty = new SerializableType<Weapon>(null);
```

### Потерянный тип

Сохранённое имя, которое перестало находиться после переименования класса, namespace или сборки. Инспектор показывает его как `<Missing …>`.

![Потерянный тип Game.Combat.Spear в поле инспектора](../Images/serializable-type-missing.png)

- <code lang="csharp">Type</code> возвращает <code lang="csharp">null</code>.
- <code lang="csharp">AssemblyQualifiedName</code> и <code lang="csharp">ToString()</code> сохраняют прежнее имя. Выберите доступный тип заново или восстановите класс с прежним именем.

## SerializableMonoScript

То же поле, но выбор переживает переименование класса: поле помнит сам ассет скрипта. Тип выбирают в инспекторе или перетаскивают на поле `.cs` из **Project**.

| После переименования `Sword.cs` → `Blade.cs` | <code lang="class-name">SerializableType</code> | <code lang="class-name">SerializableMonoScript</code> |
|---|---|---|
| <code lang="csharp">.Type</code> | <code lang="csharp">null</code> — [потерянный тип](#потерянный-тип) | <code lang="class-name">Blade</code>, в Play Mode тоже |
| Сохранённое имя | <code lang="class-name">Sword</code> | <code lang="class-name">Blade</code>, как только ассет пересохранят |

Ограничения:

- в списке только классы со своим `.cs`: верхнего уровня, не generic, с именем как у файла;
- generic-типы, вложенные классы и типы из DLL выбрать нельзя;
- публичного конструктора нет, из кода поле не создать;
- если переименовать класс без файла или файл вне Unity без `.meta`, связь теряется и поле показывает потерянный тип.

## Типы в сборке

> [!WARNING]
> В плеере обе обёртки находят тип по сохранённому имени. Если класс используется только через такой выбор, stripping может удалить его, и <code lang="csharp">.Type</code> вернёт <code lang="csharp">null</code>. Сохраните класс через <code lang="csharp">[Preserve]</code> или `link.xml`. Это относится и к строкам с <code lang="csharp">[TypeSelector]</code>.

## Пример в пакете

Выбор типов врагов и паттерна расстановки показан в примере [Types](../../Samples~/Types/Documentation/README.ru.md).
