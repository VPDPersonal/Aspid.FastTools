# Changelog

> Русская версия: [Website/i18n/ru/CHANGELOG.md](Website/i18n/ru/CHANGELOG.md).

All notable changes to **Aspid.FastTools** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] — 2026-10-22

First stable release. Unity **6000.0.53f1** or newer; every Inspector feature works in both IMGUI and UI Toolkit. The `1.0.0-rc.*` prereleases and their notes are on [GitHub Releases](https://github.com/VPDPersonal/Aspid.FastTools/releases).

### Added

- `SerializableType` and `SerializableMonoScript` store a `System.Type` in a component or an asset; `[TypeSelector]` picks it in the Inspector from compatible types.
- SerializeReference Selector picks the class of a `[SerializeReference]` field and carries compatible data over when the class changes.
- `ComponentTypeSelector` changes an added component to a derived class without losing shared field values.
- Project References finds missing `[SerializeReference]` references and `SerializableType` names across the project and repairs them in groups; the build and CI checks catch new ones.
- `EnumValues` maps enum keys to values that you edit in the Inspector, flags included.
- `this.Marker()` opens a profiler marker named after the type, the method and the line of the call; a source generator builds it.
- Analyzers `AFT0001`–`AFT0014` report `[TypeSelector]` and `this.Marker()` misuse in the IDE.
- VisualElement Extensions set element properties, styles and events in a chain.
- SerializedProperty Extensions update the object, write a value and apply the change in one chain.
- Editor Helpers: `GetDisplayName()` and `GetDisplayNameWithIndex()` label objects and components with readable names.
- The FastTools window (**Tools → Aspid 🐍 → FastTools**) with the Welcome, Asset References, Project References and Settings tabs, and themes for the FastTools windows.
- Samples for every feature, and Agent Skills that teach a coding agent to write code with FastTools.

[1.0.0]: https://github.com/VPDPersonal/Aspid.FastTools/releases/tag/v1.0.0
