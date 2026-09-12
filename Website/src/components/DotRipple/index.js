import {useEffect} from 'react';

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
const MAX_WAVES = 4;
// Trailing crests behind the main one: [delay in px behind the front, relative amplitude].
const CRESTS = [[0, 1], [2.4 * WIDTH, 0.45], [4.6 * WIDTH, 0.18]];

const gauss = (u) => Math.exp(-u * u);
const radiusAt = (elapsed) => elapsed * (SPEED + ACCEL * elapsed);

function readColors() {
  const style = getComputedStyle(document.documentElement);
  const hex = style.getPropertyValue('--venom-accent').trim();
  const value = parseInt(hex.slice(1), 16);
  return {
    accent: [(value >> 16) & 255, (value >> 8) & 255, value & 255],
    canvas: style.getPropertyValue('--venom-canvas').trim() || '#000',
  };
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

/** Sends a wave through the dot background when the user clicks the empty canvas of a docs page. */
export default function DotRipple() {
  useEffect(() => {
    if (matchMedia('(prefers-reduced-motion: reduce)').matches) return undefined;

    const canvas = document.createElement('canvas');
    canvas.className = 'dot-ripple-canvas';
    canvas.setAttribute('aria-hidden', 'true');
    document.body.appendChild(canvas);
    const ctx = canvas.getContext('2d');

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

      for (const wave of waves) {
        const r = radiusAt(now - wave.start);
        const progress = r / farthest;
        const sway = 1 + SWAY * (1 - progress) * Math.sin(progress * SWAY_CYCLES * 2 * Math.PI);
        // Only the band of the grid the wave currently touches is visited.
        const outer = r + WIDTH * 3;
        const inner = Math.max(r - CRESTS[CRESTS.length - 1][0] - WIDTH * 3, 0);
        const x0 = Math.max(Math.floor((wave.x - outer) / GRID), 0);
        const x1 = Math.min(Math.ceil((wave.x + outer) / GRID), Math.ceil(innerWidth / GRID));
        const y0 = Math.max(Math.floor((wave.y - outer) / GRID), 0);
        const y1 = Math.min(Math.ceil((wave.y + outer) / GRID), Math.ceil(innerHeight / GRID));

        for (let gy = y0; gy <= y1; gy++) {
          const cy = gy * GRID + GRID / 2;
          for (let gx = x0; gx <= x1; gx++) {
            const cx = gx * GRID + GRID / 2;
            const dx = cx - wave.x;
            const dy = cy - wave.y;
            const d = Math.hypot(dx, dy);
            if (d < inner || d > outer) continue;
            const energy = sway / (1 + d / FALLOFF);
            const h = height(d, r) * energy;
            if (Math.abs(h) < 0.03) continue;

            const nx = d > 0 ? dx / d : 0;
            const ny = d > 0 ? dy / d : 0;
            const px = cx + nx * PUSH * h;
            const py = cy + ny * PUSH * h;

            if (h > 0) {
              // Crest: the dot rises — bigger, brighter, pushed outwards. The resting dot is hidden underneath it.
              const [cr, cg, cb] = colors.accent;
              ctx.fillStyle = `rgba(${cr}, ${cg}, ${cb}, ${Math.min(0.15 + h * 0.75, 0.9)})`;
              ctx.beginPath();
              ctx.arc(px, py, BASE_DOT + LIFT * h, 0, Math.PI * 2);
              ctx.fill();
            } else {
              // Trough: the dot sinks — the resting dot is covered with the canvas colour and a fainter one drawn.
              ctx.fillStyle = colors.canvas;
              ctx.beginPath();
              ctx.arc(cx, cy, BASE_DOT + 0.6, 0, Math.PI * 2);
              ctx.fill();
              ctx.fillStyle = `rgba(120, 128, 140, ${Math.max(0.12 + h * 0.12, 0.02)})`;
              ctx.beginPath();
              ctx.arc(px, py, Math.max(BASE_DOT + h * 0.6, 0.3), 0, Math.PI * 2);
              ctx.fill();
            }
          }
        }
      }

      frame = waves.length ? requestAnimationFrame(render) : 0;
    };

    const onPointerDown = (event) => {
      if (event.button !== 0) return;
      if (!document.documentElement.classList.contains('docs-doc-page')) return;
      if (!(event.target instanceof Element) || event.target.closest(CONTENT)) return;
      colors = readColors();
      waves.push({x: event.clientX, y: event.clientY, start: performance.now()});
      if (waves.length > MAX_WAVES) waves.shift();
      if (!frame) frame = requestAnimationFrame(render);
    };

    document.addEventListener('pointerdown', onPointerDown);
    addEventListener('resize', resize);
    return () => {
      document.removeEventListener('pointerdown', onPointerDown);
      removeEventListener('resize', resize);
      if (frame) cancelAnimationFrame(frame);
      canvas.remove();
    };
  }, []);
  return null;
}
