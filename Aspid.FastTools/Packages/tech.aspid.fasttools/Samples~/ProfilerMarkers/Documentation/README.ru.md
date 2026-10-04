# Пример ProfilerMarkers

Стая кубов, у которой каждая фаза кадра видна в Profiler под своим именем.

## Как открыть

1. Импортируйте пример: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** у **ProfilerMarkers**.
2. Откройте `Scenes/ProfilerMarkers.unity` и **Window → Analysis → Profiler**, войдите в Play Mode и выберите кадр в модуле CPU.
3. В режиме **Hierarchy** разверните `PlayerLoop` до строки `Flock.Update (74)` — она лежит под строкой Unity `Flock.Update() [Invoke]`.

Нужен встроенный модуль Unity **Physics** — без него скрипты примера не компилируются.

Полное руководство — что попробовать в примере и куда смотреть в его коде — на сайте: [пример ProfilerMarkers](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials/profiler-markers).
