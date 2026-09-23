import {useEffect, useState} from 'react';

export const prefersReducedMotion = () =>
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

/** True while `ref` is on screen; animations use it to sleep when scrolled away. */
export function useInView(ref, {rootMargin = '0px', once = false} = {}) {
  const [inView, setInView] = useState(false);
  useEffect(() => {
    const element = ref.current;
    if (!element || !('IntersectionObserver' in window)) {
      setInView(true);
      return undefined;
    }
    const observer = new IntersectionObserver(([entry]) => {
      setInView(entry.isIntersecting);
      if (once && entry.isIntersecting) observer.disconnect();
    }, {rootMargin});
    observer.observe(element);
    return () => observer.disconnect();
  }, [ref, rootMargin, once]);
  return inView;
}

/**
 * Steps through `count` frames every `interval` ms while `active`; returns the current frame.
 * Reduced motion shows the `still` frame (the last one by default) without stepping.
 */
export function useLoop(count, interval, active, still = count - 1) {
  const [frame, setFrame] = useState(0);
  useEffect(() => {
    if (prefersReducedMotion()) {
      setFrame(still);
      return undefined;
    }
    if (!active) return undefined;
    const timer = setInterval(() => setFrame((value) => (value + 1) % count), interval);
    return () => clearInterval(timer);
  }, [count, interval, active, still]);
  return frame;
}
