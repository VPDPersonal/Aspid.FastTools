# ProfilerMarkers

Измеряйте метод или отдельный блок кода одним вызовом `this.Marker()`. Генератор создаёт и переиспользует статический `ProfilerMarker` для каждого места вызова; вручную объявлять маркеры и подбирать им уникальные имена не нужно.

## Быстрый старт

Добавьте `using var` в начало метода. Измерение завершится при выходе из его области видимости, в том числе через `return` или исключение.

```csharp
using UnityEngine;

public sealed class MotionSimulation : MonoBehaviour
{
    [SerializeField] private Transform[] _agents;

    private void Update()
    {
        using var marker = this.Marker();

        foreach (var agent in _agents)
            agent.position += Vector3.forward * Time.deltaTime;
    }
}
```

Отдельный `using Aspid.FastTools…` для `Marker()` не нужен: исходное расширение объявлено в глобальном пространстве имён. Генератор поставляется вместе с пакетом.

Откройте **Window → Analysis → Profiler**, запустите сцену и найдите `MotionSimulation.Update` в модуле **CPU Usage**. Имя маркера содержит также номер строки вызова. Для готовой сцены с несколькими измеряемыми этапами используйте [пример ProfilerMarkers](../../Samples~/ProfilerMarkers/Documentation/README.ru.md).

![Маркеры с именами методов и номерами строк в Unity Profiler](../Images/aspid_fasttools_profiler_markers.png)

Маркеры с именами методов и номерами строк в Unity Profiler

## Измерение отдельного блока

`using (this.Marker())` измеряет только код внутри фигурных скобок. В примере ниже `ApplyResults()` выполняется после завершения измерения:

```csharp
using (this.Marker())
{
    Simulate();
}

ApplyResults();
```

`Simulate` и `ApplyResults` здесь обозначают методы вашей системы. Вложенные области `using` позволяют отдельно измерять этапы внутри общего маркера.

## Собственное имя

Добавьте `.WithName("…")`, чтобы заменить имя метода в подписи маркера на имя этапа:

```csharp
using (this.Marker().WithName("Find neighbours"))
{
    FindNeighbours();
}
```

| Вызов внутри `Flock.Update` | Имя в Profiler |
|---|---|
| `this.Marker()` | `Flock.Update (номер строки)` |
| `this.Marker().WithName("Find neighbours")` | `Flock.Find neighbours (номер строки)` |

Имя читается из исходного кода при компиляции. Передавайте строковый литерал; переменная или интерполяция с подстановками не переименует маркер. Вызовы внутри лямбд и локальных функций относятся к ближайшему объявленному методу, полю или свойству.

## Сгенерированный код

Для каждого места вызова генератор создаёт статическое поле и добавляет расширение `Marker` для вызывающего типа. Номер строки, переданный через `CallerLineNumber`, выбирает нужный маркер.

<details>
<summary>Упрощённый пример результата генерации</summary>

```csharp
using Unity.Profiling;
using System.Runtime.CompilerServices;

internal static class __MotionSimulationProfilerMarkerExtensions
{
    private static readonly ProfilerMarker UpdateMarker =
        new("MotionSimulation.Update (9)");

    public static ProfilerMarker.AutoScope Marker(
        this MotionSimulation instance,
        [CallerLineNumber] int line = -1)
    {
#if ENABLE_PROFILER
        if (line == 9) return UpdateMarker.Auto();
#endif
        return default;
    }
}
```

Имена полей здесь сокращены для наглядности; номер строки зависит от расположения вызова в вашем файле. Этот код генерируется автоматически — добавлять его в проект вручную не нужно.

</details>

В сборке без `ENABLE_PROFILER` диспетчер возвращает пустую область `default`, и измерение не выполняется. Для generic-типов статические маркеры создаются отдельно для каждого закрытого типа; подпись включает имена аргументов типов.

> [!TIP]
> Начните с маркера на весь метод. Когда найдёте дорогой метод, добавьте отдельные именованные блоки вокруг его этапов — так результаты в Profiler проще сопоставить с кодом.
