import React from 'react';
import ThemedImage from '@theme/ThemedImage';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import types from '@site/static/img/samples/types.gif';
import weapons from '@site/static/img/samples/serialize-references.gif';
import surfaces from '@site/static/img/samples/enum-values.gif';
import flock from '@site/static/img/samples/profiler-markers.gif';
import abilities from '@site/static/img/samples/ability-catalog.png';
import typesLight from '@site/static/img/samples/types-light.gif';
import weaponsLight from '@site/static/img/samples/serialize-references-light.gif';
import surfacesLight from '@site/static/img/samples/enum-values-light.gif';
import flockLight from '@site/static/img/samples/profiler-markers-light.gif';
import abilitiesLight from '@site/static/img/samples/ability-catalog-light.png';
import styles from './styles.module.css';

const samples = [
  { id: 'enum-values', feature: 'EnumValues', image: surfaces, lightImage: surfacesLight, scene: true,
    en: ['EnumValues sample scene', "A walker crosses surface tiles, and each surface's color, trail and speed come from tables in the Inspector."],
    ru: ['Сцена примера EnumValues', 'Персонаж идёт по плиткам, а цвет, след и скорость на каждой поверхности берутся из таблиц в инспекторе.'] },
  { id: 'types', feature: 'Types', image: types, lightImage: typesLight, scene: true,
    en: ['Types sample scene', 'A spawner whose enemy type and wave pattern are picked in the Inspector, without touching code.'],
    ru: ['Сцена примера Types', 'Спавнер, в котором тип врага и схема волны выбираются в инспекторе, без правки кода.'] },
  { id: 'serialize-references', feature: 'SerializeReferences', image: weapons, lightImage: weaponsLight, scene: true,
    en: ['SerializeReferences sample scene', 'A turret whose weapons and their effects are picked in the Inspector, plus assets broken on purpose for you to repair.'],
    ru: ['Сцена примера SerializeReferences', 'Турель, у которой оружие и его эффекты выбираются в инспекторе, и нарочно сломанные ассеты, чтобы их починить.'] },
  { id: 'editor-tools', feature: 'EditorTools', image: abilities, lightImage: abilitiesLight,
    en: ['EditorTools sample window', "An editor window and an Inspector built in code with the package's helpers."],
    ru: ['Окно примера EditorTools', 'Окно редактора и инспектор, собранные в коде на хелперах пакета.'] },
  { id: 'profiler-markers', feature: 'ProfilerMarkers', image: flock, lightImage: flockLight, scene: true,
    en: ['ProfilerMarkers sample scene', 'A flock of cubes whose every frame phase shows up in the Profiler under its own name.'],
    ru: ['Сцена примера ProfilerMarkers', 'Стая кубов, у которой каждая фаза кадра видна в Profiler под своим именем.'] },
];

function Card({sample, index, ru}) {
  const [alt, description] = sample[ru ? 'ru' : 'en'];
  return (
    <Link to={`/tutorials/${sample.id}`} className={styles.card}>
      <div className={`${styles.preview}${sample.scene ? ` ${styles.scene}` : ''}`}>
        <ThemedImage sources={{dark: sample.image, light: sample.lightImage}} alt={alt} width="1440" height="810" loading={index < 2 ? 'eager' : 'lazy'} />
      </div>
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
