// Prepare a release commit: turn [Unreleased] of both changelogs into the new version, copy the changelog into the
// package and bump the version everywhere (scripts/set-version.sh). Merging the result into main publishes the release
// (.github/workflows/release.yml).
//   node scripts/prepare-release.mjs 1.0.0 [--date 2026-10-01]
import { execFileSync } from 'node:child_process';
import { copyFileSync, readFileSync, writeFileSync } from 'node:fs';

const REPO = 'https://github.com/VPDPersonal/Aspid.FastTools';
const PKG = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const args = process.argv.slice(2);
const version = args[0]?.replace(/^v/, '');
const date = args.includes('--date') ? args[args.indexOf('--date') + 1] : new Date().toISOString().slice(0, 10);
const fail = message => { console.error(message); process.exit(1); };

if (!/^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$/.test(version ?? '')) fail('usage: node scripts/prepare-release.mjs <semver> [--date YYYY-MM-DD]');
if (!/^\d{4}-\d{2}-\d{2}$/.test(date)) fail(`--date ${date} is not YYYY-MM-DD`);
process.chdir(new URL('..', import.meta.url).pathname);

for (const file of ['CHANGELOG.md', 'CHANGELOG.ru.md']) {
  const text = readFileSync(file, 'utf8');
  if (text.includes(`## [${version}]`)) fail(`${file} already has a ${version} section`);
  const start = text.indexOf('## [Unreleased]\n');
  if (start < 0) fail(`${file} has no "## [Unreleased]" heading`);
  const bodyStart = start + '## [Unreleased]\n'.length;
  const next = text.indexOf('\n## [', bodyStart);
  if (!text.slice(bodyStart, next < 0 ? undefined : next).trim()) fail(`${file}: [Unreleased] is empty, nothing to release`);

  let out = `${text.slice(0, bodyStart)}\n## [${version}] — ${date}\n${text.slice(bodyStart)}`;
  const link = `[${version}]: ${REPO}/releases/tag/v${version}\n`;
  const refs = out.search(/^\[[^\]]+\]: https?:\/\//m);
  out = refs < 0 ? `${out.trimEnd()}\n\n${link}` : out.slice(0, refs) + link + out.slice(refs);
  writeFileSync(file, out);
  console.log(`${file}: [Unreleased] -> [${version}] — ${date}`);
}
copyFileSync('CHANGELOG.md', `${PKG}/CHANGELOG.md`);
execFileSync('sh', ['scripts/set-version.sh', version], { stdio: 'inherit' });
