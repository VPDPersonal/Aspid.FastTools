import React from 'react';
import DotRipple from '../../components/DotRipple';

// Match only the flat camera background, allowing one channel step for GIF quantization.
const channelMask = (channel, tolerance = 1) => Array.from({length: 256}, (_, value) =>
  Math.abs(value - channel) <= tolerance ? 1 : 0).join(' ');

function SceneBackgroundFilter({theme, color}) {
  return (
    <filter id={`sample-scene-background-${theme}`} x="0" y="0" width="100%" height="100%" colorInterpolationFilters="sRGB">
      <feComponentTransfer in="SourceGraphic" result="channels">
        <feFuncR type="discrete" tableValues={channelMask(color[0])} />
        <feFuncG type="discrete" tableValues={channelMask(color[1])} />
        <feFuncB type="discrete" tableValues={channelMask(color[2])} />
      </feComponentTransfer>
      <feColorMatrix in="channels" type="matrix" values="0 0 0 0 0  0 0 0 0 0  0 0 0 0 0  1 1 1 0 -2" result="backgroundMask" />
      {/* Only near-background colors may join the fringe; preserve small agents and colored details. */}
      <feComponentTransfer in="SourceGraphic" result="fringeChannels">
        <feFuncR type="discrete" tableValues={channelMask(color[0], 24)} />
        <feFuncG type="discrete" tableValues={channelMask(color[1], 24)} />
        <feFuncB type="discrete" tableValues={channelMask(color[2], 24)} />
      </feComponentTransfer>
      <feColorMatrix in="fringeChannels" type="matrix" values="0 0 0 0 0  0 0 0 0 0  0 0 0 0 0  1 1 1 0 -2" result="fringeMask" />
      {/* Include the antialiased fringe, which still contains the old camera background. */}
      <feMorphology in="backgroundMask" operator="dilate" radius="1" result="expandedBackground" />
      <feGaussianBlur in="expandedBackground" stdDeviation="0.35" edgeMode="duplicate" result="softEdge" />
      <feComposite in="softEdge" in2="fringeMask" operator="in" result="softBackground" />
      <feFlood floodColor="var(--ifm-code-background)" result="surface" />
      <feComposite in="surface" in2="softBackground" operator="in" result="background" />
      <feComposite in="SourceGraphic" in2="softBackground" operator="out" result="scene" />
      {/* Add complementary masks without making the softened edge translucent. */}
      <feComposite in="background" in2="scene" operator="arithmetic" k2="1" k3="1" />
    </filter>
  );
}

export default function Root({children}) {
  return (
    <>
      <svg aria-hidden="true" width="0" height="0" style={{position: 'absolute', pointerEvents: 'none'}}>
        <defs>
          <SceneBackgroundFilter theme="light" color={[246, 241, 232]} />
          <SceneBackgroundFilter theme="dark" color={[6, 10, 15]} />
        </defs>
      </svg>
      <DotRipple />
      {children}
    </>
  );
}
