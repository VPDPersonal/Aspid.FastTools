# EnumValues

Таблица значений по членам enum, которую заполняют в инспекторе, а не в коде.

## Быстрый старт

| До — поля и switch | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private float _defaultMultiplier = 1f;&#10;[SerializeField]&#10;private float _fireMultiplier = 1.5f;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt; type switch&#10;&#123;&#10;    DamageType.Fire =&gt; _fireMultiplier,&#10;    _ =&gt; _defaultMultiplier&#10;&#125;;</code></pre> | <pre lang="csharp"><code>[SerializeField]&#10;private EnumValues&lt;DamageType, float&gt;&#10;    _multipliers;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt;&#10;    _multipliers.GetValue(type);</code></pre> |

Для ключа без своей строки <code lang="function">GetValue</code> возвращает **Default Value**.

![Таблица Multipliers в инспекторе: строка Fire со значением 1.5 и Default Value 1](../Images/enum-values-multipliers-quick-start.png)

## Заполнение в инспекторе

Строка нужна только ключу, чьё значение отличается от **Default Value**. **Populate Missing Enum Members** в контекстном меню заголовка таблицы добавляет в конец недостающие члены enum со значением **Default Value**, у <code lang="csharp">[Flags]</code> — объявленные члены вместе с именованными комбинациями.

![Populate Missing Enum Members в таблице Multipliers](../Images/enum-values-multipliers-populate.gif)

> [!NOTE]
> Строка, добавленная в пустую таблицу, показывается как `<None>` и пропускается с ошибкой в Console, пока не выбран член.

## Какой вариант выбрать

| Чем отличаются | <code lang="class-name">EnumValues&lt;TEnum, TValue&gt;</code> | <code lang="class-name">EnumValues&lt;TValue&gt;</code> |
|---|---|---|
| Где выбирается enum | Аргумент <code lang="class-name">TEnum</code> | Заголовок таблицы в инспекторе |
| Ключ в <code lang="function">GetValue</code> и <code lang="csharp">foreach</code> | <code lang="class-name">TEnum</code> | <code lang="class-name">System.Enum</code> |
| Упаковка в <code lang="function">GetValue</code> | Нет | Ключ упаковывается |
| Ключ другого enum | Не компилируется | Возвращает **Default Value** |

Из кода таблица только читается. <code lang="class-name">TValue</code> — любой тип, который сериализует Unity.

### EnumValues\<TEnum, TValue\>

Тип enum задаётся в коде: в этой таблице ключом служит только <code lang="class-name">DamageType</code>, а в инспекторе тип нельзя сменить.

```csharp
[SerializeField] private EnumValues<DamageType, float> _multipliers;

public float GetMultiplier(DamageType type) =>
    _multipliers.GetValue(type);
```

### EnumValues\<TValue\>

То же поле, но enum выбирается в заголовке таблицы:

```csharp
[SerializeField] private EnumValues<float> _multipliers;

public float GetMultiplier(DamageType type) =>
    _multipliers.GetValue(type);
```

![DamageType в окне выбора типа в заголовке Multipliers](../Images/enum-values-type-selector.png)

- Поле enum обязательное: пустое инспектор отмечает предупреждением, его находит [проверка обязательных полей](04-serialize-reference-tooling.md#где-проверяются-обязательные-поля).
- Пока enum не выбран, таблица возвращает **Default Value** и при первом обращении пишет предупреждение в Console.

## Правила поиска

| Ситуация | Результат |
|---|---|
| Несколько строк с одним ключом | Верхняя из них |
| <code lang="csharp">Ice</code> и алиас <code lang="csharp">Frost = Ice</code> | Один ключ: строка <code lang="csharp">Ice</code> отвечает и на <code lang="csharp">Frost</code> |

### Флаги

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

**Default Value** равен <code lang="csharp">0</code>, строки идут в таком порядке:

| Ключ | Значение |
|---|---|
| <code lang="csharp">Burning</code> | <code lang="csharp">0.9</code> |
| <code lang="csharp">Slowed</code> | <code lang="csharp">0.5</code> |
| <code lang="csharp">Burning &#124; Slowed</code> | <code lang="csharp">0.3</code> |
| <code lang="csharp">None</code> | <code lang="csharp">1</code> |

| Запрос | Результат |
|---|---|
| <code lang="csharp">Burning &#124; Slowed</code> | <code lang="csharp">0.3</code> — точная строка побеждает, хотя стоит ниже <code lang="csharp">Burning</code> |
| <code lang="csharp">Burning &#124; Slowed &#124; Frozen</code> | <code lang="csharp">0.9</code> — точной строки нет, первая строка, все флаги которой есть в запросе, — <code lang="csharp">Burning</code> |
| <code lang="csharp">Frozen</code> | <code lang="csharp">0</code> — **Default Value**: <code lang="csharp">None</code> совпадает только с <code lang="csharp">None</code> |

> [!NOTE]
> Побеждает верхняя подходящая строка, а не самая полная: чтобы побеждала комбинация, ставьте её выше одиночных флагов.

## Equals()

<code lang="csharp">Equals(запрос, ключ)</code> отвечает, подошла бы строка с этим ключом к запросу, по правилам поиска и не читая значений:

| Вызов | Результат |
|---|---|
| <code lang="csharp">Equals(Burning &#124; Slowed, Burning)</code> | <code lang="csharp">true</code> |
| <code lang="csharp">Equals(Burning, Burning &#124; Slowed)</code> | <code lang="csharp">false</code> |
| <code lang="csharp">Equals(Burning &#124; Slowed, None)</code> | <code lang="csharp">false</code> |
| <code lang="csharp">Equals(None, None)</code> | <code lang="csharp">true</code> |

В <code lang="class-name">EnumValues&lt;TValue&gt;</code> ключ другого enum даёт <code lang="csharp">false</code>.

## Перебор строк

```csharp
foreach (var (type, multiplier) in _multipliers)
{
    Debug.Log($"{type}: {multiplier}");
}
```

**Default Value** в перебор не входит, а <code lang="csharp">foreach</code> не выделяет память.

## Если enum изменился

Ключи хранятся по именам членов enum:

| Изменение | Результат |
|---|---|
| Члены переставлены или изменены их числовые значения | Таблица работает как раньше |
| Член переименован или удалён | Строка показывается как `<Missing Ice>` и пропускается с ошибкой в Console; вернёте имя — строка снова работает |
| Enum переименован или перенесён в другой namespace или сборку | <code lang="class-name">EnumValues&lt;TEnum, TValue&gt;</code> работает как раньше; <code lang="class-name">EnumValues&lt;TValue&gt;</code> возвращает **Default Value** и пишет ошибку, пока enum не выбран заново |
| В <code lang="class-name">EnumValues&lt;TValue&gt;</code> выбран другой enum | Ключи сохраняются: вернёте прежний enum — строки снова работают |

## Пример в пакете

Плитки и следы получают цвет из <code lang="class-name">EnumValues&lt;SurfaceType, Color&gt;</code>, а множитель скорости — из <code lang="class-name">EnumValues&lt;float&gt;</code> с выбранным в инспекторе <code lang="csharp">[Flags]</code>-enum: [EnumValues](../../Samples~/EnumValues/Documentation/README.ru.md).

![Персонаж на поверхностях сцены EnumValues](../../Samples~/EnumValues/Documentation/Images/demo.gif)
