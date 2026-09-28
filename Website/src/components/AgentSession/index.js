import React from 'react';
import clsx from 'clsx';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import {AGENT_SCENES, PluginPreview} from '@site/src/components/FeaturePreview';
import previewStyles from '@site/src/components/FeaturePreview/styles.module.css';
import styles from './styles.module.css';

/*
 * The introduction's Agent Skills card at article width, one per skill section of the Agent Skills page: the request
 * waits in the prompt, Send loads the `skill` and lands its edit, then turns into Replay. Markdown keeps a static SVG per skill
 * (`agent-skills-<skill>.svg`); `src/remark/liveDiagrams.js` swaps it for this component on the site.
 */
export default function AgentSession({alt, skill}) {
  const {i18n} = useDocusaurusContext();
  const scene = AGENT_SCENES.find((item) => item.skill === skill) ?? AGENT_SCENES[0];
  return (
    <figure className={styles.panel} aria-label={alt}>
      <div className={clsx(previewStyles.featurePreview, previewStyles.framed, styles.window)}>
        <PluginPreview ru={i18n.currentLocale === 'ru'} scene={scene} lines={scene.code.split('\n').length} manual />
      </div>
    </figure>
  );
}
