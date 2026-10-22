// Tests of scripts/check-package-files.mjs. The script finds the repository from its own location, so each test copies
// it into a small package of fixture files, breaks that package in one way and reads the verdict. ci.yml runs it:
//   node --test scripts/*.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { cpSync, mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';

const script = fileURLToPath(new URL('check-package-files.mjs', import.meta.url));
const inputs = fileURLToPath(new URL('unity-inputs.mjs', import.meta.url));
const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const DEV_TESTS = 'Aspid.FastTools/Assets/DevTests~';
const DEV_TOOLS = 'Aspid.FastTools/Assets/DevTools';
const SAMPLE_TESTS = 'Aspid.FastTools/Assets/SampleTests~';

const guid = (number) => number.toString(16).padStart(32, '0');
const meta = (number, importer = '') => `fileFormatVersion: 2\nguid: ${guid(number)}\n${importer}`;
const MONO = 'MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n';
const FOLDER = 'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n';
const asmdef = (name, references = []) => JSON.stringify({ name, references });

const LICENSE = 'MIT License\n\nCopyright (c) 2025 Vladislav Panin\n';

// A package that passes: a runtime and an editor assembly, a sample, a hidden file, a dev test, a dev tool, a sample
// test, and the license copy.
const valid = () => ({
  LICENSE,
  [`${PACKAGE}/LICENSE.md`]: LICENSE,
  [`${PACKAGE}/LICENSE.md.meta`]: meta(11),
  [`${PACKAGE}/package.json`]: JSON.stringify({ name: 'tech.aspid.fasttools', samples: [{ displayName: 'Demo', path: 'Samples~/Demo' }] }),
  [`${PACKAGE}/package.json.meta`]: meta(1),
  [`${PACKAGE}/.DS_Store`]: '',
  [`${PACKAGE}/Runtime.meta`]: meta(2, FOLDER),
  [`${PACKAGE}/Runtime/Aspid.FastTools.asmdef`]: asmdef('Aspid.FastTools'),
  [`${PACKAGE}/Runtime/Aspid.FastTools.asmdef.meta`]: meta(3),
  [`${PACKAGE}/Runtime/Item.cs`]: 'class Item { }',
  [`${PACKAGE}/Runtime/Item.cs.meta`]: meta(4, MONO),
  [`${PACKAGE}/Editor.meta`]: meta(5, FOLDER),
  [`${PACKAGE}/Editor/Aspid.FastTools.Editor.asmdef`]: asmdef('Aspid.FastTools.Editor', ['Aspid.FastTools', `GUID:${guid(3)}`, 'Unity.Mathematics']),
  [`${PACKAGE}/Editor/Aspid.FastTools.Editor.asmdef.meta`]: meta(6),
  [`${PACKAGE}/Samples~/Demo.meta`]: meta(7, FOLDER),
  [`${PACKAGE}/Samples~/Demo/Readme.md`]: '# Demo',
  [`${PACKAGE}/Samples~/Demo/Readme.md.meta`]: meta(8),
  [`${DEV_TESTS}/DevItem.cs`]: 'class DevItem { }',
  [`${DEV_TESTS}/DevItem.cs.meta`]: meta(9, MONO),
  [`${DEV_TOOLS}/DevTool.cs`]: 'class DevTool { }',
  [`${DEV_TOOLS}/DevTool.cs.meta`]: meta(12, MONO),
  [`${SAMPLE_TESTS}/SampleTest.cs`]: 'class SampleTest { }',
  [`${SAMPLE_TESTS}/SampleTest.cs.meta`]: meta(13, MONO),
});

// Run the script on the fixture files; a value of null removes a file of the valid package.
function check(t, changes = {}) {
  const dir = mkdtempSync(join(tmpdir(), 'aspid-package-files-'));
  t.after(() => rmSync(dir, { recursive: true, force: true }));
  mkdirSync(join(dir, 'scripts'));
  cpSync(script, join(dir, 'scripts/check-package-files.mjs'));
  cpSync(inputs, join(dir, 'scripts/unity-inputs.mjs'));
  for (const [file, text] of Object.entries({ ...valid(), ...changes })) {
    if (text === null) continue;
    mkdirSync(dirname(join(dir, file)), { recursive: true });
    writeFileSync(join(dir, file), text);
  }
  return spawnSync('node', ['scripts/check-package-files.mjs'], { cwd: dir, encoding: 'utf8' });
}

function assertFails(result, file, message) {
  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.ok(result.stdout.includes(`::error file=${file}::`), result.stdout);
  assert.match(result.stdout, message);
}

test('a package with a .meta for every file, unique guids and resolving references passes', (t) => {
  const result = check(t);
  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.match(result.stdout, /Package files OK/);
});

test('a file without a .meta is reported, also in Samples~, in a folder and in the dev project', (t) => {
  const metas = [`${PACKAGE}/Runtime/Item.cs.meta`, `${PACKAGE}/Samples~/Demo/Readme.md.meta`, `${PACKAGE}/Runtime.meta`, `${DEV_TESTS}/DevItem.cs.meta`, `${DEV_TOOLS}/DevTool.cs.meta`];
  for (const file of metas) assertFails(check(t, { [file]: null }), file.slice(0, -'.meta'.length), /no \.meta;/);
});

test('a .meta without its asset is reported', (t) => {
  assertFails(check(t, { [`${PACKAGE}/Runtime/Gone.cs.meta`]: meta(10, MONO) }), `${PACKAGE}/Runtime/Gone.cs.meta`, /no asset for this \.meta/);
});

test('a .meta without a valid guid is reported', (t) => {
  for (const text of ['fileFormatVersion: 2\n', `fileFormatVersion: 2\nguid: ${'A'.repeat(32)}\n`, 'fileFormatVersion: 2\nguid: 1234\n']) {
    assertFails(check(t, { [`${PACKAGE}/Runtime/Item.cs.meta`]: text + MONO }), `${PACKAGE}/Runtime/Item.cs.meta`, /no "guid:" line/);
  }
});

test('a guid used twice is reported, also between the package and the dev tests', (t) => {
  assertFails(check(t, { [`${PACKAGE}/Samples~/Demo/Readme.md.meta`]: meta(4) }), `${PACKAGE}/Samples~/Demo/Readme.md.meta`, /guid 0+4 is also the guid of/);
  assertFails(check(t, { [`${DEV_TESTS}/DevItem.cs.meta`]: meta(4, MONO) }), `${DEV_TESTS}/DevItem.cs.meta`, /guid 0+4 is also the guid of/);
});

test('a sample path that is no folder of the package is reported', (t) => {
  for (const path of ['Samples~/Missing', 'package.json']) {
    const manifest = JSON.stringify({ name: 'tech.aspid.fasttools', samples: [{ displayName: 'Demo', path }] });
    assertFails(check(t, { [`${PACKAGE}/package.json`]: manifest }), `${PACKAGE}/package.json`, new RegExp(`the path "${path}" of the sample "Demo" is not a folder`));
  }
});

test('a sample without a path is reported and does not crash the script', (t) => {
  const manifest = JSON.stringify({ name: 'tech.aspid.fasttools', samples: [{ displayName: 'Demo' }] });
  assertFails(check(t, { [`${PACKAGE}/package.json`]: manifest }), `${PACKAGE}/package.json`, /the sample "Demo" has no "path"/);
});

test('a folder that Unity ignores needs no .meta for its content, except Samples~', (t) => {
  const doc = `${PACKAGE}/Documentation~/Guide.md`;
  assert.equal(check(t, { [doc]: '# Guide' }).status, 0);
  // Unity writes no .meta into Samples~ of the dev project, so the hint names the import instead.
  const readme = `${PACKAGE}/Samples~/Demo/Readme.md`;
  assertFails(check(t, { [`${readme}.meta`]: null }), readme, /no \.meta; import the sample into a project/);
});

test('an asmdef reference that is no assembly of the package is reported', (t) => {
  const editor = `${PACKAGE}/Editor/Aspid.FastTools.Editor.asmdef`;
  assertFails(check(t, { [editor]: asmdef('Aspid.FastTools.Editor', ['Aspid.FastTools.Typo']) }), editor, /the reference "Aspid\.FastTools\.Typo" is no asmdef/);
  assertFails(check(t, { [editor]: asmdef('Aspid.FastTools.Editor', [`GUID:${guid(99)}`]) }), editor, /the reference "GUID:0+63" is no asmdef/);
  // A GUID reference to the .meta of a file that is no asmdef is no assembly either.
  assertFails(check(t, { [editor]: asmdef('Aspid.FastTools.Editor', [`GUID:${guid(4)}`]) }), editor, /the reference "GUID:0+4" is no asmdef/);
});

test('an asmdef that is not JSON is reported', (t) => {
  const runtime = `${PACKAGE}/Runtime/Aspid.FastTools.asmdef`;
  assertFails(check(t, { [runtime]: '{ "name": ' }), runtime, /not valid JSON/);
});

test('a path of 140 characters from the install folder is reported, but not in Samples~', (t) => {
  const name = `${'a'.repeat(120)}.txt`;
  const tooLong = `${PACKAGE}/Runtime/${name}`;
  assertFails(check(t, { [tooLong]: '', [`${tooLong}.meta`]: meta(10) }), tooLong, /path is \d+ characters/);
  const sample = `${PACKAGE}/Samples~/Demo/${name}`;
  assert.equal(check(t, { [sample]: '', [`${sample}.meta`]: meta(10) }).status, 0);
});

test('a .cs.meta without a MonoImporter block is reported, but not in Samples~', (t) => {
  assertFails(check(t, { [`${PACKAGE}/Runtime/Item.cs.meta`]: meta(4) }), `${PACKAGE}/Runtime/Item.cs.meta`, /no MonoImporter block/);
  const sample = `${PACKAGE}/Samples~/Demo/Sample.cs`;
  assert.equal(check(t, { [sample]: 'class Sample { }', [`${sample}.meta`]: meta(10) }).status, 0);
});

test('a .uss.meta without a ScriptedImporter block or with the ThemeStyleSheetImporter is reported', (t) => {
  const uss = `${PACKAGE}/Samples~/Demo/Style.uss`;
  const importer = (id) => `ScriptedImporter:\n  script: {fileID: ${id}, guid: 0000000000000000f000000000000000, type: 3}\n`;
  assert.equal(check(t, { [uss]: '', [`${uss}.meta`]: meta(10, importer(12385)) }).status, 0);
  assertFails(check(t, { [uss]: '', [`${uss}.meta`]: meta(10, importer(12388)) }), `${uss}.meta`, /importer fileID 12388/);
  assertFails(check(t, { [uss]: '', [`${uss}.meta`]: meta(10, 'timeCreated: 1777385488\n') }), `${uss}.meta`, /no ScriptedImporter block/);
});

test('a package LICENSE.md that is missing or differs from the root LICENSE is reported', (t) => {
  const copy = `${PACKAGE}/LICENSE.md`;
  assertFails(check(t, { [copy]: LICENSE.replace('2025', '2026') }), copy, /differs from LICENSE/);
  // A different line ending is a different file too.
  assertFails(check(t, { [copy]: LICENSE.replaceAll('\n', '\r\n') }), copy, /differs from LICENSE/);
  assertFails(check(t, { [copy]: null, [`${copy}.meta`]: null }), copy, /no LICENSE\.md in the package/);
  assertFails(check(t, { LICENSE: null }), 'LICENSE', /no LICENSE at the repository root/);
});

test('a package file that refers to a file the Unity CI ignores is reported', (t) => {
  const scene = `${PACKAGE}/Samples~/Demo/Demo.unity`;
  const sceneFiles = (text) => ({ [scene]: text, [`${scene}.meta`]: meta(14) });
  assertFails(check(t, sceneFiles(`m_Text: {guid: ${guid(8)}, type: 3}`)), scene, /refers to .*Readme\.md by its guid/);
  assertFails(check(t, { [`${PACKAGE}/Runtime/Item.cs`]: 'class Item { string A = "Demo/Readme.md"; }' }),
    `${PACKAGE}/Runtime/Item.cs`, /names "Readme\.md" in quotes/);
  assertFails(check(t, { [`${SAMPLE_TESTS}/SampleTest.cs`]: `class SampleTest { string A = "${guid(8)}"; }` }),
    `${SAMPLE_TESTS}/SampleTest.cs`, /refers to .*Readme\.md by its guid/);
  // A mention in prose and a guid of an input are no references.
  const result = check(t, { ...sceneFiles(`m_Script: {guid: ${guid(4)}}`), [`${PACKAGE}/Runtime/Item.cs`]: '// See Readme.md\nclass Item { }' });
  assert.equal(result.status, 0, result.stdout + result.stderr);
});
