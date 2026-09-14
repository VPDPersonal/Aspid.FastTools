# ProfilerMarkers

`this.Marker()` автоматически создаёт маркер Unity Profiler.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly<br />    ProfilerMarker UpdateMarker =<br />    new("MotionSimulation.Update");<br /><br />private void Update()<br />&#123;<br />    using var _ =<br />        UpdateMarker.Auto();<br />    Simulate();<br />&#125;</code></pre> | <pre lang="csharp"><code>private void Update()<br />&#123;<br />    using var _ = this.Marker();<br />    Simulate();<br />&#125;</code></pre> |

Работает в `MonoBehaviour` и обычных C#-классах. Генератор входит в пакет; расширение находится в глобальном пространстве имён — дополнительные `using`, атрибуты и `partial` не нужны. Маркер называется `Тип.Метод (строка)`.

## Область и имя

`Marker()` возвращает `ProfilerMarker.AutoScope`: замер начинается при вызове и завершается при выходе из `using`, включая `return` и исключения. `.WithName("Steering")` заменяет часть имени с методом; тип и номер строки остаются.

```csharp
public void Step()
{
    using var _ = this.Marker();

    using (this.Marker().WithName("Steering"))
    {
        foreach (var agent in _agents)
        {
            using var agentScope = this.Marker().WithName("Steering.Agent");
            ComputeSteering(agent);
        }
    }

    using (this.Marker().WithName("Integrate"))
    {
        Integrate();
    }
}
```

В Profiler при 120 итерациях цикла:

```text
FlockSimulation.Step (…)
├── FlockSimulation.Steering (…)
│   └── FlockSimulation.Steering.Agent (…) — 120 вызовов одного маркера
└── FlockSimulation.Integrate (…)
```

`WithName` принимает только строковый литерал: `"Steering"`, `@"Steering"` или `$"Steering"` без подстановок. Переменные, `const`, `nameof`, конкатенация и `$"Agent {index}"` оставляют исходное имя метода — генератор читает текст исходника и не вычисляет выражения. Сам аргумент во время выполнения всё равно вычисляется.

> [!IMPORTANT]
> Не вызывайте `this.Marker()` без `using`: замер не завершится автоматически. Область не должна пересекать `await` или `yield`; измеряйте синхронные участки отдельно ([ограничение Unity](https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-add-markers-code.html)).

## Особенности генерации

- **Номер строки.** Маркер выбирается по `CallerLineNumber`, поэтому каждому вызову в типе нужна своя строка, в том числе в разных файлах `partial`. Перенос вызова меняет суффикс имени.
- **Лямбды и локальные функции** используют ближайший объявленный член: `Ctor` для конструктора, имя свойства для аксессора.
- **Generic-типы** получают отдельные маркеры на каждый закрытый тип: `Worker<int>.Run()` → `Worker<Int32>.Run (строка)`.
- **Без `ENABLE_PROFILER`** расширение возвращает `default`: ничего не измеряется, код внутри `using` выполняется, аргументы `WithName` по-прежнему вычисляются.

<a id="result"></a>

## Результат в Profiler

Откройте **Window → Analysis → Profiler**, включите запись и запустите сцену. Выберите кадр в **CPU Usage → Hierarchy** и найдите имя типа. Deep Profile не нужен.

![Маркеры Flock и FlockSimulation в CPU Usage: у Steering.Agent — 120 вызовов](../../Samples~/ProfilerMarkers/Documentation/Images/profiler-markers.png)

Маркеры Flock и FlockSimulation в CPU Usage: у Steering.Agent — 120 вызовов

Чтобы повторить, импортируйте [пример ProfilerMarkers](../../Samples~/ProfilerMarkers/Documentation/README.ru.md#попробуйте), откройте `Scenes/ProfilerMarkers.unity` и найдите `Flock`. Время зависит от кадра и машины.
