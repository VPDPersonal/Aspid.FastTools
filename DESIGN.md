---
name: Aspid FastTools Editor
description: Dark, utilitarian design system for Unity Editor tooling — flat surfaces layered by neutral value, gemstone-themed semantic statuses in parallel text/shade scales, and a 4-stop background ramp for deep content layering.
version: 1.2.0
status: active
theme: dark

colors:
  # Core neutral surface ramp (darkest → lightest)
  bg-darkness: "#1A1A1A"
  bg-dark: "#242424"
  bg-light: "#2E2E2E"
  bg-lightness: "#383838"

  # Primary text ramp (Dimmed for eye comfort)
  text-darkness: "#6E6E6E"
  text-dark: "#969696"
  text-light: "#BEBEBE"
  text-lightness: "#DCDCDC"

  # Neutral "shade" ramp — used for separators and borders
  shade-darkness: "#2D2D2D"
  shade-dark: "#3C3C3C"
  shade-light: "#505050"
  shade-lightness: "#646464"

  # Success (Emerald) — bg / text / shade scales (darkness → lightness)
  success-bg: ["#082814", "#0C411E", "#145A2D", "#1E783C"]
  success-text: ["#376E41", "#55AF64", "#78EB91", "#B4FFC8"]
  success-shade: ["#19371E", "#285532", "#418250", "#5FBE73"]

  # Warning (Topaz)
  warning-bg: ["#2D1E05", "#55370A", "#7D5514", "#A5731E"]
  warning-text: ["#785523", "#B9873C", "#F5B955", "#FFEBAF"]
  warning-shade: ["#372814", "#5A411E", "#876432", "#B98C4B"]

  # Error (Ruby)
  error-bg: ["#320A0A", "#551414", "#7D2323", "#A53232"]
  error-text: ["#7D2D2D", "#B94141", "#EB5F5F", "#FFAFAF"]
  error-shade: ["#371919", "#5A2D2D", "#874141", "#BE5F5F"]

  # Info (Sapphire)
  info-bg: ["#081932", "#0F2D55", "#194B82", "#2369B4"]
  info-text: ["#28507D", "#4182B9", "#5AB9F0", "#BEF0FF"]
  info-shade: ["#142337", "#23415F", "#376491", "#5591C8"]

typography:
  fontFamily: "Unity default (Inter / system sans)"
  scale: "AspidLabel — 7 steps (H1..H7), selected via `aspid-fasttools-label-size--h1…h7` classes or the `--aspid-fasttools-metrics-label_size` custom property. H7 is the inspector default."

  h1: { fontSize: 36px, usage: "Hero displays · welcome screens, animated titles" }
  h2: { fontSize: 24px, usage: "Editor-window titles" }
  h3: { fontSize: 18px, usage: "Page-level titles inside windows" }
  h4: { fontSize: 16px, usage: "AspidInspectorHeader title" }
  h5: { fontSize: 14px, usage: "Section titles, AspidGradientButton labels" }
  h6: { fontSize: 13px, usage: "AspidInspectorHeader subtext, secondary captions" }
  h7: { fontSize: 12px, usage: "Default inspector body, AspidLabel base" }

# Spacing & radii are conventions, not tokens. There is no shared
# `--aspid-fasttools-metrics-spacing-*` / `--aspid-fasttools-metrics-radius-*`
# palette — values are applied directly in component stylesheets. New
# components should reuse the recurring set rather than introduce variants.
spacing:
  hairline: 2px
  base: 5px           # row gap, vertical rhythm — the unit of the system
  group: 10px         # group padding, container insets
  inspector-offset: -10px  # `.aspid-fasttools-inspector-container` left offset to align with Unity's chrome

radii:
  help-box: 8px       # AspidHelpBox
  card: 10px          # AspidBox · AspidGradientButton · headers

elevation:
  level-0: "Unity Default Grey"
  level-1: "background — {colors.bg-darkness}"
  level-2: "container — {colors.bg-dark}"
  level-3: "raised — {colors.bg-light}"

---

# Aspid FastTools — Design Language

Aspid FastTools ships as editor tooling for Unity 6.0+, inheriting Unity's dark editor chrome while layering a disciplined, **Gemstone-themed** design language for content surfaces and status semantics.

## Brand & Voice

The personality is **engineer-first**: calm, dense, and grounded. The visual tone borrows from professional IDEs: deep jewel-toned backgrounds, saturated but controlled status accents, and a clear distinction between structural lines and interactive text.

## Color System (The Gemstone Model)

Color is organized into three parallel neutral ramps and four semantic status "Gemstones". Each axis follows a strict four-stop scale: **darkness → dark → light → lightness**.

- **Neutral Surface Ramp.** `{bg-darkness}` (#1A1A1A) is for recessed strips; `{bg-dark}` (#242424) is the default card fill; `{bg-light}` (#2E2E2E) is for raised controls; `{bg-lightness}` (#383838) for the brightest neutral surface.
- **General Text Ramp.** Dimmed to avoid "neon glow" on dark backgrounds. `{text-lightness}` (#DCDCDC) for primary headers; `{text-light}` (#BEBEBE) for body text.
- **Shade Ramp.** Muted neutrals used exclusively for structural borders and separators (e.g., `{shade-dark}` #3C3C3C).
- **Gemstone Statuses.**
    - **Emerald (Success)**: Cold, clean green for validation and confirmed states.
    - **Ruby (Error)**: Deep, saturated red for destruction and invalid inputs.
    - **Topaz (Warning)**: Golden amber for advisories.
    - **Sapphire (Info)**: Royal blue for neutral callouts.

### Scaling Strategy
Each Gemstone is subdivided into:
1.  **Background Scale**: Deep, low-chroma body fills for containers.
2.  **Text Scale**: High-chroma, vibrant variants for readable labels.
3.  **Shade Scale**: Muted, mid-tone variants for borders and status lines.

## Components

The kit is split into **structural** elements (load-bearing inspector surfaces) and **decorative** elements (welcome-screen and accent flourishes).

### Structural

- **AspidBox** — The rounded-10 card. Uses `{bg-dark}` by default.
- **AspidDividingLine** — Structural rules. Always uses the `{shade}` scale or status `{shade}` variant to remain secondary to text.
- **AspidLabel** — Content blocks. Uses the `{text}` scale or status `{text}` variant for maximum contrast and "brilliance". Exposes the 7-step H1..H7 size scale via `aspid-fasttools-label-size--h1…h7`.
- **AspidInspectorHeader** — Hero strips. Pairs a 40px icon with `{typography.h4}` title and `{typography.h6}` subtext. Backgrounds are bound to Gemstone base colors.
- **AspidHelpBox** — Advice strips. Backgrounds use Gemstone base, borders use status `{shade-dark}`, and text uses status `{text-lightness}` for "etched" legibility.

### Decorative

- **AspidGradientButton** — Rounded-10 call-to-action button with a Gemstone-tinted gradient fill and `{typography.h5}` label.
- **AspidAnimatedTitle** — Animated hero title used on welcome surfaces; pairs with `{typography.h1}` display sizing.
- **AspidAnimatedLogo** — Layered animated logo with USS-driven pulse-speed and hover-amplitude controls (`AspidAnimatedLogoPulseSpeedStyle`, `AspidAnimatedLogoPulseHoverAmplitudeStyle`).
- **AspidAnimatedDotsBackground** — Subtle animated dot field used as a backdrop for welcome screens.
- **AspidHoverGradientOverlay** — Hover-tracking gradient overlay reused by other components (notably `AspidGradientButton`) to render the cursor highlight.

## Principles

1. **Text over Lines.** Text is always 20-40% more saturated/lighter than the structural borders (Shades) surrounding it.
2. **Grounded Saturation.** We avoid pure whites and neons. Depth comes from pigment richness (Gemstone hues), not luminosity.
3. **Density over Whitespace.** Designed for 320px-wide inspectors. Row heights are 18–22px; base spacing is 5px.
4. **Status as State.** Color is only used to convey status. If an element is neutral, it must stay in the neutral grey ramp.
