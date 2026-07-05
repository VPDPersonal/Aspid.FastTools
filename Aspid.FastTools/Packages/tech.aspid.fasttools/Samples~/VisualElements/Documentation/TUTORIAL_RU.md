# VisualElements — Пошаговый туториал

Расширения `VisualElement` из Aspid.FastTools превращают многословный, основанный на мутациях API UIToolkit в
**fluent, цепочечный**. Каждый `Set*` / `Add*` возвращает элемент, на котором был вызван, поэтому весь виджет — стиль,
раскладка, дети и поведение — это одно выражение:

```csharp
var badge = new Label("FREE")
    .SetColor(Color.white)
    .SetPadding(top: 3, bottom: 3, left: 10, right: 10)
    .SetBorderRadius(10);
```

В отличие от остальных примеров, уроки здесь — не поля инспектора, которые вы крутите, а **карточки кастомного
инспектора, собранного целиком в коде**. Каждая карточка `STEP` в `Scripts/Editor/VisualElementsTutorialEditor.cs` —
это один урок; читайте карточку в инспекторе и метод, который она демонстрирует, рядом.

## Открыть туториал

1. Откройте окно Welcome (**Tools → Aspid 🐍 → FastTools → Welcome**) и импортируйте пример **VisualElements**.
2. Откройте **`Scenes/VisualElementsTutorial.unity`** и выберите GameObject **VisualElements Tutorial**.
3. Инспектор покажет пять карточек STEP. Play Mode не нужен — всё работает в инспекторе редактора.

> **Компонент не содержит UI.** `VisualElementsTutorial` — это просто три сериализуемых ручки; весь инспектор
> собирается в `VisualElementsTutorialEditor.CreateInspectorGUI()`. Это разделение — простой объект-данные и
> fluent-редактор — и есть паттерн для ваших собственных инспекторов.

---

## Урок 1 — Основы fluent-стилей

**Карточка:** *STEP 1* — цветной образец.

Каждый стилевой сеттер возвращает элемент, поэтому стилизация — одна непрерывная цепочка: без локальной переменной,
без повторов `element.style.paddingTop = …`:

```csharp
var swatch = new VisualElement()
    .SetHeight(44)
    .SetBackgroundColor(_accent)
    .SetBorderColor(_cardBorder)
    .SetBorderWidth(1)
    .SetBorderRadius(8);
```

- Каждый `Set*` соответствует одному свойству `IStyle`, но читается как фраза и не разрывает цепочку.
- Обобщённый возвращаемый тип — это собственный тип элемента, поэтому цепочка сохраняет его API: `Label` остаётся
  `Label` после `.SetColor(...)`, и специфичные для `Label` сеттеры остаются доступны дальше.

---

## Урок 2 — Текст и пресеты шрифта

**Карточка:** *STEP 2* — четыре подписи: обычная, жирная, курсив, с разрядкой.

Вместо передачи сырого enum `FontStyle` **пресеты** называют намерение и сочетаются с остальными текстовыми сеттерами:

```csharp
new Label("...").SetNormalUnityFontStyleAndWeight();
new Label("...").AddBoldUnityFontStyleAndWeight();
new Label("...").AddItalicUnityFontStyleAndWeight();
new Label("...").SetFontSize(11).SetLetterSpacing(4);
```

- `Add*` **комбинирует** с текущим стилем (жирный *и* курсив — это `AddBold…().AddItalic…()`); `Set*` **заменяет** его.
  Парные `Remove*` (`RemoveBold…`, `RemoveItalic…`) снимают один флаг, не трогая другой.
- Эти пресеты есть в двух вариантах — для `VisualElement` и для `IStyle`, — поэтому работают и на элементе напрямую, и
  на разрешённом `IStyle`.

---

## Урок 3 — Раскладка и композиция

**Карточка:** *STEP 3* — строка шапки: имя способности слева, тег `ABILITY` справа.

Сеттеры flex-раскладки плюс `AddChild` собирают составной элемент на месте. `AddChild` возвращает **родителя**, поэтому
дети добавляются цепочкой:

```csharp
var row = new VisualElement()
    .SetFlexDirection(FlexDirection.Row)
    .SetAlignItems(Align.Center)
    .SetJustifyContent(Justify.SpaceBetween)
    .AddChild(name)
    .AddChild(tag);
```

- `AddChild` возвращает контейнер, на котором вызван (не ребёнка) — именно это позволяет добавлять несколько детей
  fluent-цепочкой; сам ребёнок полностью собран своей цепочкой перед передачей.
- Подпись имени берётся из `VisualElementsTutorial.AbilityName` — измените **Ability Name** на компоненте и переоткройте
  инспектор, чтобы увидеть, как значение проходит насквозь.

---

## Урок 4 — Реактивный UI

**Карточка:** *STEP 4* — поле Mana Cost с живым бейджем под ним.

`PropertyField(...).AddValueChanged(callback)` вызывает ваш колбэк на каждую правку — та же обвязка, что и в
демо-инспекторе `AbilityConfig`:

```csharp
var badge = new Label().AddBoldUnityFontStyleAndWeight();
var manaField = new PropertyField(serializedObject.FindProperty("_manaCost"))
    .AddValueChanged(_ => Refresh());

void Refresh()
{
    var isFree = tutorial.ManaCost is 0;
    var color = isFree ? _warning : _accent;
    badge.SetText(isFree ? "FREE" : $"{tutorial.ManaCost} MP")
        .SetColor(color)
        .SetBorderColor(color);
}
```

Отредактируйте **Mana Cost** в карточке: бейдж перекрашивается и при `0` переключается на **FREE**. `AddValueChanged`
привязывает колбэк и возвращает поле, поэтому остаётся внутри цепочки сборки.

---

## Урок 5 — Широта расширений элементов

**Карточка:** *STEP 5* — ProgressBar, HelpBox и Button.

Тот же fluent-стиль расширяет каждый встроенный элемент, у каждого свои типизированные сеттеры:

```csharp
var bar = new ProgressBar().SetLowValue(0f).SetHighValue(1f);
bar.SetValue(charge).SetTitle($"{charge:P0} charged");

new HelpBox().SetMessageType(HelpBoxMessageType.Info).SetText("Fully charged.").SetDisplay(DisplayStyle.Flex);

new Button().SetText("Log charge").AddClicked(() => Debug.Log($"Charge is {tutorial.Charge:P0}"));
```

- Потяните **Charge** в карточке: полоса заполняется, её заголовок обновляется вживую; HelpBox появляется (`SetDisplay`)
  только на 100%.
- `AddClicked(Action)` регистрирует обработчик клика и возвращает кнопку — нажмите **Log charge**, чтобы вывести значение.
- `SetDisplay(DisplayStyle.None)` убирает элемент из раскладки (место не резервируется); `visible` только скрыл бы его,
  сохранив занимаемый бокс.

---

## Стилизация через USS вместо инлайна

Уроки стилизуют инлайн ради самодостаточного демо, но те же расширения ведут и по USS-пути:

```csharp
element.AddClass("aspid-card");   // добавить USS-класс; стилизовать в .uss
element.RemoveClass("aspid-card");
```

Для всего сложнее одноразового инспектора предпочитайте USS-классы инлайновым цветам — код только зовёт `.AddClass(...)`,
а внешний вид живёт в таблице стилей. Собственные редакторные компоненты пакета следуют ровно этому разделению.

---

## Где смотреть в коде

| Файл | Что показывает |
|---|---|
| `Scripts/Editor/VisualElementsTutorialEditor.cs` | Все пять уроков как карточки `STEP`, каждая собрана fluent-API |
| `Scripts/Tutorial/VisualElementsTutorial.cs` | Простой компонент-данные, который рисует редактор (три сериализуемые ручки) |
| `Scripts/AbilityConfig.cs` / `Scripts/Editor/AbilityConfigEditor.cs` | Тот же API на реальном инспекторе `ScriptableObject` (демо из README) |
| [README.md](README.md) | Компактный разбор демо-инспектора |
