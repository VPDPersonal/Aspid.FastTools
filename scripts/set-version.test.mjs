// Tests of scripts/set-version.sh and scripts/check-version.mjs. Both find the repository from their own location and
// change its files, so each test runs them on a copy of the files they read. checks.yml runs it:
//   node --test scripts/*.test.mjs
// set-version.sh regenerates the root README with the dependencies of Website/, so run `npm --prefix Website ci` first.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { cpSync, existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, symlinkSync, writeFileSync } from 'node:fs';

const repository = fileURLToPath(new URL('..', import.meta.url));
const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const ANALYZER_RELEASES = 'Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/AnalyzerReleases';
const DOCS = 'Website/docs/README.md';
const RU_DOCS = 'Website/i18n/ru/docusaurus-plugin-content-docs/current/README.md';
const BADGE = 'Website/docs/Images/status-badge-preview.svg';
const RELEASES = 'https://github.com/VPDPersonal/Aspid.FastTools/releases/tag';
const FILES = [
  'scripts/set-version.sh',
  'scripts/check-version.mjs',
  'README.md',
  'CHANGELOG.md',
  'CHANGELOG.ru.md',
  `${PACKAGE}/package.json`,
  `${PACKAGE}/README.md`,
  `${ANALYZER_RELEASES}.Shipped.md`,
  'Website/package.json',
  'Website/scripts/sync-readme.mjs',
  'Website/scripts/github-readme.mjs',
  DOCS,
  RU_DOCS,
  BADGE,
];
// The analyzer rules that a stable release ships; the real file may be empty right after a release.
const UNSHIPPED = [
  '; Unshipped analyzer release',
  '',
  '### New Rules',
  '',
  'Rule ID | Category | Severity | Notes',
  '--------|----------|----------|-------',
  'AFT9999 | Usage | Error | A test rule',
  '',
].join('\n');
const NOTES = '\n### Fixed\n\n- A note for the release.\n\n';

// The tests start from the version that the repository has, a prerelease today and a stable one after 1.0.0.
// The stable version that they set is one the repository never has: after the release 1.0.0 the real Shipped.md already
// has its header, and set-version.sh would not ship the rule of the test again.
const STABLE = '99.0.0';
const current = JSON.parse(readFileSync(join(repository, PACKAGE, 'package.json'), 'utf8')).version;
const [branch, label, otherBranch, otherLabel] = current.includes('-')
  ? ['upm-preview', 'Preview', 'upm', 'Release']
  : ['upm', 'Release', 'upm-preview', 'Preview'];
const literal = (text) => text.replaceAll('.', '\\.');

const run = (command, args, cwd) => spawnSync(command, args, { cwd, encoding: 'utf8' });
const output = (result) => `${result.stdout}${result.stderr}`;
const read = (dir, file) => readFileSync(join(dir, file), 'utf8');
const write = (dir, file, text) => writeFileSync(join(dir, file), text);
const edit = (dir, file, change) => write(dir, file, change(read(dir, file)));

// A repository with the files that the two scripts read, a git repository (set-version.sh ends with `git status`) and
// the notes of the next release under [Unreleased].
function sandbox(t) {
  const modules = join(repository, 'Website/node_modules');
  assert.ok(existsSync(modules), 'Website/node_modules is missing; run: npm --prefix Website ci');
  const dir = mkdtempSync(join(tmpdir(), 'aspid-set-version-'));
  t.after(() => rmSync(dir, { recursive: true, force: true }));
  for (const file of FILES) {
    mkdirSync(dirname(join(dir, file)), { recursive: true });
    cpSync(join(repository, file), join(dir, file));
  }
  write(dir, `${ANALYZER_RELEASES}.Unshipped.md`, UNSHIPPED);
  symlinkSync(modules, join(dir, 'Website/node_modules'));
  run('git', ['init', '--quiet'], dir);
  unreleased(dir, NOTES);
  return dir;
}

// Replace what is under [Unreleased] in both CHANGELOGs.
function unreleased(dir, notes) {
  for (const file of ['CHANGELOG.md', 'CHANGELOG.ru.md']) {
    edit(dir, file, (text) => text.replace(/^(## \[Unreleased\]\n)[\s\S]*?(?=^## \[)/m, `$1${notes}`));
  }
}

const setVersion = (dir, version) => run('sh', ['scripts/set-version.sh', version], dir);
const checkVersion = (dir) => run('node', ['scripts/check-version.mjs'], dir);

function assertVersionChecked(dir) {
  const result = checkVersion(dir);
  assert.equal(result.status, 0, output(result));
}

const version = (dir) => JSON.parse(read(dir, `${PACKAGE}/package.json`)).version;
const installBranches = (text) => [...text.matchAll(/\.git#(upm(?:-preview)?)\b/g)].map((match) => match[1]);
const snapshot = (dir) => FILES.map((file) => read(dir, file)).concat(read(dir, `${ANALYZER_RELEASES}.Unshipped.md`));

test('the repository files pass the version check as they are', (t) => {
  assertVersionChecked(sandbox(t));
});

test('a prerelease moves the files to the preview channel', (t) => {
  const dir = sandbox(t);
  const result = setVersion(dir, '1.0.0-rc.99');
  assert.equal(result.status, 0, output(result));
  assert.equal(version(dir), '1.0.0-rc.99');
  assertVersionChecked(dir);

  for (const file of [DOCS, RU_DOCS]) {
    assert.match(read(dir, file), /\[!\[Preview 1\.0\.0-rc\.99\]/, file);
    assert.deepEqual([...new Set(installBranches(read(dir, file)))], ['upm-preview'], file);
  }
  assert.match(read(dir, BADGE), />Preview<\/text>/);
  assert.match(read(dir, 'README.md'), /Preview 1\.0\.0-rc\.99/);
  assert.deepEqual([...new Set(installBranches(read(dir, `${PACKAGE}/README.md`)))], ['upm-preview']);

  for (const file of ['CHANGELOG.md', 'CHANGELOG.ru.md']) {
    const changelog = read(dir, file);
    assert.match(changelog, /^## \[Unreleased\]\n\n## \[1\.0\.0-rc\.99\] — \d{4}-\d{2}-\d{2}\n\n### Fixed\n\n- A note/m, file);
    assert.ok(changelog.includes(`\n[1.0.0-rc.99]: ${RELEASES}/v1.0.0-rc.99\n`), file);
  }
  // Analyzer release headers accept only System.Version numbers, so a prerelease keeps the rules unshipped.
  assert.ok(read(dir, `${ANALYZER_RELEASES}.Unshipped.md`).includes('AFT9999'));
  assert.ok(!read(dir, `${ANALYZER_RELEASES}.Shipped.md`).includes('## Release 1.0.0-rc.99'));
});

test('a stable version moves the files to the release channel and ships the analyzer rules', (t) => {
  const dir = sandbox(t);
  const result = setVersion(dir, STABLE);
  assert.equal(result.status, 0, output(result));
  assert.equal(version(dir), STABLE);
  assertVersionChecked(dir);
  assert.equal(/The channel changed/.test(result.stdout), current.includes('-'), result.stdout);

  for (const file of [DOCS, RU_DOCS]) {
    assert.match(read(dir, file), /\[!\[Release 99\.0\.0\]/, file);
    assert.doesNotMatch(read(dir, file), /\[!\[Preview /, file);
    assert.deepEqual([...new Set(installBranches(read(dir, file)))], ['upm'], file);
  }
  assert.match(read(dir, BADGE), />Release<\/text>/);
  assert.deepEqual([...new Set(installBranches(read(dir, `${PACKAGE}/README.md`)))], ['upm']);
  assert.match(read(dir, 'CHANGELOG.md'), /^## \[99\.0\.0\] — \d{4}-\d{2}-\d{2}$/m);

  assert.ok(read(dir, `${ANALYZER_RELEASES}.Shipped.md`).includes(`\n## Release ${STABLE}\n`));
  assert.ok(read(dir, `${ANALYZER_RELEASES}.Shipped.md`).includes('AFT9999'));
  assert.ok(!read(dir, `${ANALYZER_RELEASES}.Unshipped.md`).includes('AFT9999'));
});

test('a second run with the same version repairs files and changes nothing else', (t) => {
  const dir = sandbox(t);
  assert.equal(setVersion(dir, STABLE).status, 0);
  const done = snapshot(dir);
  const again = setVersion(dir, STABLE);
  assert.equal(again.status, 0, output(again));
  assert.deepEqual(snapshot(dir), done);

  // A hand-edited package.json is repaired from the version that the run is given.
  edit(dir, `${PACKAGE}/package.json`, (text) => text.replace(`"version": "${STABLE}"`, '"version": "99.0.1"'));
  assert.notEqual(checkVersion(dir).status, 0);
  assert.equal(setVersion(dir, STABLE).status, 0);
  assert.equal(version(dir), STABLE);
});

test('a version that is not SemVer is refused before any file changes', (t) => {
  const dir = sandbox(t);
  const before = snapshot(dir);
  for (const bad of ['1.0', '1.0.0.0', 'v1.0.0', '1.0.0-', '1.0.0 ', '1.0.0\nx', 'latest']) {
    const result = setVersion(dir, bad);
    assert.notEqual(result.status, 0, JSON.stringify(bad));
    assert.match(result.stderr, /is not a SemVer version/, JSON.stringify(bad));
  }
  assert.deepEqual(snapshot(dir), before);
});

test('a CHANGELOG with nothing under [Unreleased] is refused before any file changes', (t) => {
  const dir = sandbox(t);
  unreleased(dir, '\n');
  const before = snapshot(dir);
  const result = setVersion(dir, '1.0.0-rc.99');
  assert.notEqual(result.status, 0);
  assert.match(result.stderr, /no "## \[Unreleased\]" line with notes/);
  assert.deepEqual(snapshot(dir), before);
});

// Each change leaves one file out of step with package.json.
const OUT_OF_STEP = [
  {
    name: 'a badge with another version',
    file: DOCS,
    change: (text) => text.replaceAll(`${label} ${current}`, `${label} 0.9.0`),
    message: new RegExp(`has no "${label} ${literal(current)}" badge`),
  },
  {
    name: 'a badge with a longer version',
    file: RU_DOCS,
    change: (text) => text.replaceAll(`${label} ${current}`, `${label} ${current}0`),
    message: new RegExp(`has no "${label} ${literal(current)}" badge`),
  },
  {
    name: 'a badge label of the other channel',
    file: BADGE,
    change: (text) => text.replace(`>${label}</text>`, `>${otherLabel}</text>`),
    message: new RegExp(`the badge text is not "${label}"`),
  },
  {
    name: 'an install URL of the other channel',
    file: `${PACKAGE}/README.md`,
    change: (text) => text.replaceAll(`.git#${branch}`, `.git#${otherBranch}`),
    message: new RegExp(`install URLs must use #${branch} `),
  },
  {
    name: 'a badge that links another release',
    file: RU_DOCS,
    change: (text) => text.replaceAll(`/releases/tag/v${current})`, '/releases/tag/v0.9.0)'),
    message: /does not link the v\S+ release/,
  },
  {
    name: 'a CHANGELOG without the version section',
    file: 'CHANGELOG.md',
    change: (text) => text.replace(`## [${current}]`, '## [0.9.0]'),
    message: /has no "## \[\S+\]" section/,
  },
  {
    name: 'a CHANGELOG without the release link',
    file: 'CHANGELOG.ru.md',
    change: (text) => text.replace(`\n[${current}]: `, '\n[0.9.0]: '),
    message: /has no \[\S+\] release link/,
  },
];

for (const { name, file, change, message } of OUT_OF_STEP) {
  test(`the version check reports ${name}`, (t) => {
    const dir = sandbox(t);
    edit(dir, file, change);
    const result = checkVersion(dir);
    assert.equal(result.status, 1, output(result));
    assert.match(result.stdout, message);
    assert.ok(result.stdout.includes(`file=${file}::`), result.stdout);
  });
}

test('the version check compares the files with the version in package.json', (t) => {
  const dir = sandbox(t);
  // The release 1.0.0 against files of 1.0.0-rc.N, or a prerelease against files of 1.0.0.
  const other = current.includes('-') ? current.split('-')[0] : `${current}-rc.1`;
  edit(dir, `${PACKAGE}/package.json`, (text) => text.replace(/"version": "[^"]+"/, `"version": "${other}"`));
  const result = checkVersion(dir);
  assert.equal(result.status, 1, output(result));
  assert.match(result.stdout, new RegExp(`has no "(Preview|Release) ${literal(other)}" badge`));
  assert.match(result.stdout, /the install URLs must use #upm(-preview)? for /);
});
