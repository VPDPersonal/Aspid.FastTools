import {useEffect} from 'react';

export const TINTED_WINDOWS = '.theme-admonition-info, .theme-admonition-note, .theme-admonition-warning';
export const BACKGROUND_WINDOWS = `.doc-background-window, ${TINTED_WINDOWS}`;

// Mask only the article's painted surface, leaving its content and the shared page canvas intact.
export default function useBackgroundWindows(ref, documentKey) {
  useEffect(() => {
    const column = ref.current;
    if (!column) return undefined;
    const observedWindows = new Set();
    let frame;
    const clear = () => {
      column.classList.remove('doc-column-with-windows');
      column.style.removeProperty('--doc-surface-mask');
      document.documentElement.classList.remove('docs-background-windows');
    };
    const update = () => {
      const windows = [...column.querySelectorAll(BACKGROUND_WINDOWS)]
        .filter((window) => !window.closest('details:not([open])'));
      for (const window of observedWindows) {
        if (!windows.includes(window)) {
          observer.unobserve(window);
          observedWindows.delete(window);
        }
      }
      for (const window of windows) {
        if (!observedWindows.has(window)) {
          observer.observe(window);
          observedWindows.add(window);
        }
      }
      if (!windows.length || !matchMedia('(min-width: 997px)').matches) {
        clear();
        return;
      }
      const bounds = column.getBoundingClientRect();
      if (!bounds.width || !bounds.height) return;
      let path = `M0 0H${bounds.width}V${bounds.height}H0Z`;
      for (const window of windows) {
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
    // Content mounted after hydration and disclosures can add or move windows.
    const mutations = new MutationObserver(schedule);
    mutations.observe(column, {childList: true, subtree: true});
    // Expanding a disclosure above a window can move it without resizing the column.
    column.addEventListener('toggle', schedule, true);
    addEventListener('resize', schedule);
    update();
    return () => {
      cancelAnimationFrame(frame);
      observer.disconnect();
      mutations.disconnect();
      column.removeEventListener('toggle', schedule, true);
      removeEventListener('resize', schedule);
      clear();
    };
  }, [documentKey, ref]);
}
