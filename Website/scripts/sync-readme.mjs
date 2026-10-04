/**
 * Generate the copies of hand-written Markdown, rebasing file links:
 *   package Documentation/README.md → repository README.md
 *   repository CHANGELOG.md        → package CHANGELOG.md (Unity's Package Manager reads that copy)
 * A link that leaves the package in the package copy becomes a GitHub URL, since the package ships without the repository.
 */
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {unified} from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import remarkStringify from 'remark-stringify';

const repoDir = fileURLToPath(new URL('../../', import.meta.url));
const packageDir = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const repoUrl = 'https://github.com/VPDPersonal/Aspid.FastTools/blob/main';
const copies = [
  {source: `${packageDir}/Documentation/README.md`, destination: 'README.md'},
  // Copied as text: a remark round trip would rewrite its emphasis and escape `[Unreleased]`.
  {source: 'CHANGELOG.md', destination: `${packageDir}/CHANGELOG.md`, verbatim: true},
];
const processor = unified().use(remarkParse).use(remarkGfm, {tableCellPadding: false, tablePipeAlign: false}).use(remarkStringify, {
  bullet: '-', fences: true, emphasis: '_', resourceLink: true,
});

function generate({source, destination, verbatim}) {
  const sourceDir = path.posix.dirname(source);
  const destinationDir = path.posix.dirname(destination);

  function rebase(url) {
    if (/^(?:[a-z][a-z\d+.-]*:|\/|#)/i.test(url)) return url;
    const [, file, suffix = ''] = /^([^?#]*)(.*)$/.exec(url);
    const target = path.posix.normalize(path.posix.join(sourceDir, file));
    if (destination.startsWith(`${packageDir}/`) && !target.startsWith(`${packageDir}/`)) return `${repoUrl}/${target}${suffix}`;
    return path.posix.relative(destinationDir, target) + suffix;
  }

  function visit(node) {
    if (['link', 'image', 'definition'].includes(node.type)) node.url = rebase(node.url);
    if (node.type === 'html') {
      node.value = node.value.replace(/\b(src|href)=(['"])(.*?)\2/g,
        (_, attribute, quote, url) => `${attribute}=${quote}${rebase(url)}${quote}`);
    }
    node.children?.forEach(visit);
  }

  const text = fs.readFileSync(path.join(repoDir, source), 'utf8');
  if (verbatim) {
    const markdown = text
      .replace(/(\]\()([^)\s]+)(\))/g, (_, open, url, close) => open + rebase(url) + close)
      .replace(/^(\[[^\]]+\]:\s*)(\S+)/gm, (_, label, url) => label + rebase(url));
    return `<!-- Generated from ${source}. Edit that file, then run npm --prefix Website run sync-readme. -->\n\n${markdown}`;
  }
  const tree = processor.parse(text);
  visit(tree);
  // remark escapes the bracket that opens a GitHub alert (`> [!WARNING]`); GitHub needs it literal.
  const markdown = processor.stringify(tree).replace(/^(>\s*)\\\[!(?=[A-Z]+\])/gm, '$1[!');
  return `<!-- Generated from ${source}. Edit that file, then run npm --prefix Website run sync-readme. -->\n\n${markdown}`;
}

for (const copy of copies) {
  const file = path.join(repoDir, copy.destination);
  const result = generate(copy);
  // The package copy has a committed .meta; creating the file anew would make Unity give it a new GUID.
  if (!fs.existsSync(file)) {
    console.error(`${copy.destination} is missing. Restore it from git; this script only updates existing copies.`);
    process.exitCode = 1;
  } else if (process.argv.includes('--check')) {
    if (fs.readFileSync(file, 'utf8') !== result) {
      console.error(`${copy.destination} is out of date. Run npm --prefix Website run sync-readme.`);
      process.exitCode = 1;
    } else {
      console.log(`${copy.destination} matches ${copy.source}.`);
    }
  } else {
    fs.writeFileSync(file, result);
    console.log(`Generated ${copy.destination} from ${copy.source}.`);
  }
}
