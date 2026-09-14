# ProfilerMarkers

`this.Marker()` автоматически создаёт маркер Unity Profiler.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly<br />    ProfilerMarker UpdateMarker =<br />    new("MotionSimulation.Update");<br /><br />private void Update()<br />&#123;<br />    using var _ =<br />        UpdateMarker.Auto();<br />    Simulate();<br />&#125;</code></pre> | <pre lang="csharp"><code>private void Update()<br />&#123;<br />    using var _ = this.Marker();<br />    Simulate();<br />&#125;</code></pre> |

Работает в `MonoBehaviour` и обычных C#-классах. Генератор входит в пакет; расширение находится в глобальном пространстве имён — дополнительные `using`, атрибуты и `partial` не нужны.

<details>
<summary>Пример сгенерированного кода</summary>

Для класса `MotionSimulation` с вызовом `this.Marker()` на строке 10. Полные имена типов сокращены, атрибуты `GeneratedCode` опущены.

```csharp
using Unity.Profiling;
using System.Runtime.CompilerServices;

internal static class __MotionSimulationProfilerMarkerExtensions
{
    private static readonly ProfilerMarker Update_Marker_Line_10 =
        new("MotionSimulation.Update (10)");

    public static ProfilerMarker.AutoScope Marker(
        this MotionSimulation _, [CallerLineNumber] int line = -1)
    {
#if ENABLE_PROFILER
        if (line is 10) return Update_Marker_Line_10.Auto();
#endif
        return default;
    }
}
```

</details>

## Область измерения

`Marker()` возвращает `ProfilerMarker.AutoScope`: замер начинается при вызове и завершается при выходе из `using`, включая `return` и исключения.

Один пример для вложенных этапов, собственного имени и цикла внутри класса `FlockSimulation`:

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

> [!IMPORTANT]
> Не вызывайте `this.Marker()` без `using`: замер не завершится автоматически. Область не должна пересекать `await` или `yield`; измеряйте синхронные участки отдельно ([ограничение Unity](https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-add-markers-code.html)).

## Собственное имя

Добавьте `.WithName("Steering")` непосредственно к `this.Marker()`: вместо `FlockSimulation.Step (строка)` получите `FlockSimulation.Steering (строка)`. Меняется только имя метода; тип и номер строки остаются.

- **Поддерживаются:** строковые литералы `"Steering"`, `@"Steering"` и интерполяция без подстановок `$"Steering"`.
- **Не поддерживаются:** переменные, `const`, `nameof`, конкатенация и интерполяция с подстановками (`$"Agent {index}"`). С ними остаётся исходное имя метода.

Генератор читает текст литерала из исходника, но не вычисляет выражения — даже константные. Во время выполнения аргумент `WithName` всё равно вычисляется, поэтому динамическая строка может добавить лишнюю работу в замер.

## Особенности генерации

- **Номер строки.** Сгенерированное расширение выбирает статический маркер по `CallerLineNumber`. Каждый вызов в пределах типа должен иметь свой номер строки, в том числе в разных файлах `partial`-типа. Перенос вызова меняет суффикс имени.
- **Лямбды и локальные функции.** Используется имя ближайшего объявленного метода, поля или свойства. У конструктора — `Ctor`, у аксессора — имя свойства.
- **Generic-типы.** У каждого закрытого типа свои статические маркеры. Например, `Worker<int>.Run()` → `Worker<Int32>.Run (строка)`; аргументы именуются через `typeof(T).Name`.
- **Без `ENABLE_PROFILER`.** Расширение возвращает `default`, замер не начинается, код внутри `using` выполняется. Статические поля остаются в сгенерированном исходнике, аргументы `WithName` по-прежнему вычисляются.

## Результат в Profiler

Откройте **Window → Analysis → Profiler**, включите запись и запустите сцену. Выберите кадр в **CPU Usage → Hierarchy** и найдите имя типа. Deep Profile не нужен.

![Маркеры Flock и FlockSimulation в CPU Usage: у Steering.Agent — 120 вызовов](../../Samples~/ProfilerMarkers/Documentation/Images/profiler-markers.png)

Маркеры Flock и FlockSimulation в CPU Usage: у Steering.Agent — 120 вызовов

Чтобы повторить, импортируйте [пример ProfilerMarkers](../../Samples~/ProfilerMarkers/Documentation/README.ru.md), откройте `Scenes/ProfilerMarkers.unity` и найдите `Flock` в Profiler. Время зависит от кадра и машины; эксперименты с числом агентов — в [документации примера](../../Samples~/ProfilerMarkers/Documentation/README.ru.md#попробуйте).
