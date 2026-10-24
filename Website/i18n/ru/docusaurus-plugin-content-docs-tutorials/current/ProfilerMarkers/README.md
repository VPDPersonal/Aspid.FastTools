# Пример ProfilerMarkers

Стая кубов, у которой каждая фаза кадра видна в Profiler под своим именем.

![Симуляция стаи, фазы которой измеряют маркеры.](../../../../../tutorials/ProfilerMarkers/Images/demo.gif)

Симуляция стаи, фазы которой измеряют маркеры.

## Как открыть

1. Импортируйте пример: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** у **ProfilerMarkers**.
2. Откройте `Scenes/ProfilerMarkers.unity` и **Window → Analysis → Profiler**, войдите в Play Mode и выберите кадр в модуле CPU.
3. В режиме **Hierarchy** разверните `PlayerLoop` до строки <code lang="string">Flock.Update (22)</code>.

## Попробуйте

![Маркеры под Flock.Update вложены как в коде; у FlockSimulation.Steering.Agent — 120 вызовов на 120 агентов.](../../../../../tutorials/ProfilerMarkers/Images/profiler-markers.png)

Маркеры под Flock.Update вложены как в коде; у FlockSimulation.Steering.Agent — 120 вызовов на 120 агентов.

1. **Дерево повторяет области <code lang="csharp">using</code>.** Больше вложенность ничто не задаёт:

   ```csharp
   public void Step(float deltaTime, float neighborRadius, float maxSpeed)
   {
       using var _ = this.Marker();

       using (this.Marker().WithName("Steering"))
           ComputeSteering(neighborRadius);

       using (this.Marker().WithName("Integrate"))
           Integrate(deltaTime: deltaTime, maxSpeed: maxSpeed);
   }
   ```

2. **Один маркер на цикл.** <code lang="string">FlockSimulation.Steering.Agent</code> стоит в цикле по агентам: одна строка, `Calls` равен числу агентов.
3. **Больше агентов.** В Play Mode поставьте **Count** у **Flock** = `400`: `Calls` станет `400`.
4. **Не только MonoBehaviour.** <code lang="class-name">FlockSimulation</code> — обычный C#-класс. Маркер в локальной функции <code lang="function">CreateAgent</code> берёт имя метода — <code lang="string">Flock.InitializeAgents (58)</code> — и виден в кадре, где меняется **Count**.
5. **Номер строки в имени.** Каждое имя заканчивается строкой вызова — <code lang="string">FlockSimulation.Steering (37)</code>, — поэтому два маркера одного метода не совпадут по имени.
6. **Релизная сборка.** В сборке плеера без **Development Build** маркеры ничего не замеряют.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/FlockSimulation.cs` | Маркеры на метод, на блок и на итерацию в обычном классе |
| `Scripts/Flock.cs` | Маркер в локальной функции |

Справочник — [ProfilerMarkers](../../../docusaurus-plugin-content-docs/current/09-profiler-markers.md).
