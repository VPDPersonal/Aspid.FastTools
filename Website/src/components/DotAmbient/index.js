import {useEffect} from 'react';

const GRID = 20;          // px, must match the CSS dot texture (background-size)
const MIN_WIDTH = 997;    // px, below it the article covers the canvas, so nothing is drawn
const FRAME = 50;         // ms between redraws: the glow changes slowly, 20 fps is enough
const DEPTH = 0.2;        // share of the viewport height the glow reaches at its highest, next to the article
const SIDE = 0.3;         // share of that height left at the outer edges of the screen: the glow slopes down to them
const ARTICLE = '[class*="docMainContainer_"] > .container > .row > .col:first-child';
// Waves that roughen the top edge of the glow: [wavelength px, drift px per ms, amplitude]. Different lengths and speeds,
// some drifting left and some right, so the edge keeps changing without ever moving as a whole.
const EDGE = [[1100, 0.006, 0.5], [530, -0.011, 0.3], [260, 0.017, 0.2]];

// A stable pseudo-random phase per dot, so the dots in the glow twinkle out of step.
const hash = (x, y) => {
  const h = Math.sin(x * 127.1 + y * 311.7) * 43758.5453;
  return h - Math.floor(h);
};

// Sum of drifting sines at `x`, mapped to [0, 1].
const wave = (layers, x, now) => layers.reduce((sum, [length, drift, amplitude]) =>
  sum + amplitude * (0.5 + 0.5 * Math.sin((x + now * drift) / length * 2 * Math.PI)), 0);

function readColors() {
  const style = getComputedStyle(document.documentElement);
  return {
    accent: style.getPropertyValue('--venom-ripple').trim() || style.getPropertyValue('--venom-accent').trim(),
    dark: document.documentElement.dataset.theme !== 'light',
  };
}

/** Keeps an uneven glow along the bottom of a docs page's dot background: highest next to the article, sloping down to
 *  the edges of the screen, its top edge slowly changing shape. */
export default function DotAmbient() {
  useEffect(() => {
    if (matchMedia('(prefers-reduced-motion: reduce)').matches) return undefined;

    const canvas = document.createElement('canvas');
    canvas.className = 'dot-ambient-canvas';
    canvas.setAttribute('aria-hidden', 'true');
    document.body.appendChild(canvas);
    const ctx = canvas.getContext('2d');

    const resize = () => {
      const dpr = Math.min(devicePixelRatio || 1, 2);
      canvas.width = Math.ceil(innerWidth * dpr);
      canvas.height = Math.ceil(innerHeight * dpr);
      ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    };
    resize();

    let frame = 0;
    let last = 0;
    let colors = null;
    let colorsAt = 0;
    let article = null;

    const render = (now) => {
      frame = requestAnimationFrame(render);
      if (now - last < FRAME) return;
      last = now;

      const width = innerWidth;
      const height = innerHeight;
      ctx.clearRect(0, 0, width, height);
      if (width < MIN_WIDTH || !document.documentElement.classList.contains('docs-doc-page')) return;
      // The theme or accent may change while the page is open.
      if (!colors || now - colorsAt > 1000) {
        colors = readColors();
        article = document.querySelector(ARTICLE)?.getBoundingClientRect() ?? null;
        colorsAt = now;
      }
      // Without an article the slope runs to the middle of the screen.
      const left = article ? article.left : width / 2;
      const right = article ? article.right : width / 2;

      const maxReach = height * DEPTH;
      const rows = Math.ceil(maxReach * 1.6 / GRID);
      const lastRow = Math.floor(height / GRID);
      const breathe = 0.85 + 0.15 * Math.sin(now * 0.0004);
      ctx.fillStyle = colors.accent;
      for (let gx = 0; gx <= Math.ceil(width / GRID); gx++) {
        const cx = gx * GRID + GRID / 2;
        // 0 at the outer edge of the screen, 1 at the article and behind it.
        const slope = cx < left ? cx / left : cx > right ? (width - cx) / (width - right) : 1;
        const reach = maxReach * breathe * (SIDE + (1 - SIDE) * Math.min(Math.max(slope, 0), 1)) * (0.5 + 0.5 * wave(EDGE, cx, now));
        for (let gy = lastRow; gy >= lastRow - rows; gy--) {
          const cy = gy * GRID + GRID / 2;
          const u = (height - cy) / reach;
          if (u > 1.6) break;
          const i = Math.exp(-u * u * 1.8) * (0.6 + 0.4 * Math.sin(now * 0.0015 + hash(gx, gy) * 2 * Math.PI));
          if (i < 0.05) continue;
          ctx.globalAlpha = colors.dark ? Math.min(0.06 + i * 0.6, 0.7) : Math.min(0.05 + i * 0.42, 0.5);
          ctx.beginPath();
          ctx.arc(cx, cy, 1 + 0.8 * i, 0, Math.PI * 2);
          ctx.fill();
        }
      }
      ctx.globalAlpha = 1;
    };
    frame = requestAnimationFrame(render);

    // A hidden tab stops drawing entirely.
    const onVisibility = () => {
      if (document.hidden) {
        cancelAnimationFrame(frame);
        frame = 0;
      } else if (!frame) {
        frame = requestAnimationFrame(render);
      }
    };

    addEventListener('resize', resize);
    document.addEventListener('visibilitychange', onVisibility);
    return () => {
      removeEventListener('resize', resize);
      document.removeEventListener('visibilitychange', onVisibility);
      cancelAnimationFrame(frame);
      canvas.remove();
    };
  }, []);
  return null;
}
