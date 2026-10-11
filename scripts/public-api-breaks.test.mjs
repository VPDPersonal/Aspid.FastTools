// Tests of scripts/public-api-breaks.mjs, which the title job of pr-checks.yml uses to tell whether a pull request takes
// a member line out of PublicApi.txt. ci.yml runs it:
//   node --test scripts/*.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { breakingTitleError, PUBLIC_API_PATH, removedMemberLines, removedMemberLinesBetween } from './public-api-breaks.mjs';

const HEADER = [
  '# Public API of tech.aspid.fasttools, written by PublicApiSnapshotTests. Do not edit by hand.',
  '# Update it as AGENTS.md says: run the test with ASPID_FASTTOOLS_WRITE_PUBLIC_API=1.',
  '',
];

const snapshot = (...lines) => [...HEADER, ...lines, ''].join('\n');

// The shape of the files API: hunks with three lines of context, no `---` header.
const patch = (...lines) => ['@@ -10,7 +10,7 @@ Aspid.FastTools.Ids.Id', ...lines].join('\n');

test('the snapshot path is the one the package test writes', () => {
  assert.equal(
    PUBLIC_API_PATH,
    'Aspid.FastTools/Packages/tech.aspid.fasttools/Tests/Editor/PublicApi/PublicApi.txt');
});

test('a patch that only adds lines removes nothing', () => {
  const lost = removedMemberLines(patch(
    ' Aspid.FastTools.Ids.Id',
    '     public int Value { get; }',
    '+    public int Next { get; }',
    ' ',
    '+Aspid.FastTools.Ids.IdMap',
    '+    public sealed class IdMap',
    ' Aspid.FastTools.Ids.IdRange'));

  assert.deepEqual(lost, []);
});

test('a removed member line is lost', () => {
  const lost = removedMemberLines(patch(
    ' Aspid.FastTools.Ids.Id',
    '     public int Value { get; }',
    '-    public int Next { get; }',
    '     public override string ToString()'));

  assert.deepEqual(lost, ['public int Next { get; }']);
});

test('a changed member line is lost, whatever replaces it', () => {
  const lost = removedMemberLines(patch(
    '-    public void Set(int value)',
    '+    public void Set(long value)',
    '-    public int Value { get; }',
    '+    public int Value { get; set; }'));

  assert.deepEqual(lost, ['public void Set(int value)', 'public int Value { get; }']);
});

test('a removed type or assembly line is lost too', () => {
  const lost = removedMemberLines(patch(
    '-[Aspid.FastTools.Editor]',
    '-',
    '-Aspid.FastTools.Ids.IdMap',
    '-    public sealed class IdMap'));

  assert.deepEqual(lost, ['[Aspid.FastTools.Editor]', 'Aspid.FastTools.Ids.IdMap', 'public sealed class IdMap']);
});

test('a comment header or a blank line that goes is not a member line', () => {
  const lost = removedMemberLines(patch(
    '-# Public API of tech.aspid.fasttools, written by PublicApiSnapshotTests. Do not edit by hand.',
    '+# Public API of tech.aspid.fasttools, written by the snapshot test. Do not edit by hand.',
    '-',
    '-   ',
    '\\ No newline at end of file'));

  assert.deepEqual(lost, []);
});

test('a patch with Windows line endings is read the same way', () => {
  const lost = removedMemberLines(patch('-    public int Next { get; }', '+    public long Next { get; }').replace(/\n/g, '\r\n'));

  assert.deepEqual(lost, ['public int Next { get; }']);
});

test('an empty patch (a rename without changes) removes nothing', () => {
  assert.deepEqual(removedMemberLines(''), []);
});

test('two versions that differ only in comments and blank lines lose nothing', () => {
  const before = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public int Value { get; }');
  const after = ['# A new header.', '', '[Aspid.FastTools]', '', '', 'Aspid.FastTools.Ids.Id', '    public int Value { get; }'].join('\n');

  assert.deepEqual(removedMemberLinesBetween(before, after), []);
});

test('two versions where lines are only added lose nothing', () => {
  const before = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public int Value { get; }');
  const after = snapshot(
    '[Aspid.FastTools]', '',
    'Aspid.FastTools.Ids.Id', '    public int Next { get; }', '    public int Value { get; }', '',
    'Aspid.FastTools.Ids.IdMap', '    public sealed class IdMap');

  assert.deepEqual(removedMemberLinesBetween(before, after), []);
});

test('two versions: a member removed from one type is lost although another type keeps the same line', () => {
  const before = snapshot(
    '[Aspid.FastTools]', '',
    'Aspid.FastTools.Ids.Id', '    public int Count { get; }', '',
    'Aspid.FastTools.Ids.IdMap', '    public int Count { get; }');
  const after = snapshot(
    '[Aspid.FastTools]', '',
    'Aspid.FastTools.Ids.Id', '    public int Count { get; }', '',
    'Aspid.FastTools.Ids.IdMap');

  assert.deepEqual(removedMemberLinesBetween(before, after), ['public int Count { get; }']);
});

test('two versions: a changed member and a removed type are lost', () => {
  const before = snapshot(
    '[Aspid.FastTools]', '',
    'Aspid.FastTools.Ids.Id', '    public void Set(int value)', '',
    'Aspid.FastTools.Ids.IdMap', '    public sealed class IdMap');
  const after = snapshot(
    '[Aspid.FastTools]', '',
    'Aspid.FastTools.Ids.Id', '    public void Set(long value)');

  assert.deepEqual(removedMemberLinesBetween(before, after),
    ['public void Set(int value)', 'Aspid.FastTools.Ids.IdMap', 'public sealed class IdMap']);
});

test('two versions: a type that moves to another assembly is lost from the first one', () => {
  const before = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public int Value { get; }');
  const after = snapshot('[Aspid.FastTools.Editor]', '', 'Aspid.FastTools.Ids.Id', '    public int Value { get; }');

  assert.deepEqual(removedMemberLinesBetween(before, after),
    ['[Aspid.FastTools]', 'Aspid.FastTools.Ids.Id', 'public int Value { get; }']);
});

test('two versions: a snapshot that is created or deleted', () => {
  const text = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public int Value { get; }');

  assert.deepEqual(removedMemberLinesBetween('', text), []);
  assert.deepEqual(removedMemberLinesBetween(text, ''), ['[Aspid.FastTools]', 'Aspid.FastTools.Ids.Id', 'public int Value { get; }']);
});

test('the committed snapshot loses a line only when a line is taken out of it', () => {
  const text = readFileSync(fileURLToPath(new URL(`../${PUBLIC_API_PATH}`, import.meta.url)), 'utf8');
  const lines = text.split('\n');
  const index = lines.findIndex((line) => line.startsWith('    public '));
  assert.ok(index > 0, 'the snapshot must hold indented member lines');

  assert.deepEqual(removedMemberLinesBetween(text, text), []);
  assert.deepEqual(removedMemberLinesBetween(text, lines.filter((_, i) => i !== index).join('\n')), [lines[index].trim()]);
  assert.deepEqual(removedMemberLinesBetween(text, `# A new header.\n\n${lines.filter((line) => !line.startsWith('#')).join('\n')}`), []);
});

test('the title error is empty with a bang or without a lost line', () => {
  assert.equal(breakingTitleError(['public int Next { get; }'], true), '');
  assert.equal(breakingTitleError([], false), '');
});

test('the title error names the lost lines and what to do', () => {
  const error = breakingTitleError(['public int Next { get; }'], false);

  assert.match(error, /lost 1 member line \(`public int Next \{ get; \}`\)/);
  assert.match(error, /`!` before the colon/);
  assert.match(error, /migration note under Changed/);
  assert.match(error, /CHANGELOG\.md and CHANGELOG\.ru\.md/);
});

test('the title error shows three lines and counts the rest', () => {
  const error = breakingTitleError(['a', 'b', 'c', 'd', 'e'], false);

  assert.match(error, /lost 5 member lines \(`a`, `b`, `c`, and 2 more\)/);
});
