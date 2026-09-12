import React, {useEffect, useId, useRef, useState} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import {useLocation} from '@docusaurus/router';
import styles from './styles.module.css';

/**
 * A click-toggled menu of links that closes on outside pointer, Escape and navigation.
 * `items` are `{key, label, to, active, ...linkProps}`; `up` opens the menu above the button (panel footer).
 */
export default function Dropdown({items, up, buttonClassName, children, ...buttonProps}) {
  const [open, setOpen] = useState(false);
  const root = useRef(null);
  const id = useId();
  const {pathname} = useLocation();

  useEffect(() => { setOpen(false); }, [pathname]);
  useEffect(() => {
    if (!open) return undefined;
    const onPointer = (event) => { if (!root.current?.contains(event.target)) setOpen(false); };
    const onKey = (event) => { if (event.key === 'Escape') setOpen(false); };
    document.addEventListener('pointerdown', onPointer);
    document.addEventListener('keydown', onKey);
    return () => { document.removeEventListener('pointerdown', onPointer); document.removeEventListener('keydown', onKey); };
  }, [open]);

  return (
    <div ref={root} className={styles.switcher}>
      <button type="button" {...buttonProps} className={buttonClassName} aria-haspopup="menu" aria-expanded={open} aria-controls={id} onClick={() => setOpen((value) => !value)}>
        {children}
      </button>
      <ul id={id} role="menu" className={clsx(styles.switcherMenu, up && styles.switcherMenuUp)} hidden={!open}>
        {items.map(({key, label, active, ...linkProps}) => (
          <li key={key} role="none">
            <Link role="menuitem" {...linkProps} className={clsx(styles.switcherItem, active && styles.switcherItemActive)} aria-current={active ? 'page' : undefined}>
              {label}
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
