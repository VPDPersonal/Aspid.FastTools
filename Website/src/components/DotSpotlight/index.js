import {useEffect} from 'react';
import {BACKGROUND_WINDOWS} from '../BackgroundWindows';
import {isCanvas} from '../DotRipple';

const RADIUS = 130; // px, the visible part of the 150px spotlight mask in custom.css
const ARTICLE = '[class*="docMainContainer_"] > .container > .row > .col:first-child';

const inViewport = (x, y) => x >= 0 && y >= 0 && x < innerWidth && y < innerHeight;
const distanceToRect = (x, y, rect) =>
  Math.hypot(Math.max(rect.left - x, 0, x - rect.right), Math.max(rect.top - y, 0, y - rect.bottom));

// True when any part of the spotlight around (x, y) falls on the empty canvas. The opaque article surface lies above the
// spotlight, so over the article it is lit near the surface's edge or one of its windows, and only the part past them is seen.
function nearCanvas(x, y) {
  const target = document.elementFromPoint(x, y);
  if (isCanvas(target)) return true;
  const article = target?.closest(ARTICLE);
  if (!article) return false;

  const rect = article.getBoundingClientRect();
  const beyondEdges = [
    [rect.left - 1, y, x - rect.left],
    [rect.right + 1, y, rect.right - x],
    [x, rect.top - 1, y - rect.top],
    [x, rect.bottom + 1, rect.bottom - y],
  ];
  if (beyondEdges.some(([px, py, distance]) => distance < RADIUS && inViewport(px, py) && isCanvas(document.elementFromPoint(px, py)))) {
    return true;
  }
  if (!article.matches('.doc-column-with-windows')) return false;
  return [...article.querySelectorAll(`:is(${BACKGROUND_WINDOWS})`)]
    .some((element) => !element.closest('details:not([open])') && distanceToRect(x, y, element.getBoundingClientRect()) < RADIUS);
}

/** Lights the dot texture around the pointer while it is over or close to the empty canvas of a docs page. */
export default function DotSpotlight() {
  useEffect(() => {
    if (!matchMedia('(hover: hover) and (pointer: fine)').matches) return undefined;
    if (matchMedia('(prefers-reduced-motion: reduce)').matches) return undefined;

    const spot = document.createElement('div');
    spot.className = 'dot-spotlight';
    spot.setAttribute('aria-hidden', 'true');
    document.body.appendChild(spot);

    let x = -1;
    let y = -1;
    let frame = 0;
    // Scrolling moves the page under a resting pointer, so the element under it is looked up again.
    const update = () => {
      frame = 0;
      spot.style.setProperty('--spot-x', `${x}px`);
      spot.style.setProperty('--spot-y', `${y}px`);
      spot.toggleAttribute('data-on', x >= 0 && nearCanvas(x, y));
    };
    const schedule = () => { if (!frame) frame = requestAnimationFrame(update); };
    const onMove = (event) => {
      if (event.pointerType !== 'mouse') return;
      x = event.clientX;
      y = event.clientY;
      schedule();
    };
    const onLeave = () => {
      x = -1;
      schedule();
    };

    document.addEventListener('pointermove', onMove, {passive: true});
    document.documentElement.addEventListener('pointerleave', onLeave);
    addEventListener('scroll', schedule, {passive: true});
    return () => {
      document.removeEventListener('pointermove', onMove);
      document.documentElement.removeEventListener('pointerleave', onLeave);
      removeEventListener('scroll', schedule);
      if (frame) cancelAnimationFrame(frame);
      spot.remove();
    };
  }, []);
  return null;
}
