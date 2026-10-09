# Theme Override

Your own colors for the FastTools windows and the type picker, without touching the package.

## Quick start

1. Open **Tools → Aspid 🐍 → FastTools → Settings** and find **Appearance**. The same section is in **Preferences → Aspid.FastTools**.
2. Click **Create template…** and choose where in the project to save the file. FastTools writes a `.uss` file there and assigns it as the **Theme override**.
3. In the file, uncomment the tokens you want and change their values:

```css
:root {
    --aspid-colors-bg-dark: rgb(22, 30, 52);
    --aspid-colors-surface-card: rgba(32, 44, 72, 0.6);
}
```

Assigning or clearing the **Theme override** recolors the open windows at once. To return to the default look, clear the field.

## Where it applies

- the FastTools window: Welcome, Asset References, Project References and Settings;
- the FastTools pages in **Preferences** and **Project Settings**;
- the type picker.

> [!NOTE]
> Fields drawn in a regular Inspector start from the dark palette on both skins and take part of their colors from Unity's editor theme, so the tokens change only part of their look.

## Tokens

The sheet is layered on top of the built-in palette, so declare only the tokens you want to change. On Unity's light skin the FastTools window and its pages take `Aspid-FastTools-Default-Light.uss` first, and the override still comes after it.

| Tokens | What they color |
|---|---|
| `--aspid-colors-surface-canvas` | The window background under the dots |
| `--aspid-colors-surface-card`, `--aspid-colors-surface-card-dim` | Cards, buttons and input fields; the dimmer card |
| `--aspid-colors-bg-*` | Boxes, buttons, help boxes and the picker, in four steps: `darkness`, `dark`, `light`, `lightness` |
| `--aspid-colors-text-*` | Text, from the most prominent `lightness` to the dimmest `darkness` |
| `--aspid-colors-shade-*` | Borders and dividing lines, in the same four steps |
| `--aspid-colors-overlay-*`, `--aspid-colors-slider-handle` | Hover and control fills, scrollbars and slider handles |
| `--aspid-colors-status-success-*`, `-warning-*`, `-error-*`, `-info-*` | Status backgrounds, texts (`-text-`), borders (`-shade-`), tints (`-tint`) and the dots (`-blob`) |
| `--aspid-colors-switch-accent`, `-track-border`, `-handle`, `-handle-shadow` | Switches. They are not set by default, so switches follow the editor skin |
| `--aspid-icons-*` | The Home and Settings icons and the status icons |

`Aspid-FastTools-Default-Dark.uss` in `Packages/tech.aspid.fasttools/Editor/Resources/UI/` lists every token that has a default value.

## Sharing with the team

The assignment is stored in the editor preferences of this machine, per project, so each team member assigns the sheet themselves. **Reset to defaults → Per-user** clears it. The `.uss` file is an ordinary project asset: commit it to share the colors.
