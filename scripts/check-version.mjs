// Check that the files scripts/set-version.sh writes carry the package.json version whole and its channel: a stable
// version installs from `upm` under a "Release" badge, a prerelease from `upm-preview` under a "Preview" one.
// The version is matched whole, so 1.0.0 does not pass on a file that still says 1.0.0-rc.8. release.yml runs it:
//   node scripts/check-version.mjs
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

process.chdir(fileURLToPath(new URL('..', import.meta.url)));

const PKG = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const FILES = [
  'README.md',
  'Website/docs/README.md',
  'Website/i18n/ru/docusaurus-plugin-content-docs/current/README.md',
  'Website/docs/Images/status-badge-preview.svg',
];
// Files with an install URL and no badge.
const INSTALL_FILES = [`${PKG}/README.md`];

const version = JSON.parse(readFileSync(`${PKG}/package.json`, 'utf8')).version ?? '';
const [branch, label] = version.includes('-') ? ['upm-preview', 'Preview'] : ['upm', 'Release'];
const fix = `run scripts/set-version.sh ${version}`;
const whole = (text) => new RegExp(`(^|[^0-9A-Za-z.-])${text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}([^0-9A-Za-z.-]|$)`);

let errors = 0;
const fail = (file, message) => {
  console.log(`::error file=${file}::${message}`);
  errors++;
};

const checkInstallUrls = (file, text) => {
  const urls = [...text.matchAll(/\.git#(upm(?:-preview)?)\b/g)].map((match) => match[1]);
  if (!urls.length || urls.some((url) => url !== branch)) fail(file, `the install URLs must use #${branch} for ${version}; ${fix}`);
};

for (const file of INSTALL_FILES) checkInstallUrls(file, readFileSync(file, 'utf8'));

for (const file of FILES) {
  const text = readFileSync(file, 'utf8');
  if (!whole(`${label} ${version}`).test(text)) fail(file, `has no "${label} ${version}" badge; ${fix}`);
  if (file.endsWith('.svg')) {
    if (!text.includes(`>${label}</text>`)) fail(file, `the badge text is not "${label}"; ${fix}`);
    continue;
  }
  if (!text.includes(`/releases/tag/v${version})`)) fail(file, `the badge does not link the v${version} release; ${fix}`);
  checkInstallUrls(file, text);
}

if (errors) process.exit(1);
console.log(`Version files carry ${label} ${version} (#${branch})`);
