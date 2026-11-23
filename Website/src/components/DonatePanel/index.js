import React from 'react';
import styles from './styles.module.css';

const supportMedia = 'https://vpdpersonal.github.io/VPDPersonal/support';

/** Show the introduction's localized support copy and actions. */
export default function DonatePanel({kofi, kofiLabel, store, storeLabel, children}) {
  return (
    <section className={styles.donate}>
      <p className={styles.copy}>{children}</p>
      <div className={styles.actions}>
        {kofi && (
          <a href={kofi} target="_blank" rel="noopener noreferrer">
            <img className={styles.icon} src={`${supportMedia}/kofi-cup.png`} alt="" />
            {kofiLabel}
          </a>
        )}
        {store && (
          <a className={styles.store} href={store} target="_blank" rel="noopener noreferrer">
            <img className={styles.icon} src={`${supportMedia}/unity-mark.svg`} alt="" />
            {storeLabel}
          </a>
        )}
      </div>
    </section>
  );
}
