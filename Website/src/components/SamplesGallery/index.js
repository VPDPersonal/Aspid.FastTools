import React, {useEffect, useRef, useState} from 'react';
import ThemedImage from '@theme/ThemedImage';
import Link from '@docusaurus/Link';
import {useColorMode} from '@docusaurus/theme-common';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import {prefersReducedMotion, useInView} from '../FeaturePreview/effects';
import types from '@site/static/img/samples/types.png';
import weapons from '@site/static/img/samples/serialize-references.png';
import surfaces from '@site/static/img/samples/enum-values.png';
import flock from '@site/static/img/samples/profiler-markers.png';
import abilities from '@site/static/img/samples/ability-catalog.png';
import typesLight from '@site/static/img/samples/types-light.png';
import weaponsLight from '@site/static/img/samples/serialize-references-light.png';
import surfacesLight from '@site/static/img/samples/enum-values-light.png';
import flockLight from '@site/static/img/samples/profiler-markers-light.png';
import abilitiesLight from '@site/static/img/samples/ability-catalog-light.png';
import surfacesClip from './media/enum-values.mp4';
import surfacesClipLight from './media/enum-values-light.mp4';
import typesClip from './media/types.mp4';
import typesClipLight from './media/types-light.mp4';
import weaponsClip from './media/serialize-references.mp4';
import weaponsClipLight from './media/serialize-references-light.mp4';
import flockClip from './media/profiler-markers.mp4';
import flockClipLight from './media/profiler-markers-light.mp4';
import styles from './styles.module.css';

// A scene's preview has its camera background painted in the article surface, so the scene sits on the card itself.
// `clip` / `lightClip`: a muted mp4 of it, looping while the card is on screen, starting on the preview's frame; its
// background is black (dark) or white (light), which the card blends into that surface. All come from
// docs/media/samples-gallery/scenes.py.
const samples = [
  { id: 'enum-values', feature: 'EnumValues', image: surfaces, lightImage: surfacesLight, clip: surfacesClip, lightClip: surfacesClipLight,
    en: ['EnumValues sample scene', "A walker crosses surface tiles, and each surface's color, trail and speed come from tables in the Inspector."],
    ru: ['Сцена примера EnumValues', 'Персонаж идёт по плиткам, а цвет, след и скорость на каждой поверхности берутся из таблиц в инспекторе.'] },
  { id: 'types', feature: 'Types', image: types, lightImage: typesLight, clip: typesClip, lightClip: typesClipLight,
    en: ['Types sample scene', 'A spawner whose enemy type and wave pattern are picked in the Inspector, without touching code.'],
    ru: ['Сцена примера Types', 'Спавнер, в котором тип врага и схема волны выбираются в инспекторе, без правки кода.'] },
  { id: 'serialize-references', feature: 'SerializeReferences', image: weapons, lightImage: weaponsLight, clip: weaponsClip, lightClip: weaponsClipLight,
    en: ['SerializeReferences sample scene', 'A turret whose weapons and their effects are picked in the Inspector, plus assets broken on purpose for you to repair.'],
    ru: ['Сцена примера SerializeReferences', 'Турель, у которой оружие и его эффекты выбираются в инспекторе, и нарочно сломанные ассеты, чтобы их починить.'] },
  { id: 'editor-tools', feature: 'EditorTools', image: abilities, lightImage: abilitiesLight,
    en: ['EditorTools sample window', "An editor window and an Inspector built in code with the package's helpers."],
    ru: ['Окно примера EditorTools', 'Окно редактора и инспектор, собранные в коде на хелперах пакета.'] },
  { id: 'profiler-markers', feature: 'ProfilerMarkers', image: flock, lightImage: flockLight, clip: flockClip, lightClip: flockClipLight,
    en: ['ProfilerMarkers sample scene', 'A flock of cubes whose every frame phase shows up in the Profiler under its own name.'],
    ru: ['Сцена примера ProfilerMarkers', 'Стая кубов, у которой каждая фаза кадра видна в Profiler под своим именем.'] },
];

/** The preview image, and over it the scene's clip: loaded near the viewport, playing while on screen. */
function Preview({sample, alt, eager}) {
  const ref = useRef(null);
  const videoRef = useRef(null);
  const near = useInView(ref, {rootMargin: '600px 0px', once: true});
  const visible = useInView(ref);
  // Unknown until hydration: the static HTML shows only the image, so reduced motion never fetches a clip.
  const [motion, setMotion] = useState(false);
  useEffect(() => setMotion(!prefersReducedMotion()), []);
  const light = useColorMode().colorMode === 'light';
  const clip = motion ? (light ? sample.lightClip : sample.clip) : undefined;
  // The image stays until the clip's first frame, which is the image's own frame.
  const [playing, setPlaying] = useState(false);
  useEffect(() => {
    const video = videoRef.current;
    if (!video || !near) return;
    // React does not reliably apply `muted` after hydration, and browsers only autoplay muted video.
    video.muted = true;
    if (visible) video.play().catch(() => {});
    else video.pause();
  }, [near, visible, clip]);
  return (
    <div ref={ref} className={`${styles.preview}${sample.clip ? ` ${styles.scene}` : ''}`}>
      <ThemedImage sources={{dark: sample.image, light: sample.lightImage}} alt={alt} width="1440" height="810" loading={eager ? 'eager' : 'lazy'} />
      {clip && (
        <div className={`${styles.clip}${playing ? ` ${styles.playing}` : ''}`}>
          <video ref={videoRef} src={near ? clip : undefined} muted loop playsInline preload="auto" aria-hidden="true"
            onPlaying={() => setPlaying(true)} onEmptied={() => setPlaying(false)} />
        </div>
      )}
    </div>
  );
}

function Card({sample, index, ru}) {
  const [alt, description] = sample[ru ? 'ru' : 'en'];
  return (
    <Link to={`/tutorials/${sample.id}`} className={styles.card}>
      <Preview sample={sample} alt={alt} eager={index < 2} />
      <div className={styles.content}>
        <p className={styles.feature}>{sample.feature}</p>
        <p>{description}</p>
      </div>
    </Link>
  );
}

export default function SamplesGallery() {
  const { i18n } = useDocusaurusContext();
  const ru = i18n.currentLocale === 'ru';
  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1>{ru ? 'Примеры' : 'Samples'}</h1>
        <p>{ru
          ? 'Небольшие сцены и окна редактора, на которых каждую возможность пакета можно попробовать руками.'
          : 'Small scenes and editor windows where you can try each feature of the package hands-on.'}</p>
      </header>
      <div className={styles.grid}>
        {samples.map((sample, index) => <Card key={sample.id} sample={sample} index={index} ru={ru} />)}
      </div>
    </div>
  );
}
