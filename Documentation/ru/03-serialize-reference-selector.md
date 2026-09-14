# SerializeReference Selector

Выбирайте реализацию интерфейса или базового класса прямо в поле `[SerializeReference]`. Селектор создаёт экземпляр, показывает его поля и переносит совместимые данные при смене типа. Если сохранённый тип потерян, действия восстановления появляются рядом с полем.

![Смена Pistol на Shotgun сохраняет Damage = 37 и добавляет поле Pellets](../Images/aspid_fasttools_serialize_reference_selector.gif)

Смена Pistol на Shotgun сохраняет Damage = 37 и добавляет поле Pellets

<a id="inspector-type-dropdown"></a>

## Быстрый старт

Добавьте `[TypeSelector]` рядом с `[SerializeReference]`. Реализации должны быть сериализуемыми классами, совместимыми с типом поля.

```csharp
using System;
using UnityEngine;
using Aspid.FastTools.Types;

public interface IWeapon
{
    void Fire();
}

[Serializable]
public sealed class Pistol : IWeapon
{
    [SerializeField, Min(0)] private int _damage = 10;

    public void Fire() => Debug.Log($"Pistol: {_damage} dmg");
}

[Serializable]
public sealed class Shotgun : IWeapon
{
    [SerializeField, Min(0)] private int _damage = 20;
    [SerializeField, Min(1)] private int _pellets = 6;

    public void Fire() => Debug.Log($"Shotgun: {_damage} dmg, {_pellets} pellets");
}

public sealed class Loadout : MonoBehaviour
{
    [TypeSelector]
    [SerializeReference] private IWeapon _primary;
}
```

1. Добавьте `Loadout` на GameObject и откройте поле **Primary**.
2. Выберите **Pistol** и измените **Damage** на `37`.
3. Переключите тип на **Shotgun**: значение **Damage** сохранится, рядом появится **Pellets**.
4. Выберите `<None>`, чтобы очистить ссылку.

Готовая сцена с оружием и вложенными модификаторами есть в [примере SerializeReferences](../../Samples~/SerializeReferences/Documentation/README.ru.md).

## Настройка выбора

| Задача | Как сделать |
|---|---|
| Ограничить реализации дополнительным интерфейсом | `[TypeSelector(typeof(IMelee))]`, где `IMelee` — ваш интерфейс |
| Сделать заполнение обязательным | `[TypeSelector(Required = true)]` |
| Изменить имя, группу или иконку кандидата | [`TypeSelectorDisplay`](02-serializable-types.md#typeselectordisplay) на классе |
| Скрыть класс из обычного выбора | `[TypeSelectorDisplay(Hidden = true)]` |
| Хранить несколько реализаций | Добавить оба атрибута к массиву или `List<IWeapon>` |

`Required` показывает предупреждение у пустого поля. Для проверки таких полей в CI нужен флаг [`-srGateRequired`](04-serialize-reference-tooling.md#запуск-в-ci).

Атрибут работает в инспекторах IMGUI и UI Toolkit и не попадает в сборку плеера. Для хранения **имени типа** в строке или обёртке используйте [Serializable Type System](02-serializable-types.md).

## Работа с данными

При смене реализации переносятся поля, совпадающие по имени и сериализуемой форме. Новые поля получают значения нового экземпляра; перенос несовместимых полей не гарантируется.

Правый клик по заголовку открывает **Copy / Paste**. Вставка создаёт независимый экземпляр в совместимом поле. При мультивыделении выбор типа или вставка применяется к каждому объекту в одной группе Undo; разные исходные значения отображаются как смешанное состояние.

### Вложенные ссылки

Внутренние поля `[SerializeReference]`, включая массивы и списки, получают такой же селектор без повторения `[TypeSelector]` на каждом уровне. Автоматическая отрисовка охватывает восемь уровней вложенности, после чего используется стандартная отрисовка Unity.

Если у дочернего поля уже есть собственный `[TypeSelector]` или `[CustomPropertyDrawer]` для его типа, этот drawer сохраняется.

### Generic-типы

Селектор умеет закрывать generic-кандидатов по типу поля. Например, для поля `IConverter<string, string>` кандидат `Sequence<T> : IConverter<T, T>` показывается как `Sequence<String>`. Если аргумент вывести нельзя, окно предлагает выбрать его на отдельной странице.

<details>
<summary>Совместимость и ограничения generic-аргументов</summary>

Кандидат исключается, если его нельзя закрыть под тип поля. Например, `ToString<TFrom> : IConverter<TFrom, string>` не подходит полю `IConverter<float, float>`. Если выходной параметр `IConverter` объявлен ковариантным, он может подойти полю `IConverter<float, object>`.

Аргумент, выведенный из поля, обязан быть сериализуемым как значение только там, где кандидат хранит его как значение. Параметр, используемый за `[SerializeReference]`, проверяется по правилам managed-ссылок. Страница ручного выбора аргументов предлагает сериализуемые типы.

</details>

<a id="repairing-broken-references"></a>

## Восстановление потерянного типа

После переименования, переноса или удаления класса сохранённое имя типа может перестать разрешаться. У поля появляется **Missing type**; пока данные ссылки остаются в ассете, их можно переназначить существующей реализации.

![Потерянная ссылка с действиями Fix и Smart Fix в инспекторе](../Images/aspid_fasttools_serialize_reference_repair.png)

Потерянная ссылка с действиями Fix и Smart Fix в инспекторе

- **Fix** открывает окно выбора замены и переназначает тип с сохранением данных. Работает во вложенных полях, сохранённых ассетах и Prefab Mode.
- **Smart Fix** предлагает вероятную замену по `[MovedFrom]`, имени, namespace или сборке. Применяется только по нажатию пользователя.

Для проверки и восстановления сразу нескольких ассетов переходите к [SerializeReference Tooling](04-serialize-reference-tooling.md).

## Общие ссылки и Make unique

Два поля могут хранить один экземпляр: изменение его данных отразится в обоих местах. Селектор помечает такое состояние как **Shared reference**.

![Действие Make unique создаёт независимую копию общей ссылки](../Images/aspid_fasttools_serialize_reference_make_unique.png)

Действие Make unique создаёт независимую копию общей ссылки

Нажмите **Make unique**, если поля должны редактироваться независимо. Автоматическое создание независимой копии при дублировании элемента списка управляется настройкой **Auto de-alias duplicated list elements** в [настройках FastTools](04-serialize-reference-tooling.md#проверка-перед-сборкой).

## Если нужного типа нет в списке

Проверьте, что класс сериализуемый, конкретный, совместим с типом поля и дополнительными ограничениями, не наследует `UnityEngine.Object` и не помечен `Hidden = true`.

Анализатор помогает найти ошибки до открытия инспектора: `AFT0004` сообщает о несовместимости с `UnityEngine.Object`, `AFT0005` предупреждает о потенциально пустом селекторе.
