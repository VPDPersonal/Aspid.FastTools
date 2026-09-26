// Repository consistency checks shared by CI (.github/workflows/checks.yml) and the pre-commit hook (.githooks/pre-commit):
//   node scripts/check-changes.mjs                 whole repository, files from the working tree
//   node scripts/check-changes.mjs --base <ref>    plus the rules for files changed since <ref> (a pull request)
//   node scripts/check-changes.mjs --staged        staged files only, read from the index; rebuilds stale Roslyn DLLs
// PR_TITLE, PR_AUTHOR and PR_LABELS (comma-separated) turn on the pull-request rules.
import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync } from 'node:fs';

const PKG = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const CHANGELOG = 'CHANGELOG.md';
const CHANGELOG_RU = 'CHANGELOG.ru.md';
const PKG_CHANGELOG = `${PKG}/CHANGELOG.md`;
const VERSION_FILES = ['README.md', `${PKG}/Documentation/README.md`, `${PKG}/Documentation/ru/README.md`,
  `${PKG}/Documentation/Images/status-badge-preview.svg`];
// Each Roslyn component and the DLL its Release build deploys into the package (Directory.Build.targets).
const ROSLYN = [
  { solution: 'Aspid.FastTools.Generators', project: 'Aspid.FastTools.Generators/Aspid.FastTools.Generators',
    dll: `${PKG}/Aspid.FastTools.Generators.dll` },
  { solution: 'Aspid.FastTools.Analyzers', project: 'Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers',
    dll: `${PKG}/Aspid.FastTools.Analyzers.dll` },
];
// A user-visible change: package code or the sources of the shipped Roslyn DLLs.
const USER_VISIBLE = [`${PKG}/Runtime/`, `${PKG}/Editor/`, ...ROSLYN.map(r => `${r.project}/`)];
const TITLE = /^(feat|fix|docs|perf|refactor|test|chore|build|ci|style|revert)(\([a-z0-9-]+(, ?[a-z0-9-]+)*\))?(!)?: \S.*$/;
const NEEDS_CHANGELOG = new Set(['feat', 'fix', 'perf']);

const args = process.argv.slice(2);
const staged = args.includes('--staged');
const base = args.includes('--base') ? args[args.indexOf('--base') + 1] : null;
const ci = !!process.env.GITHUB_ACTIONS;

const git = (...a) => execFileSync('git', a, { encoding: 'utf8', maxBuffer: 64 << 20 });
const lines = s => s.split('\n').filter(Boolean);
const read = file => {
  if (staged) {
    try { return git('show', `:${file}`); } catch { return null; }
  }
  return existsSync(file) ? readFileSync(file, 'utf8') : null;
};

const changed = staged ? lines(git('diff', '--cached', '--name-only'))
  : base ? lines(git('diff', '--name-only', `${base}...HEAD`)) : null;
const touched = file => changed === null || changed.includes(file);

let errors = 0;
const report = (level, file, message) => {
  if (ci) console.log(`::${level} file=${file}::${message}`);
  else console.log(`${level === 'error' ? '✖' : '⚠'} ${file}: ${message}`);
  if (level === 'error') errors++;
};
const error = (file, message) => report('error', file, message);
const warning = (file, message) => report('warning', file, message);

// The package ships a copy of the root changelog; the release workflow refuses to publish when they differ.
if (touched(CHANGELOG) || touched(PKG_CHANGELOG)) {
  if (read(CHANGELOG) !== read(PKG_CHANGELOG)) {
    error(PKG_CHANGELOG, `differs from the root ${CHANGELOG}; copy it: cp ${CHANGELOG} ${PKG_CHANGELOG}`);
  }
}

// CHANGELOG.ru.md translates CHANGELOG.md bullet for bullet: same headings, same number of bullets under each.
const outline = md => {
  const out = [];
  let section = null;
  let fence = false;
  for (const line of md.split(/\r?\n/)) {
    if (/^\s*```/.test(line)) fence = !fence;
    if (fence) continue;
    const heading = line.match(/^(#{2,6}) (.*)/);
    if (heading) out.push(section = { level: heading[1].length, text: heading[2], bullets: 0 });
    else if (section && /^\s*[-*] /.test(line)) section.bullets++;
  }
  return out;
};
if (changed !== null && touched(CHANGELOG) !== touched(CHANGELOG_RU)) {
  error(touched(CHANGELOG) ? CHANGELOG_RU : CHANGELOG, `${CHANGELOG} and ${CHANGELOG_RU} must change together`);
}
if (touched(CHANGELOG) || touched(CHANGELOG_RU)) {
  const en = outline(read(CHANGELOG) ?? '');
  const ru = outline(read(CHANGELOG_RU) ?? '');
  let release = '';
  for (let i = 0; i < Math.max(en.length, ru.length); i++) {
    const e = en[i], r = ru[i];
    if (!e || !r || e.level !== r.level || (e.level === 2 && e.text !== r.text)) {
      error(CHANGELOG_RU, `headings diverge from ${CHANGELOG} after ${release || 'the start'}: ` +
        `"${e ? '#'.repeat(e.level) + ' ' + e.text : '(end)'}" vs "${r ? '#'.repeat(r.level) + ' ' + r.text : '(end)'}"`);
      break;
    }
    if (e.level === 2) release = e.text;
    if (e.bullets !== r.bullets) {
      error(CHANGELOG_RU, `"${r.text}" in ${release} has ${r.bullets} bullets, ${CHANGELOG} "${e.text}" has ${e.bullets}`);
    }
  }
}

// The version is written by hand in several files (scripts/set-version.sh keeps them in step). A stable version
// installs from the `upm` branch under a "Release" badge, a prerelease from `upm-preview` under a "Preview" one.
// The version is matched whole: 1.0.0 must not pass on a file that still says 1.0.0-rc.8.
if ([`${PKG}/package.json`, ...VERSION_FILES].some(touched)) {
  const version = JSON.parse(read(`${PKG}/package.json`) ?? '{}').version ?? '';
  const [branch, label] = version.includes('-') ? ['upm-preview', 'Preview'] : ['upm', 'Release'];
  const whole = text => new RegExp(`(^|[^0-9A-Za-z.-])${text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}([^0-9A-Za-z.-]|$)`);
  const fix = `run scripts/set-version.sh ${version}`;
  for (const file of VERSION_FILES) {
    const text = read(file) ?? '';
    if (!whole(`${label} ${version}`).test(text)) error(file, `has no "${label} ${version}" badge; ${fix}`);
    if (file.endsWith('.svg')) {
      if (!text.includes(`>${label}</text>`)) error(file, `the badge text is not "${label}"; ${fix}`);
      continue;
    }
    if (!text.includes(`/releases/tag/v${version})`)) error(file, `the badge does not link the v${version} release; ${fix}`);
    const urls = [...text.matchAll(/\.git#(upm(?:-preview)?)\b/g)].map(m => m[1]);
    if (!urls.length || urls.some(url => url !== branch)) error(file, `the install URLs must use #${branch} for ${version}; ${fix}`);
  }
}

// Every documentation page has a Russian translation next to it.
const tracked = new Set(lines(git('ls-files', `${PKG}/Documentation`, `${PKG}/Samples~`)));
const translations = [];
for (const file of tracked) {
  let m;
  if ((m = file.match(new RegExp(`^${PKG}/Documentation/([^/]+\\.md)$`)))) translations.push([file, `${PKG}/Documentation/ru/${m[1]}`]);
  else if ((m = file.match(/^(.*\/Documentation\/[^/]+)(?<!\.ru)\.md$/)) && file.includes('/Samples~/')) translations.push([file, `${m[1]}.ru.md`]);
}
for (const [en, ru] of translations) {
  if (!tracked.has(ru)) error(en, `has no Russian translation ${ru}`);
  else if (changed !== null && touched(en) && !touched(ru)) warning(ru, `not updated together with ${en}`);
}
for (const file of tracked) {
  const m = file.match(new RegExp(`^${PKG}/Documentation/ru/([^/]+\\.md)$`));
  if (m && !tracked.has(`${PKG}/Documentation/${m[1]}`)) error(file, `has no English page ${PKG}/Documentation/${m[1]}`);
}

// Pull-request rules: a Conventional Commits title, and a changelog entry for a user-visible feat/fix/perf.
if (process.env.PR_TITLE) {
  const title = process.env.PR_TITLE;
  const bot = /\[bot\]$/.test(process.env.PR_AUTHOR ?? '');
  const labels = (process.env.PR_LABELS ?? '').split(',');
  const m = title.match(TITLE);
  if (!m) error('PR title', `"${title}" is not "type(scope): subject" (Conventional Commits; types: feat fix docs perf refactor test chore build ci style revert)`);
  else if (title.length > 72 && !bot) error('PR title', `is ${title.length} characters, at most 72`);
  const userVisible = changed?.some(file => USER_VISIBLE.some(dir => file.startsWith(dir)));
  if (m && (NEEDS_CHANGELOG.has(m[1]) || m[4]) && userVisible && !touched(CHANGELOG) && !labels.includes('no-changelog')) {
    error(CHANGELOG, `a user-visible ${m[1]}${m[4] ?? ''} needs an [Unreleased] entry in ${CHANGELOG} and ${CHANGELOG_RU} (or the no-changelog label)`);
  }
}

// A staged change to a Roslyn component must ship its rebuilt DLL.
if (staged) {
  for (const r of ROSLYN) {
    if (!changed.some(file => file.startsWith(`${r.project}/`) || file.startsWith(`${r.solution}/Directory.Build.`))) continue;
    try {
      execFileSync('dotnet', ['build', r.project, '-c', 'Release', '--nologo', '-v', 'q'], { encoding: 'utf8' });
    } catch (e) {
      if (e.code === 'ENOENT') warning(r.dll, 'dotnet is not installed; could not check that the DLL is fresh');
      else error(r.project, `Release build failed:\n${lines(e.stdout ?? '').filter(l => /error/.test(l)).slice(0, 10).join('\n')}`);
      continue;
    }
    try {
      git('diff', '--quiet', '--', r.dll);
    } catch {
      error(r.dll, `was stale and has been rebuilt; stage it: git add ${r.dll}`);
    }
  }
}

if (errors) process.exit(1);
if (!staged) console.log('Repository checks OK');
