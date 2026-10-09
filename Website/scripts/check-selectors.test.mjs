import fs from 'node:fs';
import assert from 'node:assert/strict';
import {test} from 'node:test';
import {definedClasses, fragmentsIn, hashedClasses, isSiteClass, matchingClasses} from './check-selectors.mjs';

test('fragments are read from CSS and from selectors in JS strings', () => {
  const css = ".navbar__items--right > [class*='colorModeToggle_'] { order: 3; }\na[class^=\"nav\"], [class*=card_] {}";
  const js = `const ARTICLE = '[class*="docMainContainer_"] > .container'; const name = 'theme-doc-markdown';`;
  assert.deepEqual([...fragmentsIn(css)].sort(), ['card_', 'colorModeToggle_', 'nav']);
  assert.deepEqual([...fragmentsIn(js)], ['docMainContainer_']);
});

test('only CSS-module classes with a four character hash count as built classes', () => {
  const css = '.docMainContainer_q81F{flex:1}.navbar__item,.footer__item{margin:0}.card__body.sidebar_label{x:0}'
    + '.buttonGroup_w8dF>button:hover{x:0}a[href$=".woff2"]{x:0}';
  assert.deepEqual([...hashedClasses(css)].sort(), ['buttonGroup_w8dF', 'docMainContainer_q81F']);
});

test('a fragment matches the hashed classes that contain it', () => {
  const classes = new Set(['docMainContainer_q81F', 'docMainContainer_a-b1', 'buttonGroup_w8dF']);
  assert.deepEqual(matchingClasses('docMainContainer_', classes), ['docMainContainer_q81F', 'docMainContainer_a-b1']);
  assert.deepEqual(matchingClasses('buttonGroup', classes), ['buttonGroup_w8dF']);
  assert.deepEqual(matchingClasses('docRoot_', classes), []);
});

test('classes read from another module are not defined by the stylesheet', () => {
  const css = ':global(.markdown) a.versions:not(:global(.card)) { color: red; }\n.panel { margin: 0.5rem; }';
  assert.deepEqual([...definedClasses(css)].sort(), ['panel', 'versions']);
});

test('a fragment that the site CSS modules also produce is the site\'s own', () => {
  const locals = new Set(['card', 'footer', 'docItemContainer']);
  assert.ok(isSiteClass('card_', locals));
  assert.ok(isSiteClass('docItemContainer_', locals));
  assert.ok(!isSiteClass('docMainContainer_', locals));
  assert.ok(!isSiteClass('sidebar_', locals));
});

test('the site sources still select Docusaurus classes by fragment', () => {
  const css = fs.readFileSync(new URL('../src/css/custom.css', import.meta.url), 'utf8');
  assert.ok(fragmentsIn(css).has('docMainContainer_'));
});
