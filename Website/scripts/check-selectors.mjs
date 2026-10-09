/**
 * Check that the Docusaurus classes the site selects by a name fragment still exist in the build.
 *
 * Docusaurus gives the classes of its own components a hash (`docMainContainer_q81F`), and the site selects them as
 * `[class*='docMainContainer_']` in its CSS and JS. An update can rename such a class: the build stays green and the
 * layout breaks without a message. This script collects every fragment from `src/` and `docusaurus.config.js` and looks
 * for a hashed class that contains it in the stylesheets of `build/`. Run it after `npm run build`.
 *
 * Fragments that the site's own CSS modules also produce (`card_` ← `.card`, `docItemContainer_`) are skipped: the site
 * defines those classes itself. Public class names (`theme-*`, Infima's `menu__link`) have no hash and are not checked.
 */
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const siteDir = fileURLToPath(new URL('..', import.meta.url));

/** The name fragments in attribute selectors: `[class*='card_']`, `[class^="nav"]`, `[class*=card_]`. */
export function fragmentsIn(text) {
  const fragments = new Set();
  for (const [, , fragment] of text.matchAll(/\[\s*class\s*[*^$~|]?=\s*(['"]?)([\w-]+)\1\s*\]/g)) fragments.add(fragment);
  return fragments;
}

/** The class names a stylesheet defines, without the ones it reads from another module (`:global(.markdown)`). */
export function definedClasses(css) {
  const own = css.replace(/:global\([^)]*\)/g, '');
  return new Set([...own.matchAll(/\.([A-Za-z_][\w-]*)/g)].map(([, name]) => name));
}

/** True when the site's own CSS modules produce a class that the fragment selects (`card_` ← `.card`). */
export function isSiteClass(fragment, localNames) {
  return [...localNames].some(name => `${name}_xxxx`.includes(fragment));
}

/** The CSS-module classes in the built stylesheets: `local_hash`, where the hash is four base64 characters. */
export function hashedClasses(css) {
  return new Set([...definedClasses(css)].filter(name => /[A-Za-z0-9]_[A-Za-z0-9-]{4}$/.test(name)));
}

/** The hashed class names that a fragment selects, in the order they are found. */
export function matchingClasses(fragment, classes) {
  return [...classes].filter(name => name.includes(fragment));
}

function walk(dir, accept, found = []) {
  for (const entry of fs.readdirSync(dir, {withFileTypes: true})) {
    const file = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      if (entry.name !== 'node_modules') walk(file, accept, found);
    } else if (accept(entry.name)) found.push(file);
  }
  return found;
}

const read = file => fs.readFileSync(file, 'utf8');

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const buildDir = path.resolve(process.argv[2] ?? path.join(siteDir, 'build'));
  if (!fs.existsSync(buildDir)) {
    console.error(`${buildDir} does not exist. Run "npm run build" first.`);
    process.exit(2);
  }

  const sources = [...walk(path.join(siteDir, 'src'), name => /\.(?:css|js)$/.test(name)), path.join(siteDir, 'docusaurus.config.js')];
  const fragments = new Set(sources.flatMap(file => [...fragmentsIn(read(file))]));
  const localNames = new Set(walk(path.join(siteDir, 'src'), name => name.endsWith('.module.css')).flatMap(file => [...definedClasses(read(file))]));
  const built = new Set(walk(buildDir, name => name.endsWith('.css')).flatMap(file => [...hashedClasses(read(file))]));

  let missing = 0;
  for (const fragment of [...fragments].sort()) {
    if (isSiteClass(fragment, localNames) || matchingClasses(fragment, built).length) continue;
    console.error(`[class*='${fragment}'] matches no class in ${path.relative(process.cwd(), buildDir) || '.'}`);
    missing++;
  }
  if (missing) {
    console.error(`\n${missing} selector(s) match nothing. A Docusaurus update probably renamed the class: find its new name in `
      + 'node_modules/@docusaurus/theme-classic/lib/theme and update the site CSS and JS.');
    process.exitCode = 1;
  } else {
    console.log(`Every one of the ${fragments.size} class fragments of the site matches a class in the build.`);
  }
}
