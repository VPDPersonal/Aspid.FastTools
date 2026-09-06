# Пример ProfilerMarkers

Стая кубов, которой управляет обычный C#-класс, и `this.Marker()` вокруг каждой фазы. Генератор превращает каждый вызов в статический `ProfilerMarker`, поэтому Profiler показывает именованное дерево кадра без единого поля маркера, написанного руками. Справочник — [ProfilerMarkers](../../Documentation/ru/05-profiler-markers.md).

```csharp
using var _ = this.Marker();                   // "FlockSimulation.Step (line)", до конца метода
using (this.Marker().WithName("Steering"))     // "FlockSimulation.Steering (line)", блок
    ComputeSteering(neighborRadius);
```

## Как открыть

1. Импортируйте пример и откройте `Scenes/ProfilerMarkers.unity`.
2. Откройте **Window → Analysis → Profiler**, войдите в Play Mode и выберите кадр в модуле CPU.
3. В режиме **Hierarchy** разверните `PlayerLoop → Update.ScriptRunBehaviourUpdate → Flock.Update (…)`.

## Попробуйте

1. **Дерево повторяет области `using`.** Под `Flock.Update` лежит `FlockSimulation.Step`, под ним `FlockSimulation.Steering` и `FlockSimulation.Integrate`, а рядом — `Flock.ApplyTransforms`. Вложенность не нужно настраивать, она следует за кодом.
2. **Один маркер, много сэмплов.** `Steering.Agent` стоит внутри цикла. Profiler показывает одну строку с `Calls`, равным числу агентов, а не строку на агента: имя фиксировано на точку вызова.
3. **Покрутите ручки.** Поднимите `Count` у **Flock** до 400 — `Steering` займёт весь кадр; уменьшите `Neighbor Radius`, и он снова станет дешёвым. Маркеры показывают, куда ушло время, без Deep Profile.
4. **Любой класс, любая область.** `FlockSimulation` — не `MonoBehaviour`. Локальная функция в `Flock.Start` получает маркер с именем `Start`, объемлющего метода.
5. **Суффикс со строкой.** Каждое имя заканчивается `(line)`, поэтому два маркера в одном методе не пересекаются, а маркер, перемещённый по файлу, меняет суффикс. Поищите в Profiler `FlockSimulation.`, чтобы увидеть их все.
6. **Бесплатно в релизной сборке.** Сгенерированный диспетчер обёрнут в `#if ENABLE_PROFILER`; без профайлера каждый вызов возвращает `default`.

## Куда смотреть

| Файл | Что показывает |
|---|---|
| `Scripts/FlockSimulation.cs` | Маркеры на весь метод, на блок и на итерацию в обычном классе |
| `Scripts/Flock.cs` | Точка входа кадра, маркер внутри локальной функции |
