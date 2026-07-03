# Release Checklist — стабильная версия 1.0.0

> Рабочий документ ветки `feature/serialize-reference-dropdown`. Создан 2026-07-03.
> Компаньон к [SerializeReference-Roadmap.md](SerializeReference-Roadmap.md) (там — продуктовый roadmap; здесь — только выпуск).
> Функциональная проверка вынесена в постоянный регламент: [QA-CHECKLIST_RU.md](QA-CHECKLIST_RU.md) / [QA-CHECKLIST.md](QA-CHECKLIST.md).
> Текущее состояние: пакет `1.0.0-rc.5`, PR #49 открыт (draft, mergeable), все 8 пунктов PLAN закрыты, P0 roadmap (№1, №2) закрыты.

---

## 1. Код и ветка

- [ ] Прогнать Unity EditMode-тесты в редакторе (`Aspid.FastTools.Unity.Editor.Tests` + `…SerializeReferences.Tests`) — roadmap помечает P0 №1/№2 как «код готов, прогнать Unity-тесты».
- [ ] `dotnet test` для `Aspid.FastTools.Generators` — зелёный.
- [ ] `dotnet test` для `Aspid.FastTools.Analyzers` (сабмодуль) — зелёный.
- [ ] Убедиться, что закоммиченные DLL (`Aspid.FastTools.Generators.dll`, `Aspid.FastTools.Analyzers.dll`) собраны из текущих исходников (пересобрать и сравнить, сабмодуль забамплен на актуальный коммит).
- [ ] Закоммитить / разнести текущие незакоммиченные изменения (6 файлов Settings/Provider/UI — сейчас dirty в ветке).
- [ ] Явно зафиксировать срез скоупа: P1 (№3–7) и P2 (№8–11) из roadmap **не входят** в 1.0.0 — перенести в issues/Linear, из ветки ничего не тянуть.
- [ ] Пройти финальное ревью PR #49: draft → ready, ревью, зелёный CI, merge в `main`.

## 2. Функциональная проверка

- [ ] Пройти **полностью** [QA-CHECKLIST_RU.md](QA-CHECKLIST_RU.md) (EN-версия: [QA-CHECKLIST.md](QA-CHECKLIST.md)) — разделы 1–14, включая окружения/совместимость и автотесты.
- [ ] Все найденные проблемы закрыты или осознанно отложены с issue.

## 3. Документация

- [ ] `/sync-readmes`: 4 README (root EN/RU + Documentation EN/RU) сверены с фактическим API, пути к картинкам корректны в обеих раскладках.
- [ ] TUTORIAL сэмпла (EN/RU) соответствует финальному поведению пикера.
- [ ] CHANGELOG: секцию `[Unreleased]` превратить в `[1.0.0] — <дата>`, вычитать формулировки, проверить ссылки на issues (`[#51]` и т.д.).
- [ ] QA-чек-лист актуален: каждая фича релиза представлена пунктом в **обоих** языках.
- [ ] Медиа: GIF/скриншоты в `Documentation/Images` показывают финальный UI (после рестайла Settings, футера пикера, preselect `<None>`).
- [ ] Проверить `Documentation~`/лицензию/Third-Party notices, если публикуемся в OpenUPM/Asset Store (roadmap №12 — можно после 1.0.0, но README-сравнение желательно к релизу).

## 4. Версия и публикация

- [ ] `package.json`: `1.0.0-rc.5` → `1.0.0` (+ проверить `unity: 6000.0`, `displayName`, `keywords`, `samples`-секцию).
- [ ] Тег `v1.0.0` + GitHub Release с выжимкой из CHANGELOG.
- [ ] Обновить subtree-ветки `upm` / `upm-preview`, проверить установку по обоим URL в чистый проект.
- [ ] Проверить установку через Package Manager по git-URL: пакет компилируется без ошибок/ворнингов, сэмпл импортируется.
