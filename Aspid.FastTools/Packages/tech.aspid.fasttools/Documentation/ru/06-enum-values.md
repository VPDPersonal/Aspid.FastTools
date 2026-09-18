# EnumValues

Таблица значений по ключам enum, настраиваемая в инспекторе: множители урона, цвета, звуки, ссылки на ассеты. `GetValue` возвращает значение подходящей строки, а если её нет — `Default Value`.

## Быстрый старт

Добавьте `using Aspid.FastTools.Enums;` к скрипту с `using UnityEngine;`. В примерах используется перечисление:

```csharp
public enum DamageType
{
    Physical, Fire, Ice, Poison
}
```

Одна таблица заменяет набор сериализованных полей и `switch`:

| До — отдельные поля и switch | После — EnumValues |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private float _defaultMultiplier = 1f;&#10;[SerializeField]&#10;private float _fireMultiplier = 1.5f;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt; type switch&#10;&#123;&#10;    DamageType.Fire =&gt; _fireMultiplier,&#10;    _ =&gt; _defaultMultiplier&#10;&#125;;</code></pre> | <pre lang="csharp"><code>[SerializeField]&#10;private EnumValues&lt;DamageType, float&gt;&#10;    _multipliers;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt;&#10;    _multipliers.GetValue(type);</code></pre> |

В инспекторе задайте у `Multipliers` **Default Value = 1** и добавьте строку **Fire = 1.5**:

| Вызов | Результат |
|---|---|
| `_multipliers.GetValue(DamageType.Fire)` | `1.5` — значение строки `Fire` |
| `_multipliers.GetValue(DamageType.Ice)` | `1` — строки `Ice` нет, возвращается `Default Value` |

<details>
<summary>Полный пример: компонент с множителями урона</summary>

Создайте `DamageDealer.cs` и добавьте компонент на GameObject:

```csharp
using UnityEngine;
using Aspid.FastTools.Enums;

public enum DamageType
{
    Physical, Fire, Ice, Poison
}

public sealed class DamageDealer : MonoBehaviour
{
    [SerializeField] private EnumValues<DamageType, float> _multipliers;

    public float CalculateDamage(DamageType type, float baseDamage) =>
        baseDamage * _multipliers.GetValue(type);

    [ContextMenu("Log Fire Damage")]
    private void LogFireDamage() =>
        Debug.Log(CalculateDamage(DamageType.Fire, 10f), this);
}
```

Установите **Default Value = 1**, добавьте строку **Fire = 1.5** и выберите **Log Fire Damage** в контекстном меню компонента. В Console появится `15`. Если `DamageType` уже объявлен в проекте, используйте существующее перечисление.

</details>

## Настройка в инспекторе

Так выглядят две таблицы `EnumValues<SurfaceType, Color>` из [примера EnumValues](../../Samples~/EnumValues/Documentation/README.ru.md):

![Таблицы цветов поверхностей и следов с отдельным Default Value](../../Samples~/EnumValues/Documentation/Images/surface-tables.png)

Таблицы цветов поверхностей и следов с отдельным Default Value

1. Раскройте таблицу и задайте **Default Value** — его получат ключи без собственной строки.
2. Добавьте строки вручную или нажмите правой кнопкой по свойству и выберите **Populate Missing Enum Members**.
3. Настройте значения добавленных строк.

Заполнять все члены перечисления необязательно: таблица только с отличающимися значениями остаётся рабочей.

### Populate Missing Enum Members

Команда добавляет в конец списка члены, **объявленные в enum**, но отсутствующие в таблице, и записывает в новые строки текущее `Default Value`. Существующие строки не меняются, операция отменяется через Undo. Если отсутствующих членов нет, пункт меню недоступен.

Для `[Flags]` добавляются только объявленные члены, включая именованные комбинации. Все возможные сочетания битов не создаются.

> [!NOTE]
> `Default Value` копируется в строки один раз, при заполнении. Если позже изменить его на `2`, строка `Ice`, созданная со значением `1`, продолжит возвращать `1`.

## Какой вариант выбрать

| Задача | Тип поля | Выбор enum в инспекторе |
|---|---|---|
| Перечисление известно в коде | `EnumValues<TEnum, TValue>` | Задан аргументом `TEnum`; поле типа только для чтения |
| Перечисление выбирает автор ассета | `EnumValues<TValue>` | Доступен в заголовке таблицы |

Оба варианта поддерживают `Default Value`, `[Flags]` и перебор строк. `TValue` — любой тип, который сериализует Unity: `float`, `Color`, `AudioClip`, ваш `[Serializable]`-класс.

### EnumValues\<TEnum, TValue\>

Типизированный вариант для случаев, когда все обращения идут по одному известному enum. Компилятор проверяет тип ключа, а `GetValue` не упаковывает его в `object`:

```csharp
[SerializeField] private EnumValues<DamageType, Color> _colors;

public Color GetColor(DamageType type) => _colors.GetValue(type);
```

### EnumValues\<TValue\>

В коде задан только тип значения, enum выбирается в инспекторе:

```csharp
[SerializeField] private EnumValues<float> _multipliers;

public float GetMultiplier(DamageType type) => _multipliers.GetValue(type);
```

Для этого примера выберите **DamageType** в заголовке таблицы. `GetValue` принимает `System.Enum`, поэтому компилятор пропустит ключ другого перечисления — он вернёт `Default Value`, даже если числовое значение совпало.

> [!IMPORTANT]
> Если enum не выбран, таблица возвращает `Default Value` и один раз пишет предупреждение в Console. Если сохранённый тип не найден в проекте, например после переименования, вместо предупреждения будет ошибка.

## Правила поиска

Таблица просматривается последовательно, сверху вниз. Для обычного enum побеждает первая строка с тем же числовым значением ключа, иначе — `Default Value`:

| Ситуация | Результат |
|---|---|
| Ключ найден | Значение строки, в том числе `0`, `false` или `null` |
| Ключа нет или таблица пустая | `Default Value` |
| Несколько строк с одним числовым ключом | Первая из них |
| Разные имена enum с одним числовым значением | Для поиска это один ключ |

### Флаги

Для `[Flags]`-enum после точного поиска добавляется второй проход:

1. **Точное совпадение** со всем запрошенным значением.
2. Иначе — **первая строка, все биты которой входят в запрос**.
3. Иначе — `Default Value`.

Значение `0` совпадает только с `0` и не подходит к остальным флагам как «пустая маска». Пример:

```csharp
[System.Flags]
public enum StatusEffect
{
    None = 0,
    Burning = 1,
    Slowed = 2,
    Frozen = 4
}

[SerializeField] private EnumValues<StatusEffect, float> _speedMultipliers;
```

`Default Value` равен `1`, строки идут в таком порядке:

| Ключ | Значение |
|---|---|
| `Burning` | `0.9` |
| `Slowed` | `0.5` |
| `Burning \| Slowed` | `0.3` |
| `None` | `1` |

| Аргумент GetValue | Результат | Причина |
|---|---|---|
| `Burning \| Slowed` | `0.3` | Точная строка побеждает, хотя стоит ниже отдельных флагов |
| `Burning \| Frozen` | `0.9` | Точной строки нет; первая подходящая — `Burning` |
| `Burning \| Slowed \| Frozen` | `0.9` | Точной строки нет; `Burning` стоит выше `Burning \| Slowed` |
| `Frozen` | `1` | Подходящей строки нет |
| `None` | `1` | Точная строка с нулём |

> [!NOTE]
> Поиск возвращает **одно** значение и не объединяет подходящие строки. Если для `Burning | Slowed | Frozen` нужен результат `0.3`, переместите строку `Burning | Slowed` выше `Burning`.

## Проверка ключей через Equals

`Equals(first, second)` сравнивает ключи по тем же правилам, не читая значения строк. Для обычного enum это равенство чисел. Для `[Flags]` метод проверяет, содержит ли **первый аргумент все биты второго**; ноль равен только нулю:

```csharp
var combined = StatusEffect.Burning | StatusEffect.Slowed;

_speedMultipliers.Equals(combined, StatusEffect.Burning);        // true
_speedMultipliers.Equals(StatusEffect.Burning, combined);        // false
_speedMultipliers.Equals(combined, StatusEffect.None);           // false
_speedMultipliers.Equals(StatusEffect.None, StatusEffect.None);  // true
```

Для строгого равенства значений enum используйте `==`. В `EnumValues<TValue>` оба аргумента должны принадлежать выбранному перечислению, иначе результат — `false`.

## Перебор строк

`foreach` возвращает настроенные строки в порядке списка. `Default Value` и строки с нераспознанным ключом в перебор не входят:

```csharp
foreach (var (type, multiplier) in _multipliers)
{
    Debug.Log($"{type}: {multiplier}");
}
```

Типизированная таблица выдаёт ключи `TEnum`, универсальная — `System.Enum`. Прямой `foreach` использует структурный перечислитель и не выделяет память; перебор через интерфейс `IEnumerable`, например в LINQ, упаковывает его.

## Изменение таблицы и enum

Публичный API только читает таблицу: методов `Add`, `Remove` и индексатора для записи нет, значения задаются через сериализацию Unity.

Ключи хранятся по **именам** членов enum, поэтому переставлять члены и менять их числовые значения безопасно. Новый член возвращает `Default Value`, пока для него не добавлена строка — команда **Populate Missing Enum Members** заполняет такие пропуски. Переименованный или удалённый член перестаёт распознаваться: его строка пропускается при поиске и переборе, проверьте такие ключи в инспекторе.

## Практический пример

В [примере EnumValues](../../Samples~/EnumValues/Documentation/README.ru.md) тип поверхности задаёт цвет плитки и следа через `EnumValues<SurfaceType, Color>`, а флаги рельефа — множитель скорости персонажа через `EnumValues<float>` с выбранным в инспекторе `TerrainFlags`:

![Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.](../../Samples~/EnumValues/Documentation/Images/demo.gif)

Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.

Импортируйте пример и откройте `Scenes/EnumValues.unity`. Измените цвет `Grass` в `Data/SurfacePalette.asset` — плитки перекрасятся без Play Mode. Удалите строку `Stone` из **Footprint Colors**, чтобы увидеть `Default Value` в действии. Эксперименты с флагами и перебором описаны в [документации примера](../../Samples~/EnumValues/Documentation/README.ru.md#попробуйте).
