# ProfilerMarkers

Маркеры профилировщика одной строкой, без полей и имён, которые приходится поддерживать вручную.

## Быстрый старт

| До — Unity API | После — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _marker =&#10;    new("FlockSimulation.Step");&#10;&#10;public void Step()&#10;&#123;&#10;    using var _ = _marker.Auto();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Step()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    Integrate();&#10;&#125;</code></pre> |

## Marker()

Имя маркера включает тип, имя метода или свойства и номер строки вызова:

| Где вызван <code lang="csharp">this.Marker()</code> | Имя маркера |
|---|---|
| <code lang="csharp">void Step()</code> | <code lang="string">FlockSimulation.Step (строка)</code> |
| <code lang="csharp">FlockSimulation()</code> | <code lang="string">FlockSimulation.Ctor (строка)</code> |
| <code lang="csharp">float Speed &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Speed (строка)</code> |
| <code lang="csharp">Agent this[int i] &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Indexer (строка)</code> |
| <code lang="csharp">event Action Changed</code> | <code lang="string">FlockSimulation.Changed (строка)</code> |

Номер строки меняется при перемещении вызова в коде.

<details>
<summary>Вложенные и обобщённые типы</summary>

| Место вызова | Имя маркера |
|---|---|
| Метод <code lang="csharp">Move()</code> во вложенном <code lang="class-name">FlockSimulation.Agent</code> | <code lang="string">Agent.Move (строка)</code> |
| <code lang="csharp">Step()</code> в классе <code lang="class-name">FlockSimulation&lt;Int32&gt;</code> | <code lang="string">FlockSimulation&lt;Int32&gt;.Step (строка)</code> |
| <code lang="csharp">Step()</code> в структуре <code lang="class-name">FlockSimulation&lt;T&gt;</code> | <code lang="string">FlockSimulation&lt;T&gt;.Step (строка)</code> для любого <code lang="class-name">T</code> |
| Обобщённый метод <code lang="csharp">Step&lt;T&gt;()</code> | <code lang="string">FlockSimulation.Step (строка)</code> для любого <code lang="class-name">T</code> |

</details>

## WithName()

<code lang="csharp">WithName("Steering")</code> заменяет имя метода, сохраняя тип и номер строки.

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

Имя должно быть константой: строковым литералом, <code lang="csharp">const</code>, <code lang="csharp">nameof</code> или строкой, собранной из них. Имя, известное только во время выполнения, оставляет имя метода.

## В Profiler

Маркеры создаются один раз. После инициализации повторные замеры не выделяют память.

![Схема маркеров FlockSimulation: Steering и Integrate вложены в Step, у Steering.Agent — 120 вызовов. Время приведено для примера.](../../../../docs/Images/profiler-markers-hierarchy.svg)

## Ограничения

- Вызывайте <code lang="csharp">this.Marker()</code> внутри собственного типа. Вызовы на объекте другого типа, в статическом классе или вложенном <code lang="csharp">private</code>/<code lang="csharp">protected</code> типе ненадёжны: замер может отсутствовать или попасть в чужой маркер.
- Используйте <code lang="csharp">using</code>. Отдельный вызов <code lang="csharp">this.Marker();</code> начинает замер и не завершает его.
- Закрывайте <code lang="csharp">using</code> до <code lang="csharp">yield return</code> или <code lang="csharp">await</code>. Корутина или <code lang="csharp">async</code>-метод продолжают работу в другом кадре или потоке, поэтому незакрытый замер оставляет в Profiler несбалансированные сэмплы.

> [!WARNING]
> Вызовы внутри одного типа с одинаковым номером строки делят один маркер, даже если находятся в разных файлах <code lang="csharp">partial</code>. Давайте каждой точке замера свой номер строки, иначе замеры объединятся под именем первого вызова.

## Пример в пакете

Маркеры из [WithName()](#withname) работают в сцене [ProfilerMarkers](../../docusaurus-plugin-content-docs-tutorials/current/ProfilerMarkers/README.md).

![Стая агентов в сцене ProfilerMarkers](../../../../tutorials/ProfilerMarkers/Images/demo.gif)
