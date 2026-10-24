// Check the package samples against the rules of the asp-ft-samples skill: code free of comments but the CheckNamespace banner,
// one type per file, namespaces that follow the folders below the asmdef's rootNamespace, each kind of file in its folder, scenes with no lights,
// physics or foreign materials, one-sentence package.json descriptions, and ProfilerMarkers marker names that match its
// tutorial.
// ci.yml runs it:
//   node scripts/check-samples.mjs
import { fileURLToPath } from 'node:url';
import { basename, dirname, extname, join, relative } from 'node:path';
import { existsSync, readdirSync, readFileSync } from 'node:fs';

process.chdir(fileURLToPath(new URL('..', import.meta.url)));

const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const SAMPLES = `${PACKAGE}/Samples~`;
const TUTORIALS = 'Website/tutorials';
const TUTORIALS_RU = 'Website/i18n/ru/docusaurus-plugin-content-docs-tutorials/current';
const SITE = 'https://vpdpersonal.github.io/Aspid.FastTools/tutorials/';
const MAX_LINES = 450;
const MAX_DESCRIPTION = 200;
const MARKER_SAMPLE = 'ProfilerMarkers';

// The folder of a sample that holds each kind of file. Code may also sit in a subfolder of Scripts/.
const FOLDERS = {
  '.cs': ['Scripts', 'Scripts/Editor'],
  '.asmdef': ['Scripts', 'Scripts/Editor'],
  '.unity': ['Scenes'],
  '.prefab': ['Prefabs'],
  '.asset': ['Data'],
  '.mat': ['Materials'],
  '.shader': ['Shaders'],
  '.uss': ['Styles'],
  '.ttf': ['Fonts'],
  '.otf': ['Fonts'],
  '.txt': ['Fonts'],
  '.md': ['Fonts'],
};
// Scene and prefab YAML: GameObject, Transform, Camera, MeshRenderer, MeshFilter, AudioListener, TextMesh,
// LineRenderer, MonoBehaviour, PrefabInstance, and the settings blocks every scene has (OcclusionCulling,
// RenderSettings, LightmapSettings, NavMeshSettings, SceneRoots). No light, collider or rigidbody.
const CLASS_IDS = new Set([1, 4, 20, 23, 33, 81, 102, 114, 120, 1001, 29, 104, 157, 196, 1660057539]);
const DEFAULT_RESOURCES = '0000000000000000e000000000000000';
// The built-in primitive meshes: Cube, Cylinder, Sphere, Capsule, Plane and Quad (the ones GameObject > 3D Object uses).
const MESHES = new Set([10202, 10206, 10207, 10208, 10209, 10210]);
const BANNED = [
  [/\[\s*Header\b/, '[Header]'],
  [/ReSharper/, 'a ReSharper comment'],
  [/\bDebug\s*\./, 'Debug.'],
  [/\b(?:Light|Collider|BoxCollider|SphereCollider|CapsuleCollider|MeshCollider|Rigidbody)\b/, 'a light or physics component'],
  [/\b(?:Physics|Raycast|CreatePrimitive)\b/, 'Physics or CreatePrimitive'],
];

// The asp-ft-code-style skill puts this line above every namespace; it is the one comment a sample carries.
const BANNER = '// ReSharper disable once CheckNamespace\n';

let errors = 0;
const fail = (file, message) => {
  console.log(`::error file=${file}::${message}`);
  errors++;
};

// Each file is read once: the checks below read the same code, .meta and asmdef files.
const texts = new Map();
const read = (file) => texts.get(file) ?? texts.set(file, readFileSync(file, 'utf8')).get(file);
const guidOf = (file) => (existsSync(`${file}.meta`) ? read(`${file}.meta`).match(/^guid: (\w+)/m)?.[1] : undefined);

const walk = (dir) => readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
  const path = join(dir, entry.name);
  return entry.isDirectory() ? walk(path) : [path];
});

// Replaces string and char literals with empty ones, so a URL or "//" in a string is not a comment.
const stripStrings = (code) => code.replace(/@"(?:[^"]|"")*"|\$?"(?:[^"\\\n]|\\.)*"|'(?:[^'\\\n]|\\.)'/g, '""');

const kebab = (name) => name.replace(/([a-z0-9])([A-Z])/g, '$1-$2').toLowerCase();

function checkCode(file, code) {
  if (!new RegExp(`^${BANNER}namespace `, 'm').test(code)) fail(file, `lacks "${BANNER.trim()}" on the line above its namespace`);
  const plain = stripStrings(code.replace(BANNER, ''));
  if (/\/\/|\/\*/.test(plain)) fail(file, 'has a comment; sample code carries none');
  for (const [pattern, what] of BANNED) {
    if (pattern.test(plain)) fail(file, `uses ${what}, which samples do not`);
  }
  // Inside the block-scoped namespace a top-level type is indented by exactly four spaces.
  const types = [...code.matchAll(/^ {4}(?:\[[^\]]*\]\s*)*(?:(?:public|internal|sealed|abstract|static|partial|readonly)\s+)*(?:class|struct|interface|enum|record)\s+(\w+)/gm)]
    .map((match) => match[1]);
  const name = basename(file, '.cs');
  if (types.length !== 1) fail(file, `declares ${types.length} top-level types; a sample file holds exactly one`);
  else if (types[0] !== name) fail(file, `declares ${types[0]}; the file must be named after its type`);
}

function checkAsmdefs(sample, files) {
  const asmdefs = files.filter((file) => file.endsWith('.asmdef'));
  if (asmdefs.length === 0) fail(sample, 'has no .asmdef');
  const roots = new Map();
  for (const file of asmdefs) {
    const asmdef = JSON.parse(read(file));
    roots.set(dirname(file), asmdef);
    if (!asmdef.rootNamespace) fail(file, 'sets no rootNamespace');
    if ((asmdef.defineConstraints ?? []).length) fail(file, 'has defineConstraints; a sample needs no optional module');
    if ((asmdef.versionDefines ?? []).length) fail(file, 'has versionDefines; a sample needs no optional module');
  }
  for (const file of files.filter((file) => file.endsWith('.cs'))) {
    // The nearest asmdef above the file owns it, and each folder between them adds its name to the namespace.
    const folders = [];
    let dir = dirname(file);
    while (!roots.has(dir) && dir.startsWith(`${sample}/`)) {
      folders.unshift(basename(dir));
      dir = dirname(dir);
    }
    const asmdef = roots.get(dir);
    if (!asmdef) {
      fail(file, 'has no .asmdef in its folder or above it inside the sample');
      continue;
    }
    const expected = [asmdef.rootNamespace, ...folders].join('.');
    const namespaces = [...read(file).matchAll(/^namespace\s+([\w.]+)/gm)].map((match) => match[1]);
    if (namespaces.length !== 1 || namespaces[0] !== expected) {
      fail(file, `has namespace ${namespaces.join(', ') || '(none)'}; its folder makes it ${expected}`);
    }
  }
}

function checkYaml(file, text, sample) {
  const ids = [...text.matchAll(/^--- !u!(\d+) &/gm)].map((match) => Number(match[1]));
  for (const id of new Set(ids)) {
    if (!CLASS_IDS.has(id)) fail(file, `holds Unity class ID ${id}; sample scenes hold only a camera, meshes and sample scripts`);
  }
  for (const [, id] of text.matchAll(new RegExp(`m_Mesh: \\{fileID: (\\d+), guid: ${DEFAULT_RESOURCES}`, 'g'))) {
    if (!MESHES.has(Number(id))) fail(file, `uses built-in mesh ${id}; use the Cube, Capsule or Plane primitive (10202, 10208, 10209)`);
  }
  for (const [, guid] of text.matchAll(/m_Materials:\s*\n((?:\s+- \{[^}]*\}\n)+)/g)) {
    for (const [, materialGuid] of guid.matchAll(/guid: (\w+)/g)) {
      if (!sample.guids.has(materialGuid)) fail(file, `renders with material ${materialGuid}, which is not in the ${sample.name} sample`);
    }
  }
  // Each Camera must sit on the GameObject named Main Camera and clear to a solid colour.
  for (const [, body] of text.matchAll(/^--- !u!20 &\d+\n([\s\S]*?)(?=^--- |(?![\s\S]))/gm)) {
    const owner = body.match(/m_GameObject: \{fileID: (\d+)\}/)?.[1];
    const name = owner && text.match(new RegExp(`^--- !u!1 &${owner}\\n[\\s\\S]*?m_Name: (.*)$`, 'm'))?.[1];
    if (name !== 'Main Camera') fail(file, `has a Camera on ${name ?? 'an unknown object'}; only Main Camera may have one`);
    if (!/m_ClearFlags: 2\b/.test(body)) fail(file, 'has a Camera that does not clear to a solid colour');
  }
}

function checkArt(sample, files) {
  const shaders = files.filter((file) => file.endsWith('.shader'));
  const expected = `Aspid/Samples/${sample.name}/Studio`;
  const guids = new Set();
  for (const file of shaders) {
    const name = read(file).match(/^Shader\s+"([^"]+)"/m)?.[1];
    if (name !== expected) fail(file, `is named ${name}; a sample's shader is named ${expected}`);
    const guid = guidOf(file);
    if (guid) guids.add(guid);
  }
  for (const file of files.filter((file) => file.endsWith('.mat'))) {
    const guid = read(file).match(/m_Shader: \{fileID: \d+, guid: (\w+)/)?.[1];
    if (!guids.has(guid)) fail(file, `uses shader ${guid}; a sample material uses the sample's own ${expected}`);
  }
}

// The generator names a marker after its type and member, or its WithName text, plus the line of this.Marker().
function markerNames(files) {
  const names = new Map();
  for (const file of files.filter((file) => file.endsWith('.cs'))) {
    const lines = read(file).split('\n');
    const type = basename(file, '.cs');
    let member = null;
    lines.forEach((line, index) => {
      const declaration = line.match(/^ {8}(?:(?:public|private|protected|internal|static|override|virtual|sealed|async)\s+)+[\w<>[\],.? ]+?\s+(\w+)\s*\(/);
      if (declaration) member = declaration[1];
      const call = line.match(/this\.Marker\(\)(?:\.WithName\("([^"]+)"\))?/);
      if (!call) return;
      const name = `${type}.${call[1] ?? member} (${index + 1})`;
      names.set(name.replace(/ \(\d+\)$/, ''), name);
    });
  }
  return names;
}

function checkMarkerTutorials(files) {
  const names = markerNames(files);
  for (const page of [`${TUTORIALS}/${MARKER_SAMPLE}/README.md`, `${TUTORIALS_RU}/${MARKER_SAMPLE}/README.md`]) {
    if (!existsSync(page)) continue;
    const quoted = [...read(page).matchAll(/\b((?:[A-Z]\w*)(?:\.[A-Z]\w*)+) \((\d+)\)/g)]
      .map((match) => ({ base: match[1], name: `${match[1]} (${match[2]})` }));
    for (const quote of new Set(quoted.map((quote) => quote.name))) {
      const base = quote.replace(/ \(\d+\)$/, '');
      const actual = names.get(base);
      if (actual !== quote) fail(page, `quotes ${quote}; the code makes ${actual ?? 'no such marker'}`);
    }
  }
}

function checkPackageJson(folders) {
  const file = `${PACKAGE}/package.json`;
  const samples = JSON.parse(read(file)).samples ?? [];
  const listed = new Set();
  for (const { displayName, description = '', path } of samples) {
    listed.add(displayName);
    if (path !== `Samples~/${displayName}`) fail(file, `sample ${displayName} has path ${path}; it must be Samples~/${displayName}`);
    const slug = kebab(displayName);
    const suffix = ` Tutorial: ${SITE}${slug}`;
    if (!description.endsWith(suffix)) {
      fail(file, `the description of ${displayName} must end with "${suffix}"`);
      continue;
    }
    const sentence = description.slice(0, -suffix.length);
    if (sentence.length > MAX_DESCRIPTION) fail(file, `the description of ${displayName} is ${sentence.length} characters; at most ${MAX_DESCRIPTION} before the link`);
    if (!/^[A-Z].*\.$/.test(sentence) || /[.!?]\s+[A-Z]/.test(sentence)) fail(file, `the description of ${displayName} must be one sentence`);
    for (const page of [`${TUTORIALS}/${displayName}/README.md`, `${TUTORIALS_RU}/${displayName}/README.md`]) {
      if (!existsSync(page)) fail(file, `sample ${displayName} has no tutorial page ${page}`);
    }
  }
  for (const folder of folders) {
    if (!listed.has(folder)) fail(`${SAMPLES}/${folder}`, 'is not listed in package.json samples');
  }
}

const folders = readdirSync(SAMPLES, { withFileTypes: true }).filter((entry) => entry.isDirectory()).map((entry) => entry.name);
for (const name of folders) {
  const root = `${SAMPLES}/${name}`;
  const files = walk(root).filter((file) => !file.endsWith('.meta'));
  // A scene renders with the sample's own materials, and its text with the font materials inside the sample's fonts.
  const guids = new Set(files.map(guidOf).filter(Boolean));
  const sample = { name, guids };
  for (const file of files) {
    const ext = extname(file);
    const folders = FOLDERS[ext];
    const folder = dirname(relative(root, file));
    if (!folders) fail(file, `is a ${ext || 'extensionless'} file, which no sample folder holds`);
    else if (ext === '.cs' ? !folder.startsWith('Scripts/') && folder !== 'Scripts' : !folders.includes(folder)) {
      fail(file, `belongs in ${folders.map((folder) => `${name}/${folder}/`).join(' or ')}${ext === '.cs' ? ' or a subfolder of Scripts/' : ''}`);
    }
  }
  checkArt(sample, files);
  let lines = 0;
  for (const file of files.filter((file) => file.endsWith('.cs'))) {
    const code = read(file);
    lines += code.replace(/\n$/, '').split('\n').length;
    checkCode(file, code);
  }
  if (lines > MAX_LINES) fail(root, `has ${lines} lines of C#; a sample has at most ${MAX_LINES}`);
  for (const file of files.filter((file) => file.endsWith('.uss'))) {
    if (/\/\*/.test(read(file))) fail(file, 'has a comment; sample style sheets carry none');
  }
  for (const file of files.filter((file) => /\.(unity|prefab)$/.test(file))) checkYaml(file, read(file), sample);
  checkAsmdefs(root, files);
  if (name === MARKER_SAMPLE) checkMarkerTutorials(files);
}
checkPackageJson(folders);

if (errors) process.exit(1);
console.log(`${folders.length} samples follow the Samples~ rules`);
