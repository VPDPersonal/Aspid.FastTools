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
            {/* Unity's symbol from the Asset Store header: https://cdn.sanity.io/images/fuvbjjlp/asset-store-production/fb8b7bce7aa89a11fd47c338b277680bbcaefbae-152x21.svg */}
            <svg className={styles.icon} viewBox="0 0 13.02 15" fill="#fff" aria-hidden="true">
              <path fillRule="evenodd" d="M9.40562 4.16996L7.07863 2.81674V0.167647L13.0182 3.62102V10.5278L10.74 9.20322V6.49815C10.7427 6.39847 10.6274 6.33429 10.5447 6.38481L7.77835 7.99338C7.70106 8.03844 7.64817 8.12311 7.64817 8.22006V11.4372C7.64546 11.5355 7.76072 11.6011 7.84344 11.5505L10.1704 10.1973L12.4486 11.5219L6.50908 14.9752L0.569544 11.5219L2.84772 10.1973L5.17607 11.5505C5.25879 11.6011 5.3727 11.5328 5.36999 11.4358V8.22006C5.36999 8.12857 5.32389 8.04117 5.23981 7.99202L2.47345 6.38345C2.38937 6.33156 2.27682 6.39983 2.27818 6.49678V9.20322L0 10.5278V3.62102L5.93954 0.167647V2.81674L3.61254 4.16996C3.52575 4.21638 3.52846 4.3502 3.61254 4.39663L6.3789 6.0052C6.45619 6.05026 6.55519 6.05436 6.63926 6.0052L9.40562 4.39663C9.49241 4.3502 9.4897 4.21638 9.40562 4.16996Z" />
            </svg>
            {storeLabel}
          </a>
        )}
      </div>
    </section>
  );
}
