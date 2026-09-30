import React, {useEffect, useRef, useState} from 'react';
import clsx from 'clsx';
import {Highlight} from 'prism-react-renderer';
import {useColorMode, usePrismTheme} from '@docusaurus/theme-common';
import {prefersReducedMotion, useInView, useLoop} from './effects';
import enumMedia from './media/enum.mp4';
import enumMediaLight from './media/enum-light.mp4';
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

const UI_CODE = `var header = new VisualElement()
    .SetPaddingX(12)
    .SetPaddingY(10)
    .AddChild(new Label("Ability Config")
        .SetFontSize(14));`;

const UI_STEPS = ['VisualElement', 'padding-left / right: 12', 'padding-top / bottom: 10', 'Label “Ability Config”', 'font-size: 14', 'VisualElement'];

function UiPreview() {
  const ref = useRef(null);
  const step = useLoop(6, 1100, useInView(ref), 4);
  return (
    <div ref={ref} className={styles.uiBody}>
      <Code code={UI_CODE} active={step <= 4 ? step : -1} />
      <div className={styles.uiStage} aria-hidden="true">
        <div className={styles.uiPanel} data-x={step >= 1 || undefined} data-y={step >= 2 || undefined}>
          <span className={styles.uiPadX}>12</span>
          <span className={styles.uiPadY}>10</span>
          <div className={styles.uiContent}>
            <span className={styles.uiLabel} data-show={step >= 3 || undefined} data-big={step >= 4 || undefined}>Ability Config</span>
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

/* ---------- Editor Helpers: one component, its label with and without the index ---------- */

const NAMES_CODE = `caster.GetDisplayName();
caster.GetDisplayNameWithIndex();`;

// `caster` is the second AbilityCaster on its GameObject, hence the (2).
const NAMES = [
  ['GetDisplayName()', 'Ability Caster'],
  ['GetDisplayNameWithIndex()', 'Ability Caster (2)'],
];

function NamesPreview() {
  const ref = useRef(null);
  // 0 nothing resolved, 1 the plain label, 2 the indexed one, 3–4 both hold.
  const step = useLoop(5, 950, useInView(ref));
  return (
    <div ref={ref} className={styles.namesBody}>
      <Code code={NAMES_CODE} active={step <= 2 ? step - 1 : -1} />
      <ul className={styles.names} aria-label="Editor Helpers">
        {NAMES.map(([method, label], index) => (
          <li key={method} data-done={step > index || undefined} data-active={step - 1 === index || undefined}>
            <code>{method}</code>
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
  // The light site theme shows the same clip recorded in Unity's light editor skin.
  const media = useColorMode().colorMode === 'light' ? enumMediaLight : enumMedia;
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
  }, [near, visible, still, media]);
  return (
    <div className={styles.enumPreview}>
      <video ref={ref} src={near ? (still ? `${media}#t=${ENUM_STILL}` : media) : undefined} width={1576} height={1080}
        muted loop playsInline preload={still ? 'metadata' : 'auto'} role="img"
        aria-label={ru ? 'Populate Missing Enum Members добавляет строки, сохраняя Fire = 1.5' : 'Populate Missing Enum Members adds rows, preserving Fire = 1.5'} />
    </div>
  );
}

/* ---------- Agent Skills: a request in the session, the skill's edit in the file ---------- */

// One scene per package skill: the request, the skill it loads, and the file as the skill leaves it. `added` lines open up
// when the edit lands; `indented` lines keep their old indent until then and shift under the new block. Each scene reuses
// the quick-start example of the skill's feature page, so update it when that page changes. The card on the
// introduction plays the first scene; the Agent Skills page plays each in its skill's section (`AgentSession`).
export const AGENT_SCENES = [
  {
    skill: 'aspid-profiler-marker',
    file: 'FlockSimulation.cs',
    prompt: {en: 'Profile Simulate and the neighbor search', ru: 'Замерь Simulate и отдельно поиск соседей'},
    code: `public void Simulate()
{
    using var _ = this.Marker();
    using (this.Marker().WithName("Neighbors"))
    FindNeighbors();
    Integrate();
}`,
    added: [2, 3],
    indented: [4],
  },
  {
    skill: 'aspid-visual-element-fluent',
    file: 'AbilityConfigEditor.cs',
    prompt: {en: 'Build the inspector header with a title', ru: 'Собери шапку инспектора с заголовком'},
    code: `public override VisualElement CreateInspectorGUI()
{
    return new VisualElement()
        .SetPaddingX(12)
        .SetPaddingY(10)
        .AddChild(new Label("Ability Config")
            .SetFontSize(14));
}`,
    added: [2, 3, 4, 5, 6],
  },
  {
    skill: 'aspid-serializable-type',
    file: 'WeaponMount.cs',
    prompt: {en: 'Add a weapon class picker to the Inspector, no abstract ones', ru: 'Добавь в инспектор выбор класса оружия, без абстрактных'},
    code: `public sealed class WeaponMount : MonoBehaviour
{
    [TypeSelector(Allow = TypeAllow.None)]
    [SerializeField]
    private SerializableType<Weapon> _primaryWeapon;
}`,
    added: [2, 3, 4],
  },
  {
    skill: 'aspid-enum-values',
    file: 'DamageReceiver.cs',
    prompt: {en: 'Add a damage multiplier per DamageType to the Inspector', ru: 'Добавь в инспектор множитель урона для каждого DamageType'},
    code: `public sealed class DamageReceiver : MonoBehaviour
{
    [SerializeField]
    private EnumValues<DamageType, float> _multipliers;

    public float GetMultiplier(DamageType type) =>
        _multipliers.GetValue(type);
}`,
    added: [2, 3, 4, 5, 6],
  },
];

/** Steps of one scene: 0 an empty prompt, 1 the request is typed, 2 the skill loads, 3 the edit lands, 4–5 the result holds. */
const AGENT_SCENE_STEPS = 6;
const AGENT_SCENE_STEP_MS = 1300;

/**
 * One scene of an agent session. On the introduction card it loops; with `manual` (the Agent Skills page) the request
 * waits in the prompt until the reader presses Send, plays once, and Send turns into Replay.
 */
export function PluginPreview({ru, scene = AGENT_SCENES[0], lines, manual = false}) {
  const ref = useRef(null);
  const theme = usePrismTheme();
  const looped = useLoop(AGENT_SCENE_STEPS, AGENT_SCENE_STEP_MS, useInView(ref) && !manual, 4);
  const [played, setPlayed] = useState(1);
  const [busy, setBusy] = useState(false);
  const timers = useRef([]);
  useEffect(() => () => timers.current.forEach(clearTimeout), []);
  const step = manual ? played : looped;
  const edited = step >= 3;
  const added = scene.added ?? [];
  const indented = scene.indented ?? [];

  // Replay first folds the edit back, then runs the same two beats as the loop: the skill loads, the edit lands.
  const send = () => {
    timers.current.forEach(clearTimeout);
    const start = step >= 3 ? 450 : 150;
    setPlayed(1);
    setBusy(true);
    timers.current = [
      setTimeout(() => setPlayed(2), start),
      setTimeout(() => {
        setPlayed(3);
        setBusy(false);
      }, start + AGENT_SCENE_STEP_MS),
    ];
  };
  const sendLabel = step >= 3 ? (ru ? 'Повторить' : 'Replay') : (ru ? 'Отправить' : 'Send');

  return (
    <div ref={ref} className={styles.pluginBody}>
      <div className={styles.session} data-session aria-hidden={manual ? undefined : 'true'}>
        <div className={styles.sessionBar}><span>Agent</span><span>{scene.file}</span></div>
        <div className={styles.sessionLog}>
          <div className={styles.prompt}>
            <span className={styles.promptSign}>&gt;</span>
            <span className={styles.promptTyped} data-typed={step >= 1 || undefined}>{ru ? scene.prompt.ru : scene.prompt.en}</span>
            <span className={styles.caret} data-hide={step >= 2 || undefined} />
            {manual && (
              <button type="button" className={styles.promptSend} data-replay={step >= 3 || undefined}
                disabled={busy} onClick={send} aria-label={sendLabel} title={sendLabel}>
                <svg viewBox="0 0 16 16" aria-hidden="true">
                  {step >= 3
                    ? <path d="M13.5 8A5.5 5.5 0 1 1 11.9 4.1M12 1.5V4.4H9.1" />
                    : <path d="M8 13V3M3.5 7.5L8 3l4.5 4.5" />}
                </svg>
              </button>
            )}
          </div>
          <div className={styles.event} data-show={step >= 2 || undefined}>
            <span className={styles.eventDot} data-done={edited || undefined} />
            Skill <b>{scene.skill}</b>
          </div>
          <div className={styles.event} data-show={edited || undefined}>
            <span className={styles.eventDot} data-done />
            Update <b>{scene.file}</b> <span className={styles.eventDiff}>+{added.length}</span>
          </div>
        </div>
      </div>
      <Highlight theme={theme} code={scene.code} language="csharp">
        {({tokens, getTokenProps}) => (
          <pre className={clsx(styles.snippet, styles.diff)} style={{color: theme.plain.color, ...(lines && {minHeight: `calc(${lines} * 1.75em + 28px)`})}}
            data-edited={edited || undefined}>
            {tokens.map((line, index) => (
              <span
                key={index}
                className={styles.snippetLine}
                data-added={added.includes(index) || undefined}
                data-indented={indented.includes(index) || undefined}>
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
  'agent-skills': PluginPreview,
};

/** An animated preview for a docs introduction feature card; features without one keep their README capture (`children`). */
export default function FeaturePreview({doc, ru, children}) {
  const Preview = PREVIEWS[doc];
  return Preview ? <div className={clsx(styles.featurePreview, 'feature-card__live')}><Preview ru={ru} /></div> : children;
}
