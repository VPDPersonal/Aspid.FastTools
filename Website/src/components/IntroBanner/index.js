import React, {useEffect, useRef} from 'react';
import {sendWave} from '../DotRipple';
import {INTRO_KEY} from '../../intro';
import './styles.css';

const WAVE_POWER = 2; // the wave the closed ring sends out: as strong as a fully charged click

/**
 * The README banner on the introduction. GitHub shows the PNG (`docs/images/aspid_fasttools_readme_banner.png`); the site
 * rebuilds it from the logo and live text, without a background: on wide screens the article panel leaves the banner open
 * (`custom.css`), so the page's dot canvas, its light and ripples included, shows through. Narrower, the banner paints the
 * dots itself, and `DotAmbient` draws its glow and sparks in the `__ambient` layer above them. The page hides its title,
 * so the name is the page's only `<h1>`; the banner must stay the article's first child.
 *
 * Once per tab session it makes an entrance (`src/intro.js` arms it before the first paint, `styles.css` animates it):
 * the snake's ring draws itself from the head and closes, a wave runs from it through the dots and shakes the page as a
 * burst does, the name follows the
 * wave and the banner's glow rises.
 */
export default function IntroBanner({alt}) {
  const ref = useRef(null);
  const brand = alt.indexOf('.') + 1;

  useEffect(() => {
    const root = document.documentElement;
    const banner = ref.current;
    if (!root.hasAttribute('data-intro') || !banner) return undefined;
    try {
      sessionStorage.setItem(INTRO_KEY, '1');
    } catch {
      // Storage may be blocked; the entrance then plays on every visit.
    }

    // The CSS animations may have started before hydration, so the wave waits for what is left of the ring's: the logo's
    // only animation (the build renames keyframes, so it cannot be told by name).
    const logo = banner.querySelector('.readme-banner__logo');
    const [ring] = logo.getAnimations();
    const close = () => {
      banner.dataset.glowAt = String(performance.now());
      // Below 997px the wave runs through the banner's own dots only.
      const box = logo.getBoundingClientRect();
      sendWave(box.left + box.width / 2, box.top + box.height / 2, WAVE_POWER, {shake: true});
    };
    let timer = 0;
    if (ring && ring.playState !== 'finished') timer = setTimeout(close, ring.effect.getComputedTiming().endTime - (ring.currentTime ?? 0));
    else banner.dataset.glowAt = String(performance.now());

    // Animations removed with the banner reject, so leaving the page early ends the entrance too.
    Promise.allSettled(banner.getAnimations({subtree: true}).map((animation) => animation.finished))
      .then(() => root.removeAttribute('data-intro'));
    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="readme-banner" ref={ref}>
      <div className="readme-banner__ambient" aria-hidden="true" />
      <div className="readme-banner__content">
        <div className="readme-banner__mark" aria-hidden="true">
          <div className="readme-banner__logo" />
        </div>
        <div className="readme-banner__text">
          <h1 className="readme-banner__title"><span>{alt.slice(0, brand)}</span>{alt.slice(brand)}</h1>
          {/* The brand line: English in every locale, as on the README banner. */}
          <p className="readme-banner__tagline">Boilerplate Killer for Unity</p>
        </div>
      </div>
    </div>
  );
}
