/** Generate the repository README from the package documentation, rebasing file links. */
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {unified} from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import remarkStringify from 'remark-stringify';

const repoDir = fileURLToPath(new URL('../../', import.meta.url));
const sourcePath = 'Aspid.FastTools/Packages/tech.aspid.fasttools/Documentation/README.md';
const sourceDir = path.posix.dirname(sourcePath);
const destination = path.join(repoDir, 'README.md');
const processor = unified().use(remarkParse).use(remarkGfm, {tableCellPadding: false, tablePipeAlign: false}).use(remarkStringify, {
  bullet: '-', fences: true, emphasis: '_', resourceLink: true,
});
const tree = processor.parse(fs.readFileSync(path.join(repoDir, sourcePath), 'utf8'));

function rebase(url) {
  if (/^(?:[a-z][a-z\d+.-]*:|\/|#)/i.test(url)) return url;
  const [, file, suffix = ''] = /^([^?#]*)(.*)$/.exec(url);
  return path.posix.normalize(path.posix.join(sourceDir, file)) + suffix;
}

function visit(node) {
  if (['link', 'image', 'definition'].includes(node.type)) node.url = rebase(node.url);
  if (node.type === 'html') {
    node.value = node.value.replace(/\b(src|href)=(['"])(.*?)\2/g,
      (_, attribute, quote, url) => `${attribute}=${quote}${rebase(url)}${quote}`);
  }
  node.children?.forEach(visit);
}
visit(tree);

// remark escapes the bracket that opens a GitHub alert (`> [!WARNING]`); GitHub needs it literal.
const markdown = processor.stringify(tree).replace(/^(>\s*)\\\[!(?=[A-Z]+\])/gm, '$1[!');
const result = `<!-- Generated from ${sourcePath}. Edit that file, then run npm --prefix Website run sync-readme. -->\n\n${markdown}`;
if (process.argv.includes('--check')) {
  if (!fs.existsSync(destination) || fs.readFileSync(destination, 'utf8') !== result) {
    console.error('README.md is out of date. Run npm --prefix Website run sync-readme.');
    process.exitCode = 1;
  } else {
    console.log('README.md matches the package documentation.');
  }
} else {
  fs.writeFileSync(destination, result);
  console.log('Generated README.md from the package documentation.');
}
