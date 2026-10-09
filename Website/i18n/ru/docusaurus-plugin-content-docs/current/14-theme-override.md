# Theme Override

Свои цвета для окон FastTools и окна выбора типа, без правок пакета.

## Быстрый старт

1. Откройте **Tools → Aspid 🐍 → FastTools → Settings** и найдите **Appearance**. Тот же раздел есть в **Preferences → Aspid.FastTools**.
2. Нажмите **Create template…** и выберите, где в проекте сохранить файл. FastTools запишет туда `.uss`-файл и назначит его как **Theme override**.
3. В файле раскомментируйте нужные токены и измените значения:

```css
:root {
    --aspid-colors-bg-dark: rgb(22, 30, 52);
    --aspid-colors-surface-card: rgba(32, 44, 72, 0.6);
}
```

Назначение или очистка **Theme override** перекрашивает открытые окна сразу. Чтобы вернуть стандартный вид, очистите поле.

## Где работает

- окно FastTools: Welcome, Asset References, Project References, Settings;
- страницы FastTools в **Preferences** и **Project Settings**;
- окно выбора типа.

> [!NOTE]
> Поля в обычном инспекторе начинают с тёмной палитры на любой теме и часть цветов берут из темы редактора Unity, поэтому токены меняют лишь часть их вида.

## Токены

Файл накладывается поверх встроенной палитры, поэтому объявляйте только те токены, которые хотите изменить. В светлой теме Unity окно FastTools и его страницы сначала берут `Aspid-FastTools-Default-Light.uss`, а override всё равно идёт после неё.

| Токены | Что красят |
|---|---|
| `--aspid-colors-surface-canvas` | Фон окна под точками |
| `--aspid-colors-surface-card`, `--aspid-colors-surface-card-dim` | Карточки, кнопки и поля ввода; приглушённая карточка |
| `--aspid-colors-bg-*` | Блоки, кнопки, подсказки и окно выбора, четыре ступени: `darkness`, `dark`, `light`, `lightness` |
| `--aspid-colors-text-*` | Текст, от самого яркого `lightness` до самого тусклого `darkness` |
| `--aspid-colors-shade-*` | Рамки и разделительные линии, те же четыре ступени |
| `--aspid-colors-overlay-*`, `--aspid-colors-slider-handle` | Заливки при наведении, заливки элементов управления, полосы прокрутки, ползунки |
| `--aspid-colors-status-success-*`, `-warning-*`, `-error-*`, `-info-*` | Фоны статусов, тексты (`-text-`), рамки (`-shade-`), подкраски (`-tint`) и точки (`-blob`) |
| `--aspid-colors-switch-accent`, `-track-border`, `-handle`, `-handle-shadow` | Переключатели. По умолчанию не заданы, поэтому переключатели следуют теме редактора |
| `--aspid-icons-*` | Значки вкладок Home и Settings, значки статусов |

Файл `Aspid-FastTools-Default-Dark.uss` в `Packages/tech.aspid.fasttools/Editor/Resources/UI/` содержит все токены, у которых есть значение по умолчанию.

## Общие цвета для команды

Назначение хранится в настройках редактора на этой машине, отдельно для каждого проекта, поэтому каждый участник команды назначает файл сам. **Reset to defaults → Per-user** очищает его. Сам `.uss` — обычный ассет проекта: закоммитьте его, чтобы поделиться цветами.
