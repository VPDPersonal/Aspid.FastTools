# EnumValues

Таблица значений по ключам enum, настраиваемая в инспекторе: множители урона, цвета, звуки, ссылки на ассеты. `GetValue` возвращает значение подходящей строки, а если её нет — `Default Value`.

## Быстрый старт

В примерах используется перечисление:

```csharp
public enum DamageType
{
    Physical, Fire, Ice, Poison
}
```

Добавьте `using Aspid.FastTools.Enums;` к скрипту с `using UnityEngine;`. Одна таблица заменяет набор сериализованных полей и `switch`:

| До — отдельные поля и switch | После — EnumValues |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private float _defaultMultiplier = 1f;&#10;[SerializeField]&#10;private float _fireMultiplier = 1.5f;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt; type switch&#10;&#123;&#10;    DamageType.Fire =&gt; _fireMultiplier,&#10;    _ =&gt; _defaultMultiplier&#10;&#125;;</code></pre> | <pre lang="csharp"><code>[SerializeField]&#10;private EnumValues&lt;DamageType, float&gt;&#10;    _multipliers;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt;&#10;    _multipliers.GetValue(type);</code></pre> |

![Fire использует множитель 1.5, остальные типы урона — Default Value 1](../Images/enum-values-multipliers-quick-start.png)

Fire использует множитель 1.5, остальные типы урона — Default Value 1

| Вызов | Результат |
|---|---|
| `_multipliers.GetValue(DamageType.Fire)` | `1.5` — значение строки `Fire` |
| `_multipliers.GetValue(DamageType.Ice)` | `1` — строки `Ice` нет, возвращается `Default Value` |

## Настройка в инспекторе

1. Раскройте таблицу и задайте **Default Value** — его получат ключи без собственной строки.
2. Добавьте строки вручную или нажмите правой кнопкой по свойству и выберите **Populate Missing Enum Members**.
3. Настройте значения добавленных строк.

Строки нужны только ключам, чьё значение отличается от `Default Value`.

### Populate Missing Enum Members

Добавляет в конец таблицы недостающие члены enum со значением, равным текущему `Default Value`.

![Populate Missing Enum Members добавляет строки со значением 1, сохраняя Fire = 1.5; Undo отменяет заполнение](../Images/enum-values-multipliers-populate.gif)

Populate Missing Enum Members добавляет строки со значением 1, сохраняя Fire = 1.5; Undo отменяет заполнение

Для `[Flags]` добавляются только объявленные члены, включая именованные комбинации. Все возможные сочетания битов не создаются.

## Какой вариант выбрать

| Задача | Тип поля | Выбор enum в инспекторе | Ключ в `GetValue` |
|---|---|---|---|
| Перечисление известно в коде | `EnumValues<TEnum, TValue>` | Задан аргументом `TEnum`; поле типа только для чтения | `TEnum`: проверяется компилятором, без упаковки в `object` |
| Перечисление выбирает автор ассета | `EnumValues<TValue>` | Доступен в заголовке таблицы | `System.Enum`: ключ упаковывается, а чужой enum проходит компиляцию и возвращает `Default Value` |

Оба варианта поддерживают `Default Value`, `[Flags]` и перебор строк. `TValue` — любой тип, который сериализует Unity: `float`, `Color`, `AudioClip`, ваш `[Serializable]`-класс.

### EnumValues\<TEnum, TValue\>

```csharp
[SerializeField] private EnumValues<DamageType, float> _multipliers;
```

Enum задан в коде, тип ключа проверяет компилятор. Полный пример — в [быстром старте](#быстрый-старт).

### EnumValues\<TValue\>

То же поле без `DamageType` в объявлении; enum выбирается в инспекторе:

```csharp
[SerializeField] private EnumValues<float> _multipliers;

public float GetMultiplier(DamageType type) => _multipliers.GetValue(type);
```

Для этого примера выберите **DamageType** в заголовке таблицы. Ключ другого перечисления вернёт `Default Value`, даже если числовое значение совпало.

![Откройте выбор типа в заголовке Multipliers и найдите DamageType](../Images/enum-values-type-selector.png)

Откройте выбор типа в заголовке Multipliers и найдите DamageType

> [!IMPORTANT]
> Если enum не выбран, таблица возвращает `Default Value` и при первом обращении пишет предупреждение в Console. Если сохранённый тип не найден в проекте, например после переименования, вместо предупреждения будет ошибка.

## Правила поиска

Таблица просматривается последовательно, сверху вниз. Для обычного enum побеждает первая строка с тем же числовым значением ключа, иначе — `Default Value`:

| Ситуация | Результат |
|---|---|
| Ключ найден | Значение строки, в том числе `0`, `false` или `null` |
| Ключа нет или таблица пустая | `Default Value` |
| Несколько строк с одним числовым ключом | Первая из них |
| Разные имена enum с одним числовым значением | Для поиска это один ключ |

### Флаги

Для `[Flags]` сначала ищется точное совпадение, затем проверяется вхождение флагов.

Значение `0` совпадает только с `0` и не подходит к остальным флагам как «пустая маска». Пример:

```csharp
[Flags]
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

<ol className="enum-lookup-flow">
  <li>
    <strong>Точное совпадение</strong>
    <span>Ищем весь запрошенный набор флагов.</span>
    <code>Burning | Slowed → 0.3</code>
    <small>Точная строка побеждает, даже если стоит ниже.</small>
    <em>Нет точной строки →</em>
  </li>
  <li>
    <strong>Первая подходящая строка</strong>
    <span>Все её флаги должны входить в запрос.</span>
    <code>Burning | Frozen → 0.9</code>
    <small>Выбирается Burning; порядок строк важен.</small>
    <em>Нет подходящей строки →</em>
  </li>
  <li>
    <strong>Default Value</strong>
    <span>Возвращаем значение по умолчанию.</span>
    <code>Frozen → 1</code>
    <small>Ни точной, ни подходящей строки нет.</small>
  </li>
</ol>

> [!NOTE]
> Второй проход берёт первую подходящую строку, а не самую полную. Для `Burning | Slowed | Frozen` подходят и `Burning`, и `Burning | Slowed`, но побеждает `Burning`, потому что стоит выше: результат `0.9`. Чтобы побеждала комбинация, ставьте составные строки выше одиночных флагов.

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

Ключи хранятся по **именам** членов enum:

| Изменение enum | Результат |
|---|---|
| Члены переставлены или изменены их числовые значения | Таблица работает как раньше |
| Добавлен член | Возвращает `Default Value`, пока не добавлена строка; **Populate Missing Enum Members** заполняет пропуск |
| Член переименован или удалён | Его строка не распознаётся: при инициализации в Console появляется ошибка, поиск и перебор её пропускают |

> [!NOTE]
> Инспектор показывает такую строку как `<Missing Имя>` и сохраняет её ключ, пока вы не выберете член, поэтому после возврата прежнего имени строка снова распознаётся. Так же и при смене enum в `EnumValues<TValue>`: если вернуть прежний тип, все ключи снова распознаются.

## Пример в пакете

Плитки и следы получают цвет из `EnumValues<SurfaceType, Color>`, а множитель скорости — из `EnumValues<float>` с выбранным в инспекторе `[Flags]`-enum: [EnumValues](../../Samples~/EnumValues/Documentation/README.ru.md).

![Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.](../../Samples~/EnumValues/Documentation/Images/demo.gif)

Персонаж проходит по разным поверхностям и оставляет непрерывную цветную линию.
