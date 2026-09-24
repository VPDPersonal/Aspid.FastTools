import React, {useEffect, useRef, useState} from 'react';
import clsx from 'clsx';
import {Highlight} from 'prism-react-renderer';
import {usePrismTheme} from '@docusaurus/theme-common';
import {prefersReducedMotion, useInView, useLoop} from './effects';
import enumMedia from './media/enum.mp4';
import styles from './styles.module.css';

/** Highlighted C# lines; `active` marks the line the neighbouring preview currently shows. */
function Code({code, active = -1}) {
  const theme = usePrismTheme();
  return (
    <Highlight theme={theme} code={code} language="csharp">
      {({tokens, getTokenProps}) => (
        <pre className={styles.snippet} style={{color: theme.plain.color}}>
          {tokens.map((line, index) => (
            <span key={index} className={styles.snippetLine} data-active={index === active || undefined}>
              {line.map((token, tokenIndex) => {
                const {key, ...props} = getTokenProps({token});
                return <span key={tokenIndex} {...props} />;
              })}
            </span>
          ))}
        </pre>
      )}
    </Highlight>
  );
}

/* ---------- ProfilerMarkers: the generated markers on a scrolling Profiler timeline ---------- */

const PROFILER_CODE = `public void Step()
{
    using var _ = this.Marker();
    using (this.Marker().WithName("Steering"))
        Steer(_agents);
    using (this.Marker().WithName("Integrate"))
        Integrate();
}`;

// Per frame: Steering and Integrate as fractions of Step, so consecutive frames differ like real captures.
const FRAMES = [[0.56, 0.2], [0.59, 0.17], [0.55, 0.21], [0.57, 0.19], [0.6, 0.16]];

function Frame({steering, integrate}) {
  return (
    <div className={styles.frame}>
      <span className={clsx(styles.bar, styles.barStep)} data-marker="step">FlockSimulation.Step (3)</span>
      <div className={styles.frameRow}>
        <span className={clsx(styles.bar, styles.barSteer)} data-marker="steer" style={{flexBasis: `${steering * 100}%`}}>FlockSimulation.Steering (4)</span>
        <span className={clsx(styles.bar, styles.barIntegrate)} data-marker="integrate" style={{flexBasis: `${integrate * 100}%`}}>FlockSimulation.Integrate (6)</span>
      </div>
    </div>
  );
}

// Each marker's line in PROFILER_CODE.
const MARKER_LINES = {step: 2, steer: 3, integrate: 5};

/** The innermost marker under the playhead; Step alone where no child bar covers it. The 4px gap between children
    counts as the bar before it, so Step does not flash between them. */
function useMarkerAtPlayhead(ref, active) {
  const [marker, setMarker] = useState('step');
  useEffect(() => {
    const root = ref.current;
    if (!root) return undefined;
    const playhead = root.querySelector(`.${styles.playhead}`);
    const bars = root.querySelectorAll('[data-marker="steer"], [data-marker="integrate"]');
    const measure = () => {
      const x = playhead.getBoundingClientRect().left;
      const hit = [...bars].find((bar) => {
        const {left, right} = bar.getBoundingClientRect();
        return left <= x && x < right + 4;
      });
      setMarker(hit ? hit.dataset.marker : 'step');
    };
    measure();
    if (!active || prefersReducedMotion()) return undefined;
    let frame = requestAnimationFrame(function tick() {
      measure();
      frame = requestAnimationFrame(tick);
    });
    return () => cancelAnimationFrame(frame);
  }, [ref, active]);
  return marker;
}

function ProfilerPreview() {
  const ref = useRef(null);
  const marker = useMarkerAtPlayhead(ref, useInView(ref));
  const line = MARKER_LINES[marker];
  return (
    <div ref={ref} className={styles.profilerBody}>
      <Code code={PROFILER_CODE} active={line} />
      <div className={styles.timeline} data-focus={marker} aria-label="Unity Profiler, Timeline" role="img">
        <div className={styles.timelineBar}><span>Timeline</span><span>Main Thread</span></div>
        <div className={styles.timelineTrack}>
          <div className={styles.timelineStrip}>
            {[...FRAMES, ...FRAMES].map(([steering, integrate], index) => <Frame key={index} steering={steering} integrate={integrate} />)}
          </div>
          <span className={styles.playhead} aria-hidden="true" />
        </div>
      </div>
    </div>
  );
}

/* ---------- VisualElement Extensions: each chained call builds the preview ---------- */

const UI_CODE = `var panel = new VisualElement()
    .SetPaddingX(12)
    .SetPaddingY(8)
    .AddChild(new Label("Stats")
        .SetFontSize(18));`;

const UI_STEPS = ['VisualElement', 'padding-left / right: 12', 'padding-top / bottom: 8', 'Label “Stats”', 'font-size: 18', 'VisualElement'];

function UiPreview() {
  const ref = useRef(null);
  const step = useLoop(6, 1100, useInView(ref), 4);
  return (
    <div ref={ref} className={styles.uiBody}>
      <Code code={UI_CODE} active={step <= 4 ? step : -1} />
      <div className={styles.uiStage} aria-hidden="true">
        <div className={styles.uiPanel} data-x={step >= 1 || undefined} data-y={step >= 2 || undefined}>
          <span className={styles.uiPadX}>12</span>
          <span className={styles.uiPadY}>8</span>
          <div className={styles.uiContent}>
            <span className={styles.uiLabel} data-show={step >= 3 || undefined} data-big={step >= 4 || undefined}>Stats</span>
          </div>
        </div>
        <span className={styles.uiCaption}>{UI_STEPS[step]}</span>
      </div>
    </div>
  );
}

/* ---------- SerializedProperty Extensions: one call writes, applies and records Undo ---------- */

const PROPERTY_CODE = `manaCost
    .Update()
    .SetIntAndApply(42);`;

function PropertyPreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(5, 1200, useInView(ref), 3);
  const applied = step === 2 || step === 3;
  return (
    <div ref={ref} className={styles.propertyBody}>
      <Code code={PROPERTY_CODE} active={step < 3 ? step : -1} />
      <div className={styles.inspector} aria-hidden="true">
        <div className={styles.inspectorHead}><span className={styles.inspectorIcon}>#</span>Ability Book (Script)</div>
        <div className={styles.inspectorRow}><span>Mana Cost</span><span className={styles.inspectorField} data-flash={step === 2 || undefined}>{applied ? 42 : 10}</span></div>
        <div className={styles.inspectorRow}><span>Cooldown</span><span className={styles.inspectorField}>1</span></div>
        <div className={styles.toast} data-show={step >= 2 || undefined}>
          {step === 4 ? <><kbd>⌘Z</kbd> Undo · Mana Cost</> : <>↶ {ru ? 'Записано в Undo' : 'Undo recorded'}</>}
        </div>
      </div>
    </div>
  );
}

/* ---------- Editor Helpers: type names become labels ---------- */

const NAMES_CODE = `fireAbility.GetDisplayName();
abilityConfig.GetDisplayNameWithIndex();`;

const NAMES = [
  ['FireAbility', 'Fire Ability'],
  ['AbilityConfig', 'Ability Config (1)'],
  ['AbilityConfig', 'Ability Config (2)'],
];

function NamesPreview() {
  const ref = useRef(null);
  const step = useLoop(5, 950, useInView(ref));
  return (
    <div ref={ref} className={styles.namesBody}>
      <Code code={NAMES_CODE} active={step === 1 ? 0 : step >= 2 && step <= 3 ? 1 : -1} />
      <ul className={styles.names} aria-label="Editor Helpers">
        {NAMES.map(([type, label], index) => (
          <li key={index} data-done={step > index || undefined}>
            <code><span className={styles.namesIcon} aria-hidden="true">#</span>{type}</code>
            <span className={styles.namesArrow} aria-hidden="true">↓</span>
            <span className={styles.namesLabel}>“{label}”</span>
          </li>
        ))}
      </ul>
    </div>
  );
}

/* ---------- EnumValues: Populate Missing Enum Members adds the rows and keeps Fire = 1.5 ---------- */

// The second whose frame stands in for the clip under reduced motion.
const ENUM_STILL = 8.2;

/** Loads near the viewport, plays only while visible. */
function EnumPreview({ru}) {
  const ref = useRef(null);
  const near = useInView(ref, {rootMargin: '600px 0px', once: true});
  const visible = useInView(ref);
  const [still, setStill] = useState(false);
  useEffect(() => setStill(prefersReducedMotion()), []);
  useEffect(() => {
    const video = ref.current;
    if (!video || !near || still) return;
    // React does not reliably apply `muted` after hydration, and browsers only autoplay muted video.
    video.muted = true;
    if (visible) video.play().catch(() => {});
    else video.pause();
  }, [near, visible, still]);
  return (
    <div className={styles.enumPreview}>
      <video ref={ref} src={near ? (still ? `${enumMedia}#t=${ENUM_STILL}` : enumMedia) : undefined} width={1576} height={1080}
        muted loop playsInline preload={still ? 'metadata' : 'auto'} role="img"
        aria-label={ru ? 'Populate Missing Enum Members добавляет строки, сохраняя Fire = 1.5' : 'Populate Missing Enum Members adds rows, preserving Fire = 1.5'} />
    </div>
  );
}

/* ---------- Claude Code Plugin: a request in the session, the skill's edit in the method ---------- */

// The edited method, as the skill writes it: the same idioms as the ProfilerMarkers card. The neighbour search keeps its
// old indent until the edit lands, then shifts under its new `using`.
const PLUGIN_CODE = `public void Simulate()
{
    using var _ = this.Marker();
    using (this.Marker().WithName("Neighbours"))
    FindNeighbours();
    Integrate();
}`;
const PLUGIN_ADDED = [2, 3];
const PLUGIN_INDENTED = [4];

function PluginPreview({ru}) {
  const ref = useRef(null);
  const theme = usePrismTheme();
  // 0 an empty prompt, 1 the request is typed, 2 the skill loads, 3 the edit lands, 4–5 the result holds.
  const step = useLoop(6, 1300, useInView(ref), 4);
  const edited = step >= 3;
  return (
    <div ref={ref} className={styles.pluginBody}>
      <div className={styles.session} aria-hidden="true">
        <div className={styles.sessionBar}><span>Claude Code</span><span>FlockSimulation.cs</span></div>
        <div className={styles.sessionLog}>
          <div className={styles.prompt}>
            <span className={styles.promptSign}>&gt;</span>
            <span className={styles.promptTyped} data-typed={step >= 1 || undefined}>
              {ru ? 'Замерь Simulate и отдельно поиск соседей' : 'Profile Simulate and the neighbour search'}
            </span>
            <span className={styles.caret} data-hide={step >= 2 || undefined} />
          </div>
          <div className={styles.event} data-show={step >= 2 || undefined}>
            <span className={styles.eventDot} data-done={edited || undefined} />
            Skill <b>aspid-profiler-marker</b>
          </div>
          <div className={styles.event} data-show={edited || undefined}>
            <span className={styles.eventDot} data-done />
            Update <b>FlockSimulation.cs</b> <span className={styles.eventDiff}>+2</span>
          </div>
        </div>
      </div>
      <Highlight theme={theme} code={PLUGIN_CODE} language="csharp">
        {({tokens, getTokenProps}) => (
          <pre className={clsx(styles.snippet, styles.diff)} style={{color: theme.plain.color}} data-edited={edited || undefined}>
            {tokens.map((line, index) => (
              <span
                key={index}
                className={styles.snippetLine}
                data-added={PLUGIN_ADDED.includes(index) || undefined}
                data-indented={PLUGIN_INDENTED.includes(index) || undefined}>
                {line.map((token, tokenIndex) => {
                  const {key, ...props} = getTokenProps({token});
                  return <span key={tokenIndex} {...props} />;
                })}
              </span>
            ))}
          </pre>
        )}
      </Highlight>
    </div>
  );
}

const PREVIEWS = {
  'enum-values': EnumPreview,
  'profiler-markers': ProfilerPreview,
  'visual-element-extensions': UiPreview,
  'serialized-property-extensions': PropertyPreview,
  'editor-helpers': NamesPreview,
  'claude-code-plugin': PluginPreview,
};

/** An animated preview for a docs introduction feature card; features without one keep their README capture (`children`). */
export default function FeaturePreview({doc, ru, children}) {
  const Preview = PREVIEWS[doc];
  return Preview ? <div className={clsx(styles.featurePreview, 'feature-card__live')}><Preview ru={ru} /></div> : children;
}
