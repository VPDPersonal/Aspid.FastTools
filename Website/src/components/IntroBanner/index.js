import React, {useEffect, useRef} from 'react';
import {prefersReducedMotion} from '../FeaturePreview/effects';
import banner from './media/banner.mp4';
import poster from './media/banner-poster.jpg';

/**
 * The README banner on the introduction. GitHub needs the 6 MB GIF (`docs/images/aspid_fasttools_readme_banner.gif`);
 * the site plays the same clip as a small video, re-encoded from that GIF, and shows its first frame under reduced motion.
 */
export default function IntroBanner({alt}) {
  const ref = useRef(null);
  useEffect(() => {
    const video = ref.current;
    if (!video || prefersReducedMotion()) return;
    // React does not reliably apply `muted` after hydration, and browsers only autoplay muted video.
    video.muted = true;
    video.play().catch(() => {});
  }, []);
  return (
    <video ref={ref} className="readme-banner" src={banner} poster={poster} width={1280} height={384}
      muted loop playsInline preload="auto" role="img" aria-label={alt} />
  );
}
