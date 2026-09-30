# ProfilerMarkers

Маркеры профилировщика одной строкой, без полей и имён, которые приходится поддерживать вручную.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _marker =&#10;    new("FlockSimulation.Step");&#10;&#10;public void Step()&#10;&#123;&#10;    using var _ = _marker.Auto();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Step()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    Integrate();&#10;&#125;</code></pre> |

## Marker()

Имя маркера собирается из типа и метода:

| Где вызван <code lang="csharp">this.Marker()</code> | Имя маркера |
|---|---|
| <code lang="csharp">void Step()</code> | <code lang="string">FlockSimulation.Step</code> |
| второй вызов в <code lang="csharp">void Step()</code> | <code lang="string">FlockSimulation.Step #2</code> |
| <code lang="csharp">FlockSimulation()</code> | <code lang="string">FlockSimulation.Ctor</code> |
| <code lang="csharp">float Speed &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Speed</code> |
| <code lang="csharp">Agent this[int i] &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Indexer</code> |
| <code lang="csharp">event Action Changed</code> | <code lang="string">FlockSimulation.Changed</code> |
| <code lang="csharp">class FlockSimulation.Agent &#123; void Move() &#125;</code> | <code lang="string">Agent.Move</code> |
| <code lang="csharp">class Worker&lt;T&gt; &#123; void Run() &#125;</code> | <code lang="string">Worker&lt;Int32&gt;.Run</code><br /><code lang="string">Worker&lt;Single&gt;.Run</code> |
| <code lang="csharp">struct Job&lt;T&gt; &#123; void Execute() &#125;</code> | <code lang="string">Job&lt;T&gt;.Execute</code> для любого <code lang="class-name">T</code> |
| <code lang="csharp">void Run&lt;T&gt;()</code> | <code lang="string">FlockSimulation.Run</code> для любого <code lang="class-name">T</code> |

## WithName()

<code lang="csharp">.WithName("Steering")</code> заменяет в имени маркера метод на свой текст: <code lang="string">FlockSimulation.Step</code> → <code lang="string">FlockSimulation.Steering</code>.

```csharp
public void Step()
{
    using var _ = this.Marker();

    using (this.Marker().WithName("Steering"))
    {
        foreach (var agent in _agents)
        {
            using (this.Marker().WithName("Steering.Agent"))
                ComputeSteering(agent);
        }
    }

    using (this.Marker().WithName("Integrate"))
        Integrate();
}
```

> [!NOTE]
> Работает только строковый литерал: имя генератор читает из исходника. С переменной, <code lang="csharp">const</code>, <code lang="csharp">nameof</code> или <code lang="csharp">$"Agent &#123;index&#125;"</code> останется имя метода, а аргумент всё равно будет вычисляться при каждом вызове.

## В Profiler

На каждую точку вызова генератор создаёт одно статическое поле, поэтому замер не выделяет память.

![Схема маркеров FlockSimulation: Steering и Integrate вложены в Step, у Steering.Agent — 120 вызовов. Время приведено для примера.](../Images/profiler-markers-hierarchy.svg)

## Ограничения

- **Номера повторяющегося имени** идут по порядку вызовов в типе, поэтому вызов с тем же именем, добавленный выше, перенумерует те, что ниже, — дайте вызовам, которые сравниваете между правками, свой <code lang="csharp">WithName()</code>.
- **Вызов без маркера.** Если маркер для вызова не создаётся — на объекте другого типа (<code lang="csharp">other.Marker()</code>), в статическом классе, во вложенном <code lang="csharp">private</code> или <code lang="csharp">protected</code> типе, — анализатор `AFT0010` предупредит об этом.
- **Выброшенный замер.** <code lang="csharp">this.Marker();</code> без <code lang="csharp">using</code> начинает замер, который никогда не заканчивается, — анализатор `AFT0011` предупредит об этом.

> [!WARNING]
> Замеры попадут в чужую строку Profiler, когда:
>
> - вызовы <code lang="csharp">this.Marker()</code> в разных файлах <code lang="csharp">partial</code> одного типа стоят на одной и той же строке — они делят маркер первого из них;
> - номер строки вызова без маркера совпал с вызовом в типе, объявленном в пространстве имён вызова или во внешнем для него, — <code lang="csharp">other.Marker()</code> открывает маркер этого типа, а вызов во вложенном <code lang="csharp">private</code> или <code lang="csharp">protected</code> типе — маркер базового типа, если тот не в глобальном пространстве имён.

## Пример в пакете

Маркеры из [WithName()](#withname) работают в сцене [ProfilerMarkers](../../Samples~/ProfilerMarkers/Documentation/README.ru.md).

![Стая агентов в сцене ProfilerMarkers](../../Samples~/ProfilerMarkers/Documentation/Images/demo.gif)
