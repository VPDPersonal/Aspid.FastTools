# Roadmap

> Русская версия: [ROADMAP.ru.md](ROADMAP.ru.md).

Where Aspid.FastTools goes after 1.0: the themes in order, without dates.

## 1.1

### Readable SerializeReference inspectors

A polymorphic field or list should tell what is inside without opening every element: a one-line summary of a
collapsed instance, an accent colour per type, and the type shown on a list element while it is dragged.

[#155](https://github.com/VPDPersonal/Aspid.FastTools/issues/155),
[#156](https://github.com/VPDPersonal/Aspid.FastTools/issues/156),
[#160](https://github.com/VPDPersonal/Aspid.FastTools/issues/160)

### TypeSelector that offers the right types

A new list element or an empty field starts with a sensible default type instead of nothing, and the picker hides
what does not belong: excluded types, other assemblies or namespaces, `[Obsolete]` types, or anything a custom rule
rejects.

[#154](https://github.com/VPDPersonal/Aspid.FastTools/issues/154),
[#157](https://github.com/VPDPersonal/Aspid.FastTools/issues/157)

### SerializeReference data that survives changes

Templates keep nested `[SerializeReference]` children, copied values survive an Editor restart, and a type whose
fields change can upgrade its old assets instead of losing their data.

[#158](https://github.com/VPDPersonal/Aspid.FastTools/issues/158),
[#159](https://github.com/VPDPersonal/Aspid.FastTools/issues/159),
[#162](https://github.com/VPDPersonal/Aspid.FastTools/issues/162)

### Types by name at runtime

A runtime API that turns a type name into a `Type` and an instance — for mods and data-driven content, where today
everything is Editor-only.

[#161](https://github.com/VPDPersonal/Aspid.FastTools/issues/161)

### Profiler markers by category

Markers go into the Profiler's own categories — AI, Physics, Rendering and the rest — so the Timeline colours them and a
filter hides what is not being looked at. A category is set for one call or once for a whole type.

### Quick fixes for analyzer warnings

Every FastTools warning in Rider or Visual Studio comes with a fix one click away: `using` for a marker scope that is
never closed, removing an `Allow` that has no effect on a managed reference. A selection can be wrapped in a marker
the same way.

## Later

### EnumValues that survive enum renames

Rows are keyed by member name, so renaming a member leaves its row behind. `[FormerlySerializedAs]` on the renamed
member carries the row over, and a project check lists every table with a missing key before a build ships it.

### Profiler markers everywhere

Markers in static methods and classes, where there is no `this` today; a value attached to a sample, such as the
number of agents a step processed; and a marker's timing read in a player build, for an on-screen overlay or a
performance test.

### Less boilerplate from source generators

Generated code for what is written by hand in every project: a field that adds its own `[RequireComponent]`, and
UI Toolkit event handlers bound by element name.

[#1](https://github.com/VPDPersonal/Aspid.FastTools/issues/1),
[#3](https://github.com/VPDPersonal/Aspid.FastTools/issues/3),
[#4](https://github.com/VPDPersonal/Aspid.FastTools/issues/4)

## Suggest an idea

Missing something? [Open an issue](https://github.com/VPDPersonal/Aspid.FastTools/issues/new) or add a 👍 to an
existing one — reactions help decide what comes first.
