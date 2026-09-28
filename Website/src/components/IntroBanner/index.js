import React, {useEffect, useRef, useState} from 'react';
import {prefersReducedMotion, useInView} from '../FeaturePreview/effects';
import banner from './media/banner.mp4';
import poster from './media/banner-poster.jpg';

/**
 * The README banner on the introduction. GitHub needs the 6 MB GIF (`docs/images/aspid_fasttools_readme_banner.gif`);
 * the site plays the same clip as a small video, re-encoded from that GIF, only while it is on screen, and shows its first
 * frame under reduced motion.
 */
export default function IntroBanner({alt}) {
  const ref = useRef(null);
  const visible = useInView(ref);
  // Unknown until hydration: the static HTML loads only the poster, so reduced motion never fetches the clip.
  const [still, setStill] = useState(null);
  useEffect(() => setStill(prefersReducedMotion()), []);
  useEffect(() => {
    const video = ref.current;
    if (!video || still !== false) return;
    // React does not reliably apply `muted` after hydration, and browsers only autoplay muted video.
    video.muted = true;
    if (visible) video.play().catch(() => {});
    else video.pause();
  }, [visible, still]);
  return (
    <video ref={ref} className="readme-banner" src={banner} poster={poster} width={1280} height={384}
      muted loop playsInline preload={still === false ? 'auto' : 'none'} role="img" aria-label={alt} />
  );
}
