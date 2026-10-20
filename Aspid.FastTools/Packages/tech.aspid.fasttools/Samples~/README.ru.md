# Примеры

К каждой возможности прилагается пример: небольшая сцена или editor-инструмент, который делает с этой возможностью что-то видимое. Полное руководство к нему — на [сайте](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials); на него ссылаются описание примера в Package Manager и карточка примера на вкладке **Welcome**. Импортируйте их из Package Manager (**Aspid.FastTools → Samples**) или откройте вкладку **Welcome** (`Tools → Aspid 🐍 → FastTools → Welcome`).

Это рекомендуемый порядок знакомства; каждый пример можно изучать отдельно.

| Пример | Что показывает |
|---|---|
| [EnumValues](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials/enum-values) | Персонаж на плитках поверхностей: оба варианта `EnumValues`, значения по умолчанию, правила поиска для `[Flags]` |
| [Types](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials/types) | Спавнер врагов: `SerializableMonoScript<T>`, `SerializableType<T>`, `[TypeSelectorDisplay]`, `[TypeSelector]` со ссылкой на член, `ComponentTypeSelector` |
| [SerializeReferences](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials/serialize-references) | Турель с полиморфным оружием: пикер `[SerializeReference]` во всех формах поля, сломанные ассеты для инструментов ремонта, IMGUI-инспектор |
| [EditorTools](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials/editor-tools) | Окно редактора: fluent-расширения `VisualElement`, сеттеры `SerializedProperty`, editor-хелперы, `TypeSelectorWindow` |
| [ProfilerMarkers](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials/profiler-markers) | Симуляция стаи: сгенерированное дерево маркеров в Profiler |

## Тема для записи

Откройте `Tools → Aspid 🐍 → FastTools → Sample Themes` и выберите **Light**, **Dark** или **Authored**. Предпросмотр работает в сценах EnumValues, Types, SerializeReferences и ProfilerMarkers, в том числе в Play Mode; **Authored** возвращает исходные цвета сцены.

Палитры в этом окне общие для четырёх примеров и сохраняются в `ProjectSettings/AspidFastToolsSampleThemes.asset`. Меняйте оттенки здесь перед записью. В разделе **EnumValues light surfaces** также настраиваются цвета плиток, следов и подписей. Предпросмотр использует временную копию палитры поверхностей; при сохранении сцены остаются исходные ссылки и ассеты материалов.

В светлой палитре также доступны **Cyan**, **Grid** и **Animated Color Lift** — осветление цветов эффектов, врагов и стаи. У Ability Catalog свой переключатель **Theme → Editor / Dark / Light** в заголовке окна.
