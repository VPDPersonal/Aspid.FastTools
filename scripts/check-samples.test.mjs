// Tests of scripts/check-samples.mjs. The script finds the repository from its own location, so each test copies it
// next to fixture files. ci.yml runs it:
//   node --test scripts/*.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { cpSync, mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';

const script = fileURLToPath(new URL('check-samples.mjs', import.meta.url));
const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const DEMO = `${PACKAGE}/Samples~/Demo`;
const MARKERS = `${PACKAGE}/Samples~/ProfilerMarkers`;
const TUTORIAL = 'Website/tutorials/ProfilerMarkers/README.md';
const SITE = 'https://vpdpersonal.github.io/Aspid.FastTools/tutorials/';
const SHADER_GUID = 'a'.repeat(32);
const MATERIAL_GUID = 'b'.repeat(32);

const asmdef = (name, extra = {}) => JSON.stringify({ name, rootNamespace: name, references: [], defineConstraints: [], versionDefines: [], ...extra });
const code = (namespace, body) => `using UnityEngine;\n\nnamespace ${namespace}\n{\n${body}\n}\n`;
const type = (name, members = '') => `    public sealed class ${name} : MonoBehaviour\n    {\n${members}    }`;
const meta = (guid) => `fileFormatVersion: 2\nguid: ${guid}\n`;
const material = (shader = SHADER_GUID) => `--- !u!21 &2100000\nMaterial:\n  m_Name: Floor\n  m_Shader: {fileID: 4800000, guid: ${shader}, type: 3}\n`;
const scene = (extra = '') => [
  '%YAML 1.1',
  '--- !u!29 &1', 'OcclusionCullingSettings:',
  '--- !u!1 &10', 'GameObject:', '  m_Name: Floor',
  '--- !u!33 &11', 'MeshFilter:', '  m_Mesh: {fileID: 10209, guid: 0000000000000000e000000000000000, type: 0}',
  '--- !u!23 &12', 'MeshRenderer:', '  m_Materials:', `  - {fileID: 2100000, guid: ${MATERIAL_GUID}, type: 2}`,
  '--- !u!1 &20', 'GameObject:', '  m_Name: Main Camera',
  '--- !u!20 &21', 'Camera:', '  m_GameObject: {fileID: 20}', '  m_ClearFlags: 2',
  extra,
].join('\n');
const sample = (name, slug) =>
  ({ displayName: name, description: `Press Play in ${name}.unity to see it work. Tutorial: ${SITE}${slug}`, path: `Samples~/${name}` });

const flock = [
  'using UnityEngine;',
  '',
  'namespace Demo.Markers',
  '{',
  '    public sealed class Flock : MonoBehaviour',
  '    {',
  '        private void Update()',
  '        {',
  '            using var _ = this.Marker();',
  '',
  '            using (this.Marker().WithName("Steering"))',
  '                Step();',
  '        }',
  '',
  '        private void Step() { }',
  '    }',
  '}',
  '',
].join('\n');

const valid = () => ({
  [`${PACKAGE}/package.json`]: JSON.stringify({ samples: [sample('Demo', 'demo'), sample('ProfilerMarkers', 'profiler-markers')] }),
  [`${DEMO}/Demo.asmdef`]: asmdef('Demo'),
  [`${DEMO}/Spawner.cs`]: code('Demo', type('Spawner', '        [SerializeField] private string _url = "https://example.com/a//b";\n')),
  [`${DEMO}/Demo.unity`]: scene(),
  [`${DEMO}/Art/Studio.shader`]: 'Shader "Aspid/Samples/Demo/Studio"\n{\n}\n',
  [`${DEMO}/Art/Studio.shader.meta`]: meta(SHADER_GUID),
  [`${DEMO}/Art/Floor.mat`]: material(),
  [`${DEMO}/Art/Floor.mat.meta`]: meta(MATERIAL_GUID),
  [`${DEMO}/Style.uss`]: '.demo {\n    padding: 4px;\n}\n',
  [`${MARKERS}/Demo.Markers.asmdef`]: asmdef('Demo.Markers'),
  [`${MARKERS}/Flock.cs`]: flock,
  'Website/tutorials/Demo/README.md': '# Demo\n',
  'Website/i18n/ru/docusaurus-plugin-content-docs-tutorials/current/Demo/README.md': '# Demo\n',
  [TUTORIAL]: '# ProfilerMarkers\n\nFind `Flock.Update (9)` and `Flock.Steering (11)`.\n',
  'Website/i18n/ru/docusaurus-plugin-content-docs-tutorials/current/ProfilerMarkers/README.md': '# ProfilerMarkers\n',
});

function check(t, changes = {}) {
  const dir = mkdtempSync(join(tmpdir(), 'aspid-check-samples-'));
  t.after(() => rmSync(dir, { recursive: true, force: true }));
  mkdirSync(join(dir, 'scripts'));
  cpSync(script, join(dir, 'scripts/check-samples.mjs'));
  for (const [file, text] of Object.entries({ ...valid(), ...changes })) {
    if (text === null) continue;
    mkdirSync(dirname(join(dir, file)), { recursive: true });
    writeFileSync(join(dir, file), text);
  }
  return spawnSync('node', ['scripts/check-samples.mjs'], { cwd: dir, encoding: 'utf8' });
}

function assertFails(result, file, message) {
  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.ok(result.stdout.includes(`::error file=${file}::`), result.stdout);
  assert.match(result.stdout, message);
}

test('samples that follow the rules pass, and "//" inside a string is no comment', (t) => {
  const result = check(t);
  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.match(result.stdout, /2 samples follow the Samples~ rules/);
});

test('a comment, a tooltip, a header, a ReSharper banner and Debug. are reported', (t) => {
  for (const [line, message] of [
    ['        // Spawns enemies.\n', /has a comment/],
    ['        /// <summary>Spawns.</summary>\n', /has a comment/],
    ['        /* old */\n', /has a comment/],
    ['        [Tooltip("Count")] [SerializeField] private int _count;\n', /\[Tooltip\]/],
    ['        [Header("Setup")] [SerializeField] private int _count;\n', /\[Header\]/],
    ['        private void Start() => Debug.Log(name);\n', /Debug\./],
  ]) {
    assertFails(check(t, { [`${DEMO}/Spawner.cs`]: code('Demo', type('Spawner', line)) }), `${DEMO}/Spawner.cs`, message);
  }
  const banner = `// ReSharper disable once CheckNamespace\n${code('Demo', type('Spawner'))}`;
  assertFails(check(t, { [`${DEMO}/Spawner.cs`]: banner }), `${DEMO}/Spawner.cs`, /has a comment/);
});

test('lights, physics, text and CreatePrimitive in code are reported', (t) => {
  for (const line of [
    '        private void Start() => GetComponent<Collider>().enabled = false;\n',
    '        private void Start() => Physics.Raycast(transform.position, Vector3.down);\n',
    '        private void Start() => GameObject.CreatePrimitive(PrimitiveType.Cube);\n',
    '        [SerializeField] private Light _sun;\n',
    '        [SerializeField] private TextMesh _label;\n',
  ]) {
    assertFails(check(t, { [`${DEMO}/Spawner.cs`]: code('Demo', type('Spawner', line)) }), `${DEMO}/Spawner.cs`, /samples do not/);
  }
});

test('two top-level types in a file and a file named after another type are reported', (t) => {
  const two = code('Demo', `${type('Spawner')}\n\n    public interface IPattern { }`);
  assertFails(check(t, { [`${DEMO}/Spawner.cs`]: two }), `${DEMO}/Spawner.cs`, /declares 2 top-level types/);
  assertFails(check(t, { [`${DEMO}/Spawner.cs`]: code('Demo', type('Enemy')) }), `${DEMO}/Spawner.cs`, /must be named after its type/);
});

test('code outside the asmdef folder, a missing rootNamespace and another namespace are reported', (t) => {
  assertFails(check(t, { [`${DEMO}/Scripts/Enemy.cs`]: code('Demo', type('Enemy')) }), `${DEMO}/Scripts/Enemy.cs`, /not beside an \.asmdef/);
  assertFails(check(t, { [`${DEMO}/Demo.asmdef`]: asmdef('Demo', { rootNamespace: '' }) }), `${DEMO}/Demo.asmdef`, /no rootNamespace/);
  assertFails(check(t, { [`${DEMO}/Spawner.cs`]: code('Demo.Spawning', type('Spawner')) }), `${DEMO}/Spawner.cs`, /rootNamespace is Demo/);
});

test('defineConstraints and versionDefines are reported', (t) => {
  const gated = asmdef('Demo', { defineConstraints: ['ASPID_PHYSICS'] });
  assertFails(check(t, { [`${DEMO}/Demo.asmdef`]: gated }), `${DEMO}/Demo.asmdef`, /defineConstraints/);
  const versioned = asmdef('Demo', { versionDefines: [{ name: 'com.unity.modules.physics', define: 'ASPID_PHYSICS' }] });
  assertFails(check(t, { [`${DEMO}/Demo.asmdef`]: versioned }), `${DEMO}/Demo.asmdef`, /versionDefines/);
});

test('a font, art outside Art/, a foreign shader and a misnamed shader are reported', (t) => {
  assertFails(check(t, { [`${DEMO}/Fonts/Plex.ttf`]: 'font' }), `${DEMO}/Fonts/Plex.ttf`, /is a \.ttf file/);
  assertFails(check(t, { [`${DEMO}/Floor.mat`]: material() }), `${DEMO}/Floor.mat`, /belongs in Demo\/Art/);
  assertFails(check(t, { [`${DEMO}/Art/Floor.mat`]: material('c'.repeat(32)) }), `${DEMO}/Art/Floor.mat`, /sample's own Aspid\/Samples\/Demo\/Studio/);
  assertFails(check(t, { [`${DEMO}/Art/Studio.shader`]: 'Shader "Unlit/Color"\n{\n}\n' }), `${DEMO}/Art/Studio.shader`, /is named Unlit\/Color/);
});

test('a light, a collider, a built-in mesh, a foreign material and a second camera in a scene are reported', (t) => {
  const file = `${DEMO}/Demo.unity`;
  assertFails(check(t, { [file]: scene('--- !u!108 &30\nLight:') }), file, /class ID 108/);
  assertFails(check(t, { [file]: scene('--- !u!65 &30\nBoxCollider:') }), file, /class ID 65/);
  assertFails(check(t, { [file]: scene().replace('fileID: 10209', 'fileID: 10204') }), file, /built-in mesh 10204/);
  assertFails(check(t, { [file]: scene().replace(MATERIAL_GUID, 'd'.repeat(32)) }), file, /not in Demo\/Art/);
  const second = '--- !u!1 &40\nGameObject:\n  m_Name: Minimap\n--- !u!20 &41\nCamera:\n  m_GameObject: {fileID: 40}\n  m_ClearFlags: 2';
  assertFails(check(t, { [file]: scene(second) }), file, /Camera on Minimap/);
  assertFails(check(t, { [file]: scene().replace('m_ClearFlags: 2', 'm_ClearFlags: 1') }), file, /solid colour/);
});

test('a comment in a style sheet and more than 150 lines of C# are reported', (t) => {
  assertFails(check(t, { [`${DEMO}/Style.uss`]: '/* card */\n.demo { }\n' }), `${DEMO}/Style.uss`, /style sheets carry none/);
  const long = code('Demo', type('Spawner', '        private int _a;\n'.repeat(150)));
  assertFails(check(t, { [`${DEMO}/Spawner.cs`]: long }), DEMO, /lines of C#; a sample has at most 150/);
});

test('a description that is long, two sentences or lacks its tutorial link, and a missing tutorial are reported', (t) => {
  const file = `${PACKAGE}/package.json`;
  const withDemo = (description) => JSON.stringify({ samples: [{ ...sample('Demo', 'demo'), description }, sample('ProfilerMarkers', 'profiler-markers')] });
  assertFails(check(t, { [file]: withDemo(`${'A'.repeat(200)}. Tutorial: ${SITE}demo`) }), file, /201 characters/);
  assertFails(check(t, { [file]: withDemo(`Press Play. Watch it. Tutorial: ${SITE}demo`) }), file, /one sentence/);
  assertFails(check(t, { [file]: withDemo('Press Play in Demo.unity.') }), file, /must end with " Tutorial: /);
  assertFails(check(t, { 'Website/tutorials/Demo/README.md': null }), file, /no tutorial page Website\/tutorials\/Demo\/README\.md/);
});

test('a sample folder that package.json does not list, and a listed path that differs, are reported', (t) => {
  assertFails(check(t, { [`${PACKAGE}/Samples~/Extra/Extra.asmdef`]: asmdef('Extra') }), `${PACKAGE}/Samples~/Extra`, /not listed in package\.json/);
  const moved = JSON.stringify({ samples: [{ ...sample('Demo', 'demo'), path: 'Samples~/Old' }, sample('ProfilerMarkers', 'profiler-markers')] });
  assertFails(check(t, { [`${PACKAGE}/package.json`]: moved }), `${PACKAGE}/package.json`, /path Samples~\/Old/);
});

test('a tutorial marker name whose line differs from the code is reported', (t) => {
  const stale = '# ProfilerMarkers\n\nFind `Flock.Update (9)` and `Flock.Steering (12)`.\n';
  assertFails(check(t, { [TUTORIAL]: stale }), TUTORIAL, /quotes Flock\.Steering \(12\); the code makes Flock\.Steering \(11\)/);
  const gone = '# ProfilerMarkers\n\nFind `Flock.Update (9)` and `Flock.Spawn (30)`.\n';
  assertFails(check(t, { [TUTORIAL]: gone }), TUTORIAL, /quotes Flock\.Spawn \(30\); the code makes no such marker/);
});

test('a tutorial that quotes no current marker name is skipped', (t) => {
  const result = check(t, { [TUTORIAL]: '# ProfilerMarkers\n\nFind `Flock.Update (74)` in the Profiler.\n' });
  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.ok(result.stdout.includes(`::notice file=${TUTORIAL}::quotes no current marker name`), result.stdout);
});
