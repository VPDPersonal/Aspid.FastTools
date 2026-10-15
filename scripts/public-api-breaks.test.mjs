// Tests of scripts/public-api-breaks.mjs, which the title job of pr-checks.yml uses to tell whether a pull request takes
// a member line out of PublicApi.txt. ci.yml runs it:
//   node --test scripts/*.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import {
  breakingTitleVerdict,
  isAdditiveChange,
  PUBLIC_API_FORMAT_PATH,
  PUBLIC_API_PATH,
  removedMemberLines,
  removedMemberLinesBetween,
} from './public-api-breaks.mjs';

const HEADER = [
  '# Public API of tech.aspid.fasttools, written by PublicApiSnapshotTests. Do not edit by hand.',
  '# Update it as Aspid.FastTools/AGENTS.md says: run the test with ASPID_FASTTOOLS_WRITE_PUBLIC_API=1.',
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

test('the title verdict is empty with a bang or without a lost line', () => {
  assert.deepEqual(breakingTitleVerdict(['public int Next { get; }'], true, false), { error: '', warning: '' });
  assert.deepEqual(breakingTitleVerdict(['public int Next { get; }'], true, true), { error: '', warning: '' });
  assert.deepEqual(breakingTitleVerdict([], false, false), { error: '', warning: '' });
  assert.deepEqual(breakingTitleVerdict([], false, true), { error: '', warning: '' });
});

test('the title verdict fails with the lost lines and what to do', () => {
  const { error, warning } = breakingTitleVerdict(['public int Next { get; }'], false, false);

  assert.equal(warning, '');
  assert.match(error, /lost 1 member line \(`public int Next \{ get; \}`\)/);
  assert.match(error, /`!` before the colon/);
  assert.match(error, /migration note under Changed/);
  assert.match(error, /CHANGELOG\.md and CHANGELOG\.ru\.md/);
});

test('the title verdict shows three lines and counts the rest', () => {
  const { error } = breakingTitleVerdict(['a', 'b', 'c', 'd', 'e'], false, false);

  assert.match(error, /lost 5 member lines \(`a`, `b`, `c`, and 2 more\)/);
});

test('the title verdict only warns when the snapshot format changed', () => {
  const { error, warning } = breakingTitleVerdict(['public int Next { get; }'], false, true);

  assert.equal(error, '');
  assert.match(warning, /lost 1 member line \(`public int Next \{ get; \}`\)/);
  assert.match(warning, /PublicApiSnapshot\.cs/);
  assert.match(warning, /snapshot format changed/);
  assert.match(warning, /maintainer must judge by hand/);
});

test('the format file is the code that writes the snapshot', () => {
  assert.equal(
    PUBLIC_API_FORMAT_PATH,
    'Aspid.FastTools/Packages/tech.aspid.fasttools/Tests/Editor/PublicApi/PublicApiSnapshot.cs');
});

// Lines are written as the snapshot writes them: access, modifiers, kind, name, base list, constraints.
const SAFE = [
  ['a new interface after the others',
    'public sealed class Foo : IBar', 'public sealed class Foo : IBar, IBaz'],
  ['a new interface between the others',
    'public sealed class Foo : IAlpha, IOmega', 'public sealed class Foo : IAlpha, IMiddle, IOmega'],
  ['the first interface',
    'public class Foo', 'public class Foo : IBar'],
  ['an interface of a struct',
    'public struct Foo : IBar', 'public struct Foo : IBar, IBaz'],
  ['a base class in front of the interfaces',
    'public class Foo : IBar', 'public class Foo : Base, IBar'],
  ['an interface in a base list with generic arguments',
    'public sealed class Map<TKey, TValue> : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.IDisposable',
    'public sealed class Map<TKey, TValue> : System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>, System.Collections.IEnumerable, System.IDisposable'],
  ['an interface of a class with constraints',
    'public class Foo<T> : IBar where T : struct, System.Enum', 'public class Foo<T> : IBar, IBaz where T : struct, System.Enum'],
  ['an interface of a protected nested class',
    'protected sealed class Foo', 'protected sealed class Foo : IBar'],
  ['the [Flags] attribute of an enum',
    'public enum Mode', '[Flags] public enum Mode'],
];

const LOSS = [
  ['sealed added to a class', 'public class Foo : IBar', 'public sealed class Foo : IBar'],
  ['abstract added to a class', 'public class Foo', 'public abstract class Foo'],
  ['static added to a class', 'public class Foo', 'public static class Foo'],
  ['sealed added together with an interface', 'public class Foo : IBar', 'public sealed class Foo : IBar, IBaz'],
  ['a constraint added to a type', 'public class Foo<T> : IBar', 'public class Foo<T> : IBar where T : struct'],
  ['a constraint changed on a type', 'public class Foo<T> where T : class', 'public class Foo<T> where T : struct'],
  ['an interface removed', 'public class Foo : IBar, IBaz', 'public class Foo : IBar'],
  ['an interface replaced', 'public class Foo : IBar', 'public class Foo : IQux'],
  ['the order of the interfaces changed', 'public class Foo : IBar, IBaz', 'public class Foo : IBaz, IBar'],
  ['the base class replaced', 'public class Foo : Base, IBar', 'public class Foo : Other, IBar'],
  ['a base interface added to an interface', 'public interface IFoo', 'public interface IFoo : IBar'],
  ['an underlying type added to an enum', 'public enum Mode', 'public enum Mode : byte'],
  ['an underlying type changed on an enum', 'public enum Mode : byte', 'public enum Mode : short'],
  ['an attribute taken from an enum', '[Flags] public enum Mode', 'public enum Mode'],
  ['a type renamed', 'public class Foo : IBar', 'public class Foo2 : IBar, IBaz'],
  ['readonly added to a field', 'public int Value', 'public readonly int Value'],
  ['virtual taken from a method', 'public virtual void Run()', 'public void Run()'],
  ['access narrowed', 'public void Run()', 'protected void Run()'],
  ['a return type changed', 'public int Next()', 'public long Next()'],
  ['a parameter type changed', 'public void Set(int value)', 'public void Set(long value)'],
  ['a parameter added', 'public void Set(int value)', 'public void Set(int value, int other)'],
  ['a delegate that grows', 'public delegate void Done()', 'public delegate void Done() where T : class'],
  ['a member line that grows', 'public int Count { get; }', 'public int Count { get; set; }'],
  ['the same line', 'public class Foo : IBar', 'public class Foo : IBar'],
];

for (const [name, before, after] of SAFE)
  test(`a line that gains ${name} is no loss`, () => assert.equal(isAdditiveChange(before, after), true));

for (const [name, before, after] of LOSS)
  test(`a line with ${name} stays a loss`, () => assert.equal(isAdditiveChange(before, after), false));

test('a patch that only extends a declaration removes nothing', () => {
  const lost = removedMemberLines(patch(
    ' Aspid.FastTools.Ids.Id',
    '-    public sealed class Id : IBar',
    '+    public sealed class Id : IBar, IBaz',
    '     public int Value { get; }'));

  assert.deepEqual(lost, []);
});

test('a patch that makes a declaration sealed or constrained loses it', () => {
  const lost = removedMemberLines(patch(
    '-    public class Id : IBar',
    '+    public sealed class Id : IBar',
    '-    public class Map<T> : IBar',
    '+    public class Map<T> : IBar where T : struct'));

  assert.deepEqual(lost, ['public class Id : IBar', 'public class Map<T> : IBar']);
});

test('an added declaration of another type extends nothing', () => {
  const lost = removedMemberLines(patch(
    '-    public class Id : IBar',
    '+    public class Map : IBar, IBaz'));

  assert.deepEqual(lost, ['public class Id : IBar']);
});

test('one added line extends only one removed line', () => {
  const lost = removedMemberLines(patch(
    '-    public class Id : IBar',
    '-    public class Id : IBar',
    '+    public class Id : IBar, IBaz'));

  assert.deepEqual(lost, ['public class Id : IBar']);
});

test('a patch that extends a declaration and drops a member loses only the member', () => {
  const lost = removedMemberLines(patch(
    '-    public sealed class Id : IBar',
    '+    public sealed class Id : IBar, IBaz',
    '-    public int Next { get; }'));

  assert.deepEqual(lost, ['public int Next { get; }']);
});

test('two versions: a declaration that gains an interface loses nothing', () => {
  const before = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public sealed class Id : IBar', '    public int Value { get; }');
  const after = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public sealed class Id : IBar, IBaz', '    public int Value { get; }');

  assert.deepEqual(removedMemberLinesBetween(before, after), []);
});

test('two versions: a declaration that gains sealed is lost, and so is one that gains text in another type', () => {
  const before = snapshot(
    '[Aspid.FastTools]', '',
    'Aspid.FastTools.Ids.Id', '    public class Id : IBar', '',
    'Aspid.FastTools.Ids.IdMap', '    public class IdMap : IBar');
  const after = snapshot(
    '[Aspid.FastTools]', '',
    'Aspid.FastTools.Ids.Id', '    public class Id : IBar, IBaz', '',
    'Aspid.FastTools.Ids.IdMap', '    public sealed class IdMap : IBar');

  assert.deepEqual(removedMemberLinesBetween(before, after), ['public class IdMap : IBar']);
});

test('two versions: an extended declaration in one type does not hide the loss in another', () => {
  const before = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public class Id : IBar', '', 'Aspid.FastTools.Ids.IdMap', '    public class Id : IBar');
  const after = snapshot('[Aspid.FastTools]', '', 'Aspid.FastTools.Ids.Id', '    public class Id : IBar, IBaz', '', 'Aspid.FastTools.Ids.IdMap');

  assert.deepEqual(removedMemberLinesBetween(before, after), ['public class Id : IBar']);
});
