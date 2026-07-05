# ProfilerMarkers — пошаговый туториал

`this.Marker()` даёт любому методу именованный `ProfilerMarker` без шаблонного кода. Source-генератор
Aspid.FastTools заменяет каждое место вызова уникальным маркером, привязанным к `(type, method, line)`; если
обернуть работу в `using`-область, она появится в Unity Profiler отдельной именованной строкой.

```csharp
using (this.Marker().WithName("Physics")) // Profiler: "ProfilerMarkersTutorial.Physics (<line>)"
    SimulatePhysics();
```

Каждый урок соответствует одной секции `STEP` компонента `ProfilerMarkersTutorial` и демонстрирует одну форму
вызова; поля — это «ручки нагрузки», меняющие стоимость каждого маркера.

## Открыть туториал

1. Откройте окно Welcome (**Tools → Aspid 🐍 → FastTools → Welcome**) и импортируйте пример **ProfilerMarkers**.
2. Откройте **`Scenes/ProfilerMarkersTutorial.unity`** и выберите GameObject **ProfilerMarkers Tutorial**.
3. Откройте **Window → Analysis → Profiler** и **войдите в Play Mode** — маркеры появятся под именами
   `ProfilerMarkersTutorial.*` (маркеры работают только в play).

> **Отображаемое имя имеет вид `"{Type}.{name} ({line})"`.** Суффикс `(line)` добавляется всегда, поэтому два
> маркера в одном методе никогда не конфликтуют. В `.WithName("Physics")` передавайте только короткий суффикс —
> имя типа добавится автоматически.

---

## Урок 1 — Именованные блок-маркеры

**Поля:** `_physicsIterations`, `_renderIterations` · **Маркеры:** `ProfilerMarkersTutorial.Physics`,
`ProfilerMarkersTutorial.Render`

Блок `using`-statement с явным именем ограничивает маркер блоком:

```csharp
using (this.Marker().WithName("Physics"))
    Spin(_physicsIterations, SpinOp.Sqrt);
```

- `.WithName(...)` принимает **только короткий суффикс**; полный маркер — `ProfilerMarkersTutorial.Physics (<line>)`.
- Маркер начинается при входе в `using`-блок и завершается при выходе — `Begin`/`End` вызывать не нужно.
- Увеличение `_physicsIterations` делает маркер `Physics` дороже; `Render` от этого не зависит.

---

## Урок 2 — Маркер с автоименем (без `WithName`)

**Поле:** `_inputIterations` · **Маркер:** `ProfilerMarkersTutorial.SimulateInput`

Уберите `.WithName(...)` и используйте `using`-**declaration**; генератор назовёт маркер по содержащему методу:

```csharp
private void SimulateInput()
{
    using var _ = this.Marker();  // Profiler: "ProfilerMarkersTutorial.SimulateInput (<line>)"
    Spin(_inputIterations, SpinOp.Tan);
}
```

Область маркера — остаток тела метода. Автоимя удобно, когда достаточно одного маркера на метод, а имя метода уже
говорит, что он делает.

---

## Урок 3 — Вложенные и per-iteration маркеры

**Поля:** `_aiAgents`, `_aiStepsPerAgent` · **Маркеры:** `ProfilerMarkersTutorial.AI` →
`ProfilerMarkersTutorial.AI.Agent`

Маркеры вкладываются ровно так же, как `using`-области, которые их порождают. `SimulateAI` открывает внешний
маркер `AI`, затем оборачивает каждого агента в собственный маркер `AI.Agent`:

```csharp
using (this.Marker().WithName("AI"))
{
    for (var agent = 0; agent < _aiAgents; agent++)
        using (this.Marker().WithName("AI.Agent"))
            Spin(_aiStepsPerAgent, SpinOp.Sin);
}
```

- Маркер внутри цикла — **один** маркер с N сэмплами, а не N маркеров: `(line)` в имени фиксирован для места
  вызова, поэтому каждая итерация попадает в одну запись.
- Вложенность отражает код: `AI.Agent` находится внутри `AI`, без ручной привязки parent/child.

---

## Урок 4 — Комбинированная форма

**Поле:** `_audioIterations` · **Маркер:** `ProfilerMarkersTutorial.SimulateAudio` (дважды)

Один метод может нести маркер на весь метод **и** более узкий вокруг «горячего» подэтапа. Оба получают автоимя по
методу, поэтому различают их только **номера строк**:

```csharp
private void SimulateAudio()
{
    using var _ = this.Marker();     // "ProfilerMarkersTutorial.SimulateAudio (<outer-line>)"
    Spin(_audioIterations, SpinOp.Sqrt);

    using (this.Marker())            // "ProfilerMarkersTutorial.SimulateAudio (<inner-line>)"
        Spin(_audioIterations, SpinOp.Cos);
}
```

Вы получите **два** различных маркера `ProfilerMarkersTutorial.SimulateAudio` с разными суффиксами `(line)` —
внешний покрывает весь метод, внутренний только вложенный блок. Именно поэтому генератор вшивает номер строки в
имя: тот же тип, тот же метод — и всё равно различимо.

---

## Где смотреть в коде

| Файл | Что показывает |
|---|---|
| `Scripts/Tutorial/ProfilerMarkersTutorial.cs` | Все четыре урока как секции `STEP` с «ручками нагрузки» |
| `Scripts/FrameProfiler.cs` | Те же три формы вызова в обычном компоненте (демо-сцена `ProfilerMarkers.unity`) |
| [README_RU.md](README_RU.md) | Компактный справочник по трём поддерживаемым формам |
