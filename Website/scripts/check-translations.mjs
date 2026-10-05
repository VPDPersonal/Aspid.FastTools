/**
 * Check that every Russian page keeps the structure of its English source: the same heading levels, code blocks,
 * images and link targets. Only the prose may differ, so a change made to one language and not the other fails here.
 * Prose inside code may be translated too: `//` comments, text blocks (prompts) and same-page anchors count only.
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
    .replace(/\.ru\.md$/, '.md');
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

function translations() {
  const found = [];
  function walk(dir) {
    for (const entry of fs.readdirSync(path.join(repoDir, dir), {withFileTypes: true})) {
      const file = path.posix.join(dir, entry.name);
      if (entry.isDirectory()) walk(file);
      else if (file.startsWith(I18N) ? /\.md$/.test(file) : /\.ru\.md$/.test(file)) found.push(file);
    }
  }
  walk(packageDir);
  // The changelog folder in i18n is generated from CHANGELOG.ru.md, which is checked below.
  walk(`${I18N}docusaurus-plugin-content-docs`);
  walk(`${I18N}docusaurus-plugin-content-docs-tutorials`);
  found.push('CHANGELOG.ru.md');
  return found.sort();
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  let failed = 0;
  for (const russianFile of translations()) {
    const englishFile = englishPath(russianFile);
    const read = file => fs.readFileSync(path.join(repoDir, file), 'utf8');
    if (!fs.existsSync(path.join(repoDir, englishFile))) {
      console.error(`${russianFile}: no English source ${englishFile}`);
      failed++;
      continue;
    }
    const problems = compare(outline(read(englishFile), englishFile), outline(read(russianFile), russianFile));
    if (problems.length) {
      console.error(`${russianFile} does not match ${englishFile}:\n  - ${problems.join('\n  - ')}`);
      failed++;
    }
  }
  if (failed) {
    console.error(`\n${failed} translation(s) out of step. Make the same structural change in both languages.`);
    process.exitCode = 1;
  } else {
    console.log('Every translation matches its English page.');
  }
}
