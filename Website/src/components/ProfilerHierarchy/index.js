import React, {useEffect, useRef, useState} from 'react';
import clsx from 'clsx';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import {prefersReducedMotion, useInView} from '@site/src/components/FeaturePreview/effects';
import styles from './styles.module.css';

/*
 * The FlockSimulation markers from the WithName() example on the ProfilerMarkers page, as CPU Usage → Hierarchy
 * shows them. The tree is built in the order the `using` scopes open, Steering.Agent counts its 120 loop
 * iterations into one row, then new frames arrive with their own timings. Markdown keeps the static SVG
 * (`profiler-markers-hierarchy.svg`); `src/remark/liveDiagrams.js` swaps it for this component on the site.
 * Once built, the tree behaves like the real one: a click selects a row, the caret folds a branch, and the arrow keys,
 * Enter and Space follow the WAI-ARIA tree pattern.
 */

// Milliseconds from the start of the build: when each row appears, and when a parent unfolds its children.
const AT = {step: 0, stepOpen: 380, steer: 640, steerOpen: 980, agent: 1240, countFrom: 1380, countTo: 2700, integrate: 2980};
const LIVE_AT = 3900;
const FRAME_MS = 1100;
const FIRST_FRAME = 1284;
const AGENTS = 120;

// Per frame: Steering.Agent and Integrate in ms. Steering adds its own 0.01 ms, Step sums its children.
const FRAMES = [[0.36, 0.01], [0.41, 0.01], [0.34, 0.02], [0.39, 0.01], [0.37, 0.01], [0.44, 0.02], [0.35, 0.01]];

const easeOut = (value) => 1 - (1 - value) ** 3;
const clamp01 = (value) => Math.min(1, Math.max(0, value));

/** Milliseconds of animation played so far; it only advances while `active` and restarts with `run`. */
function useElapsed(active, run) {
  const [elapsed, setElapsed] = useState(0);
  const played = useRef(0);
  useEffect(() => {
    played.current = 0;
    setElapsed(0);
  }, [run]);
  useEffect(() => {
    if (prefersReducedMotion()) {
      setElapsed(LIVE_AT);
      return undefined;
    }
    if (!active) return undefined;
    let last = performance.now();
    let frame = requestAnimationFrame(function tick(now) {
      played.current += Math.min(now - last, 100);
      last = now;
      // Quantised, so React re-renders about 30 times a second rather than on every display frame.
      setElapsed(Math.floor(played.current / 33) * 33);
      frame = requestAnimationFrame(tick);
    });
    return () => cancelAnimationFrame(frame);
  }, [active, run]);
  return elapsed;
}

/** The whole view at `t` ms: which rows are visible and open, the counted calls and every row's time. */
function snapshot(t) {
  const live = Math.max(0, Math.floor((t - LIVE_AT) / FRAME_MS));
  const [agentMs, integrateMs] = FRAMES[live % FRAMES.length];
  const counted = t >= AT.countTo ? AGENTS : Math.round(AGENTS * easeOut(clamp01((t - AT.countFrom) / (AT.countTo - AT.countFrom))));
  const agent = t >= LIVE_AT ? agentMs : (agentMs * counted) / AGENTS;
  const steering = agent + 0.01;
  const integrate = t >= AT.integrate ? integrateMs : 0;
  return {
    live,
    frame: FIRST_FRAME + live,
    rows: [
      {id: 'step', shown: t >= AT.step, open: t >= AT.stepOpen, calls: 1, ms: steering + integrate},
      {id: 'steer', shown: t >= AT.steer, open: t >= AT.steerOpen, calls: 1, ms: steering},
      {id: 'agent', shown: t >= AT.agent, calls: counted, ms: agent},
      {id: 'integrate', shown: t >= AT.integrate, calls: 1, ms: integrate},
    ],
  };
}

// Marker name, source line, depth, swatch, and the tree guides drawn at each ancestor level. The swatches are the bar
// colours of the ProfilerMarkers timeline on the introduction (`FeaturePreview`), so the two previews read as one.
// A guide is `[kind, first]`: `first` lifts it up to the parent's caret.
const ROWS = {
  step: {name: 'FlockSimulation.Step', line: 3, depth: 0, parent: true, swatch: 'step', guides: []},
  steer: {name: 'FlockSimulation.Steering', line: 5, depth: 1, parent: true, swatch: 'steer', guides: [['tee', true]]},
  agent: {name: 'FlockSimulation.Steering.Agent', line: 9, depth: 2, swatch: 'steer', guides: [['pass'], ['elbow', true]]},
  integrate: {name: 'FlockSimulation.Integrate', line: 14, depth: 1, swatch: 'integrate', guides: [['elbow']]},
};
const ORDER = ['step', 'steer', 'agent', 'integrate'];
const PARENT = {steer: 'step', agent: 'steer', integrate: 'step'};

/** The row's ancestors, nearest first. */
function ancestors(id) {
  const chain = [];
  for (let parent = PARENT[id]; parent; parent = PARENT[parent]) chain.push(parent);
  return chain;
}

/** A number that flashes when a new frame changes it. */
function Cell({value, live, className}) {
  return <span className={clsx(styles.num, className)} key={live}>{value}</span>;
}

export default function ProfilerHierarchy({alt}) {
  const {i18n} = useDocusaurusContext();
  const ru = i18n.currentLocale === 'ru';
  const ref = useRef(null);
  const rowRefs = useRef({});
  const [run, setRun] = useState(0);
  const [picked, setPicked] = useState(null);
  const [folded, setFolded] = useState(() => new Set());
  const inView = useInView(ref, {rootMargin: '-15% 0px'});
  const view = snapshot(useElapsed(inView, run));

  // Nothing is selected until the reader picks a row: the page points at GC Alloc, not at one marker.
  const selected = picked;
  const rows = view.rows.map((row) => ({
    ...row,
    open: row.open && !folded.has(row.id),
    visible: row.shown && !ancestors(row.id).some((id) => folded.has(id)),
  }));
  const visible = rows.filter((row) => row.visible).map((row) => row.id);

  function replay() {
    setRun((value) => value + 1);
    setPicked(null);
    setFolded(new Set());
  }

  function select(id, focus = false) {
    setPicked(id);
    if (focus) rowRefs.current[id]?.focus();
  }

  function toggle(id, open = folded.has(id)) {
    const next = new Set(folded);
    if (open) next.delete(id);
    else next.add(id);
    setFolded(next);
    // Folding a branch moves the selection out of it, as the Profiler does.
    if (!open && selected && ancestors(selected).includes(id)) select(id, true);
  }

  function onKeyDown(event) {
    const id = selected && visible.includes(selected) ? selected : visible[0];
    if (!id) return;
    const index = visible.indexOf(id);
    const {parent} = ROWS[id];
    const open = rows.find((row) => row.id === id).open;
    const actions = {
      ArrowDown: () => select(visible[Math.min(index + 1, visible.length - 1)], true),
      ArrowUp: () => select(visible[Math.max(index - 1, 0)], true),
      Home: () => select(visible[0], true),
      End: () => select(visible[visible.length - 1], true),
      ArrowRight: () => (parent && !open ? toggle(id, true) : parent && select(visible[index + 1], true)),
      ArrowLeft: () => (parent && open ? toggle(id, false) : PARENT[id] && select(PARENT[id], true)),
      Enter: () => parent && toggle(id, !open),
      ' ': () => parent && toggle(id, !open),
    };
    if (!actions[event.key]) return;
    event.preventDefault();
    actions[event.key]();
  }

  return (
    <div ref={ref} className={styles.panel}>
      <div className={styles.window}>
        <div className={styles.titleBar}>
          <span>Hierarchy</span>
          <span className={styles.frame}>Frame <b key={view.frame}>{view.frame}</b></span>
          <button type="button" className={styles.replay} onClick={replay}
            aria-label={ru ? 'Повторить анимацию' : 'Replay the animation'} title={ru ? 'Повторить' : 'Replay'}>
            <svg viewBox="0 0 16 16" aria-hidden="true"><path d="M13.5 8A5.5 5.5 0 1 1 11.9 4.1M12 1.5V4.4H9.1" /></svg>
          </button>
        </div>
        <div className={styles.table} role="tree" aria-label={alt} onKeyDown={onKeyDown}>
          <div className={clsx(styles.row, styles.head)} aria-hidden="true">
            <span>Marker</span><span className={styles.calls}>Calls</span><span className={styles.alloc}>GC Alloc</span><span>Time ms</span>
          </div>
          {rows.map((row) => {
            const {name, line, depth, parent, swatch, guides} = ROWS[row.id];
            const isSelected = row.id === selected;
            // Roving tabindex: the selected row, or the first one before anything is selected, takes the focus.
            const focusable = row.visible && (isSelected || (!visible.includes(selected) && row.id === visible[0]));
            return (
              <div key={row.id} ref={(element) => { rowRefs.current[row.id] = element; }}
                role="treeitem" aria-level={depth + 1} aria-selected={isSelected}
                aria-expanded={parent ? row.open : undefined} aria-hidden={!row.visible || undefined}
                tabIndex={focusable ? 0 : -1}
                className={clsx(styles.row, isSelected && styles.selected)}
                data-shown={row.shown || undefined}
                data-folded={(row.shown && !row.visible) || undefined}
                style={{'--depth': depth}}
                onClick={() => row.visible && select(row.id)}
                onDoubleClick={() => parent && toggle(row.id, !row.open)}>
                <span className={styles.marker}>
                  {guides.map(([kind, first], level) => <span key={level} className={clsx(styles[kind], first && styles.first)} style={{'--level': level}} />)}
                  {parent
                    ? <span className={styles.caret} data-open={row.open || undefined}
                        onClick={(event) => { event.stopPropagation(); toggle(row.id, !row.open); select(row.id); }} />
                    : <span className={styles.leaf} />}
                  <span className={styles.swatch} data-marker={swatch} />
                  <span className={styles.name}>{name}</span> <span className={styles.line}>({line})</span>
                </span>
                <span className={clsx(styles.num, styles.calls)}>{row.calls}</span>
                <span className={clsx(styles.num, styles.alloc)}>0 B</span>
                <Cell value={row.ms.toFixed(2)} live={view.live} className={view.live > 0 && styles.flash} />
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
