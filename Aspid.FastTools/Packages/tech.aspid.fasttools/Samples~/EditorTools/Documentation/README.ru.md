# Пример EditorTools

Окно редактора и инспектор, собранные в коде на хелперах пакета.

![Кнопка Halve cooldown, +5 MP меняет стоимость и перезарядку, а Undo возвращает прежние значения.](Images/demo.gif)

Кнопка Halve cooldown, +5 MP меняет стоимость и перезарядку, а Undo возвращает прежние значения.

## Как открыть

1. Импортируйте пример: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** у **EditorTools**. Сцены у него нет.
2. Откройте **Tools → Aspid 🐍 → FastTools → Samples → Ability Catalog**: слева способности проекта, среди них четыре ассета из `Data/`, справа — выбранная.

## Попробуйте

1. **ListView одной цепочкой.** Список слева собран одной цепочкой расширений <code lang="class-name">ListView</code>, включая <code lang="function">SetMakeItem</code>, <code lang="function">SetBindItem</code> и <code lang="function">AddSelectionChanged</code>. Наберите текст в поле поиска: <code lang="function">AddValueChanged</code> фильтрует источник, а <code lang="function">RefreshItems</code> перерисовывает список.
2. **Привязка.** Поля имени, описания, стоимости и перезарядки лежат в одном контейнере, привязанном через <code lang="csharp">.BindTo(serializedObject)</code>. Измените имя — список и заголовок обновятся, а Undo вернёт прежнее.
3. **Типизированные сеттеры.** Нажмите **Halve cooldown, +5 MP**. Обработчик записывает два свойства и применяет их один раз, поэтому оба откатываются одним Undo:

   ```csharp
   serializedObject.Update();
   var cooldown = serializedObject.FindProperty("_cooldown");
   var manaCost = serializedObject.FindProperty("_manaCost");
   cooldown.SetFloat(cooldown.floatValue * 0.5f);
   manaCost.SetIntAndApply(manaCost.intValue + 5);
   ```

4. **Окно выбора из кода.** Нажмите **Change…** рядом с **Effect**: <code lang="csharp">TypeSelectorWindow.Show()</code> открывает у кнопки окно выбора <code lang="csharp">[TypeSelector]</code> только с реализациями <code lang="class-name">IAbilityEffect</code> и записывает выбор в строковое свойство. Выберите <code lang="class-name">HealEffect</code> и поменяйте **Mana Cost**: описание эффекта следует за значением, в том числе при Undo и Redo.
5. **Открыть скрипт.** Дважды кликните по заголовку способности справа: <code lang="function">AddOpenScriptCommand</code> откроет `AbilityConfig.cs` в IDE.
6. **Инспектор.** Выберите `Data/Sprint.asset` в окне Project. <code lang="class-name">AbilityConfigEditor</code> рисует карточку с заголовком из <code lang="csharp">GetDisplayName()</code> — «Ability Config», бейдж стоимости и предупреждение, пока **Mana Cost** равен `0`; обоими управляет <code lang="csharp">PropertyField.AddValueChanged()</code>. Поставьте стоимость `10` и верните обратно.
7. **Create.** Нажмите **Create**: новый ассет появится рядом с выбранным и сразу выделится в списке.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/Editor/AbilityCatalogWindow.cs` | Расширения <code lang="class-name">ListView</code>, <code lang="function">BindTo</code>, <code lang="function">SetFloat</code> и <code lang="function">SetIntAndApply</code>, <code lang="csharp">TypeSelectorWindow.Show()</code> с <code lang="class-name">TypeSelectorFilter</code>, <code lang="function">AddOpenScriptCommand</code> |
| `Scripts/Editor/AbilityConfigEditor.cs` | Кастомный инспектор на сеттерах стиля и раскладки, <code lang="function">GetDisplayName</code> |
| `Scripts/AbilityConfig.cs` | Данные; <code lang="csharp">[TypeSelector]</code> на строке эффекта даёт инспектору ассета то же окно выбора |
| `Scripts/Effects/` | Классы эффектов, которые предлагает окно выбора |

Справочник — [VisualElement Extensions](../../../Documentation/ru/07-visual-element-extensions.md), [SerializedProperty Extensions](../../../Documentation/ru/08-serialized-property-extensions.md), [Editor Helpers](../../../Documentation/ru/09-editor-helpers.md) и [TypeSelectorWindow](../../../Documentation/ru/02-serializable-types.md#typeselectorwindow).
