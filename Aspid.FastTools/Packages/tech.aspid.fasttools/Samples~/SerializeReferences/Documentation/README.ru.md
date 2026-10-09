# Пример SerializeReferences

Турель, у которой оружие и его эффекты выбираются в инспекторе, и нарочно сломанные ассеты, чтобы их починить.

## Как открыть

1. Импортируйте пример: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** у **SerializeReferences**.
2. Откройте `Scenes/SerializeReferences.unity` и войдите в Play Mode: основное и запасное оружие по очереди бьют манекен, а каждый удар виден в Console.

Ассеты в `Presets/` и `Prefabs/` сломаны нарочно. Проверка сборки их находит: перед сборкой при **Build / CI gate** = `Fail` почините их, удалите пример или добавьте его папку в **Excluded scan folders**.

Полное руководство — что попробовать в примере и куда смотреть в его коде — на сайте: [пример SerializeReferences](https://vpdpersonal.github.io/Aspid.FastTools/ru/tutorials/serialize-references).
