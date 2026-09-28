# ProfilerMarkers

Маркеры профилировщика одной строкой, без полей и имён, которые приходится поддерживать вручную.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _marker =&#10;    new("FlockSimulation.Step");&#10;&#10;public void Step()&#10;&#123;&#10;    using var _ = _marker.Auto();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Step()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    Integrate();&#10;&#125;</code></pre> |

## Marker()

Имя маркера собирается из типа, метода и номера строки вызова:

| Где вызван <code lang="csharp">this.Marker()</code> | Имя маркера |
|---|---|
| <code lang="csharp">void Step()</code> | <code lang="string">FlockSimulation.Step (строка)</code> |
| <code lang="csharp">FlockSimulation()</code> | <code lang="string">FlockSimulation.Ctor (строка)</code> |
| <code lang="csharp">float Speed &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Speed (строка)</code> |
| <code lang="csharp">Agent this[int i] &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Indexer (строка)</code> |
| <code lang="csharp">event Action Changed</code> | <code lang="string">FlockSimulation.Changed (строка)</code> |
| <code lang="csharp">class FlockSimulation.Agent &#123; void Move() &#125;</code> | <code lang="string">Agent.Move (строка)</code> |
| <code lang="csharp">class Worker&lt;T&gt; &#123; void Run() &#125;</code> | <code lang="string">Worker&lt;Int32&gt;.Run (строка)</code><br /><code lang="string">Worker&lt;Single&gt;.Run (строка)</code> |
| <code lang="csharp">struct Job&lt;T&gt; &#123; void Execute() &#125;</code> | <code lang="string">Job&lt;T&gt;.Execute (строка)</code> для любого <code lang="class-name">T</code> |
| <code lang="csharp">void Run&lt;T&gt;()</code> | <code lang="string">FlockSimulation.Run (строка)</code> для любого <code lang="class-name">T</code> |

## WithName()

<code lang="csharp">.WithName("Steering")</code> заменяет в имени маркера метод на свой текст: <code lang="string">FlockSimulation.Step (5)</code> → <code lang="string">FlockSimulation.Steering (5)</code>.

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

- **Номер строки в имени** меняется, когда вызов переезжает: сравнивайте захваты до и после правки по имени без номера.
- **Вызов без маркера.** Если маркер для вызова не создаётся — на объекте другого типа (<code lang="csharp">other.Marker()</code>), в статическом классе, во вложенном <code lang="csharp">private</code> или <code lang="csharp">protected</code> типе, — анализатор `AFT0010` предупредит об этом.
- **Выброшенный замер.** <code lang="csharp">this.Marker();</code> без <code lang="csharp">using</code> начинает замер, который никогда не заканчивается, — анализатор `AFT0011` предупредит об этом.

> [!WARNING]
> Вызовы <code lang="csharp">this.Marker()</code> в разных файлах <code lang="csharp">partial</code> одного типа, стоящие на одной и той же строке, делят маркер первого из них — замеры второго попадут в чужую строку Profiler.

## Пример в пакете

Маркеры из [WithName()](#withname) работают в сцене [ProfilerMarkers](../../Samples~/ProfilerMarkers/Documentation/README.ru.md).

![Стая агентов в сцене ProfilerMarkers](../../Samples~/ProfilerMarkers/Documentation/Images/demo.gif)
