# Примеры

К каждой возможности прилагается пример: небольшая сцена или editor-инструмент, который делает с этой возможностью что-то видимое, и короткий `README.md` о том, как его открыть, со ссылкой на полное руководство на [сайте](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials). Импортируйте их из Package Manager (**Aspid.FastTools → Samples**) или откройте вкладку **Welcome** (`Tools → Aspid 🐍 → FastTools → Welcome`).

Это рекомендуемый порядок знакомства; каждый пример можно изучать отдельно.

| Пример | Что показывает |
|---|---|
| [EnumValues](EnumValues/Documentation/README.ru.md) | Персонаж на плитках поверхностей: оба варианта `EnumValues`, значения по умолчанию, правила поиска для `[Flags]` |
| [Types](Types/Documentation/README.ru.md) | Спавнер врагов: `SerializableMonoScript<T>`, `SerializableType<T>`, `[TypeSelectorDisplay]`, `[TypeSelector]` со ссылкой на член, `ComponentTypeSelector` |
| [SerializeReferences](SerializeReferences/Documentation/README.ru.md) | Турель с полиморфным оружием: пикер `[SerializeReference]` во всех формах поля, сломанные ассеты для инструментов ремонта, IMGUI-инспектор |
| [EditorTools](EditorTools/Documentation/README.ru.md) | Окно редактора и инспектор: fluent-расширения `VisualElement`, сеттеры `SerializedProperty`, editor-хелперы, `TypeSelectorWindow` |
| [ProfilerMarkers](ProfilerMarkers/Documentation/README.ru.md) | Симуляция стаи: сгенерированное дерево маркеров в Profiler |

## Render pipeline

Сцены используют собственный unlit-шейдер и однотонный фон камеры, поэтому в Built-in Render Pipeline и URP выглядят одинаково. HDRP рисует их темнее, поверх своего неба по умолчанию. Чтобы вернуть исходный вид в HDRP, выберите **Main Camera**, задайте **Background Type** = **Color** и **Background Color** = `#060A0F`, а затем добавьте глобальный Volume с **Tonemapping → Mode** = **None** и **Bloom → Intensity** = `0`. Тогда **Sample Themes** перекрашивает всё, кроме фона: он остаётся **Background Color**.

## Тема для записи

Откройте `Tools → Aspid 🐍 → FastTools → Sample Themes` и выберите **Light**, **Dark** или **Authored**. Предпросмотр работает в сценах EnumValues, Types, SerializeReferences и ProfilerMarkers, в том числе в Play Mode; **Authored** возвращает исходные цвета сцены.

Палитры в этом окне общие для четырёх примеров и сохраняются в `ProjectSettings/AspidFastToolsSampleThemes.asset`. Меняйте оттенки здесь перед записью. В разделе **EnumValues light surfaces** также настраиваются цвета плиток, следов и подписей. Предпросмотр использует временную копию палитры поверхностей; при сохранении сцены остаются исходные ссылки и ассеты материалов.

В светлой палитре также доступны **Cyan**, **Grid** и **Animated Color Lift** — осветление цветов эффектов, врагов и стаи. У Ability Catalog свой переключатель **Theme → Editor / Dark / Light** в заголовке окна.
