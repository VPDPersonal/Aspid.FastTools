# VisualElement Extensions

Интерфейс UI Toolkit одной цепочкой вызовов — без отдельной строки на каждое свойство и стиль.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Ability Config");&#10;title.style.fontSize = 14;&#10;&#10;var header = new VisualElement();&#10;header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.paddingTop = 10;&#10;header.style.paddingBottom = 10;&#10;header.Add(title);</code></pre> | <pre lang="csharp"><code>var header = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(10)&#10;    .AddChild(new Label("Ability Config")&#10;        .SetFontSize(14));</code></pre> |

Сеттер возвращает тот же тип: <code lang="csharp">new Button().SetText("Create")</code> — это <code lang="class-name">Button</code>. <code lang="function">AddChild</code> и другие операции с дочерними элементами возвращают родителя. Методы на <code lang="csharp">style</code>, <code lang="csharp">textEdition</code> и <code lang="csharp">textSelection</code> возвращают этот объект, а не элемент.

## Имена методов

Имена методов строятся по одному правилу:

| Unity | FastTools | Примеры |
|---|---|---|
| свойство <code lang="csharp">x</code> | <code lang="csharp">SetX(value)</code> | <code lang="csharp">tooltip</code> → <code lang="function">SetTooltip</code> |
| свойство <code lang="csharp">isX</code> | <code lang="csharp">SetX(value)</code>, без <code lang="csharp">is</code> | <code lang="csharp">isDelayed</code> → <code lang="function">SetDelayed</code> |
| свойство стиля <code lang="csharp">style.x</code> | <code lang="csharp">SetX(value)</code> на элементе и на <code lang="csharp">style</code> | <code lang="csharp">style.fontSize</code> → <code lang="function">SetFontSize</code> |
| событие <code lang="csharp">x</code> | <code lang="function">AddX</code> / <code lang="function">RemoveX</code> | <code lang="csharp">clicked</code> → <code lang="function">AddClicked</code> |
| свойство-делегат <code lang="csharp">x</code> | <code lang="function">SetX</code>; у <code lang="class-name">Action&lt;…&gt;</code> ещё <code lang="function">AddX</code> / <code lang="function">RemoveX</code> | <code lang="csharp">bindItem</code> → <code lang="function">SetBindItem</code>, <code lang="function">AddBindItem</code> |
| метод <code lang="csharp">M()</code> | <code lang="csharp">MSelf()</code> | <code lang="csharp">Focus()</code> → <code lang="csharp">FocusSelf()</code> |

Правило покрывает свойства и события всех элементов — от <code lang="class-name">VisualElement</code> до <code lang="class-name">MultiColumnTreeView</code>; полный список — в [справочнике API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.UIElements). Дочерние элементы и USS-классы переименованы отдельно — см. разделы ниже. Исключения:

- <code lang="class-name">EnumField</code> и <code lang="class-name">EnumFlagsField</code> получают <code lang="function">Initialize</code> вместо <code lang="function">Init</code>;
- <code lang="csharp">Button.SetClickable</code> принимает и <code lang="class-name">Clickable</code>, и <code lang="class-name">Action</code>;
- <code lang="csharp">IsFocused()</code> проверяет фокус: <code lang="csharp">search.IsFocused()</code> вместо <code lang="csharp">search.focusController?.focusedElement == search</code>.

## Дочерние элементы

| Unity | FastTools |
|---|---|
| <code lang="function">Add</code> | <code lang="function">AddChild</code> |
| несколько <code lang="function">Add</code> подряд | <code lang="function">AddChildren</code> |
| <code lang="function">Insert</code> | <code lang="function">InsertChild</code> |
| несколько <code lang="function">Insert</code> подряд | <code lang="function">InsertChildren</code> |
| <code lang="function">Remove</code> | <code lang="function">RemoveChild</code> |
| <code lang="function">RemoveAt</code> | <code lang="function">RemoveChildAt</code> |
| <code lang="function">Clear</code> | <code lang="function">ClearChildren</code> |

У каждого метода есть вариант <code lang="csharp">…If(condition, …)</code>; <code lang="function">AddChildren</code> и <code lang="function">InsertChildren</code> принимают и <code lang="csharp">params</code>, и коллекции:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (isFree)&#10;    body.Add(helpBox);</code></pre> | <pre lang="csharp"><code>body.AddChildIf(isFree, helpBox);</code></pre> |

## Стили

Одно значение задаёт все стороны, <code lang="csharp">X</code> — левую и правую, <code lang="csharp">Y</code> — верхнюю и нижнюю. Пропущенные именованные параметры оставляют прежнее значение:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>header&#10;    .SetPaddingX(12)&#10;    .SetBorderWidth(bottom: 1);</code></pre> |

| Метод | Задаёт | Варианты |
|---|---|---|
| <code lang="function">SetMargin</code>, <code lang="function">SetPadding</code> | <code lang="csharp">margin…</code>, <code lang="csharp">padding…</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>, <code lang="csharp">Top</code>, <code lang="csharp">Right</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code> |
| <code lang="function">SetBorderWidth</code>, <code lang="function">SetBorderColor</code> | <code lang="csharp">border…Width</code>, <code lang="csharp">border…Color</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>, <code lang="csharp">Top</code>, <code lang="csharp">Right</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code> |
| <code lang="function">SetUnitySlice</code> | <code lang="csharp">unitySlice…</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>, <code lang="csharp">Top</code>, <code lang="csharp">Right</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code> |
| <code lang="function">SetDistance</code> | <code lang="csharp">top</code>, <code lang="csharp">right</code>, <code lang="csharp">bottom</code>, <code lang="csharp">left</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code>; одна сторона: <code lang="function">SetTop</code>, <code lang="function">SetRight</code>, <code lang="function">SetBottom</code>, <code lang="function">SetLeft</code> |
| <code lang="function">SetBorderRadius</code> | <code lang="csharp">border…Radius</code> | <code lang="csharp">Top</code>, <code lang="csharp">Bottom</code>, <code lang="csharp">Left</code>, <code lang="csharp">Right</code>, <code lang="csharp">TopLeft</code>, <code lang="csharp">TopRight</code>, <code lang="csharp">BottomRight</code>, <code lang="csharp">BottomLeft</code> |
| <code lang="function">SetSize</code>, <code lang="function">SetMinSize</code>, <code lang="function">SetMaxSize</code> | <code lang="csharp">width</code> и <code lang="csharp">height</code>, <code lang="csharp">min…</code>, <code lang="csharp">max…</code> | одно значение, два или одно именованное |
| <code lang="function">SetBackgroundPosition</code> | <code lang="csharp">backgroundPositionX</code>, <code lang="csharp">backgroundPositionY</code> | <code lang="csharp">X</code>, <code lang="csharp">Y</code> |

Сторона всегда идёт после свойства: <code lang="csharp">borderTopWidth</code> → <code lang="function">SetBorderWidthTop</code>, <code lang="csharp">borderTopLeftRadius</code> → <code lang="function">SetBorderRadiusTopLeft</code>.

Сеттеры цвета принимают и HTML-строку (<code lang="csharp">"#FFC24D"</code>, <code lang="csharp">"red"</code>), а сеттеры ассетов — путь в <code lang="csharp">Resources</code>:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FFC24D", out var color))&#10;    badge.style.color = color;</code></pre> | <pre lang="csharp"><code>badge.SetColor("#FFC24D");</code></pre> |
| <pre lang="csharp"><code>root.style.backgroundImage = Resources&#10;    .Load&lt;Texture2D&gt;("UI/Card");</code></pre> | <pre lang="csharp"><code>root.SetBackgroundImageFromResources(&#10;    "UI/Card");</code></pre> |

Если строка не разбирается как цвет или по пути нет ассета, метод пишет предупреждение в консоль и оставляет элемент без изменений. Путь в <code lang="csharp">Resources</code> принимают также <code lang="function">SetImageFromResources</code>, <code lang="function">SetSpriteFromResources</code>, <code lang="function">SetVectorImageFromResources</code>, <code lang="function">AddStyleSheetFromResources</code> и <code lang="function">RemoveStyleSheetFromResources</code>.

### Bold и italic

<code lang="csharp">SetNormalUnityFontStyleAndWeight()</code> сбрасывает оба флага; остальные пресеты меняют один флаг и сохраняют второй:

| Метод | Меняет |
|---|---|
| <code lang="function">AddBold…</code> | <code lang="csharp">Normal</code> → <code lang="csharp">Bold</code>, <code lang="csharp">Italic</code> → <code lang="csharp">BoldAndItalic</code> |
| <code lang="function">RemoveBold…</code> | <code lang="csharp">Bold</code> → <code lang="csharp">Normal</code>, <code lang="csharp">BoldAndItalic</code> → <code lang="csharp">Italic</code> |
| <code lang="function">AddItalic…</code> | <code lang="csharp">Normal</code> → <code lang="csharp">Italic</code>, <code lang="csharp">Bold</code> → <code lang="csharp">BoldAndItalic</code> |
| <code lang="function">RemoveItalic…</code> | <code lang="csharp">Italic</code> → <code lang="csharp">Normal</code>, <code lang="csharp">BoldAndItalic</code> → <code lang="csharp">Bold</code> |

> [!NOTE]
> Пресеты читают текущее значение из <code lang="csharp">style</code> элемента, а не итоговый стиль: начертание, заданное в USS, считается <code lang="csharp">Normal</code>.

## USS-классы и таблицы стилей

| Unity | FastTools |
|---|---|
| <code lang="function">AddToClassList</code> | <code lang="function">AddClass</code> |
| <code lang="function">RemoveFromClassList</code> | <code lang="function">RemoveClass</code> |
| <code lang="function">ToggleInClassList</code> | <code lang="function">ToggleClass</code> |
| <code lang="function">EnableInClassList</code> | <code lang="function">EnableClass</code> |
| <code lang="function">ClearClassList</code> | <code lang="function">ClearClasses</code> |
| <code lang="csharp">styleSheets.Add</code> | <code lang="function">AddStyleSheet</code> |
| <code lang="csharp">styleSheets.Remove</code> | <code lang="function">RemoveStyleSheet</code> |

## Значения и события

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.value = 10;</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10);</code></pre> |
| <pre lang="csharp"><code>manaCost.SetValueWithoutNotify(10);</code></pre> | <pre lang="csharp"><code>manaCost.SetValue(10, notify: false);</code></pre> |
| <pre lang="csharp"><code>manaCost.RegisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.AddValueChanged(&#10;    OnManaCostChanged);</code></pre> |
| <pre lang="csharp"><code>manaCost.UnregisterValueChangedCallback(&#10;    OnManaCostChanged);</code></pre> | <pre lang="csharp"><code>manaCost.RemoveValueChanged(&#10;    OnManaCostChanged);</code></pre> |

Типизированные перегрузки есть для всех полей Unity, а при установленном <code lang="csharp">com.unity.mathematics</code> — и для его типов, поэтому <code lang="csharp">AddValueChanged(evt =&gt; …)</code> не требует аргументов типа. Для своих типов — <code lang="csharp">SetValue&lt;T, TValue&gt;</code> и <code lang="csharp">AddValueChanged&lt;TField, TValue&gt;</code>.

## Манипуляторы

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>title.AddClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>var clickable = new Clickable(Refresh);&#10;title.AddManipulator(clickable);</code></pre> | <pre lang="csharp"><code>title.AddClickable(&#10;    Refresh, out var clickable);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new KeyboardNavigationManipulator(&#10;        OnNavigate));</code></pre> | <pre lang="csharp"><code>title.AddKeyboardNavigationManipulator(&#10;    OnNavigate);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new ContextualMenuManipulator(&#10;        BuildMenu));</code></pre> | <pre lang="csharp"><code>title.AddContextualMenuManipulator(&#10;    BuildMenu);</code></pre> |

У <code lang="function">AddClickable</code> и методов <code lang="csharp">Add…Manipulator</code> есть перегрузка с <code lang="csharp">out</code>, которая сохраняет манипулятор для <code lang="function">RemoveManipulatorSelf</code>. <code lang="function">AddClickable</code> принимает и <code lang="class-name">Action&lt;EventBase&gt;</code>, и <code lang="class-name">Action</code> с <code lang="csharp">delay</code> и <code lang="csharp">interval</code>.

## Расширения редактора

| Unity | FastTools |
|---|---|
| <code lang="csharp">bindingPath</code> + <code lang="csharp">Bind(serializedObject)</code> | <code lang="csharp">BindTo(serializedObject, path)</code> |
| <code lang="csharp">bindingPath</code> | <code lang="function">SetBindingPath</code> |
| <code lang="function">BindProperty</code> | <code lang="function">BindPropertyTo</code> |
| <code lang="function">Bind</code> | <code lang="function">BindTo</code> |
| <code lang="function">Unbind</code> | <code lang="function">UnbindFrom</code> |

У <code lang="class-name">PropertyField</code> есть <code lang="function">SetLabel</code> и <code lang="function">AddValueChanged</code> / <code lang="function">RemoveValueChanged</code> с <code lang="class-name">SerializedPropertyChangeEvent</code>:

```csharp
var manaCost = new PropertyField(
        serializedObject.FindProperty("_manaCost"))
    .SetLabel("Mana cost")
    .AddValueChanged(_ => Refresh());
```

Для записи в свойство из собственного кода используйте [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Скрипт и окно-владелец

<code lang="function">AddOpenScriptCommand</code> открывает в IDE скрипт <code lang="class-name">MonoBehaviour</code> или <code lang="class-name">ScriptableObject</code> по двойному клику левой кнопкой; для других объектов ничего не добавляет:

```csharp
title.AddOpenScriptCommand(target);
```

<code lang="csharp">GetOwnerWindow()</code> возвращает окно, на панели которого лежит элемент; иначе — окно в фокусе, затем окно под курсором или <code lang="csharp">null</code>.

## Собственные свойства USS

<code lang="function">TryGetByEnum</code> читает строковое свойство USS и разбирает его как enum без учёта регистра:

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(&#10;        ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw,&#10;        ignoreCase: true,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(&#10;        ThemeProperty,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## Пример в пакете

В [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) окно каталога и инспектор <code lang="class-name">AbilityConfig</code> собраны на этих расширениях: <code lang="class-name">ListView</code> одной цепочкой, <code lang="function">BindTo</code> для полей, <code lang="function">AddOpenScriptCommand</code> на заголовке.

![Окно Ability Catalog из примера EditorTools](../../Samples~/EditorTools/Documentation/Images/demo.gif)
