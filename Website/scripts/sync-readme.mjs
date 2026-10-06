/**
 * Generate the repository README.md from Website/docs/README.md in GitHub's layout (the translations live on the site
 * only), rebasing file links to the repository root. The package CHANGELOG.md is generated at release instead
 * (scripts/package-changelog.mjs).
 */
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {unified} from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import remarkStringify from 'remark-stringify';
import {githubLayout} from './github-readme.mjs';

const repoDir = fileURLToPath(new URL('../../', import.meta.url));
const repository = 'https://github.com/VPDPersonal/Aspid.FastTools';
const site = 'https://vpdpersonal.github.io/Aspid.FastTools/';
// LOCALES in docusaurus.config.js, the default one first.
const locales = ['en', 'ru'];
const source = 'Website/docs/README.md';
const destination = 'README.md';
const processor = unified().use(remarkParse).use(remarkGfm, {tableCellPadding: false, tablePipeAlign: false}).use(remarkStringify, {
  bullet: '-', fences: true, emphasis: '_', resourceLink: true,
});

function generate() {
  const sourceDir = path.posix.dirname(source);
  const destinationDir = path.posix.dirname(destination);

  function rebase(url) {
    if (/^(?:[a-z][a-z\d+.-]*:|\/|#)/i.test(url)) return url;
    const [, file, suffix = ''] = /^([^?#]*)(.*)$/.exec(url);
    const target = path.posix.normalize(path.posix.join(sourceDir, file));
    return path.posix.relative(destinationDir, target) + suffix;
  }

  function visit(node) {
    if (['link', 'image', 'definition'].includes(node.type)) node.url = rebase(node.url);
    if (node.type === 'html') {
      node.value = node.value.replace(/\b(src|srcset|href)=(['"])(.*?)\2/g,
        (_, attribute, quote, url) => `${attribute}=${quote}${rebase(url)}${quote}`);
    }
    node.children?.forEach(visit);
  }

  const tree = processor.parse(fs.readFileSync(path.join(repoDir, source), 'utf8'));
  visit(tree);
  githubLayout(tree, {
    site,
    locales,
    repository,
    previews: path.posix.relative(destinationDir, 'docs/images/readme-previews'),
    exists: (url) => fs.existsSync(path.join(repoDir, destinationDir, url)),
  });
  // remark escapes the bracket that opens a GitHub alert (`> [!WARNING]`); GitHub needs it literal.
  const markdown = processor.stringify(tree).replace(/^(>\s*)\\\[!(?=[A-Z]+\])/gm, '$1[!');
  return `<!-- Generated from ${source}. Edit that file, then run npm --prefix Website run sync-readme. -->\n\n${markdown}`;
}

const file = path.join(repoDir, destination);
const result = generate();
if (process.argv.includes('--check')) {
  if (!fs.existsSync(file) || fs.readFileSync(file, 'utf8') !== result) {
    console.error(`${destination} is out of date. Run npm --prefix Website run sync-readme.`);
    process.exitCode = 1;
  } else {
    console.log(`${destination} matches ${source}.`);
  }
} else {
  fs.writeFileSync(file, result);
  console.log(`Generated ${destination} from ${source}.`);
}
