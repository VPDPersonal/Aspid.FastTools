---
name: Aspid FastTools Editor
description: Dark, utilitarian design system for Unity Editor tooling — flat surfaces layered by neutral value, gemstone-themed semantic statuses in parallel text/shade scales, and a 4-stop background ramp for deep content layering.
version: 1.1.0
status: active
theme: dark

colors:
  # Core neutral surface ramp (darkest → lightest)
  darkness: "#1A1A1A"
  dark: "#242424"
  light: "#2E2E2E"
  lightness: "#383838"

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

  # Gemstone Bases — deep body fills for status backgrounds
  success: "#0C411E"  # Emerald
  warning: "#55370A"  # Topaz
  error: "#551414"    # Ruby
  info: "#0F2D55"     # Sapphire

  # Success (Emerald) scale (darkness → lightness)
  success-text: ["#376E41", "#55AF64", "#78EB91", "#B4FFC8"]
  success-shade: ["#19371E", "#285532", "#418250", "#5FBE73"]
  success-bg: ["#082814", "#0C411E", "#145A2D", "#1E783C"]

  # Warning (Topaz) scale
  warning-text: ["#785523", "#B9873C", "#F5B955", "#FFEB64"]
  warning-shade: ["#372814", "#5A411E", "#876432", "#B98C4B"]
  warning-bg: ["#2D1E05", "#55370A", "#7D5514", "#A5731E"]

  # Error (Ruby) scale
  error-text: ["#7D2D2D", "#B94141", "#EB5F5F", "#FFAFAF"]
  error-shade: ["#371919", "#5A2D2D", "#874141", "#BE5F5F"]
  error-bg: ["#320A0A", "#551414", "#7D2323", "#A53232"]

  # Info (Sapphire) scale
  info-text: ["#28507D", "#4182B9", "#5AB9F0", "#BEF0FF"]
  info-shade: ["#142337", "#23415F", "#376491", "#5591C8"]
  info-bg: ["#081932", "#0F2D55", "#194B82", "#2369B4"]

typography:
  fontFamily: "Unity default (Inter / system sans)"

  h1:
    fontSize: 16px
    fontWeight: "700"
    usage: "Inspector header titles"
  h2:
    fontSize: 14px
    fontWeight: "600"
    usage: "Section titles"
  h3:
    fontSize: 12px
    fontWeight: "500"
    usage: "Subheaders / header subtext"
  body:
    fontSize: 12px
    fontWeight: "400"
    usage: "Default inspector body text"

spacing:
  unit: 5px
  micro: 2px
  sm: 5px
  lg: 10px
  inspector-offset: -10px

radii:
  xs: 3px
  sm: 5px
  lg: 10px

elevation:
  level-0: "Unity Default Grey"
  level-1: "background — {colors.darkness}"
  level-2: "container — {colors.dark}"
  level-3: "raised — {colors.light}"

---

# Aspid FastTools — Design Language

Aspid FastTools ships as editor tooling for Unity 2022.3+, inheriting Unity's dark editor chrome while layering a disciplined, **Gemstone-themed** design language for content surfaces and status semantics.

## Brand & Voice

The personality is **engineer-first**: calm, dense, and grounded. The visual tone borrows from professional IDEs: deep jewel-toned backgrounds, saturated but controlled status accents, and a clear distinction between structural lines and interactive text.

## Color System (The Gemstone Model)

Color is organized into three parallel neutral ramps and four semantic status "Gemstones". Each axis follows a strict four-stop scale: **darkness → dark → light → lightness**.

- **Neutral Surface Ramp.** `{darkness}` (#1A1A1A) is for recessed strips; `{dark}` (#242424) is the default card fill; `{light}` (#2E2E2E) is for raised controls.
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

- **AspidBox** — The rounded-10 card. Uses `{colors.dark}` by default.
- **AspidDividingLine** — Structural rules. Always uses the `{shade}` scale or status `{shade}` variant to remain secondary to text.
- **AspidLabel** — Content blocks. Uses the `{text}` scale or status `{text}` variant for maximum contrast and "brilliance".
- **AspidInspectorHeader** — Hero strips. Pairs a 40px icon with `{typography.h1}`. Backgrounds are bound to Gemstone base colors.
- **AspidHelpBox** — Advice strips. Backgrounds use Gemstone base, borders use status `{shade-dark}`, and text uses status `{text-lightness}` for "etched" legibility.

## Principles

1. **Text over Lines.** Text is always 20-40% more saturated/lighter than the structural borders (Shades) surrounding it.
2. **Grounded Saturation.** We avoid pure whites and neons. Depth comes from pigment richness (Gemstone hues), not luminosity.
3. **Density over Whitespace.** Designed for 320px-wide inspectors. Row heights are 18–22px; base spacing is 5px.
4. **Status as State.** Color is only used to convey status. If an element is neutral, it must stay in the neutral grey ramp.
