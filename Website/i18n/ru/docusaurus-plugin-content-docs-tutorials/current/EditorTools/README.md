# Пример EditorTools

Окно редактора, собранное в коде на хелперах пакета.

![Кнопка Reset to defaults возвращает стоимости и перезарядке значения по умолчанию, а Undo — прежние значения.](../../../../../tutorials/EditorTools/Images/demo.gif)

Кнопка Reset to defaults возвращает стоимости и перезарядке значения по умолчанию, а Undo — прежние значения.

## Как открыть

1. Импортируйте пример: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** у **EditorTools**. Сцены у него нет.
2. Откройте **Tools → Aspid 🐍 → FastTools → Samples → Ability Catalog**: слева способности проекта, среди них четыре ассета из `Data/`, справа — выбранная.

## Попробуйте

1. **ListView одной цепочкой.** Список слева собран одной цепочкой расширений <code lang="class-name">ListView</code>, включая <code lang="function">SetMakeItem</code>, <code lang="function">SetBindItem</code> и <code lang="function">AddSelectionChanged</code>. Наберите текст в поле поиска: <code lang="function">AddValueChanged</code> фильтрует источник, а <code lang="function">RefreshItems</code> перерисовывает список.
2. **Привязка.** Поля имени, описания, стоимости и перезарядки лежат в одном контейнере, привязанном через <code lang="csharp">.BindTo(serializedObject)</code>. Измените имя — список и заголовок обновятся, а Undo вернёт прежнее.
3. **Типизированные сеттеры.** Выберите **Fireball**: кнопка **Reset to defaults** видна, пока **Mana Cost** или **Cooldown** отличается от инициализатора поля в <code lang="class-name">AbilityConfig</code>; значения читаются из нового экземпляра. Нажмите её: обработчик записывает два свойства и применяет их один раз, поэтому кнопка скрывается, а одно Undo откатывает оба:

   ```csharp
   serializedObject.Update();
   serializedObject.FindProperty("_cooldown").SetFloat(defaults.Cooldown);
   serializedObject.FindProperty("_manaCost").SetIntAndApply(defaults.ManaCost);
   ```

4. **Окно выбора из кода.** Кликните по полю **Effect**: <code lang="csharp">TypeSelectorWindow.Show()</code> открывает у поля окно выбора <code lang="csharp">[TypeSelector]</code> только с реализациями <code lang="class-name">IAbilityEffect</code> и записывает выбор в строковое свойство. На эффектах стоит <code lang="csharp">[TypeSelectorDisplay]</code>: окно выбора показывает их как Damage, Heal и Shield в группе Effects, с иконками и подсказками, а поле — то же имя и иконку. Стиль `EffectPicker.uss`, переданный в <code lang="csharp">TypeSelectorWindow.Show()</code>, оформляет окно выбора как каталог. Выберите **Heal** и поменяйте **Mana Cost**: описание эффекта под полем следует за значением, в том числе при Undo и Redo.
5. **Открыть скрипт.** Наведите курсор на заголовок способности справа: подсказка называет скрипт через <code lang="csharp">GetDisplayName()</code> — «Ability Config». Дважды кликните по заголовку: <code lang="function">AddOpenScriptCommand</code> откроет `AbilityConfig.cs` в IDE.
6. **Создание и удаление.** Нажмите **+**: новый ассет появится рядом с выбранным и сразу выделится в списке. Наведите курсор на строку: её кнопки выделяют ассет в окне Project, а после подтверждения переносят его в корзину.
7. **Тема FastTools.** Откройте **Tools → Aspid 🐍 → FastTools → Settings** и выберите другую **Theme** в разделе **Appearance**: <code lang="function">AddAspidWindowTheme</code> сразу перекрасит открытый каталог.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/Editor/AbilityCatalogWindow.cs` | Расширения <code lang="class-name">ListView</code>, <code lang="function">BindTo</code>, <code lang="function">SetFloat</code> и <code lang="function">SetIntAndApply</code>, <code lang="csharp">TypeSelectorWindow.Show()</code> с <code lang="class-name">TypeSelectorFilter</code>, <code lang="function">GetDisplayName</code>, <code lang="function">AddOpenScriptCommand</code>, <code lang="function">AddAspidWindowTheme</code> |
| `Scripts/Editor/EffectPicker.uss` | Окно выбора в стиле каталога, на тех же токенах |
| `Scripts/Editor/AbilityCatalog.uss` | Цвета окна, взятые только из токенов `--aspid-colors-*` темы |
| `Scripts/AbilityConfig.cs` | Данные; <code lang="csharp">[TypeSelector]</code> на строке эффекта даёт инспектору ассета то же окно выбора |
| `Scripts/Effects/` | Классы эффектов, которые предлагает окно выбора, с именами, группой и иконками из <code lang="csharp">[TypeSelectorDisplay]</code> |

Справочник — [VisualElement Extensions](../../../docusaurus-plugin-content-docs/current/10-visual-element-extensions.md), [SerializedProperty Extensions](../../../docusaurus-plugin-content-docs/current/11-serialized-property-extensions.md), [Editor Helpers](../../../docusaurus-plugin-content-docs/current/12-editor-helpers.md) и [TypeSelectorWindow](../../../docusaurus-plugin-content-docs/current/03-type-selector.md#typeselectorwindow).
