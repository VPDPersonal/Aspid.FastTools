import React from 'react';
import cup from './kofi-cup.png';
import styles from './styles.module.css';

/** Show the introduction's localized support copy and actions. */
export default function DonatePanel({kofi, kofiLabel, store, storeLabel, children}) {
  return (
    <section className={styles.donate}>
      <p className={styles.copy}>{children}</p>
      <div className={styles.actions}>
        {kofi && (
          <a href={kofi} target="_blank" rel="noopener noreferrer">
            <img className={styles.icon} src={cup} alt="" />
            {kofiLabel}
          </a>
        )}
        {store && (
          <a className={styles.store} href={store} target="_blank" rel="noopener noreferrer">
            <svg className={styles.icon} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" aria-hidden="true">
              <path d="m12 2 9 5v10l-9 5-9-5V7z M3 7l9 5 9-5 M12 12v10" />
            </svg>
            {storeLabel}
          </a>
        )}
      </div>
    </section>
  );
}
