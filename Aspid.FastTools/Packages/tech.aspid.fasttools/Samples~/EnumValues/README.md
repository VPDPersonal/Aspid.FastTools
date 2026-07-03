# EnumValues Sample

A tiny combat damage system that maps enum members to typed values through `EnumValues<TValue>` and its typed twin `EnumValues<TEnum, TValue>`. `DamageDealer` picks a `DamageType` and `StatusEffect` in the Inspector, then on `Space` applies damage — pulling the damage multiplier, log color, and speed modifier from three `EnumValues` fields. The color field is the typed variant (`EnumValues<DamageType, Color>` — no type-picker row in the Inspector); the other two pick their enum in the Inspector.

> **New here? Start with [TUTORIAL.md](TUTORIAL.md)** ([RU](TUTORIAL_RU.md)) — a guided, step-by-step tour (Lessons 1–5) built around `Scripts/Tutorial/EnumValuesTutorial.cs` and `Scenes/EnumValuesTutorial.unity`. This page is the demo-scene walkthrough; the tutorial teaches the workflow.

The code lives in `Scripts/` — see the inline comments in `DamageDealer.cs` and `StatusEffect.cs`.

## Lookup rules for `[Flags]` keys

1. **Exact match wins first**, regardless of entry order: looking up `Burning | Slowed` returns the `Burning | Slowed` entry even though it is listed *last*.
2. **No exact match → first contained entry wins**: looking up `Burning | Frozen | Slowed` (no such entry) returns the first entry (in list order) whose flags are all contained in the value — here `Burning`.
3. **Nothing matches → default value**: `Stunned` has no entry, so the lookup falls back to `_defaultValue`. `None` (zero) only ever matches a `None` entry, never a flag entry.

## How to run

Open `Scenes/EnumValues.unity` and enter Play Mode. The scene hosts a pre-seeded `DamageDealer` wired up from `Prefabs/EnumValues.prefab`.

Press `Space` and the Console prints `Fire hit: 15 dmg (speed mod: 0.40)` in orange — the composite `Burning | Slowed` entry wins by exact match even though it is listed last. Then try the other lookup rules in the Inspector:

- set `_activeEffects` to `Burning | Frozen | Slowed` → `0.90` (no exact entry; first contained entry `Burning` wins);
- set it to `Stunned` (or `None`) → `1.00` (no entry matches; default value).
