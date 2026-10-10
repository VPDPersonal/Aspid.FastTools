// Tests of scripts/check-unity-minimum.mjs. The script finds the repository from its own location, so each test copies
// it next to fixture files. checks.yml runs it:
//   node --test scripts/*.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { cpSync, mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';

const script = fileURLToPath(new URL('check-unity-minimum.mjs', import.meta.url));
const PACKAGE_JSON = 'Aspid.FastTools/Packages/tech.aspid.fasttools/package.json';
const MATRIX = '.github/workflows/tests.yml';
const MENTIONS = [
  'AGENTS.md',
  '.github/claude-review.md',
  '.github/ISSUE_TEMPLATE/release_checklist.yml',
  'skills/aspid-visual-element-fluent/SKILL.md',
];

const valid = () => ({
  [PACKAGE_JSON]: JSON.stringify({ unity: '6000.0', unityRelease: '53f1' }),
  [MATRIX]: '# 6000.0.53f1 is the minimum.\n          - name: minimum\n            unity: 6000.0.53f1\n          - name: latest\n            unity: 6000.6.4f1\n',
  ...Object.fromEntries(MENTIONS.map((file) => [file, 'The package supports Unity 6000.0.53f1.\n'])),
});

function check(t, changes = {}) {
  const dir = mkdtempSync(join(tmpdir(), 'aspid-unity-minimum-'));
  t.after(() => rmSync(dir, { recursive: true, force: true }));
  mkdirSync(join(dir, 'scripts'));
  cpSync(script, join(dir, 'scripts/check-unity-minimum.mjs'));
  for (const [file, text] of Object.entries({ ...valid(), ...changes })) {
    mkdirSync(dirname(join(dir, file)), { recursive: true });
    writeFileSync(join(dir, file), text);
  }
  return spawnSync('node', ['scripts/check-unity-minimum.mjs'], { cwd: dir, encoding: 'utf8' });
}

test('files that all name the minimum of package.json pass', (t) => {
  const result = check(t);
  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.match(result.stdout, /minimum Unity 6000\.0\.53f1 is the same in 5 files/);
});

test('a file that names another minimum is reported', (t) => {
  for (const file of MENTIONS) {
    const result = check(t, { [file]: 'The package supports Unity 6000.0.40f1.\n' });
    assert.equal(result.status, 1, result.stdout + result.stderr);
    assert.ok(result.stdout.includes(`::error file=${file}::`), result.stdout);
    assert.match(result.stdout, /6000\.0\.53f1/);
  }
});

test('a second mention left on the old minimum is reported', (t) => {
  const result = check(t, { [MENTIONS[0]]: 'Unity 6000.0.53f1 is the minimum.\nThe package promises 6000.0.40f1.\n' });
  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.ok(result.stdout.includes(`::error file=${MENTIONS[0]}::names Unity 6000.0.40f1`), result.stdout);
});

test('a raised minimum in package.json reports every file that still names the old one', (t) => {
  const result = check(t, { [PACKAGE_JSON]: JSON.stringify({ unity: '6000.0', unityRelease: '60f1' }) });
  assert.equal(result.status, 1, result.stdout + result.stderr);
  for (const file of [...MENTIONS, MATRIX]) assert.ok(result.stdout.includes(`::error file=${file}::`), file);
});

test('the CI matrix must test the minimum, not only mention it', (t) => {
  const result = check(t, { [MATRIX]: '# 6000.0.53f1 is the minimum.\n          - name: latest\n            unity: 6000.6.4f1\n' });
  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.ok(result.stdout.includes(`::error file=${MATRIX}::`), result.stdout);
  assert.match(result.stdout, /matrix row "name: minimum" with "unity: 6000\.0\.53f1"/);
});

test('only the minimum row of the matrix counts', (t) => {
  const result = check(t, {
    [PACKAGE_JSON]: JSON.stringify({ unity: '6000.2', unityRelease: '15f1' }),
    [MATRIX]: '          - name: minimum\n            unity: 6000.0.53f1\n          - name: \'6000.2\'\n            unity: 6000.2.15f1\n',
    ...Object.fromEntries(MENTIONS.map((file) => [file, 'The package supports Unity 6000.2.15f1.\n'])),
  });
  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.ok(result.stdout.includes(`::error file=${MATRIX}::`), result.stdout);
});

test('a package.json without unityRelease names a minor version', (t) => {
  const text = 'The package supports Unity 6000.0.\n';
  const files = Object.fromEntries(MENTIONS.map((file) => [file, text]));
  const matrix = { [MATRIX]: '          - name: minimum\n            unity: 6000.0\n' };
  assert.equal(check(t, { [PACKAGE_JSON]: JSON.stringify({ unity: '6000.0' }), ...files, ...matrix }).status, 0);
  assert.equal(check(t, { [PACKAGE_JSON]: JSON.stringify({ unityRelease: '53f1' }) }).status, 1);
});
