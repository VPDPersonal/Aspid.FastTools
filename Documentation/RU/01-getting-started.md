# Начало работы

## Установка

Установите Aspid.FastTools через UPM: в Package Manager нажмите **+ → Install package from git URL…** и вставьте один из URL ниже.

> [!NOTE]
> **Миграция с `com.aspid.fasttools`:** в мае 2026 пакет переименован в `tech.aspid.fasttools`. Для Unity это другой пакет, поэтому установки со старым id не получают обновлений — удалите запись `com.aspid.fasttools` из `Packages/manifest.json` и установите `tech.aspid.fasttools` по одному из URL ниже.

### Stable

Ветка `upm` всегда указывает на последний **стабильный** релиз:

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm
```

Чтобы установить конкретную версию, укажите неизменяемый per-release тег `upm/<version>` — например, `upm/1.0.0` после выхода релиза 1.0.0 (список доступных версий — на странице [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm/<version>
```

Предпочитаете установку вручную? Скачайте `.unitypackage` со страницы [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases) или возьмите пакет в [Unity Asset Store](https://assetstore.unity.com/packages/slug/365584).

### Preview

Ветка `upm-preview` всегда указывает на последний **preview** релиз (rc, beta, alpha, …):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

Конкретные preview-версии используют ту же схему per-release тегов:

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/1.0.0-rc.7
```

## Примеры

К каждой возможности прилагается пример, который одновременно служит туториалом. Импортируйте их из Package Manager (**Aspid.FastTools → Samples**) или откройте вкладку **Welcome** (`Tools → Aspid 🐍 → FastTools → Welcome`).

| Пример | Что показывает |
|---|---|
| [Types](../../Samples~/Types/README.ru.md) | `SerializableType<T>`, `[TypeSelector]`, `ComponentTypeSelector` в маленькой системе способностей |
| [SerializeReferences](../../Samples~/SerializeReferences/README.ru.md) | Пикер `[SerializeReference]`: одиночные поля, списки, сужение, вложенность, generics, `Required` |
| [EnumValues](../../Samples~/EnumValues/README.ru.md) | Отображения по enum с обработкой `[Flags]` в инспекторах UI Toolkit и IMGUI |
| [Ids](../../Samples~/Ids/README.ru.md) | Структуры `IId`, `[UniqueId]` и ассеты `IdRegistry` |
| [ProfilerMarkers](../../Samples~/ProfilerMarkers/README.ru.md) | `this.Marker()` и сгенерированные маркеры в Profiler |
| [VisualElements](../../Samples~/VisualElements/README.ru.md) | Кастомный инспектор на fluent-расширениях `VisualElement` |
