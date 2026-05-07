# Пример VisualElements

Кастомный UIToolkit Inspector для `ScriptableObject` `AbilityConfig`, демонстрирующий fluent-API расширений `VisualElement` — карточный inspector, собранный целиком в коде, со статус-бейджем, реагирующим на правки Mana Cost.

Смотрите:

- `Scripts/Editor/AbilityConfigEditor.cs:23` — `CreateInspectorGUI` собирает карточку из обычных `VisualElement` / `Label` / `HelpBox` с цепочкой Aspid fluent-расширений (`.SetPadding`, `.SetBorderRadius`, `.SetBorderColor`, `.SetFlexDirection`, `.AddChild`).
- `Scripts/Editor/AbilityConfigEditor.cs:38` — `target.GetScriptName()` (из `Aspid.FastTools.Editors`) используется как заголовок шапки.
- `Scripts/Editor/AbilityConfigEditor.cs:65` — `PropertyField(...).AddValueChanged(_ => UpdateState())` перезапускает логику бейджа и help-box при каждой правке Mana Cost.
- `Scripts/Editor/AbilityConfigEditor.cs:88` — `UpdateState()` переключает текст и цвет бейджа и `helpBox.SetDisplay(...)`, когда `ManaCost is 0`.

Чтобы попробовать:

1. Выберите `ScriptableObjects/fireball_1.asset` (платная) или `ScriptableObjects/fireball_free.asset` в окне Project — кастомный inspector появится в панели Inspector.
2. Отредактируйте поля. Установите `Mana Cost` в `0`, чтобы увидеть, как статус-бейдж переключится на "FREE", и появится встроенный help box с предупреждением.
3. Или создайте свой через `Assets > Create > Aspid > Samples > FastTools > Ability Config`.
