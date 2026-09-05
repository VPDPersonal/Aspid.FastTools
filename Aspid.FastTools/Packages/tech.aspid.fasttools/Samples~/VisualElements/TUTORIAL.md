# VisualElements — Step-by-Step Tutorial

The Aspid.FastTools `VisualElement` extensions turn UIToolkit's verbose, mutation-style API into a **fluent, chainable**
one. Every `Set*` / `Add*` returns the element it was called on, so a whole widget — style, layout, children and
behaviour — is one expression:

```csharp
var badge = new Label("FREE")
    .SetColor(Color.white)
    .SetPadding(top: 3, bottom: 3, left: 10, right: 10)
    .SetBorderRadius(10);
```

Unlike the other samples, the lessons here are not inspector fields you tweak — they are **cards in a custom
inspector built entirely in code**. Each `STEP` card in `Scripts/Editor/VisualElementsTutorialEditor.cs` is one lesson;
read the card in the Inspector and the method it demonstrates side by side.

## Open the tutorial

1. Open the Welcome window (**Tools → Aspid 🐍 → FastTools → Welcome**) and import the **VisualElements** sample.
2. Open **`Scenes/VisualElementsTutorial.unity`** and select the **VisualElements Tutorial** GameObject.
3. The Inspector shows five STEP cards. No Play Mode needed — everything runs in the editor inspector.

> **The component holds no UI.** `VisualElementsTutorial` is just three serialized knobs; the entire inspector is
> composed by `VisualElementsTutorialEditor.CreateInspectorGUI()`. That split — plain data object, fluent editor — is
> the pattern to copy for your own inspectors.

---

## Lesson 1 — Fluent style basics

**Card:** *STEP 1* — a coloured swatch.

Every style setter returns the element, so styling is one uninterrupted chain — no local variable, no
`element.style.paddingTop = …` repetition:

```csharp
var swatch = new VisualElement()
    .SetHeight(44)
    .SetBackgroundColor(_accent)
    .SetBorderColor(_cardBorder)
    .SetBorderWidth(1)
    .SetBorderRadius(8);
```

- Each `Set*` maps to one `IStyle` property but reads as a sentence and never breaks the chain.
- The generic return type is the element's own type, so the chain keeps the element's API — a `Label` stays a `Label`
  after `.SetColor(...)`, so `Label`-only setters remain available downstream.

---

## Lesson 2 — Text & font presets

**Card:** *STEP 2* — four labels: normal, bold, italic, letter-spaced.

Instead of passing a raw `FontStyle` enum, the **presets** name the intent and compose with the other text setters:

```csharp
new Label("...").SetNormalUnityFontStyleAndWeight();
new Label("...").AddBoldUnityFontStyleAndWeight();
new Label("...").AddItalicUnityFontStyleAndWeight();
new Label("...").SetFontSize(11).SetLetterSpacing(4);
```

- `Add*` **combines** with the current style (bold *and* italic is `AddBold…().AddItalic…()`); `Set*` **replaces** it.
  `Remove*` counterparts (`RemoveBold…`, `RemoveItalic…`) drop one flag without touching the other.
- These presets live in both `VisualElement` and `IStyle` flavours, so they work on an element directly or on a
  resolved `IStyle`.

---

## Lesson 3 — Layout & composition

**Card:** *STEP 3* — a header row: the ability name on the left, an `ABILITY` tag on the right.

Flex layout setters plus `AddChild` build a composite element in place. `AddChild` returns the **parent**, so children
are appended in a chain:

```csharp
var row = new VisualElement()
    .SetFlexDirection(FlexDirection.Row)
    .SetAlignItems(Align.Center)
    .SetJustifyContent(Justify.SpaceBetween)
    .AddChild(name)
    .AddChild(tag);
```

- `AddChild` returns the container it was called on (not the child), which is what lets you append several children
  fluently; the child was fully built by its own chain before being handed in.
- The name label is fed from `VisualElementsTutorial.AbilityName` — change **Ability Name** on the component and
  reopen the inspector to see it flow through.

---

## Lesson 4 — Reactive UI

**Card:** *STEP 4* — a Mana Cost field with a live badge below it.

`PropertyField(...).AddValueChanged(callback)` runs your callback on every edit — the same wiring the `AbilityConfig`
demo inspector uses:

```csharp
var badge = new Label().AddBoldUnityFontStyleAndWeight();
var manaField = new PropertyField(serializedObject.FindProperty("_manaCost"))
    .AddValueChanged(_ => Refresh());

void Refresh()
{
    var isFree = tutorial.ManaCost is 0;
    var color = isFree ? _warning : _accent;
    badge.SetText(isFree ? "FREE" : $"{tutorial.ManaCost} MP")
        .SetColor(color)
        .SetBorderColor(color);
}
```

Edit **Mana Cost** in the card: the badge re-colours and switches to **FREE** at `0`. `AddValueChanged` binds the
callback and returns the field, so it stays inside the build chain.

---

## Lesson 5 — Element extensions breadth

**Card:** *STEP 5* — a ProgressBar, a HelpBox and a Button.

The same fluent style extends every built-in element, each with its own typed setters:

```csharp
var bar = new ProgressBar().SetLowValue(0f).SetHighValue(1f);
bar.SetValue(charge).SetTitle($"{charge:P0} charged");

new HelpBox().SetMessageType(HelpBoxMessageType.Info).SetText("Fully charged.").SetDisplay(DisplayStyle.Flex);

new Button().SetText("Log charge").AddClicked(() => Debug.Log($"Charge is {tutorial.Charge:P0}"));
```

- Drag **Charge** on the card: the bar fills and its title updates live; the HelpBox appears (`SetDisplay`) only at
  100%.
- `AddClicked(Action)` registers a click handler and returns the button — press **Log charge** to print the value.
- `SetDisplay(DisplayStyle.None)` removes an element from layout (no space reserved); `.SetVisibility(...)` /
  `visible` would only hide it while keeping its box.

---

## Styling via USS instead of inline

The lessons style inline for a self-contained demo, but the same extensions carry the USS route:

```csharp
element.AddClass("aspid-card");   // add a USS class; style it in a .uss stylesheet
element.RemoveClass("aspid-card");
```

For anything beyond a one-off inspector, prefer USS classes over inline colours — code only calls `.AddClass(...)`,
the look lives in the stylesheet. The package's own editor components follow exactly this split.

---

## Where to look in code

| File | Shows |
|---|---|
| `Scripts/Editor/VisualElementsTutorialEditor.cs` | All five lessons as `STEP` cards, each built with the fluent API |
| `Scripts/Tutorial/VisualElementsTutorial.cs` | The plain data component the editor draws (three serialized knobs) |
| `Scripts/AbilityConfig.cs` / `Scripts/Editor/AbilityConfigEditor.cs` | The same API on a real `ScriptableObject` inspector (the demo in README) |
| [README.md](README.md) | The compact demo-inspector walkthrough |
