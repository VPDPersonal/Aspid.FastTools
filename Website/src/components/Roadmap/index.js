import React, {useRef} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useBrokenLinks from '@docusaurus/useBrokenLinks';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import {usePluginData} from '@docusaurus/useGlobalData';
import {Code} from '@site/src/components/FeaturePreview';
import {useInView, useLoop} from '@site/src/components/FeaturePreview/effects';
import fp from '@site/src/components/FeaturePreview/styles.module.css';
import styles from './styles.module.css';

const REPO = 'https://github.com/VPDPersonal/Aspid.FastTools';

function useRu() {
  return useDocusaurusContext().i18n.currentLocale === 'ru';
}

/** Issue states from GitHub, read at build time (`src/plugins/github-issues`); empty when the build was offline. */
function useIssues() {
  return usePluginData('fasttools-github-issues')?.issues ?? {};
}

function progress(numbers, issues) {
  const known = numbers.filter((number) => issues[number]);
  return {done: known.filter((number) => issues[number].state === 'closed').length, total: numbers.length};
}

const plural = (count, one, few, many) => {
  const mod10 = count % 10;
  const mod100 = count % 100;
  if (mod10 === 1 && mod100 !== 11) return one;
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return few;
  return many;
};

/* ---------- Track: the released version, then each stage of the page ---------- */

/** The stages under the lede, left to right; the package's own version is where the reader is now. */
export function RoadmapTrack({stages}) {
  const ru = useRu();
  const issues = useIssues();
  const {siteConfig} = useDocusaurusContext();
  const version = siteConfig.customFields.packageVersion;
  const prerelease = version.includes('-');
  const list = JSON.parse(stages);
  const themes = (count) => (ru ? `${count} ${plural(count, 'тема', 'темы', 'тем')}` : `${count} ${count === 1 ? 'theme' : 'themes'}`);
  return (
    <nav className={styles.track} aria-label={ru ? 'Этапы' : 'Stages'}>
      <Link className={styles.stop} data-kind="now" to="/changelog">
        <span className={styles.node} aria-hidden="true" />
        <span className={styles.stopLabel}>{version.replace(/\.0(-.*)?$/, '')}</span>
        <span className={styles.stopCaption}>
          {prerelease ? (ru ? 'Релиз-кандидат' : 'Release candidate') : (ru ? 'Выпущено' : 'Released')}
          <span className={styles.stopHere}>{ru ? 'вы здесь' : 'you are here'}</span>
        </span>
        <span className={styles.stopMeta}>{version}</span>
      </Link>
      {list.map((stage, index) => {
        const {done, total} = progress(stage.issues, issues);
        return (
          <a key={stage.id} className={styles.stop} data-kind={index === 0 ? 'next' : 'later'} href={`#${stage.id}`}>
            <span className={styles.node} aria-hidden="true" />
            <span className={styles.stopLabel}>{stage.label}</span>
            <span className={styles.stopCaption}>{themes(stage.themes)}</span>
            <span className={styles.stopMeta}>
              <span className={styles.meter} aria-hidden="true"><span style={{width: `${total ? (done / total) * 100 : 0}%`}} /></span>
              {done}/{total} issues
            </span>
          </a>
        );
      })}
    </nav>
  );
}

/* ---------- Theme cards ---------- */

function IssueChip({number, issue}) {
  const ru = useRu();
  const done = issue?.state === 'closed';
  return (
    <a className={styles.issue} data-done={done || undefined} href={`${REPO}/issues/${number}`} target="_blank" rel="noopener noreferrer"
      title={issue?.title}>
      <span className={styles.issueState} aria-label={done ? (ru ? 'готово' : 'done') : (ru ? 'открыто' : 'open')} />
      #{number}
      {issue?.votes > 0 && <span className={styles.issueVotes}>👍 {issue.votes}</span>}
    </a>
  );
}

/** One theme: a live preview of the feature on the left, the text and its issues on the right. */
export function RoadmapTheme({id, issues: numbers, children}) {
  const ru = useRu();
  const issues = useIssues();
  const theme = THEMES[id];
  const list = numbers ? numbers.split(',').map(Number) : [];
  const {done, total} = progress(list, issues);
  const Preview = theme?.preview;
  // The card replaces the theme's heading, so it registers the heading's anchor that the sidebar links to.
  useBrokenLinks().collectAnchor(id);
  return (
    <article id={id} className={clsx('feature-card', styles.theme)}>
      {Preview && (
        <div className="feature-card__preview">
          <div className={clsx(fp.featurePreview, 'feature-card__live', styles.preview)}><Preview ru={ru} /></div>
        </div>
      )}
      <div className="feature-card__body">
        {theme && <p className={styles.area}>{theme.area}</p>}
        {children}
        {list.length > 0 && (
          <div className={styles.issues}>
            {list.map((number) => <IssueChip key={number} number={number} issue={issues[number]} />)}
            {done > 0 && <span className={styles.issuesDone}>{done}/{total} {ru ? 'готово' : 'done'}</span>}
          </div>
        )}
      </div>
    </article>
  );
}

/* ---------- Call for ideas: the Markdown paragraph, two actions and the most wanted ideas not on the roadmap ---------- */

export function RoadmapIdea({href, exclude = '', children}) {
  const ru = useRu();
  const issues = useIssues();
  const planned = new Set(exclude.split(',').filter(Boolean).map(Number));
  const wanted = Object.entries(issues)
    .filter(([number, issue]) => issue.state === 'open' && !planned.has(Number(number)))
    .sort(([a, x], [b, y]) => y.votes - x.votes || b - a)
    .slice(0, 3);
  return (
    <section className={styles.idea}>
      <div className={styles.ideaMain}>
        <div className={styles.ideaCopy}>
          {children}
        </div>
        <div className={styles.ideaActions}>
          <a className={styles.primary} href={href} target="_blank" rel="noopener noreferrer">{ru ? 'Предложить' : 'Suggest'}</a>
          <a className={styles.secondary} href={`${REPO}/issues?q=is%3Aissue+is%3Aopen+sort%3Areactions-%2B1-desc`} target="_blank"
            rel="noopener noreferrer">{ru ? 'Все идеи' : 'All ideas'}</a>
        </div>
      </div>
      {wanted.length > 0 && (
        <ul className={styles.wanted} aria-label={ru ? 'Ещё не в планах' : 'Not planned yet'}>
          {wanted.map(([number, issue]) => (
            <li key={number}>
              <a href={`${REPO}/issues/${number}`} target="_blank" rel="noopener noreferrer">
                <span className={styles.wantedNumber}>#{number}</span>
                <span className={styles.wantedTitle}>{issue.title}</span>
                <span className={styles.wantedVotes}>👍 {issue.votes}</span>
              </a>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

/* ---------- Previews: each theme's feature as it will look, drawn like the introduction's live cards ---------- */

function Caption({children}) {
  return <span className={styles.caption}>{children}</span>;
}

function InspectorHead({children}) {
  return <div className={fp.inspectorHead}><span className={fp.inspectorIcon}>#</span>{children}</div>;
}

// Readable inspectors: today a collapsed list shows only types; then a summary (#155), a colour per type (#156) and the
// type on the element being dragged (#160).
const ABILITIES = [
  {type: 'Fireball', color: '#e2706a', summary: 'Damage 40 · Radius 3'},
  {type: 'Heal', color: '#5cb46b', summary: 'Amount 25 · Self'},
  {type: 'Dash', color: '#4a90d0', summary: 'Distance 6 · 0.2 s'},
];
// Row order per step: Heal is dragged below Dash in the last two steps.
const ORDER = [[0, 1, 2], [0, 1, 2], [0, 1, 2], [0, 1.55, 1], [0, 2, 1]];

function InspectorPreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(5, 1500, useInView(ref), 4);
  const captions = ru
    ? ['Сегодня: только тип', 'Сводка свёрнутого элемента · #155', 'Цвет у каждого типа · #156', 'Тип при перетаскивании · #160', 'Тип при перетаскивании · #160']
    : ['Today: the type only', 'Collapsed summary · #155', 'A colour per type · #156', 'Type while dragging · #160', 'Type while dragging · #160'];
  return (
    <div ref={ref} className={clsx(fp.inspector, styles.stage)} aria-hidden="true">
      <InspectorHead>Ability Deck (Script)</InspectorHead>
      <div className={styles.body}>
      <div className={styles.listHead}><span>▾ Abilities</span><span className={styles.count}>3</span></div>
      <div className={styles.rows} style={{'--rows': 3}}>
        {ABILITIES.map((ability, index) => (
          <div key={ability.type} className={styles.row} style={{'--slot': ORDER[step][index], '--type': ability.color}}
            data-colour={step >= 2 || undefined} data-dragging={(index === 1 && step === 3) || undefined}>
            <span className={styles.handle}>≡</span>
            <span className={styles.fold}>▸</span>
            <b>{ability.type}</b>
            <span className={styles.summary} data-show={step >= 1 || undefined}>{ability.summary}</span>
            {index === 1 && <span className={styles.badge} data-show={step === 3 || undefined}>{ability.type}</span>}
          </div>
        ))}
      </div>
      </div>
      <Caption>{captions[step]}</Caption>
    </div>
  );
}

// TypeSelector: a new element starts as the default type (#154); the picker drops what does not belong (#157).
const WEAPONS = [
  {name: 'Sword'},
  {name: 'Bow'},
  {name: 'Staff'},
  {name: 'OldSword', note: '[Obsolete]'},
  {name: 'DebugWeapon', note: 'Tests.Editor'},
];

function PickerPreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(6, 1400, useInView(ref), 4);
  const captions = ru
    ? ['Список оружия', 'Новый элемент сразу Sword · #154', 'Пикер типов', 'Лишнее скрыто фильтрами · #157', 'Лишнее скрыто фильтрами · #157', 'Список оружия']
    : ['A weapon list', 'A new element starts as Sword · #154', 'The type picker', 'Filters hide the rest · #157', 'Filters hide the rest · #157', 'A weapon list'];
  return (
    <div ref={ref} className={clsx(fp.inspector, styles.stage)} aria-hidden="true">
      <InspectorHead>Weapon Rack (Script)</InspectorHead>
      <div className={styles.body}>
      <div className={styles.listHead}><span>▾ Weapons</span><span className={styles.count}>{step >= 1 && step < 5 ? 2 : 1}</span></div>
      <div className={styles.rows} style={{'--rows': 2}}>
        <div className={styles.row} style={{'--slot': 0}}><span className={styles.fold}>▸</span><b>Bow</b></div>
        <div className={styles.row} style={{'--slot': 1}} data-optional data-new={step >= 1 && step < 5 || undefined}>
          <span className={styles.fold}>▸</span><b>Sword</b><span className={styles.tag}>Default</span>
        </div>
      </div>
      <div className={styles.listFoot}><span data-press={step === 1 || undefined}>+</span><span>−</span></div>
      </div>
      <div className={styles.picker} data-show={(step >= 2 && step <= 4) || undefined}>
        <div className={styles.pickerSearch}>⌕ Weapon</div>
        {WEAPONS.map((weapon) => (
          <div key={weapon.name} className={styles.pickerRow} data-noise={weapon.note ? true : undefined}
            data-hidden={(weapon.note && step >= 3) || undefined} data-picked={weapon.name === 'Sword' || undefined}>
            {weapon.name}{weapon.note && <span>{weapon.note}</span>}
          </div>
        ))}
      </div>
      <Caption>{captions[step]}</Caption>
    </div>
  );
}

// Data that survives: a field changes type; today the value is lost, with an upgrade callback it moves over (#162).
const FIELD_BEFORE = `[Serializable]
public class Fireball : Ability
{
    public float damage;
}`;
const FIELD_AFTER = FIELD_BEFORE.replace('float damage', 'DamageRange damage');

function UpgradePreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(5, 1500, useInView(ref), 3);
  const captions = ru
    ? ['Ассет со значением 40', 'Тип поля поменялся', 'Сегодня: значение потеряно', 'Апгрейд переносит данные · #162', 'Апгрейд переносит данные · #162']
    : ['An asset holding 40', 'The field changes type', 'Today: the value is lost', 'An upgrade moves the data · #162', 'An upgrade moves the data · #162'];
  const value = step >= 3 ? 40 : 0;
  return (
    <div ref={ref} className={styles.split}>
      <Code code={step >= 1 ? FIELD_AFTER : FIELD_BEFORE} active={step === 1 ? 3 : -1} />
      <div className={clsx(fp.inspector, styles.stage)} aria-hidden="true">
        <InspectorHead>Fireball</InspectorHead>
        {step === 0
          ? <div className={fp.inspectorRow}><span>Damage</span><span className={fp.inspectorField}>40</span></div>
          : (
            <div className={fp.inspectorRow}>
              <span>Damage</span>
              <span className={styles.pair} data-lost={step === 1 || step === 2 || undefined} data-kept={step >= 3 || undefined}>
                <span className={fp.inspectorField} data-flash={step === 3 || undefined}>Min {value}</span>
                <span className={fp.inspectorField} data-flash={step === 3 || undefined}>Max {value}</span>
              </span>
            </div>
          )}
        <Caption>{captions[step]}</Caption>
      </div>
    </div>
  );
}

// Runtime: a type name in mod data becomes a Type and then an instance (#161).
const MOD_DATA = `{
  "ability": "Fireball",
  "damage": 40
}`;

function RuntimePreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(5, 1200, useInView(ref), 4);
  return (
    <div ref={ref} className={styles.split}>
      <div>
        <div className={styles.fileBar}><span>mods/abilities.json</span><span>{ru ? 'данные мода' : 'mod data'}</span></div>
        <Code code={MOD_DATA} language="json" active={step === 1 ? 1 : step === 3 ? 2 : -1} />
      </div>
      <div className={styles.flow} aria-hidden="true">
        <span className={styles.flowNode} data-on={step >= 1 || undefined}>"Fireball"</span>
        <span className={styles.flowArrow} data-on={step >= 2 || undefined}>→</span>
        <span className={styles.flowNode} data-on={step >= 2 || undefined}><small>Type</small> Fireball</span>
        <span className={styles.flowArrow} data-on={step >= 3 || undefined}>→</span>
        <span className={styles.flowNode} data-on={step >= 3 || undefined}><small>new</small> Fireball {'{'} damage = 40 {'}'}</span>
      </div>
    </div>
  );
}

// Generators: a field attribute, and the class attribute the generator writes for it (proposal in #1).
const GENERATOR_SOURCE = `public partial class HealthBar : MonoBehaviour
{
    [RequiredComponent]
    private TMP_Text _label;
}`;
const GENERATOR_OUTPUT = `[RequireComponent(typeof(TMP_Text))]
partial class HealthBar { }`;

function GeneratorPreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(4, 1500, useInView(ref), 2);
  return (
    <div ref={ref} className={styles.split}>
      <Code code={GENERATOR_SOURCE} active={step === 1 ? 2 : -1} />
      <div className={styles.generated} data-show={step >= 2 || undefined}>
        <div className={styles.fileBar}><span>HealthBar.g.cs</span><span>{ru ? 'сгенерировано' : 'generated'}</span></div>
        <Code code={GENERATOR_OUTPUT} active={step === 2 ? 0 : -1} />
      </div>
    </div>
  );
}

// Categories: today every marker is a Scripts sample; with a category the Timeline colours it and a filter hides the rest.
const CATEGORY_CODE = `using var _ = this.Marker();
using (this.Marker().WithName("Steering")
    .WithCategory(ProfilerCategory.Ai))
    Steer(_agents);
using (this.Marker().WithName("Integrate")
    .WithCategory(ProfilerCategory.Physics))
    Integrate();`;

const CATEGORIES = [
  {name: 'Scripts', color: '#86b9ec'},
  {name: 'AI', color: '#c792ea'},
  {name: 'Physics', color: '#ecc676'},
];

function CategoryPreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(4, 1600, useInView(ref), 2);
  const coloured = step >= 1;
  const physicsOff = step === 2;
  const captions = ru
    ? ['Сегодня: всё в Scripts', 'Цвет по категории', 'Фильтр скрыл Physics', 'Цвет по категории']
    : ['Today: all in Scripts', 'Coloured by category', 'The filter hides Physics', 'Coloured by category'];
  const bar = (name, category, basis) => (
    <span className={clsx(fp.bar, styles.categoryBar)} style={{flexBasis: basis, '--category': coloured ? category.color : CATEGORIES[0].color}}
      data-off={(physicsOff && category.name === 'Physics') || undefined}>{name}</span>
  );
  return (
    <div ref={ref} className={styles.split}>
      <Code code={CATEGORY_CODE} active={step === 1 ? 2 : step === 2 ? 5 : -1} />
      <div className={clsx(fp.timeline, styles.stage, styles.captioned)} aria-hidden="true">
        <div className={fp.timelineBar}><span>Timeline</span><span>Main Thread</span></div>
        <div className={styles.legend}>
          {CATEGORIES.map((category) => (
            <span key={category.name} className={styles.legendChip} style={{'--category': category.color}}
              data-show={coloured || undefined} data-off={(physicsOff && category.name === 'Physics') || undefined}>{category.name}</span>
          ))}
        </div>
        <div className={styles.frame}>
          {bar('FlockSimulation.Step (3)', CATEGORIES[0], '100%')}
          <div className={styles.frameRow}>
            {bar('FlockSimulation.Steering (4)', CATEGORIES[1], '62%')}
            {bar('FlockSimulation.Integrate (6)', CATEGORIES[2], '30%')}
          </div>
        </div>
        <Caption>{captions[step]}</Caption>
      </div>
    </div>
  );
}

// Quick fixes: AFT0011 underlines a scope without `using`; the light bulb offers the fix and applies it.
const FIX_BEFORE = `public void Step()
{
    this.Marker();
    Integrate();
}`;
const FIX_AFTER = FIX_BEFORE.replace('    this.Marker();', '    using var _ = this.Marker();');

function QuickFixPreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(5, 1400, useInView(ref), 3);
  const fixed = step >= 3;
  return (
    <div ref={ref} className={clsx(styles.fix, styles.stage)} data-warn={!fixed || undefined}>
      <div className={styles.fileBar}><span>FlockSimulation.cs</span><span>Rider</span></div>
      <Code code={fixed ? FIX_AFTER : FIX_BEFORE} active={step === 3 ? 2 : -1} />
      <div className={styles.fixHint} data-show={step === 1 || undefined}>
        <b>AFT0011</b> {ru ? 'scope маркера никогда не закрывается' : 'the marker scope is never disposed'}
      </div>
      <div className={styles.fixMenu} data-show={step === 2 || undefined}>
        <span className={styles.fixBulb}>💡</span>
        <span data-picked>{ru ? 'Добавить using var _ =' : 'Add using var _ ='}</span>
        <span>{ru ? 'Обернуть в using (…) { }' : 'Wrap in using (…) { }'}</span>
      </div>
      <div className={styles.problems}>
        <div className={styles.problemsBar}><span>{ru ? 'Проблемы' : 'Problems'}</span><span>{fixed ? 0 : 1}</span></div>
        {fixed
          ? <div className={styles.problem} data-clear>✓ {ru ? 'Нет предупреждений' : 'No warnings'}</div>
          : <div className={styles.problem}><b>⚠ AFT0011</b> FlockSimulation.cs:3</div>}
      </div>
      <Caption>{fixed ? (ru ? 'Исправлено в один клик' : 'Fixed in one click') : (ru ? 'Предупреждение анализатора' : 'An analyzer warning')}</Caption>
    </div>
  );
}

// EnumValues renames: a renamed member leaves its row as <Missing Frozen>; [FormerlySerializedAs] carries it over.
const ENUM_BEFORE = `public enum StatusEffect
{
    Burning,
    Frozen,
}`;
const ENUM_RENAMED = ENUM_BEFORE.replace('Frozen', 'Chilled');
const ENUM_MIGRATED = ENUM_RENAMED.replace('    Chilled,', '    [FormerlySerializedAs("Frozen")]\n    Chilled,');

function EnumRenamePreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(5, 1500, useInView(ref), 3);
  const code = step === 0 ? ENUM_BEFORE : step <= 2 ? ENUM_RENAMED : ENUM_MIGRATED;
  const captions = ru
    ? ['Таблица по enum', 'Член переименован', 'Сегодня: строка потеряна', 'Строка перенесена', 'Строка перенесена']
    : ['A table over an enum', 'A member is renamed', 'Today: the row is lost', 'The row carries over', 'The row carries over'];
  const second = step === 0 ? 'Frozen' : step <= 2 ? '<Missing Frozen>' : 'Chilled';
  return (
    <div ref={ref} className={styles.split}>
      <Code code={code} active={step === 1 ? 3 : step === 3 ? 3 : -1} />
      <div className={clsx(fp.inspector, styles.stage)} aria-hidden="true">
        <InspectorHead>Status Resistances (Script)</InspectorHead>
        <div className={fp.inspectorRow}><span className={styles.enumKey}>Burning</span><span className={fp.inspectorField}>0.9</span></div>
        <div className={fp.inspectorRow}>
          <span className={styles.enumKey} data-lost={(step >= 1 && step <= 2) || undefined}>{second}</span>
          <span className={fp.inspectorField} data-flash={step === 3 || undefined}>0.5</span>
        </div>
        <Caption>{captions[step]}</Caption>
      </div>
    </div>
  );
}

// Markers everywhere: a value on the sample in the Timeline, and the marker's timing on screen in a player build.
function EverywherePreview({ru}) {
  const ref = useRef(null);
  const step = useLoop(4, 1600, useInView(ref), 2);
  return (
    <div ref={ref} className={styles.split}>
      <div className={clsx(fp.timeline, styles.everywhereTimeline)} aria-hidden="true">
        <div className={fp.timelineBar}><span>Timeline</span><span>Main Thread</span></div>
        <div className={clsx(styles.frame, styles.frameBesideTip)}>
          <span className={clsx(fp.bar, styles.categoryBar)} style={{'--category': '#86cf91'}}>FlockSimulation.Step</span>
          <div className={styles.frameRow}>
            <span className={clsx(fp.bar, styles.categoryBar)} style={{flexBasis: '80%', '--category': '#86b9ec'}} data-hover={step >= 1 || undefined}>
              FlockSimulation.Steering
            </span>
          </div>
        </div>
        <div className={styles.sampleTip} data-show={step >= 1 || undefined}>
          <b>FlockSimulation.Steering</b>
          <span>0.84 ms</span>
          <span className={styles.sampleData}>{ru ? 'Агентов' : 'Agents'}: 120</span>
        </div>
      </div>
      <div className={styles.game} aria-hidden="true">
        <span className={styles.gameLabel}>{ru ? 'Билд игрока' : 'Player build'}</span>
        <div className={styles.hud} data-show={step >= 2 || undefined}>
          <span>Steering</span><b>0.84 ms</b>
          <span>Integrate</span><b>0.31 ms</b>
        </div>
      </div>
    </div>
  );
}

// English heading anchor → the theme's area and preview. Update the preview when a theme's issues change.
const THEMES = {
  'readable-serializereference-inspectors': {area: 'SerializeReference · Inspector', preview: InspectorPreview},
  'typeselector-that-offers-the-right-types': {area: 'TypeSelector', preview: PickerPreview},
  'serializereference-data-that-survives-changes': {area: 'SerializeReference · Data', preview: UpgradePreview},
  'types-by-name-at-runtime': {area: 'Runtime', preview: RuntimePreview},
  'profiler-markers-by-category': {area: 'ProfilerMarkers', preview: CategoryPreview},
  'quick-fixes-for-analyzer-warnings': {area: 'Analyzers', preview: QuickFixPreview},
  'enumvalues-that-survive-enum-renames': {area: 'EnumValues', preview: EnumRenamePreview},
  'profiler-markers-everywhere': {area: 'ProfilerMarkers', preview: EverywherePreview},
  'less-boilerplate-from-source-generators': {area: 'Source Generators', preview: GeneratorPreview},
};
