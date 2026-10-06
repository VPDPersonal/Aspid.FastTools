// Generate the package CHANGELOG.md and its .meta from the root CHANGELOG.md. Unity's Package Manager reads that copy.
// Both files are gitignored: release.yml generates them and commits them in its runner before the subtree split.
// A link that leaves the package becomes a GitHub URL, since the package ships without the repository.
// The text is copied as is: a Markdown round trip would rewrite its emphasis and escape `[Unreleased]`.
//   node scripts/package-changelog.mjs
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { readFileSync, writeFileSync } from 'node:fs';

process.chdir(fileURLToPath(new URL('..', import.meta.url)));

const SOURCE = 'CHANGELOG.md';
const PKG = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const DESTINATION = `${PKG}/CHANGELOG.md`;
const REPO_URL = 'https://github.com/VPDPersonal/Aspid.FastTools/blob/main';
// The GUID the copy had while it was committed: a new one on every release would break references to it.
const META = [
  'fileFormatVersion: 2',
  'guid: abb6755971db45c5bdfcb1c77c739335',
  'TextScriptImporter:',
  '  externalObjects: {}',
  '  userData: ',
  '  assetBundleName: ',
  '  assetBundleVariant: ',
  '',
].join('\n');

function rebase(url) {
  if (/^(?:[a-z][a-z\d+.-]*:|\/|#)/i.test(url)) return url;
  const [, file, suffix = ''] = /^([^?#]*)(.*)$/.exec(url);
  const target = path.posix.normalize(path.posix.join(path.posix.dirname(SOURCE), file));
  if (!target.startsWith(`${PKG}/`)) return `${REPO_URL}/${target}${suffix}`;
  return path.posix.relative(PKG, target) + suffix;
}

const markdown = readFileSync(SOURCE, 'utf8')
  .replace(/(\]\()([^)\s]+)(\))/g, (_, open, url, close) => open + rebase(url) + close)
  .replace(/^(\[[^\]]+\]:\s*)(\S+)/gm, (_, label, url) => label + rebase(url));
writeFileSync(DESTINATION, `<!-- Generated at release from the repository's ${SOURCE}. Edit that file. -->\n\n${markdown}`);
writeFileSync(`${DESTINATION}.meta`, META);
console.log(`Generated ${DESTINATION} and its .meta from ${SOURCE}.`);
