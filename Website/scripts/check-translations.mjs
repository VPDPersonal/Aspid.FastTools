/**
 * Check that every English page has a Russian twin and that the twin keeps the structure of its source: the same
 * heading levels, code blocks, images and link targets. Only the prose may differ, so a change made to one language and
 * not the other fails here. Prose inside code may be translated too: `//` comments, text blocks (prompts) and same-page
 * anchors count only. The two changelogs must also list the same releases with the same number of entries.
 */
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {unified} from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';

const repoDir = fileURLToPath(new URL('../../', import.meta.url));
const packageDir = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const parser = unified().use(remarkParse).use(remarkGfm);

const I18N = 'Website/i18n/ru/';

/**
 * The English file a translated path stands for: `Website/i18n/ru/<plugin>/current/` maps to the plugin's folder
 * (`docs`, `tutorials`), a `.ru` suffix is dropped.
 */
export function englishPath(file) {
  return file
    .replace(/^Website\/i18n\/ru\/docusaurus-plugin-content-docs\/current\//, 'Website/docs/')
    .replace(/^Website\/i18n\/ru\/docusaurus-plugin-content-docs-tutorials\/current\//, 'Website/tutorials/')
    .replace(/\.ru\.(mdx?)$/, '.$1');
}

/** The Russian twin an English path needs: the inverse of `englishPath`. */
export function russianPath(file) {
  if (file.startsWith('Website/docs/')) return file.replace('Website/docs/', `${I18N}docusaurus-plugin-content-docs/current/`);
  if (file.startsWith('Website/tutorials/')) return file.replace('Website/tutorials/', `${I18N}docusaurus-plugin-content-docs-tutorials/current/`);
  return file.replace(/\.(mdx?)$/, '.ru.$1');
}

/**
 * True for an English page that needs a Russian twin: the docs, the tutorials, the sample READMEs and the changelog.
 * The package README, the licences and the root README have none; the site builds the root README from the docs.
 */
export function needsTranslation(file) {
  if (/^Website\/(?:docs|tutorials)\/.+\.mdx?$/.test(file)) return true;
  return file === 'CHANGELOG.md' || (file.startsWith(`${packageDir}/Samples~/`) && /(?:^|\/)README\.md$/.test(file));
}

/** The parts of a page that must match across languages; links and images are resolved to English repository paths. */
export function outline(markdown, file) {
  const dir = path.posix.dirname(file);
  const resolve = url => /^(?:[a-z][a-z\d+.-]*:|\/|#)/i.test(url)
    ? url
    : englishPath(path.posix.normalize(path.posix.join(dir, url.replace(/[?#].*$/, ''))));
  const code = (value, lang = '') => /^(?:text|md|markdown)?$/.test(lang) ? '' : value.replace(/(^|\s)\/\/.*$/gm, '$1').trimEnd();
  const parts = {headings: [], code: [], images: [], links: []};
  (function visit(node) {
    if (node.type === 'heading') parts.headings.push(node.depth);
    if (node.type === 'code') parts.code.push(code(node.value, node.lang ?? ''));
    if (node.type === 'image') parts.images.push(resolve(node.url));
    if (node.type === 'link' || node.type === 'definition') parts.links.push(resolve(node.url));
    if (node.type === 'html') {
      for (const [, body] of node.value.matchAll(/<pre>([\s\S]*?)<\/pre>/g)) parts.code.push(code(body, 'csharp'));
      for (const [, url] of node.value.matchAll(/\bsrc(?:set)?=(['"])(.*?)\1/g).map(m => [m[1], m[2]])) parts.images.push(resolve(url));
    }
    node.children?.forEach(visit);
  })(parser.parse(markdown));
  // Site URLs carry the locale (/ru/docs); a link may point to the matching page in either language.
  parts.links = parts.links.map(url => url.startsWith('#') ? '#'
    : url.replace(/(vpdpersonal\.github\.io\/Aspid\.FastTools)\/ru\//, '$1/'));
  return parts;
}

/** Human-readable differences between an English page and its translation; empty when they match. */
export function compare(english, russian) {
  const problems = [];
  const describe = {headings: 'heading levels', code: 'code blocks', images: 'images', links: 'link targets'};
  for (const key of Object.keys(describe)) {
    const a = english[key], b = russian[key];
    if (a.length !== b.length) {
      problems.push(`${describe[key]}: ${a.length} in English, ${b.length} in Russian`);
      continue;
    }
    const index = a.findIndex((value, i) => value !== b[i]);
    if (index >= 0) problems.push(`${describe[key]} #${index + 1} differs:\n      en: ${JSON.stringify(a[index]).slice(0, 160)}\n      ru: ${JSON.stringify(b[index]).slice(0, 160)}`);
  }
  return problems;
}

/**
 * The releases of a changelog as a flat list of headings: `## [version] — date` and the sections under it, each with
 * the number of its top-level list items. Section names are translated, so only the version headings carry a text to match.
 */
export function changelogOutline(markdown) {
  const headings = [];
  for (const node of parser.parse(markdown).children) {
    if (node.type === 'heading' && node.depth >= 2) {
      const text = markdown.slice(node.position.start.offset, node.position.end.offset).replace(/^#+\s*/, '');
      headings.push({depth: node.depth, text, items: 0});
    } else if (node.type === 'list' && headings.length) headings[headings.length - 1].items += node.children.length;
  }
  return headings;
}

/** Differences between the English and the Russian changelog in releases, dates and entry counts; empty when they match. */
export function compareChangelogs(english, russian) {
  const a = changelogOutline(english), b = changelogOutline(russian);
  if (a.length !== b.length) return [`changelog headings: ${a.length} in English, ${b.length} in Russian`];
  const problems = [];
  let release = '';
  a.forEach((heading, i) => {
    if (heading.depth === 2) release = heading.text;
    const other = b[i];
    if (heading.depth !== other.depth || (heading.depth === 2 && heading.text !== other.text)) {
      problems.push(`changelog heading #${i + 1} differs:\n      en: ${heading.text}\n      ru: ${other.text}`);
    } else if (heading.items !== other.items) {
      problems.push(`${release} / ${heading.text} entries: ${heading.items} in English, ${other.items} in Russian`);
    }
  });
  return problems;
}

/** Every file under `dir` (relative to the repository root) that `accept` takes. */
function list(dir, accept) {
  const found = [];
  function walk(current) {
    for (const entry of fs.readdirSync(path.join(repoDir, current), {withFileTypes: true})) {
      const file = path.posix.join(current, entry.name);
      if (entry.isDirectory()) walk(file);
      else if (accept(file)) found.push(file);
    }
  }
  walk(dir);
  return found;
}

function translations() {
  const found = [
    ...list(packageDir, file => /\.ru\.mdx?$/.test(file)),
    // The changelog folder in i18n is generated from CHANGELOG.ru.md, which is checked below.
    ...list(`${I18N}docusaurus-plugin-content-docs`, file => /\.mdx?$/.test(file)),
    ...list(`${I18N}docusaurus-plugin-content-docs-tutorials`, file => /\.mdx?$/.test(file)),
    'CHANGELOG.ru.md',
  ];
  return found.sort();
}

function sources() {
  const found = [
    ...list('Website/docs', needsTranslation),
    ...list('Website/tutorials', needsTranslation),
    ...list(`${packageDir}/Samples~`, needsTranslation),
    'CHANGELOG.md',
  ];
  return found.sort();
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  let failed = 0;
  const exists = file => fs.existsSync(path.join(repoDir, file));
  // The site shows the English text where a translation is missing, so a missing twin must fail here.
  for (const englishFile of sources()) {
    if (exists(russianPath(englishFile))) continue;
    console.error(`${englishFile}: no Russian translation ${russianPath(englishFile)}`);
    failed++;
  }
  for (const russianFile of translations()) {
    const englishFile = englishPath(russianFile);
    const read = file => fs.readFileSync(path.join(repoDir, file), 'utf8');
    if (!exists(englishFile)) {
      console.error(`${russianFile}: no English source ${englishFile}`);
      failed++;
      continue;
    }
    const problems = compare(outline(read(englishFile), englishFile), outline(read(russianFile), russianFile));
    if (englishFile === 'CHANGELOG.md') problems.push(...compareChangelogs(read(englishFile), read(russianFile)));
    if (problems.length) {
      console.error(`${russianFile} does not match ${englishFile}:\n  - ${problems.join('\n  - ')}`);
      failed++;
    }
  }
  if (failed) {
    console.error(`\n${failed} translation(s) out of step. Make the same structural change in both languages.`);
    process.exitCode = 1;
  } else {
    console.log('Every English page has a translation that matches it.');
  }
}
