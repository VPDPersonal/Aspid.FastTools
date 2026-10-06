// Records the introduction's animated feature previews (Website/src/components/FeaturePreview) and the install panel's
// Package Manager walk-through (Website/src/components/InstallPanel) from a built site into looping WebPs for the GitHub
// README: docs/images/readme-previews/<name>[-light].webp. Headless Chrome runs on
// virtual time, so every frame lands on its exact timestamp however slow the capture is.
//   Website/scripts/serve-all.sh
//   node docs/media/readme-previews/record.mjs http://localhost:<port>/Aspid.FastTools/ [name …]
// Needs Node 22+ (global WebSocket), Google Chrome (CHROME overrides the macOS path) and img2webp (brew install webp).
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {spawn, execFileSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';

const CHROME = process.env.CHROME ?? '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
const OUT = fileURLToPath(new URL('../../images/readme-previews/', import.meta.url));
// One loop of each preview, from FeaturePreview: `useLoop(count, interval)` steps, or the Profiler's 22 s ticker.
// `fps` divides the loop into whole frames; the Profiler's timeline changes every frame, so it takes fewer.
// A feature preview is found by its card's doc page; `box` finds any other preview. The loop starts when the preview
// scrolls into view, and `warmup` passes before the first frame: one loop by default, which settles transitions.
// Lossy WebP leaves ghosts of earlier frames around the patches it updates on these flat interfaces, so a preview is
// lossless; `encode` overrides that: the Profiler's timeline scrolls every frame, and only mixed frames keep it small.
const FPS = 20;
const ENCODE = ['-lossless', '-m', '6'];
const PREVIEWS = {
  // InstallPanel: the sum of DURATIONS. It plays PLAYS (2) times, so a short warm-up records the first play, which runs
  // into the second one's first frame.
  'install': {loop: 1300 + 1400 + 1800 + 1100 + 2600, warmup: 100,
    box: `document.querySelector('section[class*="install_"] [class*="preview_"]')`},
  'profiler-markers': {loop: 22000, fps: 12.5, encode: ['-mixed', '-q', '90', '-m', '6']},
  'visual-element-extensions': {loop: 6 * 1100},
  'serialized-property-extensions': {loop: 5 * 1200},
  'editor-helpers': {loop: 5 * 950},
  'agent-skills': {loop: 6 * 1300},
};
// The preview box takes 56% of the card; this viewport makes it 480×360 CSS px, recorded at 2x.
const VIEWPORT = {width: 1180, height: 1000, deviceScaleFactor: 2};

const [base, ...only] = process.argv.slice(2);
if (!base) {
  console.error('usage: node docs/media/readme-previews/record.mjs <site base URL> [doc …]');
  process.exit(1);
}
const unknown = only.filter((doc) => !(doc in PREVIEWS));
if (unknown.length) {
  console.error(`No loop length for ${unknown.join(', ')}; known previews: ${Object.keys(PREVIEWS).join(', ')}`);
  process.exit(1);
}
if (typeof WebSocket === 'undefined') {
  console.error(`Node ${process.versions.node} has no global WebSocket; run the script with Node 22 or newer.`);
  process.exit(1);
}
const docs = only.length ? only : Object.keys(PREVIEWS);

const profile = fs.mkdtempSync(path.join(os.tmpdir(), 'readme-previews-'));
const chrome = spawn(CHROME, ['--headless=new', '--remote-debugging-port=0', `--user-data-dir=${profile}`,
  '--hide-scrollbars', '--mute-audio', '--no-first-run', '--force-color-profile=srgb', 'about:blank'], {stdio: ['ignore', 'ignore', 'pipe']});
const endpoint = await new Promise((resolve, reject) => {
  let log = '';
  chrome.stderr.on('data', (chunk) => {
    log += chunk;
    const match = /DevTools listening on (ws:\/\/\S+)/.exec(log);
    if (match) resolve(match[1]);
  });
  chrome.on('exit', () => reject(new Error(`Chrome exited:\n${log}`)));
  chrome.on('error', (error) => reject(new Error(`Chrome did not start (${error.message}); set CHROME to its path.`)));
}).catch((error) => {
  fs.rmSync(profile, {recursive: true, force: true});
  console.error(error.message);
  process.exit(1);
});
// A stopped run takes its Chrome along.
for (const signal of ['SIGINT', 'SIGTERM']) process.on(signal, () => {
  chrome.kill();
  process.exit(1);
});

const socket = new WebSocket(endpoint);
await new Promise((resolve) => socket.addEventListener('open', resolve, {once: true}));
let nextId = 0;
const pending = new Map();
const listeners = new Set();
socket.addEventListener('message', ({data}) => {
  const message = JSON.parse(data);
  if (message.id !== undefined) {
    const {resolve, reject} = pending.get(message.id);
    pending.delete(message.id);
    if (message.error) reject(new Error(`${message.error.message} (${message.error.code})`));
    else resolve(message.result);
  } else {
    listeners.forEach((listener) => listener(message));
  }
});
const send = (method, params = {}, sessionId) => new Promise((resolve, reject) => {
  const id = ++nextId;
  pending.set(id, {resolve, reject});
  socket.send(JSON.stringify({id, method, params, sessionId}));
});
const once = (method, sessionId) => new Promise((resolve) => {
  const listener = (message) => {
    if (message.method === method && message.sessionId === sessionId) {
      listeners.delete(listener);
      resolve(message.params);
    }
  };
  listeners.add(listener);
});

/** Opens the English introduction in `theme`, with virtual time paused once it has loaded. */
async function open(theme) {
  const {targetId} = await send('Target.createTarget', {url: 'about:blank'});
  const {sessionId} = await send('Target.attachToTarget', {targetId, flatten: true});
  const page = (method, params) => send(method, params, sessionId);
  await page('Page.enable');
  await page('Emulation.setDeviceMetricsOverride', {...VIEWPORT, mobile: false});
  // The entrance has played, so the banner rests. The frame of a card or the install panel, its rounded corner and the
  // line between preview and text go: on GitHub the table cell is the frame.
  await page('Page.addScriptToEvaluateOnNewDocument', {source: `
    localStorage.setItem('theme', '${theme}');
    sessionStorage.setItem('aspid-intro-played', '1');
    addEventListener('DOMContentLoaded', () => document.head.insertAdjacentHTML('beforeend', '<style>'
      + '.feature-card { border: 0 !important; border-radius: 0 !important; }'
      + '.feature-card__preview { border-right: 0 !important; }'
      + '.feature-card__live [class*="inspector_"], .feature-card__live [class*="uiStage_"] { border-radius: 0 !important; }'
      + 'section[class*="install_"] { border: 0 !important; border-radius: 0 !important; }'
      + 'section[class*="install_"] [class*="preview_"] { border-right: 0 !important; }'
      + '</style>'));`});
  const loaded = once('Page.loadEventFired', sessionId);
  await page('Page.navigate', {url: new URL('docs', base).href});
  await loaded;
  await page('Runtime.evaluate', {expression: 'document.fonts.ready.then(() => true)', awaitPromise: true});
  await page('Emulation.setVirtualTimePolicy', {policy: 'pause'});
  return {page, sessionId, close: () => send('Target.closeTarget', {targetId})};
}

/** Lets `ms` of page time pass: timers, CSS animations and frames advance exactly that much. */
async function advance({page, sessionId}, ms) {
  const expired = once('Emulation.virtualTimeBudgetExpired', sessionId);
  await page('Emulation.setVirtualTimePolicy', {policy: 'advance', budget: ms});
  await expired;
}

async function record(tab, doc, file) {
  const card = `[...document.querySelectorAll('.feature-card__title a')].find((a) => a.pathname.endsWith('/${doc}'))
    ?.closest('.feature-card')?.querySelector('.feature-card__preview:has(.feature-card__live)')`;
  const {result} = await tab.page('Runtime.evaluate', {returnByValue: true, expression: `(() => {
    const box = ${PREVIEWS[doc].box ?? card};
    if (!box) return null;
    box.scrollIntoView({block: 'center'});
    // The screenshot clip is in document coordinates.
    const {x, y, width, height} = box.getBoundingClientRect();
    return {x: x + scrollX, y: y + scrollY, width, height};
  })()`});
  if (!result.value) throw new Error(`No animated preview for ${doc}`);
  const clip = {...result.value, scale: 1};
  const {loop, fps = FPS, warmup = loop + 500, encode = ENCODE} = PREVIEWS[doc];
  await advance(tab, warmup);
  const frames = fs.mkdtempSync(path.join(os.tmpdir(), `${doc}-`));
  const count = Math.round(loop * fps / 1000);
  for (let index = 0; index < count; index++) {
    const {data} = await tab.page('Page.captureScreenshot', {format: 'png', clip});
    fs.writeFileSync(path.join(frames, `${String(index).padStart(4, '0')}.png`), Buffer.from(data, 'base64'));
    await advance(tab, 1000 / fps);
  }
  const list = fs.readdirSync(frames).sort().map((name) => path.join(frames, name));
  execFileSync('img2webp', ['-loop', '0', ...encode, '-d', String(1000 / fps), ...list, '-o', file], {stdio: 'ignore'});
  fs.rmSync(frames, {recursive: true});
  console.log(`${path.relative(process.cwd(), file)}: ${count} frames, ${(fs.statSync(file).size / 1024).toFixed(0)} KB`);
}

try {
  fs.mkdirSync(OUT, {recursive: true});
  for (const theme of ['dark', 'light']) {
    // A fresh page per preview: each one's loop starts on its own scroll into view.
    for (const doc of docs) {
      const tab = await open(theme);
      await record(tab, doc, path.join(OUT, `${doc}${theme === 'light' ? '-light' : ''}.webp`));
      await tab.close();
    }
  }
} finally {
  socket.close();
  const exited = new Promise((resolve) => chrome.once('exit', resolve));
  chrome.kill();
  await exited;
  fs.rmSync(profile, {recursive: true, force: true});
}
