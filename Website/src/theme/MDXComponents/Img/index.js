import React, {useEffect, useRef, useState} from 'react';
import {createPortal} from 'react-dom';
import OriginalImg from '@theme-original/MDXComponents/Img';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';

export default function DocImage(props) {
  const ru = useDocusaurusContext().i18n.currentLocale === 'ru';
  const [preview, setPreview] = useState(null);
  const dialog = useRef(null);
  const opener = useRef(null);
  useEffect(() => {
    if (!preview) return undefined;
    const overflow = document.body.style.overflow;
    dialog.current.showModal();
    document.body.style.overflow = 'hidden';
    return () => {
      document.body.style.overflow = overflow;
      opener.current?.focus();
    };
  }, [preview]);
  function show(event) {
    if (event.currentTarget.closest('a')) return;
    opener.current = event.currentTarget;
    setPreview({src: event.currentTarget.currentSrc, scene: !!event.currentTarget.closest('.sample-scene')});
  }
  return <>
    <OriginalImg {...props} className={`doc-zoom-image ${props.className || ''}`} role="button" tabIndex={0}
      aria-label={`${ru ? 'Увеличить изображение' : 'Enlarge image'}: ${props.alt || ''}`}
      aria-haspopup="dialog" onClick={show} onKeyDown={(event) => {
        if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); show(event); }
      }} />
    {preview && createPortal(<dialog ref={dialog} className="doc-image-dialog" aria-label={props.alt || (ru ? 'Просмотр изображения' : 'Image preview')}
      onCancel={() => setPreview(null)} onClick={() => setPreview(null)}>
      <button type="button" className="doc-image-close" onClick={() => setPreview(null)} autoFocus>{ru ? 'Закрыть' : 'Close'} <kbd>Esc</kbd></button>
      <div className={preview.scene ? 'sample-scene' : undefined}><img src={preview.src} alt={props.alt || ''} /></div>
      {props.alt && <p>{props.alt}</p>}
    </dialog>, document.body)}
  </>;
}
