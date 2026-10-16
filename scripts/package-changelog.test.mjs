// Tests of scripts/package-changelog.mjs. The script finds the repository from its own location, so each test copies
// it next to a fixture CHANGELOG.md. ci.yml runs it:
//   node --test scripts/*.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { cpSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';

const script = fileURLToPath(new URL('package-changelog.mjs', import.meta.url));
const PKG = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const BLOB = 'https://github.com/VPDPersonal/Aspid.FastTools/blob';

// The package copy of `changelog`, generated with `args`, without the header comment.
function generate(t, changelog, ...args) {
  const dir = mkdtempSync(join(tmpdir(), 'aspid-package-changelog-'));
  t.after(() => rmSync(dir, { recursive: true, force: true }));
  mkdirSync(join(dir, 'scripts'));
  mkdirSync(join(dir, PKG), { recursive: true });
  cpSync(script, join(dir, 'scripts/package-changelog.mjs'));
  writeFileSync(join(dir, 'CHANGELOG.md'), changelog);

  const result = spawnSync('node', ['scripts/package-changelog.mjs', ...args], { cwd: dir, encoding: 'utf8' });
  assert.equal(result.status, 0, `${result.stdout}${result.stderr}`);
  const text = readFileSync(join(dir, PKG, 'CHANGELOG.md'), 'utf8');
  const header = "<!-- Generated at release from the repository's CHANGELOG.md. Edit that file. -->\n\n";
  assert.ok(text.startsWith(header), text);
  return { text: text.slice(header.length), meta: readFileSync(join(dir, PKG, 'CHANGELOG.md.meta'), 'utf8') };
}

test('the text is copied as is, with the header comment and the committed GUID', (t) => {
  const changelog = '# Changelog\n\n## [Unreleased]\n\n### Fixed\n\n- *A* note with `code` and **bold**.\n';
  const { text, meta } = generate(t, changelog);
  assert.equal(text, changelog);
  assert.match(meta, /^guid: abb6755971db45c5bdfcb1c77c739335$/m);
  assert.match(meta, /^TextScriptImporter:$/m);
});

test('a link that leaves the package points at main, or at the given tag', (t) => {
  assert.equal(generate(t, 'See [the guide](CONTRIBUTING.md#setup).\n').text,
    `See [the guide](${BLOB}/main/CONTRIBUTING.md#setup).\n`);
  assert.equal(generate(t, 'See [the guide](./docs/a.md?plain=1).\n', 'v1.0.0').text,
    `See [the guide](${BLOB}/v1.0.0/docs/a.md?plain=1).\n`);
});

test('a link into the package becomes relative to the package', (t) => {
  assert.equal(generate(t, `[Setup](${PKG}/SETUP.md) and [samples](${PKG}/Samples~/Intro/README.md).\n`).text,
    '[Setup](SETUP.md) and [samples](Samples~/Intro/README.md).\n');
});

test('an absolute URL, a root path, an anchor and a mail link stay as they are', (t) => {
  const changelog = '[a](https://example.com/x.md) [b](/x.md) [c](#fixed) [d](mailto:me@example.com)\n';
  assert.equal(generate(t, changelog).text, changelog);
});

test('a reference definition is rebased like an inline link', (t) => {
  assert.equal(generate(t, '[1.0.0]: https://example.com/v1.0.0\n[guide]: Website/docs/README.md\n').text,
    `[1.0.0]: https://example.com/v1.0.0\n[guide]: ${BLOB}/main/Website/docs/README.md\n`);
});

test('a link with a title or an angle-bracket destination is rebased', (t) => {
  assert.equal(generate(t, '[a](docs/a.md "The A") [b](<docs/b c.md>) [c](docs/c.md \'C\')\n[d]: <docs/d.md>\n').text,
    `[a](${BLOB}/main/docs/a.md "The A") [b](<${BLOB}/main/docs/b c.md>) [c](${BLOB}/main/docs/c.md 'C')\n` +
    `[d]: <${BLOB}/main/docs/d.md>\n`);
});

test('a link in a code span or a fenced code block stays as it is', (t) => {
  const changelog = [
    'Write `[a](docs/a.md)` or ``[b](docs/b.md)``, then [c](docs/c.md).',
    '',
    '```md',
    '[d](docs/d.md)',
    '[e]: docs/e.md',
    '```',
    '',
    '~~~',
    '[f](docs/f.md)',
    '~~~',
    '',
    '[`code` in a link](docs/g.md)',
    '',
  ].join('\n');
  const expected = changelog
    .replace('[c](docs/c.md)', `[c](${BLOB}/main/docs/c.md)`)
    .replace('(docs/g.md)', `(${BLOB}/main/docs/g.md)`);
  assert.equal(generate(t, changelog).text, expected);
});

test('a stray or an escaped backtick opens no code span, and a span ends at a run as long as its opening', (t) => {
  const changelog = [
    'A stray ` backtick.',
    '',
    'See [a](docs/a.md), then `code`.',
    'An escaped \\` here, [b](docs/b.md), and \\` there.',
    '``a span with ` inside [c](docs/c.md)`` and [d](docs/d.md).',
    '',
  ].join('\n');
  const expected = changelog
    .replace('(docs/a.md)', `(${BLOB}/main/docs/a.md)`)
    .replace('(docs/b.md)', `(${BLOB}/main/docs/b.md)`)
    .replace('(docs/d.md)', `(${BLOB}/main/docs/d.md)`);
  assert.equal(generate(t, changelog).text, expected);
});
