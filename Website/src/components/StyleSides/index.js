import React, {useRef, useState} from 'react';
import clsx from 'clsx';
import InlineCode from '@site/src/components/InlineCode';
import styles from './styles.module.css';

/*
 * The "Call | Sets" table of the VisualElement Extensions page drawn as an element: pick a call and the sides, corners,
 * sizes or offset it writes light up. Markdown keeps the table; `src/remark/liveDiagrams.js` passes its rows here as
 * `StyleSidesRow` children, so the calls and their descriptions stay in the page and in each translation.
 * The drawing is read from the call itself (`SetPaddingX(8)`, `SetBorderRadiusTop(6)`, `SetMinSize(24, 16)`, `SetTop(8)`).
 */

const SIDES = ['top', 'right', 'bottom', 'left'];
const CORNERS = {Top: ['tl', 'tr'], Bottom: ['bl', 'br'], Left: ['tl', 'bl'], Right: ['tr', 'br']};

/** What a call writes: padding per side, rounded corners, size labels or an offset from the parent. */
export function parseCall(call) {
  const match = call.match(/^(\w+)\((.*)\)$/);
  if (!match) return {};
  const [, name, list] = match;
  const args = list.split(',').map((arg) => arg.trim()).filter(Boolean);
  const named = Object.fromEntries(args.filter((arg) => arg.includes(':')).map((arg) => arg.split(':').map((part) => part.trim())));
  const [value, second] = args.filter((arg) => !arg.includes(':'));
  let part;
  if ((part = name.match(/^SetPadding(X|Y|Top|Right|Bottom|Left)?$/))) {
    const sides = {X: ['left', 'right'], Y: ['top', 'bottom']}[part[1]] ?? (part[1] ? [part[1].toLowerCase()] : value ? SIDES : []);
    return {padding: value ? Object.fromEntries(sides.map((side) => [side, value])) : named};
  }
  if ((part = name.match(/^SetBorderRadius(Top|Bottom)?(Left|Right)?$/))) {
    const [, vertical, horizontal] = part;
    if (vertical && horizontal) return {corners: [`${vertical[0]}${horizontal[0]}`.toLowerCase()]};
    return {corners: CORNERS[vertical ?? horizontal] ?? ['tl', 'tr', 'br', 'bl']};
  }
  if ((part = name.match(/^Set(Top|Right|Bottom|Left)$/))) return {offset: {side: part[1].toLowerCase(), value}};
  if ((part = name.match(/^Set(Min|Max)?Size$/))) {
    const prefix = part[1]?.toLowerCase();
    const label = (axis) => (prefix ? `${prefix}${axis}` : axis.toLowerCase());
    return {size: [`${label('Width')} ${value}`, `${label('Height')} ${second ?? value}`]};
  }
  return {};
}

// Element, parent and the gaps the offset arrows cross, in viewBox units.
const E = {x0: 70, y0: 58, x1: 250, y1: 188};
const P = {x0: 12, y0: 10, x1: 308, y1: 234};
const R = 14;
const PAD = 28;

function edgePath(corners) {
  const r = (corner) => (corners.includes(corner) ? R : 0);
  const [tl, tr, br, bl] = ['tl', 'tr', 'br', 'bl'].map(r);
  const arc = (radius, x, y) => (radius ? `A${radius} ${radius} 0 0 1 ${x} ${y}` : '');
  return `M${E.x0 + tl} ${E.y0} H${E.x1 - tr} ${arc(tr, E.x1, E.y0 + tr)} V${E.y1 - br} ${arc(br, E.x1 - br, E.y1)}`
    + ` H${E.x0 + bl} ${arc(bl, E.x0, E.y1 - bl)} V${E.y0 + tl} ${arc(tl, E.x0 + tl, E.y0)} Z`;
}

const CORNER_PATHS = {
  tl: `M${E.x0} ${E.y0 + R} A${R} ${R} 0 0 1 ${E.x0 + R} ${E.y0}`,
  tr: `M${E.x1 - R} ${E.y0} A${R} ${R} 0 0 1 ${E.x1} ${E.y0 + R}`,
  br: `M${E.x1} ${E.y1 - R} A${R} ${R} 0 0 1 ${E.x1 - R} ${E.y1}`,
  bl: `M${E.x0 + R} ${E.y1} A${R} ${R} 0 0 1 ${E.x0} ${E.y1 - R}`,
};

const BANDS = {
  top: {x: E.x0, y: E.y0, width: E.x1 - E.x0, height: PAD, tx: 160, ty: E.y0 + PAD / 2},
  bottom: {x: E.x0, y: E.y1 - PAD, width: E.x1 - E.x0, height: PAD, tx: 160, ty: E.y1 - PAD / 2},
  left: {x: E.x0, y: E.y0 + PAD, width: PAD + 4, height: E.y1 - E.y0 - 2 * PAD, tx: E.x0 + (PAD + 4) / 2, ty: 123},
  right: {x: E.x1 - PAD - 4, y: E.y0 + PAD, width: PAD + 4, height: E.y1 - E.y0 - 2 * PAD, tx: E.x1 - (PAD + 4) / 2, ty: 123},
};

// From the parent's edge to the element's, the arrowhead on the element.
const OFFSETS = {
  top: {line: [160, P.y0 + 2, 160, E.y0 - 2], head: `M156 ${E.y0 - 7} L160 ${E.y0 - 1} L164 ${E.y0 - 7}`, tx: 168, ty: 38, anchor: 'start'},
  bottom: {line: [160, P.y1 - 2, 160, E.y1 + 2], head: `M156 ${E.y1 + 7} L160 ${E.y1 + 1} L164 ${E.y1 + 7}`, tx: 168, ty: 216, anchor: 'start'},
  left: {line: [P.x0 + 2, 123, E.x0 - 2, 123], head: `M${E.x0 - 7} 119 L${E.x0 - 1} 123 L${E.x0 - 7} 127`, tx: 41, ty: 114, anchor: 'middle'},
  right: {line: [P.x1 - 2, 123, E.x1 + 2, 123], head: `M${E.x1 + 7} 119 L${E.x1 + 1} 123 L${E.x1 + 7} 127`, tx: 279, ty: 114, anchor: 'middle'},
};

function Diagram({spec}) {
  const padding = spec.padding;
  const corners = spec.corners ?? [];
  const offset = spec.offset && OFFSETS[spec.offset.side];
  return (
    <svg className={styles.diagram} viewBox="0 0 320 244" aria-hidden="true">
      <rect className={clsx(styles.parent, offset && styles.on)} x={P.x0} y={P.y0} width={P.x1 - P.x0} height={P.y1 - P.y0} rx="6" />
      <text className={clsx(styles.label, offset && styles.shown)} x={P.x0 + 8} y={P.y0 + 16}>parent</text>
      {SIDES.map((side) => {
        const band = BANDS[side];
        const set = padding && side in padding;
        return (
          <g key={side}>
            <rect className={clsx(styles.band, set && styles.on)} x={band.x} y={band.y} width={band.width} height={band.height} />
            <text className={clsx(styles.value, padding && styles.shown, set && styles.on)} x={band.tx} y={band.ty} textAnchor="middle" dominantBaseline="central">
              {set ? padding[side] : '—'}
            </text>
          </g>
        );
      })}
      <rect className={styles.content} x={E.x0 + PAD + 4} y={E.y0 + PAD} width={E.x1 - E.x0 - 2 * (PAD + 4)} height={E.y1 - E.y0 - 2 * PAD} />
      <text className={styles.label} x="160" y="123" textAnchor="middle" dominantBaseline="central">content</text>
      <path className={styles.edge} d={edgePath(corners)} />
      {Object.entries(CORNER_PATHS).map(([corner, d]) => (
        <path key={corner} className={clsx(styles.corner, corners.includes(corner) && styles.on)} d={d} />
      ))}
      <path className={clsx(styles.dim, spec.size && styles.on)} d={`M${E.x0} 208 H${E.x1} M${E.x0} 203 V213 M${E.x1} 203 V213`} />
      <text className={clsx(styles.value, spec.size && styles.shown, styles.on)} x="160" y="226" textAnchor="middle">{spec.size?.[0]}</text>
      <path className={clsx(styles.dim, spec.size && styles.on)} d={`M270 ${E.y0} V${E.y1} M265 ${E.y0} H275 M265 ${E.y1} H275`} />
      <text className={clsx(styles.value, spec.size && styles.shown, styles.on)} x="286" y="123" textAnchor="middle" transform="rotate(90 286 123)">{spec.size?.[1]}</text>
      {offset && (
        <g>
          <line className={clsx(styles.dim, styles.on)} x1={offset.line[0]} y1={offset.line[1]} x2={offset.line[2]} y2={offset.line[3]} />
          <path className={clsx(styles.dim, styles.on)} d={offset.head} />
          <text className={clsx(styles.value, styles.shown, styles.on)} x={offset.tx} y={offset.ty} textAnchor={offset.anchor}>
            {`${spec.offset.side} ${spec.offset.value}`}
          </text>
        </g>
      )}
    </svg>
  );
}

/** One table row, read by `StyleSides`; it never renders on its own. */
export function StyleSidesRow() {
  return null;
}

export default function StyleSides({alt, children}) {
  const rows = React.Children.toArray(children)
    .filter((child) => typeof child?.props?.call === 'string')
    .map((child) => ({call: child.props.call, sets: child.props.children, spec: parseCall(child.props.call)}));
  const [selected, setSelected] = useState(0);
  const buttons = useRef([]);
  const row = rows[selected];
  if (!row) return null;

  function move(index) {
    setSelected(index);
    buttons.current[index]?.focus();
  }

  function onKeyDown(event) {
    const last = rows.length - 1;
    const next = {ArrowDown: selected + 1, ArrowUp: selected - 1, Home: 0, End: last}[event.key];
    if (next === undefined) return;
    event.preventDefault();
    move((next + rows.length) % rows.length);
  }

  return (
    <div className={styles.panel}>
      <div className={styles.window}>
        <div className={styles.side}>
          <ul className={styles.calls} role="listbox" aria-label={alt} onKeyDown={onKeyDown}>
            {rows.map(({call}, index) => (
              <li key={call} role="presentation">
                <button
                  type="button"
                  ref={(element) => { buttons.current[index] = element; }}
                  className={styles.call}
                  role="option"
                  aria-selected={index === selected}
                  tabIndex={index === selected ? 0 : -1}
                  onClick={() => setSelected(index)}>
                  <InlineCode code={call} language="csharp" />
                </button>
              </li>
            ))}
          </ul>
        </div>
        <Diagram spec={row.spec} />
        <div className={styles.sets} aria-live="polite">{row.sets}</div>
      </div>
    </div>
  );
}
