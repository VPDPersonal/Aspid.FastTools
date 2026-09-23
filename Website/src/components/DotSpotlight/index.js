import {useEffect} from 'react';
import {isCanvas} from '../DotRipple';

/** Lights the dot texture around the pointer while it moves over the empty canvas of a docs page. */
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
      spot.toggleAttribute('data-on', x >= 0 && isCanvas(document.elementFromPoint(x, y)));
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
