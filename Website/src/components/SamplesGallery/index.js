import React from 'react';
import ThemedImage from '@theme/ThemedImage';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
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
import styles from './styles.module.css';

const samples = [
  { id: 'enum-values', feature: 'EnumValues', image: surfaces, lightImage: surfacesLight,
    en: ['Surface laboratory', 'Explore how surfaces change colors, trails and movement.'],
    ru: ['Лаборатория поверхностей', 'Исследуйте, как поверхности меняют цвет, следы и скорость движения.'] },
  { id: 'types', feature: 'Types', image: types, lightImage: typesLight,
    en: ['Spawn arena', 'Choose enemy behaviours and arrange the next wave.'],
    ru: ['Арена появления врагов', 'Выбирайте поведение врагов и схему появления следующей волны.'] },
  { id: 'serialize-references', feature: 'SerializeReferences', image: weapons, lightImage: weaponsLight,
    en: ['Weapon lab', 'Switch weapons, combine effects and see every hit.'],
    ru: ['Оружейная лаборатория', 'Меняйте оружие, сочетайте эффекты и наблюдайте результат каждого попадания.'] },
  { id: 'editor-tools', feature: 'EditorTools', image: abilities, lightImage: abilitiesLight,
    en: ['Ability catalog', 'Tune abilities in a custom editor with asset binding and Undo.'],
    ru: ['Каталог способностей', 'Настраивайте способности в редакторе с привязкой к ассетам и поддержкой Undo.'] },
  { id: 'profiler-markers', feature: 'ProfilerMarkers', image: flock, lightImage: flockLight,
    en: ['Flock observatory', 'Watch the simulation, then inspect its work in the Profiler.'],
    ru: ['Обсерватория стаи', 'Наблюдайте за симуляцией и изучайте её работу в Profiler.'] },
];

export default function SamplesGallery() {
  const { i18n } = useDocusaurusContext();
  const ru = i18n.currentLocale === 'ru';
  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <h1>{ru ? 'Примеры в действии' : 'Samples in action'}</h1>
      </header>
      <div className={styles.grid}>
        {samples.map((sample, index) => {
          const [name, description] = sample[ru ? 'ru' : 'en'];
          return (
            <Link key={sample.id} to={`/tutorials/${sample.id}`} className={styles.card}>
              <div className={`${styles.preview}${sample.id !== 'editor-tools' ? ' sample-scene' : ''}`}>
                <ThemedImage sources={{dark: sample.image, light: sample.lightImage}} alt={name} width="1440" height="810" loading={index > 1 ? 'lazy' : 'eager'} />
              </div>
              <div className={styles.content}>
                <p className={styles.feature}><span className={styles.index}>{String(index + 1).padStart(2, '0')} /</span> {sample.feature}</p>
                <p>{description}</p>
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
