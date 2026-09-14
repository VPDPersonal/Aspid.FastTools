# EnumValues

Таблица значений по ключам enum, которую можно настроить в инспекторе: множители урона, цвета, звуки или ссылки на ассеты. `GetValue` возвращает значение подходящей строки, а если её нет — заданный `Default Value`.

## Быстрый старт

Добавьте `using Aspid.FastTools.Enums;` к скрипту с `using UnityEngine;`. Для примеров используем перечисление:

```csharp
public enum DamageType
{
    Physical, Fire, Ice, Poison
}
```

Внутри компонента таблица заменяет отдельные сериализованные поля и выбор через `switch`:

| До — отдельные поля и switch | После — EnumValues |
|---|---|
| <pre lang="csharp"><code>[SerializeField]<br />private float _defaultMultiplier = 1f;<br />[SerializeField]<br />private float _fireMultiplier = 1.5f;<br /><br />public float GetMultiplier(<br />    DamageType type) =&gt; type switch<br />&#123;<br />    DamageType.Fire =&gt; _fireMultiplier,<br />    _ =&gt; _defaultMultiplier<br />&#125;;</code></pre> | <pre lang="csharp"><code>[SerializeField]<br />private EnumValues&lt;DamageType, float&gt;<br />    _multipliers = new();<br /><br />public float GetMultiplier(<br />    DamageType type) =&gt;<br />    _multipliers.GetValue(type);</code></pre> |

Для такого же результата задайте у `_multipliers` **Default Value = 1** и добавьте строку **Fire = 1.5**. Значения справа настраиваются в инспекторе; `new()` создаёт пустую таблицу с исходным значением `0` для `float`.

| Вызов | Результат при этих настройках |
|---|---|
| `_multipliers.GetValue(DamageType.Fire)` | `1.5` — значение строки `Fire` |
| `_multipliers.GetValue(DamageType.Ice)` | `1` — строки `Ice` нет |

<details>
<summary>Полный пример: компонент с множителями урона</summary>

Создайте `DamageDealer.cs`, добавьте компонент на GameObject и настройте таблицу `Multipliers`:

```csharp
using UnityEngine;
using Aspid.FastTools.Enums;

public enum DamageType
{
    Physical, Fire, Ice, Poison
}

public sealed class DamageDealer : MonoBehaviour
{
    [SerializeField] private EnumValues<DamageType, float> _multipliers = new();

    public float CalculateDamage(DamageType type, float baseDamage) =>
        baseDamage * _multipliers.GetValue(type);

    [ContextMenu("Log Fire Damage")]
    private void LogFireDamage() =>
        Debug.Log(CalculateDamage(DamageType.Fire, 10f), this);
}
```

Установите **Default Value = 1**, добавьте строку **Fire = 1.5** и выберите **Log Fire Damage** в контекстном меню компонента. В Console появится `15`.

Если `DamageType` уже объявлен в проекте, используйте существующее перечисление.

</details>

## Настройка в инспекторе

В таблице `Multipliers` тип ключа — `DamageType`, а значение каждой строки — `float`:

1. Раскройте таблицу и задайте **Default Value** для ключей без собственной строки.
2. Добавьте нужные строки вручную или нажмите правой кнопкой по свойству и выберите **Populate Missing Enum Members**.
3. Настройте значения добавленных строк, например **Fire = 1.5**.

Заполнять все члены перечисления необязательно. Таблица только с отличающимися значениями остаётся рабочей: остальные ключи используют `Default Value`.

### Populate Missing Enum Members

Команда добавляет в конец списка отсутствующие **имена, объявленные в enum**, и копирует в новые строки текущее `Default Value`. Уже существующие строки и их значения сохраняются. Операцию можно отменить через Undo.

Для `[Flags]` добавляются объявленные члены, включая именованные комбинации. Все возможные сочетания битов автоматически не создаются. Если отсутствующих имён нет, пункт меню недоступен.

Изменение `Default Value` после заполнения не заменяет значения существующих строк. Например, строка `Ice`, созданная со значением `1`, продолжит возвращать `1`, даже если позже задать по умолчанию `2`.

## Какой вариант выбрать

| Задача | Тип поля | Выбор enum в инспекторе |
|---|---|---|
| Перечисление известно в коде | `EnumValues<TEnum, TValue>` | Задан аргументом `TEnum`; поле типа только для чтения |
| Автор ассета должен выбирать перечисление | `EnumValues<TValue>` | Доступен в заголовке таблицы |

Оба варианта поддерживают значение по умолчанию, `[Flags]` и перебор записей. `TValue` должен поддерживаться сериализацией Unity: например, `float`, `Color`, `AudioClip` или ваш сериализуемый класс.

### EnumValues\<TEnum, TValue\>

Используйте типизированный вариант, когда все обращения идут по одному известному enum. Компилятор проверит тип ключа:

```csharp
[SerializeField] private EnumValues<DamageType, Color> _colors = new();

public Color GetColor(DamageType type) => _colors.GetValue(type);
```

Поиск сравнивает числовые значения ключей. После инициализации таблицы типизированный `GetValue` не упаковывает ключ в `object`.

### EnumValues\<TValue\>

Здесь в коде задан только тип значения, а enum выбирается в инспекторе:

```csharp
[SerializeField] private EnumValues<float> _multipliers = new();

public float GetMultiplier(DamageType type) => _multipliers.GetValue(type);
```

Для этого примера выберите **DamageType** в заголовке таблицы. Метод принимает `System.Enum`, поэтому компилятор допускает ключи других перечислений; во время поиска их тип проверяется. Ключ другого enum возвращает `Default Value`, даже если его числовое значение совпало.

Если тип enum не выбран, таблица возвращает `Default Value` и при инициализации пишет предупреждение в Console. Если сохранённый тип не удалось найти, пишет ошибку и также возвращает значение по умолчанию.

## Как работает GetValue

Для обычного enum поиск возвращает первую строку с точным числовым значением ключа. Если такой строки нет, результат — `Default Value`.

| Ситуация | Результат |
|---|---|
| Ключ найден | Значение его строки |
| Таблица пустая или ключ отсутствует | `Default Value` |
| В найденной строке `0`, `false` или `null` | Именно это значение; поиск не переходит к `Default Value` |
| Есть несколько строк с одинаковым числовым ключом | Первая из них |
| Разные имена enum имеют одно числовое значение | Для поиска это один ключ |

Таблица просматривается последовательно, а не как словарь. Для `[Flags]` после точного поиска добавляется второй проход по правилам ниже.

## Как сопоставляются флаги

Объявите `[Flags]`-enum и поле таблицы:

```csharp
[System.Flags]
public enum StatusEffect
{
    None = 0,
    Burning = 1,
    Slowed = 2,
    Frozen = 4
}
```

```csharp
[SerializeField]
private EnumValues<StatusEffect, float> _speedMultipliers = new();
```

Поиск идёт в таком порядке:

1. **Точное совпадение** со всем запрошенным значением.
2. Если его нет — **первая строка, все биты которой входят в запрос**.
3. Если ни одна строка не подходит — **Default Value**.

Значение `0` совпадает только с `0` и не служит универсальным совпадением для остальных флагов.

### Точное и частичное совпадение

Пусть `Default Value` равен `1`, а строки идут в таком порядке:

| Ключ | Значение |
|---|---|
| `Burning` | `0.9` |
| `Slowed` | `0.5` |
| `Burning \| Slowed` | `0.3` |
| `None` | `1` |

| Аргумент GetValue | Результат | Причина |
|---|---|---|
| `Burning \| Slowed` | `0.3` | Точная строка выигрывает, хотя стоит ниже отдельных флагов |
| `Burning \| Frozen` | `0.9` | Точной строки нет; первая подходящая — `Burning` |
| `Frozen` | `1` | Подходящей строки нет |
| `None` | `1` | Точная строка с нулём |

Например:

```csharp
var multiplier = _speedMultipliers.GetValue(
    StatusEffect.Burning | StatusEffect.Slowed); // 0.3
```

### Когда важен порядок строк

Для запроса `Burning | Slowed | Frozen` точной строки нет. Первой подходит `Burning`, поэтому результат — `0.9`. Если переместить составную строку `Burning | Slowed` выше неё, результат станет `0.3`.

Поиск выбирает **одно значение**. Он не суммирует и не перемножает значения подходящих строк и не выбирает автоматически наиболее полную комбинацию.

## Проверка ключей через Equals

`Equals(first, second)` проверяет ключи по тем же правилам сопоставления, не читая значения строк. Для обычного enum это точное равенство. Для `[Flags]` метод проверяет, содержит ли **первый аргумент все биты второго**, с отдельным правилом для нуля:

```csharp
var combined = StatusEffect.Burning | StatusEffect.Slowed;

_speedMultipliers.Equals(combined, StatusEffect.Burning); // true
_speedMultipliers.Equals(StatusEffect.Burning, combined); // false
_speedMultipliers.Equals(combined, StatusEffect.None);    // false
_speedMultipliers.Equals(StatusEffect.None, StatusEffect.None); // true
```

У флагов такая проверка зависит от порядка аргументов. Для строгого равенства значений enum используйте обычный `==`. В универсальном `EnumValues<TValue>` оба аргумента должны принадлежать выбранному перечислению; иначе результат — `false`.

## Перебор записей

Прямой `foreach` возвращает настроенные строки в порядке списка:

```csharp
foreach (var entry in _multipliers)
{
    Debug.Log($"{entry.Key}: {entry.Value}");
}
```

`Default Value` в перебор не входит. Ключи, которые не удалось разобрать, пропускаются. Типизированная таблица выдаёт ключи `TEnum`, универсальная — `System.Enum`.

Оба варианта используют структурный перечислитель. После инициализации прямой `foreach` не выделяет память на перечислитель; перебор через интерфейс `IEnumerable` упаковывает его. Код внутри цикла, например интерполяция строки для `Debug.Log`, может выделять память отдельно.

## Изменение таблицы и enum

Значения таблицы настраиваются через сериализацию Unity. Публичный API предназначен для чтения, проверки ключей и перебора: методов `Add`, `Remove` и индексатора для записи нет.

Ключи хранятся как строки с именами членов enum. Новый член использует `Default Value`, пока для него не добавлена строка; команда **Populate Missing Enum Members** помогает заполнить такие пропуски. Переименование или удаление существующего члена требует проверки и переназначения сохранённых ключей.

## Практический пример

В [примере EnumValues](../../Samples~/EnumValues/Documentation/README.ru.md) тип поверхности задаёт цвет плитки и следа, а флаги рельефа — множитель скорости персонажа. В `SurfacePalette.asset` две типизированные таблицы `EnumValues<SurfaceType, Color>`:

![Таблицы цветов поверхностей и следов с отдельным Default Value](../../Samples~/EnumValues/Documentation/Images/surface-tables.png)

Таблицы цветов поверхностей и следов с отдельным Default Value

Так `SurfacePalette` получает цвет плитки из показанной таблицы:

```csharp
[SerializeField] private EnumValues<SurfaceType, Color> _tileColors;

public Color GetTileColor(SurfaceType surface) =>
    _tileColors.GetValue(surface);
```

Импортируйте пример и откройте `Scenes/EnumValues.unity`. Измените цвет `Grass` в `Data/SurfacePalette.asset`: плитки перекрасятся. Затем удалите строку `Stone` из `Footprint Colors`, чтобы увидеть применение `Default Value`. Инструкции по запуску и эксперименты с флагами есть в [документации примера](../../Samples~/EnumValues/Documentation/README.ru.md).
