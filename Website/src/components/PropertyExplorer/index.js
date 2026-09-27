import React, {useRef, useState} from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import InlineCode from '@site/src/components/InlineCode';
import styles from './styles.module.css';

/*
 * The AbilityBook from the "C# field from the Inspector" section of SerializedProperty Extensions, drawn as its Inspector.
 * A click on a row shows what the reflection methods return for that property, so the reader sees which C# field each
 * Inspector row stands for. It replaces the page's result table on the site (`src/remark/propertyExplorer.js`);
 * GitHub and Unity keep the table. Keep ROWS in step with that table.
 */
const ROWS = [
  {depth: 0, label: 'Abilities', value: '1', open: true, path: '_abilities', type: 'List<Ability>', field: 'AbilityBook._abilities',
    owner: {en: ['the ', 'AbilityBook', ''], ru: ['экземпляр ', 'AbilityBook', '']}},
  {depth: 1, label: 'Element 0', open: true, path: '_abilities[0]', type: 'Ability', field: 'AbilityBook._abilities',
    owner: {en: ['the ', 'AbilityBook', ''], ru: ['экземпляр ', 'AbilityBook', '']}},
  {depth: 2, label: 'Name', value: 'Fireball', path: '_abilities[0].Name', type: 'string', field: 'Ability.Name',
    owner: {en: ['the ', 'Ability', ' at index 0'], ru: ['', 'Ability', ' с индексом 0']}},
  {depth: 0, label: 'Effect', value: 'Burn Effect', picker: true, open: true, path: '_effect', type: 'IAbilityEffect', field: 'AbilityBook._effect',
    owner: {en: ['the ', 'AbilityBook', ''], ru: ['экземпляр ', 'AbilityBook', '']}},
  {depth: 1, label: 'Damage', value: '5', path: '_effect.Damage', type: 'float', field: 'BurnEffect.Damage',
    owner: {en: ['the ', 'BurnEffect', ' instance'], ru: ['экземпляр ', 'BurnEffect', '']}},
];

export default function PropertyExplorer() {
  const {i18n} = useDocusaurusContext();
  const ru = i18n.currentLocale === 'ru';
  const [current, setCurrent] = useState(2);
  const rowRefs = useRef([]);
  const row = ROWS[current];
  const [before, type, after] = row.owner[ru ? 'ru' : 'en'];

  const onKeyDown = (event) => {
    const step = {ArrowDown: 1, ArrowUp: -1}[event.key];
    if (!step) return;
    event.preventDefault();
    const next = Math.min(ROWS.length - 1, Math.max(0, current + step));
    setCurrent(next);
    rowRefs.current[next]?.focus();
  };

  return (
    <div className={styles.panel}>
      <div className={styles.window}>
        <div className={styles.inspector}>
          <div className={styles.inspectorHead}><span className={styles.inspectorIcon}>#</span>Ability Book (Script)</div>
          <div role="radiogroup" aria-label={ru ? 'Свойства AbilityBook' : 'AbilityBook properties'} onKeyDown={onKeyDown}>
            {ROWS.map((item, index) => (
              <button key={item.path} type="button" role="radio" aria-checked={index === current}
                tabIndex={index === current ? 0 : -1} ref={(element) => { rowRefs.current[index] = element; }}
                className={styles.row} style={{'--depth': item.depth}} onClick={() => setCurrent(index)}>
                <span className={styles.label}>
                  {item.open !== undefined && <span className={styles.caret} aria-hidden="true">▾</span>}
                  {item.label}
                </span>
                {item.value !== undefined && (
                  <span className={styles.field} data-picker={item.picker || undefined}>{item.value}</span>
                )}
              </button>
            ))}
          </div>
        </div>
        <dl className={styles.results} aria-live="polite">
          <div className={styles.result}><dt><InlineCode code="propertyPath" language="csharp" /></dt><dd><InlineCode code={`"${row.path}"`} language="csharp" /></dd></div>
          <div className={styles.result}><dt><InlineCode code="GetPropertyType()" language="csharp" /></dt><dd><InlineCode code={row.type} language="csharp" /></dd></div>
          <div className={styles.result}><dt><InlineCode code="GetFieldInfo()" language="csharp" /></dt><dd><InlineCode code={row.field} language="csharp" /></dd></div>
          <div className={styles.result}><dt><InlineCode code="GetDeclaringInstance()" language="csharp" /></dt><dd>{before}<InlineCode code={type} language="class-name" />{after}</dd></div>
        </dl>
      </div>
    </div>
  );
}
