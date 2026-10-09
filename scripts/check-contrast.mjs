// Check the WCAG contrast of the Editor palette without Unity: text against the background it sits on, with
// Default-Dark on either skin, Default-Light in an Aspid window on the light skin, and the light-skin overrides of the
// type picker dropdown. Text needs 4.5:1; large text, glyph icons and UI borders need 3:1.
//   node scripts/check-contrast.mjs [--all]
// Pairs come from two sources: the list below, for a background and a text set in different rules (a component box and
// the labels in it), and every package rule that sets both `color` and `background-color`. A pair with a `baseline` is
// known to be below the minimum: it passes while it stays at that ratio and fails if it drops lower.
import { readdirSync, readFileSync } from 'node:fs';
import { join, relative } from 'node:path';

const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const UI = join(PACKAGE, 'Editor/Resources/UI');
const DARK_SHEET = join(UI, 'Aspid-FastTools-Default-Dark.uss');
const LIGHT_SHEET = join(UI, 'Aspid-FastTools-Default-Light.uss');
const TYPE_SELECTOR_SHEET = join(UI, 'Types/Aspid-FastTools-TypeSelector.uss');
const LIGHT_DROPDOWN_SELECTOR = '.aspid-fasttools-type-selector--dropdown.aspid-fasttools-skin--light';

const TEXT = 4.5;
const LARGE = 3;

// Unity's theme variables (USS built-in variable reference: Professional = dark skin, Personal = light skin).
const UNITY = {
  dark: {
    '--unity-colors-window-background': '#383838',
    '--unity-colors-dropdown-background': '#515151',
    '--unity-colors-dropdown-border': '#303030',
    '--unity-colors-default-text': '#D2D2D2',
    '--unity-colors-label-text': '#C4C4C4',
    '--unity-colors-warning-text': '#F4BC02',
    '--unity-colors-helpbox-text': '#BDBDBD',
  },
  light: {
    '--unity-colors-window-background': '#C8C8C8',
    '--unity-colors-dropdown-background': '#DFDFDF',
    '--unity-colors-dropdown-border': '#B2B2B2',
    '--unity-colors-default-text': '#090909',
    '--unity-colors-label-text': '#090909',
    '--unity-colors-warning-text': '#333308',
    '--unity-colors-helpbox-text': '#161616',
  },
};

const STATUSES = ['success', 'warning', 'error', 'info'];

// A background is one colour or a list composited bottom-up; a colour is a token without its "--aspid-colors-"
// prefix, a Unity variable or a literal. `fg` and `baseline` may differ per context.
const CARD = ['surface-canvas', 'surface-card'];
const WINDOW = ['dark', 'light-window'];
const INSPECTOR = ['dark', 'light-inspector'];
const DROPDOWN = ['dark-dropdown', 'light-dropdown'];

const DARK_DROPDOWN = value => ({ 'dark-dropdown': value });

const LIST_BG = '--unity-colors-window-background';
const ROW_HOVER = ['--unity-colors-window-background', 'status-info-shade-darkness'];
const ROW_SELECTED = ['--unity-colors-window-background', 'status-info-shade-dark'];

const PAIRS = [
  // Window cards: settings sections, welcome samples, reference graph nodes, repair groups.
  { where: 'card title', fg: 'text-lightness', bg: CARD, min: TEXT, in: WINDOW },
  { where: 'card caption', fg: 'text-light', bg: CARD, min: TEXT, in: WINDOW },
  { where: 'card description', fg: 'text-dark', bg: CARD, min: TEXT, in: WINDOW },
  { where: 'card link', fg: 'status-success-text-dark', bg: CARD, min: TEXT, in: WINDOW },
  { where: 'card link hover', fg: 'status-success-text-light', bg: CARD, min: TEXT, in: WINDOW },
  ...['warning', 'info'].flatMap(s => [
    { where: `card ${s} action`, fg: `status-${s}-text-dark`, bg: CARD, min: TEXT, in: WINDOW },
    { where: `card ${s} label`, fg: `status-${s}-text-light`, bg: CARD, min: TEXT, in: WINDOW },
    { where: `hovered ${s} action`, fg: `status-${s}-text-light`, bg: 'bg-dark', min: TEXT, in: WINDOW },
  ]),
  { where: 'card error', fg: 'status-error-text-light', bg: CARD, min: TEXT, in: WINDOW },
  { where: 'hovered row caption', fg: 'text-light', bg: 'bg-dark', min: TEXT, in: WINDOW },
  { where: 'hovered row detail', fg: 'text-dark', bg: 'bg-dark', min: TEXT, in: WINDOW },

  // AspidHelpBox: title and message take the lightness theme (AspidHelpBoxPreset).
  { where: 'help box', fg: 'text-lightness', bg: 'bg-dark', min: TEXT, in: WINDOW },
  ...STATUSES.map(s => (
    { where: `help box ${s}`, fg: `status-${s}-text-lightness`, bg: `status-${s}-light`, min: TEXT, in: WINDOW })),

  // InspectorNotice links on Unity's inspector background; the light skin hovers to a darker brown.
  { where: 'notice link', fg: '--unity-colors-warning-text', bg: LIST_BG, min: TEXT, in: INSPECTOR },
  {
    where: 'notice link hover',
    fg: { 'dark': 'status-warning-text-lightness', 'light-inspector': 'status-warning-shade-dark' },
    bg: LIST_BG, min: TEXT, in: INSPECTOR,
  },

  // Type picker dropdown: rows on Unity's popup background, header and footer on bg-dark. The dark skin keeps the
  // window palette, made for darker cards, so some of its text is still below the minimum there.
  { where: 'picker row title', fg: 'text-lightness', bg: LIST_BG, min: TEXT, in: DROPDOWN },
  { where: 'picker row count', fg: 'text-dark', bg: LIST_BG, min: TEXT, in: DROPDOWN, baseline: DARK_DROPDOWN(3.96) },
  {
    where: 'picker selected row count',
    fg: 'text-dark', bg: ROW_SELECTED, min: TEXT, in: DROPDOWN, baseline: DARK_DROPDOWN(3.56),
  },
  { where: 'picker hovered row count', fg: 'text-dark', bg: ROW_HOVER, min: TEXT, in: DROPDOWN },
  {
    where: 'picker error',
    fg: 'status-error-text-light', bg: LIST_BG, min: TEXT, in: DROPDOWN, baseline: DARK_DROPDOWN(3.53),
  },
  { where: 'picker breadcrumb', fg: 'text-light', bg: 'bg-dark', min: TEXT, in: DROPDOWN },
  { where: 'picker breadcrumb hover', fg: 'status-info-text-lightness', bg: 'bg-dark', min: TEXT, in: DROPDOWN },
  {
    where: 'picker footer hint',
    fg: 'text-darkness', bg: 'bg-dark', min: TEXT, in: DROPDOWN, baseline: DARK_DROPDOWN(3.04),
  },
  { where: 'picker favorite star', fg: 'status-warning-text-light', bg: ROW_SELECTED, min: LARGE, in: DROPDOWN },
  { where: 'picker favorite star hover', fg: 'status-warning-text-lightness', bg: ROW_HOVER, min: LARGE, in: DROPDOWN },
  {
    where: 'picker favorite toggle',
    fg: 'text-darkness', bg: ROW_SELECTED, min: LARGE, in: DROPDOWN, baseline: DARK_DROPDOWN(2.06),
  },
  { where: 'picker check mark', fg: 'status-success-text-dark', bg: ROW_SELECTED, min: LARGE, in: DROPDOWN },
];

// --- USS parsing ---

function rules(file) {
  const css = readFileSync(file, 'utf8').replace(/\/\*[\s\S]*?\*\//g, '');
  return [...css.matchAll(/([^{}]+)\{([^{}]*)\}/g)].map(([, selector, body]) => ({
    selector: selector.trim().replace(/\s+/g, ' '),
    declarations: Object.fromEntries(body.split(';')
      .map(d => d.trim())
      .filter(d => d.includes(':'))
      .map(d => [d.slice(0, d.indexOf(':')).trim(), d.slice(d.indexOf(':') + 1).trim().replace(/\s+/g, ' ')])),
  }));
}

function tokens(file, selector) {
  const rule = rules(file).find(r => r.selector === selector);
  if (!rule) throw new Error(`${file}: no "${selector}" rule`);
  return Object.fromEntries(Object.entries(rule.declarations).filter(([name]) => name.startsWith('--')));
}

const darkTokens = tokens(DARK_SHEET, ':root');
const lightTokens = tokens(LIGHT_SHEET, '.aspid-fasttools-palette--light');
const dropdownTokens = tokens(TYPE_SELECTOR_SHEET, LIGHT_DROPDOWN_SELECTOR);

// Custom properties of one element resolve against that element's own values, so each context is one flat map.
const CONTEXTS = {
  'dark': { ...darkTokens, ...UNITY.dark },
  'light-window': { ...darkTokens, ...lightTokens, ...UNITY.light },
  'light-inspector': { ...darkTokens, ...UNITY.light },
  'dark-dropdown': { ...darkTokens, ...UNITY.dark },
  'light-dropdown': { ...darkTokens, ...UNITY.light, ...dropdownTokens },
};

// --- Colours ---

function resolve(value, context, seen = new Set()) {
  const ref = value.match(/^var\(\s*(--[\w-]+)\s*\)$/);
  if (ref) {
    if (seen.has(ref[1])) throw new Error(`cyclic ${ref[1]}`);
    if (!(ref[1] in context)) throw new Error(`${ref[1]} is not defined`);
    return resolve(context[ref[1]], context, new Set(seen).add(ref[1]));
  }
  if (value.startsWith('--')) return resolve(`var(${value})`, context, seen);
  if (!value.startsWith('#') && !value.startsWith('rgb')) return resolve(`var(--aspid-colors-${value})`, context, seen);
  return parse(value);
}

function parse(value) {
  let m = value.match(/^#([0-9a-f]{3}|[0-9a-f]{6})$/i);
  if (m) {
    const hex = m[1].length === 3 ? [...m[1]].map(c => c + c).join('') : m[1];
    const [r, g, b] = [0, 2, 4].map(i => parseInt(hex.slice(i, i + 2), 16));
    return { r, g, b, a: 1 };
  }
  m = value.match(/^rgba?\(\s*([\d.]+)\s*,\s*([\d.]+)\s*,\s*([\d.]+)\s*(?:,\s*([\d.]+)\s*)?\)$/);
  if (m) return { r: +m[1], g: +m[2], b: +m[3], a: m[4] === undefined ? 1 : +m[4] };
  throw new Error(`cannot parse colour "${value}"`);
}

const over = (top, bottom) => ({
  r: top.r * top.a + bottom.r * (1 - top.a),
  g: top.g * top.a + bottom.g * (1 - top.a),
  b: top.b * top.a + bottom.b * (1 - top.a),
  a: 1,
});

function layers(bg, context) {
  const list = (Array.isArray(bg) ? bg : [bg]).map(v => resolve(v, context));
  return list.reduce((below, top) => over(top, below), { r: 0, g: 0, b: 0, a: 1 });
}

function luminance({ r, g, b }) {
  const channel = c => (c /= 255) <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4;
  return 0.2126 * channel(r) + 0.7152 * channel(g) + 0.0722 * channel(b);
}

function ratio(fg, bg) {
  const [a, b] = [luminance(fg), luminance(bg)].sort((x, y) => y - x);
  return (a + 0.05) / (b + 0.05);
}

// --- Rules that set both colours ---

function sheets(dir) {
  return readdirSync(dir, { withFileTypes: true }).flatMap(e => {
    const path = join(dir, e.name);
    if (e.isDirectory()) return sheets(path);
    return e.name.endsWith('.uss') && path !== LIGHT_SHEET ? [path] : [];
  });
}

const ruleChecks = sheets(join(PACKAGE, 'Editor')).flatMap(file => rules(file)
  .filter(r => r.declarations['color'] && r.declarations['background-color'])
  .filter(r => isOpaque(r.declarations['background-color']))
  .map(r => ({
    where: `${relative(UI, file)} ${r.selector}`,
    fg: r.declarations['color'],
    bg: [...CARD, r.declarations['background-color']],
    min: TEXT,
    in: WINDOW,
  })));

// A transparent background only clears Unity's default fill; an unresolved one is reported by the check itself.
function isOpaque(value) {
  try {
    return resolve(value, CONTEXTS.dark).a > 0;
  } catch {
    return true;
  }
}

// --- Report ---

const showAll = process.argv.includes('--all');
const checks = [...PAIRS, ...ruleChecks].flatMap(pair => pair.in.map(name => ({ ...pair, name })));
let failures = 0;
let known = 0;

for (const check of checks) {
  const context = CONTEXTS[check.name];
  const fg = typeof check.fg === 'object' ? check.fg[check.name] : check.fg;
  const baseline = check.baseline?.[check.name];
  let value, error;
  try {
    const bg = layers(check.bg, context);
    value = ratio(over(resolve(fg, context), bg), bg);
  } catch (e) {
    error = e.message;
  }

  // Ratios compare at the two decimals they are printed with, so a baseline is the printed value.
  const shown = error ? 0 : Math.floor(value * 100) / 100;
  let status = 'ok';
  if (error || shown < check.min) status = baseline !== undefined && shown >= baseline ? 'base' : 'FAIL';
  else if (baseline !== undefined) status = 'ok, drop the baseline';

  if (status === 'FAIL') failures++;
  if (status === 'base') known++;
  if (status === 'ok' && !showAll) continue;
  console.log(`${status.padEnd(4)} ${error ? 'error' : shown.toFixed(2).padStart(5)} / ${check.min}  ` +
    `${check.name.padEnd(15)} ${check.where}: ${fg} on ${[].concat(check.bg).join(' + ')}` +
    (error ? ` (${error})` : '') + (status === 'base' ? ` (baseline ${baseline})` : ''));
}

console.log(`${checks.length - failures - known}/${checks.length} pairs pass, ${known} at their baseline`);
if (failures) {
  console.log(`::error::${failures} colour pairs are below the WCAG contrast minimum`);
  process.exit(1);
}
