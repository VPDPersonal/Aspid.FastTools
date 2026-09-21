# ComponentTypeSelector

`ComponentTypeSelector` меняет тип уже добавленного компонента или ScriptableObject через инспектор. При переключении между наследниками значения общих полей сохраняются.

## Быстрый старт

Добавьте поле в базовый класс. В селекторе появятся совместимые конкретные типы; пункта `<None>` нет.

```csharp
using UnityEngine;
using Aspid.FastTools.Types;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private ComponentTypeSelector _enemyType;
    [SerializeField, Min(0)] private float _health = 100f;
}
```

Сохраните базу в `EnemyBase.cs`. Создайте наследников в **отдельных файлах**, совпадающих с именами классов:

| FastEnemy.cs | ArmoredEnemy.cs |
|---|---|
| <pre lang="csharp"><code>using UnityEngine;&#10;&#10;public sealed class FastEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField] private float _speed = 25f;&#10;&#125;</code></pre> | <pre lang="csharp"><code>using UnityEngine;&#10;&#10;public sealed class ArmoredEnemy : EnemyBase&#10;&#123;&#10;    [SerializeField] private int _armor = 10;&#10;&#125;</code></pre> |

Добавьте **FastEnemy** на GameObject, задайте **Health = 75** и через селектор выберите **ArmoredEnemy**. Общий `Health` сохранится, поле `Speed` исчезнет, появится `Armor`. Уникальные поля прежнего класса не следует считать сохранёнными для обратного переключения.

![Смена типа компонента через ComponentTypeSelector](../Images/component-type-selector.gif)

Смена типа компонента через ComponentTypeSelector

Выбранный класс должен иметь собственный файл скрипта, который распознаёт Unity. Если подходящий скрипт не найден, тип не меняется, а Console показывает предупреждение.

## Пример в пакете

Переключение типа компонента показано в примере [Types](../../Samples~/Types/Documentation/README.ru.md).
