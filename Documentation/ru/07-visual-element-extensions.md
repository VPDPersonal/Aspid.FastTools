# VisualElement Extensions

Интерфейс UI Toolkit одной цепочкой вызовов — без отдельной строки на каждое свойство и стиль.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var title = new Label("Ability Config");&#10;title.style.fontSize = 14;&#10;&#10;var header = new VisualElement();&#10;header.style.paddingLeft = 12;&#10;header.style.paddingRight = 12;&#10;header.style.paddingTop = 10;&#10;header.style.paddingBottom = 10;&#10;header.Add(title);</code></pre> | <pre lang="csharp"><code>var header = new VisualElement()&#10;    .SetPaddingX(12)&#10;    .SetPaddingY(10)&#10;    .AddChild(new Label("Ability Config")&#10;        .SetFontSize(14));</code></pre> |

Цепочка сохраняет тип: <code lang="csharp">new Button().SetText("Create")</code> — это <code lang="class-name">Button</code>, а <code lang="function">AddChild</code> возвращает родителя, а не добавленный элемент.

## Имена методов

| Unity | FastTools |
|---|---|
| <code lang="csharp">tooltip = "Mana cost"</code> | <code lang="csharp">SetTooltip("Mana cost")</code> |
| <code lang="csharp">isDelayed = true</code> | <code lang="csharp">SetDelayed(true)</code> |
| <code lang="csharp">style.fontSize = 14</code> | <code lang="csharp">SetFontSize(14)</code> |
| <code lang="csharp">clicked += Refresh</code> | <code lang="csharp">AddClicked(Refresh)</code> |
| <code lang="csharp">bindItem = BindRow</code> | <code lang="csharp">SetBindItem(BindRow)</code> |
| <code lang="csharp">Focus()</code> | <code lang="csharp">FocusSelf()</code> |
| <code lang="csharp">Init(theme)</code> | <code lang="csharp">Initialize(theme)</code> |

Остальные методы Unity переименованы по смыслу — они в разделах ниже. Полный список методов — в [справочнике API](https://vpdpersonal.github.io/Aspid.FastTools/ru/api/Aspid.FastTools.UIElements).

## Дочерние элементы

| Unity | FastTools |
|---|---|
| <code lang="function">Add</code> | <code lang="function">AddChild</code>, <code lang="function">AddChildren</code> |
| <code lang="function">Insert</code> | <code lang="function">InsertChild</code>, <code lang="function">InsertChildren</code> |
| <code lang="function">Remove</code> | <code lang="function">RemoveChild</code>, <code lang="function">RemoveChildren</code> |
| <code lang="function">RemoveAt</code> | <code lang="function">RemoveChildAt</code> |
| <code lang="function">Clear</code> | <code lang="function">ClearChildren</code> |

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (isFree)&#10;    body.Add(helpBox);</code></pre> | <pre lang="csharp"><code>body.AddChildIf(isFree, helpBox);</code></pre> |

Вариант <code lang="csharp">…If</code> есть у каждого метода из первой таблицы.

## Стили

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>header.style.marginTop = 4;&#10;header.style.marginBottom = 4;&#10;header.style.borderBottomWidth = 1;</code></pre> | <pre lang="csharp"><code>header&#10;    .SetMarginY(4)&#10;    .SetBorderWidth(bottom: 1);</code></pre> |

| Вызов | Задаёт |
|---|---|
| <code lang="csharp">SetPadding(8)</code> | все стороны |
| <code lang="csharp">SetPaddingX(8)</code> | левую и правую |
| <code lang="csharp">SetPaddingY(8)</code> | верхнюю и нижнюю |
| <code lang="csharp">SetPadding(top: 8, left: 4)</code> | верхнюю и левую, остальные не меняет |
| <code lang="csharp">SetBorderRadiusTop(6)</code> | оба верхних угла |
| <code lang="csharp">SetSize(24, 16)</code> | <code lang="csharp">width</code> и <code lang="csharp">height</code> |
| <code lang="csharp">SetTop(8)</code> | <code lang="csharp">top</code>, как <code lang="csharp">SetDistance(top: 8)</code> |

Остальные методы стилей работают по тому же принципу.

### Цвет из строки и ассеты из Resources

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (ColorUtility.TryParseHtmlString(&#10;        "#FFC24D", out var color))&#10;    badge.style.color = color;</code></pre> | <pre lang="csharp"><code>badge.SetColor("#FFC24D");</code></pre> |
| <pre lang="csharp"><code>root.style.backgroundImage = Resources&#10;    .Load&lt;Texture2D&gt;("UI/Card");</code></pre> | <pre lang="csharp"><code>root.SetBackgroundImageFromResources(&#10;    "UI/Card");</code></pre> |

Вариант <code lang="csharp">…FromResources</code> есть также у <code lang="function">SetImage</code>, <code lang="function">SetSprite</code>, <code lang="function">SetVectorImage</code>, <code lang="function">AddStyleSheet</code> и <code lang="function">RemoveStyleSheet</code>.

### Жирный и курсив

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
| <code lang="function">styleSheets.Add</code> | <code lang="function">AddStyleSheet</code> |
| <code lang="function">styleSheets.Remove</code> | <code lang="function">RemoveStyleSheet</code> |

## Значения и события

| Unity | FastTools |
|---|---|
| <code lang="csharp">value = 10</code> | <code lang="csharp">SetValue(10)</code> |
| <code lang="csharp">SetValueWithoutNotify(10)</code> | <code lang="csharp">SetValue(10, notify: false)</code> |
| <code lang="function">RegisterValueChangedCallback</code> | <code lang="function">AddValueChanged</code> |
| <code lang="function">UnregisterValueChangedCallback</code> | <code lang="function">RemoveValueChanged</code> |

- <code lang="csharp">AddValueChanged(evt =&gt; …)</code> не требует аргументов типа: перегрузки есть для всех полей Unity, а при установленном <code lang="csharp">com.unity.mathematics</code> — и для его типов;
- для своих типов — <code lang="csharp">SetValue&lt;T, TValue&gt;(…)</code> и <code lang="csharp">AddValueChanged&lt;TField, TValue&gt;(…)</code>.

## Фокус

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (manaCost.focusController?&#10;        .focusedElement == manaCost)&#10;    Refresh();</code></pre> | <pre lang="csharp"><code>if (manaCost.IsFocused())&#10;    Refresh();</code></pre> |

## Манипуляторы

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new Clickable(Refresh));</code></pre> | <pre lang="csharp"><code>title.AddClickable(Refresh);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new KeyboardNavigationManipulator(&#10;        OnNavigate));</code></pre> | <pre lang="csharp"><code>title.AddKeyboardNavigationManipulator(&#10;    OnNavigate);</code></pre> |
| <pre lang="csharp"><code>title.AddManipulator(&#10;    new ContextualMenuManipulator(&#10;        BuildMenu));</code></pre> | <pre lang="csharp"><code>title.AddContextualMenuManipulator(&#10;    BuildMenu);</code></pre> |
| <pre lang="csharp"><code>var clickable = new Clickable(Refresh);&#10;title.AddManipulator(clickable);&#10;title.RemoveManipulator(clickable);</code></pre> | <pre lang="csharp"><code>title.AddClickable(&#10;    Refresh, out var clickable);&#10;title.RemoveManipulatorSelf(clickable);</code></pre> |

## Расширения редактора

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>manaCost.bindingPath = "_manaCost";&#10;manaCost.Bind(serializedObject);</code></pre> | <pre lang="csharp"><code>manaCost.BindTo(&#10;    serializedObject, "_manaCost");</code></pre> |
| <pre lang="csharp"><code>manaCost.bindingPath = "_manaCost";</code></pre> | <pre lang="csharp"><code>manaCost.SetBindingPath(&#10;    "_manaCost");</code></pre> |
| <pre lang="csharp"><code>manaCost.BindProperty(property);</code></pre> | <pre lang="csharp"><code>manaCost.BindPropertyTo(property);</code></pre> |
| <pre lang="csharp"><code>manaCost.Unbind();</code></pre> | <pre lang="csharp"><code>manaCost.UnbindFrom();</code></pre> |

У <code lang="class-name">PropertyField</code> есть <code lang="function">SetLabel</code> и <code lang="function">AddValueChanged</code> / <code lang="function">RemoveValueChanged</code> с <code lang="class-name">SerializedPropertyChangeEvent</code>:

```csharp
var manaCost = new PropertyField(
        serializedObject.FindProperty("_manaCost"))
    .SetLabel("Mana cost")
    .AddValueChanged(_ => Refresh());
```

Для записи в свойство из собственного кода используйте [SerializedProperty Extensions](08-serialized-property-extensions.md).

### Открыть скрипт по двойному клику

<code lang="function">AddOpenScriptCommand</code> открывает в IDE скрипт <code lang="class-name">MonoBehaviour</code> или <code lang="class-name">ScriptableObject</code> по двойному клику левой кнопкой; для других объектов ничего не добавляет:

```csharp
title.AddOpenScriptCommand(target);
```

### Окно элемента

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>var panel = title.panel;&#10;var window = Resources&#10;    .FindObjectsOfTypeAll&lt;EditorWindow&gt;()&#10;    .FirstOrDefault(w =&gt; panel ==&#10;        w.rootVisualElement.panel);&#10;if (!window)&#10;    window = EditorWindow.focusedWindow;&#10;if (!window)&#10;    window = EditorWindow.mouseOverWindow;</code></pre> | <pre lang="csharp"><code>var window = title.GetOwnerWindow();</code></pre> |

## Собственные свойства USS

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>if (evt.customStyle.TryGetValue(&#10;        ThemeProperty, out var raw)&#10;    &amp;&amp; Enum.TryParse(raw,&#10;        ignoreCase: true,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> | <pre lang="csharp"><code>if (evt.customStyle.TryGetByEnum(&#10;        ThemeProperty,&#10;        out PreviewTheme theme))&#10;    ApplyTheme(theme);</code></pre> |

## Пример в пакете

Окно каталога и инспектор <code lang="class-name">AbilityConfig</code> в примере [EditorTools](../../Samples~/EditorTools/Documentation/README.ru.md) собраны на этих расширениях.

![Окно Ability Catalog из примера EditorTools](../../Samples~/EditorTools/Documentation/Images/demo.gif)
