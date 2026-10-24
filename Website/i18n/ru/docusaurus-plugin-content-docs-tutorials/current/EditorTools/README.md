# Пример EditorTools

Окно редактора, собранное в коде на хелперах пакета.

![Кнопка Reset to defaults возвращает стоимости и перезарядке значения по умолчанию, а Undo — прежние значения.](../../../../../tutorials/EditorTools/Images/demo.gif)

Кнопка Reset to defaults возвращает стоимости и перезарядке значения по умолчанию, а Undo — прежние значения.

## Как открыть

1. Импортируйте пример: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** у **EditorTools**. Сцены у него нет.
2. Откройте **Tools → Aspid 🐍 → FastTools → Samples → Ability Catalog**: слева способности, справа выбранная.

## Попробуйте

1. **ListView одной цепочкой.** Список собран одной цепочкой расширений <code lang="class-name">ListView</code>. Наберите текст в поле поиска — список отфильтруется.
2. **Привязка.** Поля привязаны через <code lang="csharp">.BindTo(serializedObject)</code>. Измените имя — список и заголовок обновятся.
3. **Сеттеры свойств.** Выберите **Fireball** и нажмите **Reset to defaults**: **Mana Cost** и **Cooldown** применяются один раз, поэтому одно Undo откатывает оба.

   ```csharp
   serializedObject.FindProperty("_cooldown")
       .Update()
       .SetFloat(AbilityConfig.DefaultCooldown);
   serializedObject.FindProperty("_manaCost")
       .SetIntAndApply(AbilityConfig.DefaultManaCost);
   ```

4. **Окно выбора из кода.** Кликните по **Effect**: <code lang="csharp">TypeSelectorWindow.Show()</code> открывает окно выбора <code lang="csharp">[TypeSelector]</code> только с классами <code lang="class-name">IAbilityEffect</code>.
5. **Открыть скрипт.** Дважды кликните по заголовку способности: <code lang="function">AddOpenScriptCommand</code> откроет `AbilityConfig.cs`. Подсказка заголовка называет скрипт через <code lang="csharp">GetDisplayName()</code>.
6. **Тема FastTools.** Выберите другую **Theme** в **Tools → Aspid 🐍 → FastTools → Settings** → **Appearance**: <code lang="function">AddAspidWindowTheme</code> сразу перекрасит каталог.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/Editor/AbilityCatalogWindow.cs` | Все вызовы выше |
| `Styles/` | Цвета из токенов темы `--aspid-colors-*` |
| `Scripts/Effects/` | Имена, группа и иконки из <code lang="csharp">[TypeSelectorDisplay]</code> |

Справочник — [VisualElement Extensions](../../../docusaurus-plugin-content-docs/current/10-visual-element-extensions.md), [SerializedProperty Extensions](../../../docusaurus-plugin-content-docs/current/11-serialized-property-extensions.md), [Editor Helpers](../../../docusaurus-plugin-content-docs/current/12-editor-helpers.md) и [TypeSelectorWindow](../../../docusaurus-plugin-content-docs/current/03-type-selector.md#typeselectorwindow).
