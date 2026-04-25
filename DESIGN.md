---
name: Aspid FastTools Editor
description: Dark, utilitarian design system for Unity Editor tooling — flat surfaces layered by neutral value, semantic status colors in a four-stop darkness/dark/light/lightness scale, generous rounded corners, and a tight 5 px rhythm tuned for high-density inspector UI.
version: 1.0.0
status: active
theme: dark

colors:
  # Core neutral surface ramp (darkest → lightest)
  darkness: "#1A1A1A"
  dark: "#262626"
  light: "#2E2E2E"
  lightness: "#D2D2D2"

  # Neutral "shade" ramp — used for text, separators, and borders against surfaces
  shade-darkness: "#3C3C3C"
  shade-dark: "#787878"
  shade-light: "#C0C0C0"
  shade-lightness: "#FFFFFF"

  # Semantic base colors — muted, low-chroma body fills for containers/headers
  success: "#0C5830"
  warning: "#78580C"
  error: "#580C0C"
  info: "#0C5878"

  # Success four-stop scale (darkness → lightness)
  success-darkness: "#1E3C1E"
  success-dark: "#3C783C"
  success-light: "#60C060"
  success-lightness: "#7FFF7F"

  # Warning four-stop scale
  warning-darkness: "#463C1E"
  warning-dark: "#8C783C"
  warning-light: "#C0A030"
  warning-lightness: "#FFD440"

  # Error four-stop scale
  error-darkness: "#4F1E1E"
  error-dark: "#9E3C3C"
  error-light: "#C03030"
  error-lightness: "#FF4040"

  # Info four-stop scale
  info-darkness: "#1E3C46"
  info-dark: "#3C788C"
  info-light: "#30A0C0"
  info-lightness: "#40D4FF"

  # Extras observed in component skins
  error-text-hint: "#FF6666"
  warning-text-hint: "#AA8833"
  separator-hairline: "#383838"
  muted-id-label: "#808080"
  button-legacy: "#585858"
  button-legacy-hover: "#757575"
  hover-tint: "#E1E1E1"
  on-status-surface: "#FFFFFF"

typography:
  fontFamily: "Unity default (Inter / system sans)"

  h1:
    fontSize: 16px
    fontWeight: "700"
    lineHeight: 20px
    usage: "Inspector header titles"

  h2:
    fontSize: 14px
    fontWeight: "600"
    lineHeight: 18px
    usage: "Section titles"

  h3:
    fontSize: 12px
    fontWeight: "500"
    lineHeight: 16px
    usage: "Subheaders / header subtext"

  body:
    fontSize: 12px
    fontWeight: "400"
    lineHeight: 16px
    usage: "Default inspector body text"

  meta:
    fontSize: 11px
    fontWeight: "400"
    lineHeight: 14px
    usage: "ID chip labels, registry metadata, small captions"

  hint:
    fontSize: 10px
    fontWeight: "400"
    fontStyle: italic
    lineHeight: 13px
    usage: "Inline validation hints, int-only notices"

  icon-button:
    fontSize: 14px
    fontWeight: "600"
    lineHeight: 14px
    usage: "Glyph buttons (+, ×, ✓) in inline editors"

spacing:
  unit: 5px           # Base grid; nearly every dimension is a multiple of 5
  micro: 2px          # Hairline gaps between inline controls
  xs: 3px             # Tight padding in pill/chip corners
  sm: 5px             # Default padding inside cards, default margin
  md: 8px             # Pill label horizontal padding, button pads
  lg: 10px            # Container padding, row height bump
  xl: 20px            # Section separation
  row-height-sm: 18px # Small inline icon buttons
  row-height-md: 20px # List rows, toolbar controls
  row-height-lg: 22px # ID selector items
  row-height-xl: 40px # Inspector header image slot
  inspector-offset: -10px # Cancels Unity's default inspector left indent

radii:
  xs: 3px     # Tight glyph buttons
  sm: 5px     # Foldout shells, EnumValues header/container
  md: 6px     # Inline warning bars
  lg: 10px    # Default — cards, boxes, pill chips, helpboxes, entry rows
  pill: 10px  # ID chips read as pills because their height ≈ 2 × radius
  none: 0

elevation:
  model: "Value layering — depth comes from stacking neutrals, never from shadow."
  level-0: "background — host editor surface (Unity's default grey)"
  level-1: "surface — {colors.darkness} (recessed, e.g. help boxes on status-neutral)"
  level-2: "container — {colors.dark} inspector card fill"
  level-3: "raised — {colors.light} pill chips and inline controls lifted one step"
  level-4: "hover/active — shade-darkness or semantic -darkness tint on the same plane"

borders:
  hairline: 1px       # Separators, card outlines, chip rings
  medium: 2px         # Emphasized dividers, low-DPI hairline fallback
  bold: 3px           # Section dividers, low-DPI medium fallback
  low-dpi-bonus: 1px  # All divider widths gain +1 px at low DPI for legibility

shadows:
  none: "none"
  notes: "The system deliberately has no shadows; depth is expressed through the neutral value ramp and 1 px borders at the darkness end of the palette."

motion:
  philosophy: "Inherit Unity UIToolkit defaults. Hover and focus are instant value swaps — no eased transitions, no animation curves. The UI should feel immediate and tool-like, never playful."
  hover-feedback: "Instant background + border color swap toward the matching semantic -darkness/-lightness pair."
  focus-feedback: "Unity-default focus ring; not overridden."

status:
  success:
    icon: "medium green checkmark (1020×1008 asset)"
    surface: "{colors.success}"
    border: "{colors.success-lightness}"
    text-on-surface: "{colors.on-status-surface}"
    hover-surface: "{colors.success-darkness}"
    hover-accent: "{colors.success-lightness}"

  warning:
    icon: "medium yellow exclamation (1020×1008 asset)"
    surface: "{colors.warning}"
    border: "{colors.warning-lightness}"
    text-on-surface: "{colors.on-status-surface}"

  error:
    icon: "medium red cross (1020×1008 asset)"
    surface: "{colors.error}"
    border: "{colors.error-lightness}"
    text-on-surface: "{colors.on-status-surface}"
    inline-text: "{colors.error-text-hint}"

  info:
    surface: "{colors.info}"
    border: "{colors.info-lightness}"
    text-on-surface: "{colors.on-status-surface}"

components:
  box:
    role: "Generic rounded card wrapper."
    backgroundColor: "{colors.dark}"
    padding: "{spacing.lg}"
    radius: "{radii.lg}"
    variants:
      darkness: "{colors.darkness}"
      dark: "{colors.dark}"
      light: "{colors.light}"
      lightness: "{colors.lightness}"
      status-success: "{colors.success}"
      status-warning: "{colors.warning}"
      status-error: "{colors.error}"

  dividing-line:
    role: "Horizontal or vertical rule in neutral or semantic tint."
    sizes:
      thin: 1px
      medium: 2px
      bold: 3px
    low-dpi: "Add 1 px to each size."
    color-neutral: "{colors.shade-dark}"
    color-status-success: "{colors.success-dark}"
    color-status-warning: "{colors.warning-dark}"
    color-status-error: "{colors.error-dark}"
    color-status-info: "{colors.info-dark}"

  label:
    role: "Text element with built-in tint + optional divider slot."
    typography: "{typography.body}"
    color-neutral: "{colors.shade-light}"
    color-status-success: "{colors.success-light}"
    color-status-warning: "{colors.warning-light}"
    color-status-error: "{colors.error-light}"
    color-status-info: "{colors.info-light}"
    whitespace: "normal by default; ellipsis when placed in a header row"

  inspector-header:
    role: "Hero strip at the top of an inspector, conveys target status."
    padding: "5px 10px"
    flex-direction: row
    icon-slot: "40 × 40 px"
    icon-gap: 10px
    title: "{typography.h1}"
    subtitle: "{typography.h3}"
    default-status: success
    background: "{components.box.backgroundColor}"
    radius: "{radii.lg}"
    gradient-hint: "Uses --aspid-fasttools-colors-gradient bound to the current status base."

  help-box:
    role: "Single-line advisory strip with left icon."
    padding: "5px 10px"
    radius: "{radii.lg}"
    border: "{borders.hairline}"
    default-border: "{colors.lightness}"
    default-background: "{colors.darkness}"
    icon-size: "32–34 px"
    icon-gap: 9px
    status-variants: "success | warning | error | info — background = {colors.$status}, border = {colors.$status-lightness}"

  pill-chip:
    role: "Numeric ID badge inside registry rows."
    height: 22px
    min-width: 50px
    padding: "2px 8px"
    radius: "{radii.lg}"
    border: "1px solid {colors.darkness}"
    color: "{colors.shade-light}"
    backgroundColor: "{colors.light}"
    typography: "{typography.meta}"
    align: middle-center
    error-variant:
      color: "{colors.error-lightness}"
      backgroundColor: "{colors.error-darkness}"

  registry-row:
    role: "Single entry in an ID registry list."
    padding: 5px
    margin-right: 5px
    margin-bottom: 3px
    min-height: 22px
    radius: "{radii.lg}"
    backgroundColor: "{colors.shade-darkness}"
    text-field-gap: 5px
    inline-error-text: "{colors.error-text-hint}"

  icon-button:
    role: "20 × 18 glyph button used for add/confirm/delete inside rows."
    size: "20 × 18 px"
    radius: "{radii.xs}"
    margin-left: 5px
    color: "{colors.lightness}"
    border: "1px solid {colors.darkness}"
    backgroundColor: "{colors.light}"
    typography: "{typography.icon-button}"
    variants:
      delete-hover:
        color: "{colors.error-lightness}"
        borderColor: "{colors.error-lightness}"
        backgroundColor: "{colors.error-darkness}"
      confirm-hover:
        color: "{colors.success-lightness}"
        borderColor: "{colors.success-lightness}"
        backgroundColor: "{colors.success-darkness}"

  toolbar:
    role: "Sticky filter/sort bar above list content."
    padding-bottom: 5px
    border-bottom: "1px solid {colors.separator-hairline}"
    flex-direction: row
    children: "Toolbar search field, enum fields flex-grow:1 each."

  foldout-group:
    role: "Collapsible section for grouped registry entries."
    toggle-margin-left: 0
    header-surface: "{colors.light}"
    header-radius-top: "{radii.sm}"
    body-surface: "{colors.shade-darkness}"
    body-radius-bottom: "{radii.sm}"
    border: "1px solid {colors.dark}"

  warning-bar:
    role: "Inline compact error/warning strip above a form control."
    padding: "5px 8px"
    radius: "{radii.md}"
    backgroundColor: "{colors.error-darkness}"
    label-color: "#FF9999"
    label-size: "{typography.meta.fontSize}"
    action-button-padding: "2px 8px"

  type-selector-item:
    role: "Row in the TypeSelector popup window."
    height: 20px
    padding-inline: 5px
    title-color: "{colors.shade-light}"
    arrow-color: "{colors.shade-dark}"

  id-selector-item:
    role: "Row in the Id picker popup."
    height: 22px
    padding-inline: 6px
    name-align: middle-left
    id-color: "{colors.muted-id-label}"
    id-size: 11px
---

# Aspid FastTools — Design Language

Aspid FastTools ships as editor tooling for Unity 2022.3+, so this design system never fights the host: it inherits Unity's dark editor chrome, the system font stack, and UIToolkit's layout primitives, and then layers on a small, disciplined vocabulary for **content surfaces, status semantics, and inline actions inside inspectors and popup windows**. The goal is to make a generated ID registry or a `[SerializableType]` drawer feel like a first-class part of the Unity editor — recognizably ours, but never theatrical.

## Brand & Voice

The personality is **engineer-first**: calm, dense, legible. Designs here are not marketing pages — every pixel is in service of a reflection-driven inspector that a developer will stare at for hours. The visual tone borrows from professional DAWs and IDEs: dark greys, high-contrast text, saturated status accents used sparingly, and zero decorative motion. When a flourish appears (a 40 px status icon in an inspector header, a green glow on a confirm button), it is load-bearing: it tells you what state you are in or what will happen if you click.

## Color System

Color is organized into two parallel four-stop scales plus four semantic statuses, each of which is itself a four-stop scale. The progression is always named the same way: **darkness → dark → light → lightness**. This is both a naming convention and a contract — any component that exposes a `darkness | dark | light | lightness` class variant will read the correct value from the matching scale.

- **Neutral surfaces (`darkness` → `lightness`).** The darkest tone (#1A1A1A) is reserved for recessed strips and deep help-box backgrounds; `dark` (#262626) is the default card fill; `light` (#2E2E2E) is the one-step raise used by pill chips and inline controls; `lightness` (#D2D2D2) is used as an on-dark text tone and as a "blank slate" help-box border.
- **Shade scale.** A parallel neutral ramp used specifically for text, borders, and separators: `shade-darkness` (#3C3C3C) for subtle rules, `shade-dark` (#787878) for secondary metadata, `shade-light` (#C0C0C0) for primary on-dark copy, `shade-lightness` (#FFFFFF) for the rare pure-white emphasis.
- **Semantic statuses (success / warning / error / info).** Each status has a muted body color (e.g. success #0C5830) for filled surfaces and a four-stop ramp (`-darkness`, `-dark`, `-light`, `-lightness`) for borders, text, dividers, and hover states. The ramp is intentionally low-chroma at the dark end (near-neutral) and saturated at the light end so it stacks legibly on top of the neutral ramp.

Status colors are **semantic, not decorative**. Red (`error`) always means "this input is invalid, this operation will destroy data, or this entry is broken"; yellow (`warning`) is reserved for advisories like the int-only-registry hint; green (`success`) confirms validation and commit affordances; blue (`info`) is used for neutral contextual callouts.

## Typography

Type is tight and functional. There is no custom font: the system uses whatever Unity's editor resolves (effectively Inter / a sans-serif system stack) and leans on size and weight to build hierarchy.

- **H1 (16 px / 700)** is reserved for the inspector header title. It is always paired with a 40 px status icon and a thin subtitle below.
- **H3 (12 px / 500)** is the subtitle slot under H1 and the default for foldout section headers.
- **Body (12 px / 400)** is the working size for property labels and text fields.
- **Meta (11 px / 400)** appears inside pill chips, in the muted ID column of the picker, and in toolbar captions.
- **Hint (10 px / italic)** is used sparingly for inline validation copy (e.g. the yellow "int-only registry" line under the Id drawer).
- **Icon button (14 px / 600)** carries the glyph characters `+`, `×`, `✓` inside 20 × 18 px buttons.

Line length is controlled by the host inspector width; labels set `overflow: hidden` with `text-overflow: ellipsis` so truncation, not wrapping, is the preferred failure mode in header rows. Body-level `AspidLabel` uses `white-space: normal` so multi-line help copy is allowed.

## Spacing & Rhythm

The system uses a **5 px base grid**. Almost every padding, margin, gap, and border-radius in the codebase is a multiple of 5 (5, 10, 20) with a few principled exceptions: 2 px for hairline gaps between inline controls, 3 px for snug chip internals, 8 px for pill-label horizontal padding, and a deliberate −10 px left offset that cancels Unity's default inspector indent so custom drawers align with native property fields.

Row heights cluster around **18 / 20 / 22 px**: 18 for compact icon buttons, 20 for list rows and toolbar controls, 22 for ID-picker entries and registry chip heights. The 40 × 40 header icon is the only element that breaks the rhythm — it intentionally sits as a hero.

## Shape & Radii

Corners are generous for editor software. The default radius is **10 px**, applied to nearly every content container: cards, help boxes, pill chips, registry rows, add/confirm toolbars. Smaller radii are used deliberately: **3 px** on tight 20 × 18 glyph buttons (they need to read as crisp controls, not blobs), **5 px** on foldout shell headers and containers (a softer rectangle that reads as structural chrome), **6 px** on inline warning bars (tight but not sharp). There is no square-cornered surface in the system; if something is sharp, it is a separator or a Unity-owned control.

## Elevation & Depth

Depth is conveyed **entirely through the neutral value ramp**. There are no drop shadows, no blur, no translucency. A UI that sits "higher" simply uses a lighter neutral: a pill chip on a `dark` card is filled with `light`; a hover state on an icon button swaps to the status `-darkness` tint on the same plane. The only thing approximating a shadow is a 1 px border drawn in `darkness` around raised chips, which crisps the edge against the card beneath it.

Borders also scale by DPI: dividing lines come in thin/medium/bold (1/2/3 px), and each gains +1 px at low DPI so hairlines never disappear. This is important because Unity's editor can render at non-integer scales.

## Status & Feedback

Status is a first-class axis. An `AspidInspectorHeader`, an `AspidHelpBox`, or an `AspidDividingLine` all accept a `status-success | -warning | -error | -info` class that repaints their surface, border, text, and (for headers) icon in lockstep. This is why the color scales are all four stops: a header uses the `base` tone for its body, the `-lightness` tone for its border, and the `-darkness` tone for a hover surface, and everything stays visually aligned across the four states.

Error state bleeds into text as well: inline form errors are rendered at `#FF6666` — slightly softer than pure `error-lightness` — to keep them legible over dark cards without screaming. Warning hints use `#AA8833` with italic 10 px copy, a deliberately understated variant for "you probably want to know this, but it isn't wrong."

Interactive feedback is instantaneous. Hovering a delete button swaps its fill to `error-darkness`, its border and glyph to `error-lightness`, and that's it — no transition, no scale, no ease. The same pattern applies in green for confirm buttons. The absence of motion is the motion design: tool UIs should feel like direct manipulation, not animation.

## Component Vocabulary

The editor ships a small set of composable primitives; every inspector in the codebase is assembled from them.

- **AspidBox** — the rounded-10 card. It is the default container for grouped content, and it is what defines the "a FastTools inspector looks like this" feeling.
- **AspidDividingLine** — horizontal or vertical rule; thin/medium/bold; optionally tinted by status. Its role is to replace heavier structural borders and keep the UI quiet.
- **AspidLabel** — text block that accepts the neutral and status tint classes, with an optional nested `AspidDividingLine` for a built-in underline.
- **AspidInspectorHeader** — the hero strip at the top of an inspector: 40 px status icon, H1 title, H3 subtitle, 5 × 10 padding, rounded-10 body, status-tinted gradient variable. It is the one place the system permits a bit of visual weight.
- **AspidHelpBox** — bordered 10 × 5 padded strip with a 32–34 px icon and flexible text area, used for persistent advisories at the top of a panel.
- **Pill chip + Registry row** — the ID registry's signature pattern. A 22 px row with a pill-shaped ID badge on the left, a text field in the middle, and two glyph buttons on the right. The chip itself is `light` on `darkness` border, 11 px meta text, center-aligned. When the entry is invalid, the chip swaps to the error pair (`error-darkness` fill, `error-lightness` text) and a small italic error caption appears below the row.
- **Icon button (20 × 18)** — the workhorse interaction surface. Neutral at rest, semantic on hover. Always 3 px radius, always preceded by a 5 px left margin so rows of buttons breathe.
- **Toolbar + Foldout group** — a filter/sort bar separated from the list below by a 1 px hairline at `#383838`, followed by grouped foldouts with `light` headers and `shade-darkness` bodies. The foldout shell uses 5 px radii (rather than 10) because it reads as structural chrome, not content.
- **Warning bar** — a dense inline `error-darkness` strip with a softened `#FF9999` label and a right-aligned action button. Padding is 5 × 8, radius is 6; it is deliberately tighter than a help box so it can live above a form field without disrupting the row rhythm.

## Layout Principles

1. **Align to the host grid, not your own.** Inspectors start with a −10 px left margin to cancel Unity's default inspector indent, so custom drawers sit flush with native `PropertyField`s.
2. **One container, one radius.** Do not nest rounded-10 boxes inside other rounded-10 boxes. If you need a raised element inside a card, use a 10 px pill chip or a 5 px foldout header — the shape itself communicates the nesting level.
3. **Status is the loudest thing on the screen.** Because the neutral palette is so muted, a single saturated `-lightness` accent instantly draws the eye. Use at most one status color per visible region; if you need to show both a warning and an error, stack them vertically rather than placing them side by side.
4. **Separators over boxes.** When in doubt, use an `AspidDividingLine` instead of wrapping content in another card. The system is intentionally flatter than native Unity inspectors.
5. **Density over whitespace.** Row heights are 18–22 px and gaps are 2–5 px because these panels often live in a 320 px-wide inspector. Preserve that density; if you find yourself reaching for 16 px gaps, you are designing for a different context.

## Accessibility Notes

- Text-on-surface pairs meet WCAG AA at the chosen sizes: `shade-light` on `dark` and `shade-light` on `darkness` both clear the 4.5:1 threshold for body text, and `-lightness` status text on the matching status surface clears the 3:1 large-text threshold used for headers.
- Status is never encoded in color alone. Inspector headers pair their status color with a distinct icon (green check / yellow exclamation / red cross), and error chips pair their red fill with explicit error copy below the row.
- Focus uses Unity UIToolkit's default outline; do not override it. Hover states are an enhancement, not the primary affordance.
- Low-DPI rendering is explicitly supported: all dividers carry a low-DPI modifier that bumps line weight by 1 px so 1 px rules don't vanish at fractional scales.

## What this system is *not*

- **Not a content system.** There are no marketing type scales, no display sizes, no illustrated states. If you need a hero, you are in the wrong tool.
- **Not a motion system.** Transitions, keyframed animations, and easing curves are intentionally absent. Any future motion should be Unity's own (e.g. foldout expand) and should remain imperceptible.
- **Not a light theme.** Every value is tuned against a dark editor chrome. A light-theme variant would require a full re-tint of both the neutral ramp and the four status scales; it is not a drop-in inversion.
- **Not skinnable at runtime.** Surfaces are colored via USS classes, not runtime tokens. Variants are added by composing classes (`aspid-fasttools-background aspid-fasttools-dark aspid-fasttools-status-error`), not by mutating CSS variables from code.
