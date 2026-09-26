import {useEffect} from 'react';
import {BACKGROUND_WINDOWS, TINTED_WINDOWS} from '../BackgroundWindows';
import {addWave, rowWaves} from './waves';

// Anything that reads as "content" rather than canvas. Only the filled parts of the navigation panel and the TOC count,
// so the empty space under a short menu still behaves like background.
const CONTENT = [
  '[class*="docMainContainer_"] > .container > .row > .col:first-child',
  '.theme-doc-sidebar-container [class*="header_"]',
  '.theme-doc-sidebar-container [class*="footer_"]',
  '.menu__list-item',
  '.table-of-contents li',
  '.navbar',
  'a', 'button', 'input', 'textarea', 'select', 'label',
  'dialog', '[role="dialog"]', '[role="menu"]',
].join(', ');

/** True when `target` is the empty canvas of a docs page: the dots show there, and a click or the pointer may light them. */
export function isCanvas(target) {
  if (!document.documentElement.classList.contains('docs-doc-page')) return false;
  if (!(target instanceof Element)) return false;
  // Only windows the article surface is actually cut out for: below 997px the mask is dropped, so notices and image
  // panels are opaque content again and the dots under them are hidden.
  if (target.closest(`.doc-column-with-windows :is(${BACKGROUND_WINDOWS})`)) return !target.closest('a, button, [role="button"]');
  return !target.closest(CONTENT);
}

const GRID = 20;          // px, must match the CSS dot texture (background-size)
const BASE_DOT = 1;       // px, radius of the resting CSS dot
const SPEED = 0.42;       // px per ms at the start — the same pace on every screen size
const ACCEL = 0.00014;    // px per ms², the wave picks up speed as it runs out, so it leaves the screen sooner
const WIDTH = 46;         // px, half-width of a crest
const LIFT = 2.2;         // px, how much a dot grows at a full-strength crest
const PUSH = 3.5;         // px, how far a dot is pushed outwards at a full-strength crest
const FALLOFF = 700;      // px, distance at which a wave has lost half its energy
const SWAY_CYCLES = 3;    // the crest swells and sinks this many times as it travels
const SWAY = 0.4;         // relative amplitude of that swaying; it damps out with the wave
// Overlapping waves add up; the sum is clamped to what a single wave can reach, so a burst of clicks does not blow the dots up.
const MIN_HEIGHT = -1;
const MAX_HEIGHT = 1 + SWAY;
// Trailing crests behind the main one: [delay in px behind the front, relative amplitude].
const CRESTS = [[0, 1], [2.4 * WIDTH, 0.45], [4.6 * WIDTH, 0.18]];

const COUNTER_KEY = 'aspid-dot-ripple-clicks';
const COUNTER_IDLE = 2500; // ms after the last click before the counter fades out

const gauss = (u) => Math.exp(-u * u);
const radiusAt = (elapsed) => elapsed * (SPEED + ACCEL * elapsed);

function readColors() {
  const style = getComputedStyle(document.documentElement);
  const hex = (style.getPropertyValue('--venom-ripple').trim() || style.getPropertyValue('--venom-accent').trim());
  const value = parseInt(hex.slice(1), 16);
  return {
    accent: [(value >> 16) & 255, (value >> 8) & 255, value & 255],
    canvas: style.getPropertyValue('--venom-canvas').trim() || '#000',
  };
}

// Read once per frame, outside the dot loop: scrolling and resizing can move a window during a wave.
function readTintedWindows() {
  return [...document.querySelectorAll(`.doc-column-with-windows :is(${TINTED_WINDOWS})`)]
    .filter((element) => !element.closest('details:not([open])'))
    .map((element) => ({rect: element.getBoundingClientRect(), color: getComputedStyle(element).borderTopColor}))
    .filter(({rect}) => rect.bottom > 0 && rect.top < innerHeight && rect.right > 0 && rect.left < innerWidth);
}

// Signed height of the water at distance `d` from the origin of a wave whose front is at radius `r`.
// A crest is followed by a trough, and every following crest is weaker. Range roughly [-1, 1].
function height(d, r) {
  let h = 0;
  for (const [lag, amplitude] of CRESTS) {
    const crest = r - lag;
    if (crest < -WIDTH * 3) continue;
    const u = (d - crest) / WIDTH;
    h += amplitude * (gauss(u) - 0.55 * gauss(u + 1.5));
  }
  return h;
}

function readClicks() {
  try {
    return Number(localStorage.getItem(COUNTER_KEY)) || 0;
  } catch {
    return 0;
  }
}

function saveClicks(clicks) {
  try {
    localStorage.setItem(COUNTER_KEY, String(clicks));
  } catch {
    // Storage may be blocked; the counter then only lasts for this page.
  }
}

/** Sends a wave through the dot background when the user clicks the empty canvas of a docs page, and counts the clicks. */
export default function DotRipple() {
  useEffect(() => {
    if (matchMedia('(prefers-reduced-motion: reduce)').matches) return undefined;

    const canvas = document.createElement('canvas');
    canvas.className = 'dot-ripple-canvas';
    canvas.setAttribute('aria-hidden', 'true');
    document.body.appendChild(canvas);
    const ctx = canvas.getContext('2d');

    const counter = document.createElement('div');
    counter.className = 'dot-ripple-counter';
    counter.setAttribute('aria-hidden', 'true');
    document.body.appendChild(counter);
    let clicks = readClicks();
    let counterTimer = 0;

    let waves = [];
    let frame = 0;
    let colors = readColors();

    const resize = () => {
      const dpr = Math.min(devicePixelRatio || 1, 2);
      canvas.width = Math.ceil(innerWidth * dpr);
      canvas.height = Math.ceil(innerHeight * dpr);
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    };
    resize();

    const render = (now) => {
      ctx.clearRect(0, 0, innerWidth, innerHeight);
      const farthest = Math.hypot(innerWidth, innerHeight) + CRESTS[CRESTS.length - 1][0] + WIDTH * 3;
      waves = waves.filter((wave) => radiusAt(now - wave.start) < farthest);
      const tintedWindows = waves.length ? readTintedWindows() : [];

      // Per-frame state of every live wave: its front, how much it currently sways, and the ring of the grid it touches.
      const live = waves.map((wave) => {
        const r = radiusAt(now - wave.start);
        const progress = r / farthest;
        return {
          x: wave.x,
          y: wave.y,
          r,
          sway: 1 + SWAY * (1 - progress) * Math.sin(progress * SWAY_CYCLES * 2 * Math.PI),
          outer: r + WIDTH * 3,
          inner: Math.max(r - CRESTS[CRESTS.length - 1][0] - WIDTH * 3, 0),
        };
      });

      // Only the part of the grid some wave currently touches is visited.
      let x0 = Infinity, x1 = -Infinity, y0 = Infinity, y1 = -Infinity;
      for (const wave of live) {
        x0 = Math.min(x0, Math.floor((wave.x - wave.outer) / GRID));
        x1 = Math.max(x1, Math.ceil((wave.x + wave.outer) / GRID));
        y0 = Math.min(y0, Math.floor((wave.y - wave.outer) / GRID));
        y1 = Math.max(y1, Math.ceil((wave.y + wave.outer) / GRID));
      }
      x0 = Math.max(x0, 0);
      x1 = Math.min(x1, Math.ceil(innerWidth / GRID));
      y0 = Math.max(y0, 0);
      y1 = Math.min(y1, Math.ceil(innerHeight / GRID));

      for (let gy = y0; gy <= y1; gy++) {
        const cy = gy * GRID + GRID / 2;
        const row = rowWaves(live, cy);
        if (!row.length) continue;
        for (let gx = x0; gx <= x1; gx++) {
          const cx = gx * GRID + GRID / 2;
          // Every wave lifts the dot and pushes it away from its own origin; the dot is drawn once with the sum.
          let h = 0;
          let pushX = 0;
          let pushY = 0;
          for (const {wave, dy, near, far} of row) {
            const dx = cx - wave.x;
            const adx = Math.abs(dx);
            if (adx > far || adx < near) continue;
            const d = Math.hypot(dx, dy);
            if (d < wave.inner || d > wave.outer) continue;
            const waveHeight = height(d, wave.r) * wave.sway / (1 + d / FALLOFF);
            h += waveHeight;
            if (d > 0) {
              pushX += dx / d * waveHeight;
              pushY += dy / d * waveHeight;
            }
          }
          if (Math.abs(h) < 0.03) continue;

          const scale = Math.min(Math.max(h, MIN_HEIGHT), MAX_HEIGHT) / h;
          h *= scale;
          const px = cx + PUSH * pushX * scale;
          const py = cy + PUSH * pushY * scale;
          const tint = tintedWindows.find(({rect}) => px >= rect.left && px <= rect.right && py >= rect.top && py <= rect.bottom);

          if (h > 0) {
            // Crest: the dot rises — bigger, brighter, pushed outwards. The resting dot is hidden underneath it.
            const [cr, cg, cb] = colors.accent;
            ctx.fillStyle = tint?.color ?? `rgb(${cr}, ${cg}, ${cb})`;
            ctx.globalAlpha = Math.min(0.15 + h * 0.75, 0.9);
            ctx.beginPath();
            ctx.arc(px, py, BASE_DOT + LIFT * h, 0, Math.PI * 2);
            ctx.fill();
            ctx.globalAlpha = 1;
          } else {
            // Trough: the dot sinks — the resting dot is covered with the canvas colour and a fainter one drawn.
            ctx.fillStyle = colors.canvas;
            ctx.beginPath();
            ctx.arc(cx, cy, BASE_DOT + 0.6, 0, Math.PI * 2);
            ctx.fill();
            ctx.fillStyle = tint?.color ?? 'rgb(120, 128, 140)';
            ctx.globalAlpha = Math.max(0.12 + h * 0.12, 0.02);
            ctx.beginPath();
            ctx.arc(px, py, Math.max(BASE_DOT + h * 0.6, 0.3), 0, Math.PI * 2);
            ctx.fill();
            ctx.globalAlpha = 1;
          }
        }
      }

      frame = waves.length ? requestAnimationFrame(render) : 0;
    };

    const onPointerDown = (event) => {
      if (event.button !== 0 || !isCanvas(event.target)) return;
      colors = readColors();
      addWave(waves, {x: event.clientX, y: event.clientY, start: performance.now()});

      clicks += 1;
      saveClicks(clicks);
      counter.textContent = clicks.toLocaleString();
      counter.toggleAttribute('data-on', true);
      counter.animate([{transform: 'scale(1.25)'}, {transform: 'scale(1)'}], {duration: 250, easing: 'ease-out'});
      clearTimeout(counterTimer);
      counterTimer = setTimeout(() => counter.removeAttribute('data-on'), COUNTER_IDLE);
      if (!frame) frame = requestAnimationFrame(render);
    };

    document.addEventListener('pointerdown', onPointerDown);
    addEventListener('resize', resize);
    return () => {
      document.removeEventListener('pointerdown', onPointerDown);
      removeEventListener('resize', resize);
      if (frame) cancelAnimationFrame(frame);
      clearTimeout(counterTimer);
      canvas.remove();
      counter.remove();
    };
  }, []);
  return null;
}
