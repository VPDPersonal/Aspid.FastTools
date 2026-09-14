# EnumValues

Настраивайте урон, цвета, звуки и другие значения для каждого члена enum прямо в инспекторе. В коде достаточно вызвать `GetValue`: таблица вернёт найденное значение или заданный `Default Value`.

![Таблицы поверхностей: enum-ключи и значения редактируются в инспекторе](../../Samples~/EnumValues/Documentation/Images/surface-tables.png)

Таблицы поверхностей: enum-ключи и значения редактируются в инспекторе

## Быстрый старт

Если перечисление известно в коде, используйте `EnumValues<TEnum, TValue>`. Например, таблицу множителей урона:

```csharp
using UnityEngine;
using Aspid.FastTools.Enums;

public enum DamageType { Physical, Fire, Ice, Poison }

public sealed class DamageDealer : MonoBehaviour
{
    [SerializeField] private EnumValues<DamageType, float> _multipliers;

    public float CalculateDamage(DamageType type, float baseDamage) =>
        baseDamage * _multipliers.GetValue(type);
}
```

1. Добавьте `DamageDealer` на GameObject и раскройте `Multipliers` в инспекторе.
2. Задайте **Default Value = 1** — урон без отдельной настройки останется прежним.
3. В контекстном меню свойства выберите **Populate Missing Enum Members**. Отсутствующие ключи добавятся с текущим значением по умолчанию.
4. Установите для `Fire` значение `1.5`. Вызов `CalculateDamage(DamageType.Fire, 10)` вернёт `15`.

Чтобы проверить таблицы в действии, импортируйте [пример EnumValues](../../Samples~/EnumValues/Documentation/README.ru.md): там тип поверхности управляет цветом и следом персонажа.

## Какой вариант выбрать

| Задача | Тип поля | Выбор enum в инспекторе |
|---|---|---|
| Перечисление известно при написании кода | `EnumValues<TEnum, TValue>` | Зафиксирован аргументом `TEnum` |
| Перечисление должен выбрать автор ассета | `EnumValues<TValue>` | Доступен в заголовке таблицы |

Оба варианта поддерживают значение по умолчанию, `[Flags]` и перебор записей. Тип `TValue` должен поддерживаться сериализацией Unity.

### EnumValues\<TEnum, TValue\>

Типизированная таблица проверяет тип ключа при компиляции. Поиск сравнивает закэшированные числовые значения enum без упаковки ключа в `object`.

```csharp
[SerializeField] private EnumValues<DamageType, Color> _colors;

public Color GetColor(DamageType type) => _colors.GetValue(type);
```

### EnumValues\<TValue\>

Универсальная таблица хранит тип enum, выбранный в инспекторе. Для неё можно использовать один и тот же класс конфигурации с разными перечислениями.

```csharp
[SerializeField] private EnumValues<float> _multipliers;

public float GetMultiplier(DamageType type) => _multipliers.GetValue(type);
```

Для этого примера выберите `DamageType` в заголовке таблицы. Передавайте в `GetValue` ключи выбранного перечисления.

## Как сопоставляются флаги

Для `[Flags]` поиск идёт в два этапа: сначала **точное совпадение**, затем первая запись, все биты которой входят в запрошенное значение. Если совпадений нет, возвращается `Default Value`. Значение `0` совпадает только с `0`.

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

Допустим, таблица содержит следующие записи в указанном порядке, а `Default Value` равен `1`:

| Ключ | Значение |
|---|---|
| `Burning` | `0.9` |
| `Slowed` | `0.5` |
| `Burning \| Slowed` | `0.3` |
| `None` | `1` |

| Запрос | Результат | Причина |
|---|---|---|
| `Burning \| Slowed` | `0.3` | Точное совпадение имеет приоритет, даже если стоит ниже |
| `Burning \| Frozen` | `0.9` | Точного совпадения нет; первая подходящая запись — `Burning` |
| `Frozen` | `1` | Нет подходящей записи, используется `Default Value` |
| `None` | `1` | Точное совпадение с нулём |

Порядок записей важен при частичном совпадении. Если для `Burning | Slowed | Frozen` составная запись должна победить отдельный `Burning`, переместите её выше. Значения подходящих записей не суммируются и не перемножаются.

## Перебор записей

```csharp
foreach (var entry in _multipliers)
    Debug.Log($"{entry.Key}: {entry.Value}");
```

Оба варианта используют структурный перечислитель при прямом `foreach` по коллекции. Приведение к `IEnumerable` может привести к упаковке перечислителя. Типизированный вариант реализует `IEnumerable<KeyValuePair<TEnum, TValue>>`, универсальный — `IEnumerable<KeyValuePair<Enum, TValue>>`.
