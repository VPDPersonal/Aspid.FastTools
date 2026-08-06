# Release Checklist — стабильная версия 1.0.0

> Рабочий документ выпуска 1.0.0. Создан 2026-07-03, актуализирован 2026-08-06 по итогам аудита релизной готовности.
> Компаньон к [SerializeReference-Roadmap.md](SerializeReference-Roadmap.md) (там — продуктовый roadmap; здесь — только выпуск).
> Функциональная проверка вынесена в постоянный регламент: [QA-CHECKLIST_RU.md](QA-CHECKLIST_RU.md) / [QA-CHECKLIST.md](QA-CHECKLIST.md).
> Текущее состояние: пакет `1.0.0-rc.5`, анализаторы влиты в основной репозиторий (PR #152). Релизный PR будет новым: старый draft PR #48 закрыт как протухший, ветка `chore/release-1.0.0` отстала от `main` на 558 коммитов — пересоздавать поверх свежего `main`.

---

## 1. Код и тесты

- [x] Unity EditMode-тесты (`Aspid.FastTools.Unity.Editor.Tests` + `…SerializeReferences.Tests`) — прогнаны 2026-08-06 в редакторе Unity 6000.4.0f1: **310/310 passed**, 0 warnings.
- [x] `dotnet test` для `Aspid.FastTools.Generators` — **38/38** зелёные (2026-08-06).
- [x] `dotnet test` для `Aspid.FastTools.Analyzers` — **47/47** зелёные (2026-08-06).
- [ ] Перед тегом: `git status` чист, закоммиченные DLL (`Aspid.FastTools.Generators.dll`, `Aspid.FastTools.Analyzers.dll`) — Release-сборки из текущих исходников. С детерминированной сборкой (отдельный PR аудита) проверка — пересборка в Release + `git diff` по DLL (diff пуст ⇒ DLL актуальны).
- [ ] CI EditMode-тесты: добавить секрет `UNITY_LICENSE` и влить PR #138 — либо осознанно отложить на после 1.0.0, тогда обязателен прогон в редакторе непосредственно перед тегом.

## 2. Функциональная проверка

- [ ] Пройти **полностью** [QA-CHECKLIST_RU.md](QA-CHECKLIST_RU.md) (EN-версия: [QA-CHECKLIST.md](QA-CHECKLIST.md)) — разделы 1–14, включая окружения/совместимость и автотесты.
- [ ] Все найденные проблемы закрыты или осознанно отложены с issue.

## 3. Документация

- [ ] `/sync-readmes`: 4 README (root EN/RU + Documentation EN/RU) сверены с фактическим API, пути к картинкам корректны в обеих раскладках.
- [ ] Миграционная заметка о смене id пакета (`com.aspid.fasttools` → `tech.aspid.fasttools`, коммит 733b901b) присутствует во всех 4 README, EN/RU симметрично.
- [ ] TUTORIAL сэмпла SerializeReferences (EN/RU) соответствует финальному поведению пикера.
- [ ] CHANGELOG: секцию `[Unreleased]` превратить в `[1.0.0] — <дата>` (в `CHANGELOG.md` и `CHANGELOG_RU.md`), вычитать формулировки, проверить ссылки на issues.
- [ ] В релизном коммите синхронизировать копию `CHANGELOG.md` внутри пакета с корневым: ветка `upm` поставляет `CHANGELOG.md` в корне пакета, а в исходниках пакета его нет — при обновлении subtree копия не должна протухнуть.
- [ ] QA-чек-лист актуален: каждая фича релиза представлена пунктом в **обоих** языках.
- [ ] Медиа: GIF/скриншоты в `Documentation/Images` показывают финальный UI. Переснять устаревшие (скриншот пикера снят до рестайла из PR #150) и после пересъёмки убрать 6 служебных комментариев `TODO(media)` из `Samples~/SerializeReferences/Documentation`.
- [ ] Проверить `Documentation~`/лицензию/Third-Party notices, если публикуемся в OpenUPM/Asset Store (roadmap №12 — можно после 1.0.0).

## 4. Версия и публикация

- [ ] `package.json`: `1.0.0-rc.5` → `1.0.0` (+ проверить `unity: 6000.0`, `displayName`, `keywords`, `samples`-секцию).
- [ ] Открыть новый релизный PR поверх свежего `main` (ветку `chore/release-1.0.0` не реанимировать), ревью, зелёный CI, merge.
- [ ] Тег `v1.0.0` + GitHub Release с выжимкой из CHANGELOG.
- [ ] Обновить subtree-ветки `upm` / `upm-preview`, проверить установку по обоим URL в чистый проект: пакет компилируется без ошибок/ворнингов, сэмпл импортируется.
