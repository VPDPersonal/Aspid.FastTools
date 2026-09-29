import React, {Children, useRef} from 'react';
import {useWalkthrough} from '@site/src/components/FeaturePreview/effects';
import panel from '@site/src/components/InstallPanel/styles.module.css';
import styles from './styles.module.css';

// Frames: 0 the Tools menu, 1 Scan Project, 2 the missing group, 3 Fix all picks Pistol, 4 Rewrite, 5 the result.
const DURATIONS = [1700, 1100, 1700, 1800, 1700, 2600];
// The quick-start step each frame illustrates; the last frame marks every step done.
const STEP_OF_FRAME = [0, 1, 1, 2, 2, 3];
const FIRST_FRAME_OF_STEP = [0, 1, 3];
const PLAYS = 2;

const EDITOR_MENUS = ['File', 'Edit', 'Assets', 'GameObject', 'Component', 'Tools', 'Window', 'Help'];
// The menu items of `TabWindow`, grouped as Unity groups them by priority.
const FASTTOOLS_MENU = [['Welcome'], ['Asset References', 'Project References'], ['Settings']];
const ENTRIES = [
  ['BrokenArsenalPreset.asset', 8000],
  ['BrokenArsenalPreset.asset', 8001],
  ['BrokenArsenalPreset.asset', 8003],
  ['BrokenWeaponPreset.asset', 7000],
];

function Menu({className, open, children}) {
  return <ul className={`${styles.menu} ${className}`} data-open={open || undefined}>{children}</ul>;
}

/** The Aspid FastTools window, Project References tab, drawn from the package's dark theme and its real strings. */
function FastToolsWindow({frame}) {
  const scanned = frame >= 2;
  const picking = frame === 3;
  const clean = frame === 5;
  return (
    <div className={styles.window} data-open={frame >= 1 || undefined}>
      <div className={styles.tabs}>
        <span className={styles.tabIcon}><i className={styles.iconHome} /></span>
        <span className={styles.tab}>Asset References<em>⌥2</em></span>
        <span className={styles.tab} data-active="true">Project References<em>⌥3</em></span>
        <span className={styles.tabIcon}><i className={styles.iconSettings} /></span>
      </div>
      <div className={styles.body}>
        <div className={styles.panel}>
          <strong>Find missing references</strong>
          <span>Sweep every asset under Assets/ for broken [SerializeReference] types and bulk-fix them by type.</span>
          <span className={styles.button} data-press={frame === 1 || undefined}>{scanned ? 'Rescan' : 'Scan Project'}</span>
        </div>
        {!scanned && (
          <div className={styles.empty}>
            <strong>Project not scanned</strong>
            <span>Run Scan Project to map every broken [SerializeReference] type across your assets — then repair each missing type in bulk.</span>
          </div>
        )}
        {scanned && (
          <div className={styles.results}>
            {/* The results header keeps its warning colour, clean project included. */}
            <div className={styles.header}>{clean ? 'No missing references' : '4 missing references'}</div>
            <p className={styles.hint}>
              {clean
                ? "Nothing left to repair. Rescan to sweep the project again and confirm it's clean."
                : 'Each group is a broken stored type — Fix all re-points its every entry to one replacement, or to <None>.'}
            </p>
            {clean ? (
              <div className={styles.summary}>
                <span className={styles.warningIcon}>⚠</span>
                <div>
                  <div className={styles.summaryHead}>
                    <strong>Rewrote 4 references</strong>
                    <span className={styles.undo}>Undo</span>
                  </div>
                  <span>Replaced missing 'Game.Weapons.GhostWeapon' with 'Game.Weapons.Pistol'.</span>
                </div>
              </div>
            ) : (
              <div className={styles.card} data-picking={picking || undefined}>
                <div className={styles.cardHead}>
                  <b>GhostWeapon</b>
                  <span>4 entries · 2 files</span>
                  <span className={styles.fixAll}>Fix all (4)  {picking ? '▲' : '▼'}</span>
                </div>
                {picking && (
                  <div className={styles.picker}>
                    <span className={styles.search}><span className={styles.typed}>Pistol</span><i>×</i></span>
                    <span className={styles.pick}><span className={styles.script}>#</span>Weapons/Ranged/Pistol</span>
                  </div>
                )}
                <ul className={styles.entries}>
                  {ENTRIES.map(([file, rid]) => <li key={rid}><span>Assets/Presets/{file}</span><span>rid {rid}</span></li>)}
                </ul>
              </div>
            )}
          </div>
        )}
      </div>
      {/* EditorUtility.DisplayDialog, a macOS alert, reduced to its title, question and buttons. */}
      <div className={styles.dialog} data-open={frame === 4 || undefined}>
        <div className={styles.alert}>
          <strong>Repair Missing References</strong>
          <p>Rewrite 4 reference(s) in 2 file(s) to 'Game.Weapons.Pistol'?</p>
          <div className={styles.alertButtons}>
            <span>Cancel</span>
            <span data-default="true">Rewrite</span>
          </div>
        </div>
      </div>
    </div>
  );
}

function Stage({frame}) {
  const open = frame === 0;
  // Each submenu sits in the item that opens it, so the cascade follows the Tools label wherever it lands.
  const menus = (
    <Menu className={styles.menuTools} open={open}>
      <li data-hover="true">
        Aspid 🐍<i>▸</i>
        <Menu className={styles.submenu} open={open}>
          <li data-hover="true">
            FastTools<i>▸</i>
            <Menu className={`${styles.submenu} ${styles.submenuLast}`} open={open}>
              {FASTTOOLS_MENU.map((group, index) => (
                <React.Fragment key={group[0]}>
                  {index > 0 && <li className={styles.separator} />}
                  {group.map((item) => <li key={item} data-hover={item === 'Project References' || undefined}>{item}</li>)}
                </React.Fragment>
              ))}
            </Menu>
          </li>
        </Menu>
      </li>
    </Menu>
  );
  return (
    <div className={styles.stage} data-frame={frame} aria-hidden="true">
      <div className={styles.menubar}>
        {EDITOR_MENUS.map((item) => (
          <span key={item} data-open={(item === 'Tools' && open) || undefined}>{item}{item === 'Tools' && menus}</span>
        ))}
      </div>
      <FastToolsWindow frame={frame} />
    </div>
  );
}

/**
 * The SerializeReference Tooling quick start: a Project References walk-through beside the page's own steps,
 * drawn like the introduction's install panel. The steps come from the Markdown list, so they stay translated there.
 */
export default function ProjectReferencesPanel({children}) {
  const ref = useRef(null);
  const [frame, setFrame] = useWalkthrough(ref, DURATIONS, PLAYS);
  const current = STEP_OF_FRAME[frame];
  const steps = Children.toArray(children);

  return (
    <section ref={ref} className={`${panel.install} ${styles.section}`}>
      <div className={`${panel.top} ${styles.top}`}>
        <div className={`${panel.preview} ${styles.preview}`}><Stage frame={frame} /></div>
        <ol className={panel.steps}>
          {steps.map((step, index) => (
            <li key={index} data-active={current === index || undefined} data-done={current > index || undefined}>
              <button type="button" onClick={() => setFrame(FIRST_FRAME_OF_STEP[index])}>
                <span className={panel.stepIndex}>{current > index ? '✓' : index + 1}</span>
                <span>{step}</span>
              </button>
            </li>
          ))}
        </ol>
      </div>
    </section>
  );
}
