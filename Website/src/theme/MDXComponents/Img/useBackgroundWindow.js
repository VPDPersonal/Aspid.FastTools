import {useEffect} from 'react';

// Mask only the article's painted surface, leaving its content and the shared page canvas intact.
export default function useBackgroundWindow(ref, enabled) {
  useEffect(() => {
    if (!enabled || !ref.current) return undefined;
    const panel = ref.current;
    const column = panel.closest('[class*="docMainContainer_"] > .container > .row > .col:first-child');
    if (!column) return undefined;
    let frame;
    const clear = () => {
      column.classList.remove('doc-column-with-windows');
      column.style.removeProperty('--doc-surface-mask');
      document.documentElement.classList.remove('docs-background-windows');
    };
    const update = () => {
      if (!matchMedia('(min-width: 997px)').matches) {
        clear();
        return;
      }
      const bounds = column.getBoundingClientRect();
      if (!bounds.width || !bounds.height) return;
      let path = `M0 0H${bounds.width}V${bounds.height}H0Z`;
      for (const window of column.querySelectorAll('.doc-background-window')) {
        const rect = window.getBoundingClientRect();
        const x = rect.left - bounds.left;
        const y = rect.top - bounds.top;
        const right = x + rect.width;
        const bottom = y + rect.height;
        const radius = Math.min(parseFloat(getComputedStyle(window).borderTopLeftRadius) || 0, rect.width / 2, rect.height / 2);
        path += `M${x + radius} ${y}H${right - radius}Q${right} ${y} ${right} ${y + radius}V${bottom - radius}Q${right} ${bottom} ${right - radius} ${bottom}H${x + radius}Q${x} ${bottom} ${x} ${bottom - radius}V${y + radius}Q${x} ${y} ${x + radius} ${y}Z`;
      }
      const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${bounds.width} ${bounds.height}"><path fill="white" fill-rule="evenodd" d="${path}"/></svg>`;
      column.style.setProperty('--doc-surface-mask', `url("data:image/svg+xml,${encodeURIComponent(svg)}")`);
      column.classList.add('doc-column-with-windows');
      document.documentElement.classList.add('docs-background-windows');
    };
    const schedule = () => {
      cancelAnimationFrame(frame);
      frame = requestAnimationFrame(update);
    };
    const observer = new ResizeObserver(schedule);
    observer.observe(column);
    observer.observe(panel);
    // Expanding a disclosure above the image can move it without resizing the column.
    column.addEventListener('toggle', schedule, true);
    addEventListener('resize', schedule);
    update();
    return () => {
      cancelAnimationFrame(frame);
      observer.disconnect();
      column.removeEventListener('toggle', schedule, true);
      removeEventListener('resize', schedule);
      clear();
    };
  }, [enabled, ref]);
}
