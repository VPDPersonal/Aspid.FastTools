import React, {useEffect, useRef, useState} from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import CodeBlock from '@theme/CodeBlock';
import {prefersReducedMotion, useInView} from '@site/src/components/FeaturePreview/effects';
import styles from './styles.module.css';

const RELEASES = 'https://github.com/VPDPersonal/Aspid.FastTools/releases';

const TEXT = {
  en: {
    steps: [
      <>Open <b>Window → Package Manager</b></>,
      <>Choose <b>+ → Install package from git URL…</b></>,
      <>Paste the URL and press <b>Install</b></>,
    ],
    latest: 'Latest preview',
    pinned: (version) => `Pin ${version}`,
    pick: 'Choose a version',
    versions: 'All versions',
    installed: 'Installed from Git',
  },
  ru: {
    steps: [
      <>Откройте <b>Window → Package Manager</b></>,
      <>Выберите <b>+ → Install package from git URL…</b></>,
      <>Вставьте URL и нажмите <b>Install</b></>,
    ],
    latest: 'Последняя preview',
    pinned: (version) => `Версия ${version}`,
    pick: 'Выберите версию',
    versions: 'Все версии',
    installed: 'Установлен из Git',
  },
};

// Frames: 0 the window opens, 1 the + menu, 2 the URL is typed, 3 Install runs, 4 the package is in the list.
const DURATIONS = [1300, 1400, 1800, 1100, 2600];
// The instruction step each frame illustrates; the last frame marks every step done.
const STEP_OF_FRAME = [0, 1, 2, 2, 3];
const FIRST_FRAME_OF_STEP = [0, 1, 2];
// How many times the walk-through plays on its own before it rests on the last frame.
const PLAYS = 2;
const MENU = ['Install package from disk…', 'Install package from tarball…', 'Install package from git URL…', 'Install package by name…'];

/**
 * Plays the frames while visible, {@link PLAYS} times, then rests on the last one;
 * a click on a step or a version tab jumps to a frame and plays once more from there.
 */
function useFrames(ref) {
  const visible = useInView(ref);
  const [frame, setFrame] = useState(0);
  const [plays, setPlays] = useState(PLAYS);
  // Bumped by a jump, so a jump to the frame already shown still restarts its timer.
  const [jumps, setJumps] = useState(0);
  const [still, setStill] = useState(false);
  useEffect(() => setStill(prefersReducedMotion()), []);
  useEffect(() => {
    const last = frame === DURATIONS.length - 1;
    if (still || !visible || (last && plays <= 1)) return undefined;
    const timer = setTimeout(() => {
      if (last) setPlays((value) => value - 1);
      setFrame(last ? 0 : frame + 1);
    }, DURATIONS[frame]);
    return () => clearTimeout(timer);
  }, [frame, plays, jumps, visible, still]);
  const jump = (value) => {
    setPlays(1);
    setFrame(value);
    setJumps((count) => count + 1);
  };
  return [still ? DURATIONS.length - 1 : frame, jump];
}

function PackageManager({frame, url, version, text}) {
  const done = frame === 4;
  return (
    <div className={styles.pm} data-frame={frame} aria-hidden="true">
      <div className={styles.pmTitle}><span>Package Manager</span></div>
      <div className={styles.pmToolbar}>
        <span className={styles.pmPlus} data-press={frame === 1 || undefined}>+ ▾</span>
        <span className={styles.pmFilter}>Packages: In Project ▾</span>
      </div>
      <div className={styles.pmMain}>
        <ul className={styles.pmList}>
          <li><span>Input System</span><span>1.11.2</span></li>
          <li><span>Universal RP</span><span>17.0.3</span></li>
          <li className={styles.pmNew} data-show={done || undefined}><span>Aspid.FastTools</span><span>{version}</span></li>
        </ul>
        <div className={styles.pmDetail}>
          {done ? (
            <>
              <strong>Aspid.FastTools</strong>
              <span className={styles.pmId}>tech.aspid.fasttools</span>
              <span className={styles.pmBadge}>✓ {text.installed}</span>
            </>
          ) : (
            <>
              <span className={styles.pmSkeleton} style={{width: '62%'}} />
              <span className={styles.pmSkeleton} style={{width: '40%'}} />
              <span className={styles.pmSkeleton} style={{width: '78%'}} />
            </>
          )}
        </div>
      </div>
      <ul className={styles.pmMenu} data-open={frame === 1 || undefined}>
        {MENU.map((item, index) => <li key={item} data-hover={index === 2 || undefined}>{item}</li>)}
      </ul>
      <div className={styles.pmPopover} data-open={frame === 2 || frame === 3 || undefined}>
        <span className={styles.pmInput}><span className={styles.pmTyped} data-typed={frame >= 2 || undefined}>{url}</span></span>
        <span className={styles.pmInstall} data-press={frame === 3 || undefined}>Install</span>
        <span className={styles.pmProgress} data-run={frame === 3 || undefined} />
      </div>
    </div>
  );
}

/** The pinned-version tab: it opens a list of the versions, upwards over the walk-through, since the card clips below. */
function VersionTab({versions, value, active, text, onChoose}) {
  const ref = useRef(null);
  const [open, setOpen] = useState(false);
  useEffect(() => {
    if (!open) return undefined;
    const close = (event) => {
      if (event.type === 'keydown' ? event.key === 'Escape' : !ref.current?.contains(event.target)) setOpen(false);
    };
    document.addEventListener('pointerdown', close);
    document.addEventListener('keydown', close);
    return () => {
      document.removeEventListener('pointerdown', close);
      document.removeEventListener('keydown', close);
    };
  }, [open]);
  const choose = (version) => {
    setOpen(false);
    onChoose(version);
  };
  return (
    <div ref={ref} className={styles.versionTab}>
      <button
        type="button"
        role="radio"
        aria-checked={active}
        aria-haspopup="listbox"
        aria-expanded={open}
        onClick={() => (active ? setOpen(!open) : choose(value))}>
        {text.pinned(value)}
        <span className={styles.caret} aria-hidden="true" />
      </button>
      <ul className={styles.versionMenu} role="listbox" aria-label={text.pick} data-open={open || undefined}>
        {versions.map((version) => (
          <li key={version} role="option" aria-selected={version === value}>
            <button type="button" tabIndex={open ? 0 : -1} onClick={() => choose(version)}>{version}</button>
          </li>
        ))}
      </ul>
    </div>
  );
}

/** The site's install section: a Package Manager walk-through beside the steps, and the URL as a code block to copy. */
export default function InstallPanel({url}) {
  const {siteConfig, i18n} = useDocusaurusContext();
  const {packageVersion, packageVersions} = siteConfig.customFields;
  const text = TEXT[i18n.currentLocale] ?? TEXT.en;
  const ref = useRef(null);
  const [frame, setFrame] = useFrames(ref);
  const [pinned, setPinned] = useState(false);
  const [version, setVersion] = useState(packageVersion);
  const shown = pinned ? `${url}/${version}` : url;
  // Another version types another URL, so the walk-through plays again from the start.
  const choose = (nextPinned, nextVersion = version) => {
    if (nextPinned === pinned && nextVersion === version) return;
    setPinned(nextPinned);
    setVersion(nextVersion);
    setFrame(0);
  };
  const current = STEP_OF_FRAME[frame];

  return (
    <section ref={ref} className={styles.install}>
      <div className={styles.top}>
        <div className={styles.preview}><PackageManager frame={frame} url={shown} version={version} text={text} /></div>
        <ol className={styles.steps}>
          {text.steps.map((step, index) => (
            <li key={index} data-active={current === index || undefined} data-done={current > index || undefined}>
              <button type="button" onClick={() => setFrame(FIRST_FRAME_OF_STEP[index])}>
                <span className={styles.stepIndex}>{current > index ? '✓' : index + 1}</span>
                <span>{step}</span>
              </button>
            </li>
          ))}
        </ol>
      </div>
      <div className={styles.bar}>
        <div className={styles.meta}>
          <div className={styles.segmented} role="radiogroup">
            <button type="button" role="radio" aria-checked={!pinned} onClick={() => choose(false)}>{text.latest}</button>
            <VersionTab versions={packageVersions} value={version} active={pinned} text={text} onChoose={(value) => choose(true, value)} />
          </div>
          <a className={styles.versions} href={RELEASES} target="_blank" rel="noopener noreferrer">{text.versions}</a>
        </div>
        <div className={styles.field}><CodeBlock language="text">{shown}</CodeBlock></div>
      </div>
    </section>
  );
}
