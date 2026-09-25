import React, {useEffect, useRef, useState} from 'react';
import clsx from 'clsx';
import {translate} from '@docusaurus/Translate';
import {useDoc} from '@docusaurus/plugin-content-docs/client';
import TOCItems from '@theme/TOCItems';

/**
 * Laptop-width table of contents (997–1400px, see custom.css): a pinned button in the lower right corner of the
 * viewport opens a popover with the same outline as the desktop column. Picking a section, Esc or a click
 * outside closes it.
 */
export default function FloatingToc() {
  const {toc, frontMatter} = useDoc();
  const [open, setOpen] = useState(false);
  const root = useRef(null);
  const label = translate({id: 'theme.TOCCollapsible.toggleButtonLabel', message: 'On this page'});

  useEffect(() => {
    if (!open) return undefined;
    const onKey = (event) => { if (event.key === 'Escape') setOpen(false); };
    const onPointer = (event) => { if (!root.current?.contains(event.target)) setOpen(false); };
    document.addEventListener('keydown', onKey);
    document.addEventListener('pointerdown', onPointer);
    return () => {
      document.removeEventListener('keydown', onKey);
      document.removeEventListener('pointerdown', onPointer);
    };
  }, [open]);

  if (frontMatter.hide_table_of_contents || toc.length === 0) return null;
  return (
    <div ref={root} className={clsx('floating-toc', open && 'floating-toc--open')}>
      <div
        id="floating-toc-panel"
        className="floating-toc__panel"
        hidden={!open}
        onClick={(event) => { if (event.target.closest('a')) setOpen(false); }}>
        <div className="floating-toc__title">{label}</div>
        <TOCItems
          toc={toc}
          minHeadingLevel={frontMatter.toc_min_heading_level}
          maxHeadingLevel={frontMatter.toc_max_heading_level}
          className="table-of-contents"
          linkClassName="table-of-contents__link"
          linkActiveClassName="table-of-contents__link--active"
        />
      </div>
      <button
        type="button"
        className="clean-btn floating-toc__button"
        aria-label={label}
        title={label}
        aria-expanded={open}
        aria-controls="floating-toc-panel"
        onClick={() => setOpen((value) => !value)}>
        <svg width="18" height="18" viewBox="0 0 18 18" fill="none" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" aria-hidden="true">
          <path d="M3 4.5h12M3 9h12M3 13.5h8" />
        </svg>
      </button>
    </div>
  );
}
