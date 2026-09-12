# Начало работы

## Установка

Установите Aspid.FastTools через UPM: в Package Manager нажмите **+ → Install package from git URL…** и вставьте один из URL ниже.

### Stable

Ветка `upm` всегда указывает на последний **стабильный** релиз:

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm
```

Чтобы установить конкретную версию, укажите неизменяемый per-release тег `upm/<version>` — например, `upm/1.0.0` после выхода релиза 1.0.0 (список доступных версий — на странице [Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases)):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm/<version>
```

### Preview

Ветка `upm-preview` всегда указывает на последний **preview** релиз (rc, beta, alpha, …):

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview
```

Конкретные preview-версии используют ту же схему per-release тегов:

```
https://github.com/VPDPersonal/Aspid.FastTools.git#upm-preview/1.0.0-rc.8
```

## Примеры

К каждой возможности прилагается пример: небольшая сцена или editor-инструмент, который делает с этой возможностью что-то видимое, и `README.md` с тем, что попробовать и куда смотреть в коде. Импортируйте их из Package Manager (**Aspid.FastTools → Samples**) или откройте вкладку **Welcome** (`Tools → Aspid 🐍 → FastTools → Welcome`).

В [обзоре примеров](../../Samples~/README.ru.md) все они перечислены в рекомендуемом порядке знакомства с описанием того, что показывает каждый.
