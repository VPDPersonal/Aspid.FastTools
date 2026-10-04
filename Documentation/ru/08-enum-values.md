# EnumValues

Таблица значений по членам enum, которую заполняют в инспекторе, а не в коде.

## Быстрый старт

| До — поля и switch | После — FastTools |
|---|---|
| <pre lang="csharp"><code>[SerializeField]&#10;private float _default = 1f;&#10;[SerializeField]&#10;private float _fire = 1.5f;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt; type switch&#10;&#123;&#10;    DamageType.Fire =&gt; _fire,&#10;    _ =&gt; _default&#10;&#125;;</code></pre> | <pre lang="csharp"><code>[SerializeField]&#10;private EnumValues&lt;DamageType, float&gt;&#10;    _multipliers;&#10;&#10;public float GetMultiplier(&#10;    DamageType type) =&gt;&#10;    _multipliers.GetValue(type);</code></pre> |

<code lang="function">GetValue</code> возвращает значение из подходящей строки таблицы, а если такой строки нет — **Default Value**.

![Таблица Multipliers в инспекторе: строка Fire со значением 1.5 и Default Value 1](../Images/enum-values-multipliers-quick-start.png)

## Заполнение в инспекторе

Для enum без <code lang="csharp">[Flags]</code> задайте общее значение в **Default Value**, а отдельные строки добавьте для тех членов, которым нужно другое значение.

**Populate Missing Enum Members** в контекстном меню заголовка таблицы добавляет строки для недостающих членов enum в конец таблицы и копирует в них **Default Value**.

Для <code lang="csharp">[Flags]</code> автоматически добавляются только объявленные члены enum. Например, если в enum объявлено <code lang="csharp">FireAndIce = Fire | Ice</code>, команда добавит отдельную строку с ключом <code lang="csharp">FireAndIce</code>. Комбинации без отдельного имени можно добавить вручную.

![Populate Missing Enum Members в таблице Multipliers](../Images/enum-values-multipliers-populate.gif)

> [!NOTE]
> Строка, добавленная в пустую таблицу, показывается как `<None>`: пока член не выбран, поиск её пропускает, а при обращении к таблице в Console появляется ошибка.

## Какой вариант выбрать

| Чем отличаются | <code lang="class-name">EnumValues&lt;TEnum, TValue&gt;</code> | <code lang="class-name">EnumValues&lt;TValue&gt;</code> |
|---|---|---|
| Где выбирается enum | Аргумент <code lang="class-name">TEnum</code> | Заголовок таблицы в инспекторе |
| Ключ в <code lang="function">GetValue</code> и <code lang="csharp">foreach</code> | <code lang="class-name">TEnum</code> | <code lang="class-name">System.Enum</code> |
| Упаковка в <code lang="function">GetValue</code> | Нет | Ключ упаковывается |
| Ключ другого enum | Не компилируется | Возвращает **Default Value** |

Значения таблицы задаются в инспекторе; из кода их можно только читать.

В варианте <code lang="class-name">EnumValues&lt;TValue&gt;</code> поле множителей объявляется как <code lang="class-name">EnumValues&lt;float&gt;</code>, а <code lang="class-name">DamageType</code> выбирается в заголовке таблицы:

```csharp
[SerializeField]
private EnumValues<float>
    _multipliers;
```

![DamageType в окне выбора типа в заголовке Multipliers](../Images/enum-values-type-selector.png)

- Выбор enum обязателен: если поле пустое, инспектор показывает **Required type is not set**. Это поле также учитывает [проверка обязательных полей](07-serialize-reference-validation.md#что-проверяет-каждый-запуск).
- При первом обращении к таблице с пустым полем enum в Console появляется предупреждение, а <code lang="function">GetValue</code> возвращает **Default Value**.

## Правила поиска

Ключ строки — выбранный в ней член enum. Если в таблице несколько строк с одинаковым ключом, используется первая сверху. Члены enum с одинаковым числовым значением тоже считаются одним ключом: например, при объявлении <code lang="csharp">Frost = Ice</code> оба имени обозначают одно значение.

| Строки в инспекторе, сверху вниз | Аргумент <code lang="function">GetValue</code> | Возвращаемое значение |
|---|---|---|
| <code lang="csharp">Fire</code> → <code lang="csharp">0.9</code><br/><code lang="csharp">Fire</code> → <code lang="csharp">0.5</code> | <code lang="csharp">DamageType.Fire</code> | <code lang="csharp">0.9</code> |
| <code lang="csharp">Ice</code> → <code lang="csharp">0.5</code> | <code lang="csharp">DamageType.Frost</code> | <code lang="csharp">0.5</code> |

### Флаги

```csharp
[Flags]
public enum DamageType
{
    None = 0,
    Fire = 1,
    Ice = 2,
    Frost = Ice,
    FireAndIce = Fire | Ice,
    Poison = 4
}

[SerializeField]
private EnumValues<DamageType, float>
    _multipliers;
```

Для <code lang="csharp">[Flags]</code> поиск проходит в два этапа:

1. Сначала ищется **точное совпадение**: ключ строки содержит тот же набор флагов, что и переданный аргумент.
2. Если точного совпадения нет, выбирается **первая сверху** строка, все флаги которой есть в переданном значении.

Пусть **Default Value** равен <code lang="csharp">0</code>, а таблица в инспекторе заполнена так:

| Порядок | Ключ | Значение |
|---|---|---|
| 1 | <code lang="csharp">Fire</code> | <code lang="csharp">0.9</code> |
| 2 | <code lang="csharp">Ice</code> | <code lang="csharp">0.5</code> |
| 3 | <code lang="csharp">FireAndIce</code> | <code lang="csharp">0.3</code> |
| 4 | <code lang="csharp">None</code> | <code lang="csharp">1</code> |

Для этой таблицы вызовы <code lang="function">GetValue</code> дадут следующие результаты:

| Аргумент | Значение | Почему |
|---|---|---|
| <code lang="csharp">Fire &#124; Ice</code> | <code lang="csharp">0.3</code> | Точное совпадение с <code lang="csharp">FireAndIce</code> (строка 3): оба флага совпадают |
| <code lang="csharp">Fire &#124; Ice &#124; Poison</code> | <code lang="csharp">0.9</code> | В аргументе есть все флаги строк 1, 2 и 3. Точного совпадения нет, поэтому выбрана строка 1 |
| <code lang="csharp">Ice &#124; Poison</code> | <code lang="csharp">0.5</code> | Подходит <code lang="csharp">Ice</code> (строка 2). Для строк 1 и 3 нужен ещё <code lang="csharp">Fire</code>, которого в аргументе нет |
| <code lang="csharp">Poison</code> | <code lang="csharp">0</code> | Подходящих строк нет — **Default Value** |
| <code lang="csharp">None</code> | <code lang="csharp">1</code> | Строка с нулевым ключом подходит только для нулевого аргумента |

> [!NOTE]
> Если перенести строку <code lang="csharp">FireAndIce</code> выше <code lang="csharp">Fire</code>, вызов для <code lang="csharp">Fire | Ice | Poison</code> вернёт <code lang="csharp">0.3</code>. Так порядок строк задаёт приоритет, когда точного совпадения нет.
>
> Строка со значением, равным **Default Value**, тоже участвует в поиске. Если добавить строку <code lang="csharp">Fire | Poison</code> со значением <code lang="csharp">0</code>, вызов для этой комбинации вернёт <code lang="csharp">0</code> по точному совпадению, вместо <code lang="csharp">0.9</code> из строки <code lang="csharp">Fire</code>.

## Equals()

Метод таблицы проверяет, подходит ли ключ к запросу, не читая значений:

```csharp
var request =
    DamageType.Fire | DamageType.Ice;
var key = DamageType.Fire;
_multipliers.Equals(request, key);
```

| Запрос | Ключ | Результат |
|---|---|---|
| <code lang="csharp">DamageType.Fire &#124; DamageType.Ice</code> | <code lang="csharp">DamageType.Fire</code> | <code lang="csharp">true</code> |
| <code lang="csharp">DamageType.Fire</code> | <code lang="csharp">DamageType.Fire &#124; DamageType.Ice</code> | <code lang="csharp">false</code> |
| <code lang="csharp">DamageType.Fire &#124; DamageType.Ice</code> | <code lang="csharp">DamageType.None</code> | <code lang="csharp">false</code> |
| <code lang="csharp">DamageType.None</code> | <code lang="csharp">DamageType.None</code> | <code lang="csharp">true</code> |

## Перебор строк

```csharp
var total = 0f;
foreach (var entry in _multipliers)
    total += entry.Value;
```

**Default Value** и строки с неразрешёнными ключами в перебор не входят. После первого обращения, которое инициализирует ключи, прямой <code lang="csharp">foreach</code> по таблице не выделяет память.

## Если enum изменился

Ключи хранятся по именам членов enum:

| Изменение | Результат |
|---|---|
| Члены переставлены или изменены их числовые значения | Строки остаются привязаны к именам. Изменение алиасов или состава битов флагов может изменить результат поиска |
| Член переименован или удалён | Строка показывается как `<Missing Ice>` и пропускается с ошибкой в Console; вернёте имя — строка снова работает |
| Enum переименован или перенесён в другой namespace или сборку | <code lang="class-name">EnumValues&lt;TEnum, TValue&gt;</code> работает как раньше; <code lang="class-name">EnumValues&lt;TValue&gt;</code> возвращает **Default Value** и пишет ошибку, пока enum не выбран заново |
| В <code lang="class-name">EnumValues&lt;TValue&gt;</code> выбран другой enum | Ключи сохраняются: вернёте прежний enum — строки снова работают |

## Пример в пакете

В примере [EnumValues](../../Samples~/EnumValues/Documentation/README.ru.md) плитки и следы получают цвет из <code lang="class-name">EnumValues&lt;SurfaceType, Color&gt;</code>, а множитель скорости — из <code lang="class-name">EnumValues&lt;float&gt;</code> с <code lang="csharp">[Flags]</code>-enum, выбранным в инспекторе.

![При переходе на другую поверхность меняются цвет следа и скорость персонажа.](../../Samples~/EnumValues/Documentation/Images/demo.gif)
