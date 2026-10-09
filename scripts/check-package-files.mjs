// Check the package files that Unity or the Asset Store Validator complain about later:
//  1. Path length: the Validator warns when a path is 140 characters or longer, counted from "Aspid/FastTools/"
//     (the install folder under Assets/) without the .meta files.
//  2. Every .cs.meta of the package and the dev tests has a MonoImporter block, so all of them stay in the format Unity
//     writes for a new file. Samples~ is skipped: it is not imported in the dev project.
//  3. A .uss.meta importer (samples included) is the StyleSheetImporter (12385), not the ThemeStyleSheetImporter (12388);
//     Unity rewrites 12388 in the working tree when it imports the sample.
//  4. Every file and folder of the package and the dev tests has a .meta, and every .meta has its asset. Unity cannot
//     write a .meta into an installed package: it ignores the file there and warns about a .meta without an asset.
//     Samples~ is copied into the project on import, and its .meta files keep the GUIDs that its scenes refer to.
//     Another folder that ends with "~" is ignored with its content.
//  5. Every .meta has a guid, and no guid is used twice in the package and the dev tests.
//  6. Every "path" in the "samples" of package.json is a folder of the package.
//  7. Every reference of an asmdef of the package is an asmdef of the package, or an assembly of EXTERNAL_ASSEMBLIES.
// checks.yml runs it. It has no dependencies:
//   node scripts/check-package-files.mjs
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { basename, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

process.chdir(fileURLToPath(new URL('..', import.meta.url)));

const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const DEV_TESTS = 'Aspid.FastTools/Assets/DevTests';
const INSTALL_PREFIX = 'Aspid/FastTools/';
const PATH_LIMIT = 140;
// Assemblies that the asmdefs of the package use and the package does not contain. A reference by GUID cannot name one.
const EXTERNAL_ASSEMBLIES = new Set(['Unity.Mathematics', 'UnityEngine.TestRunner', 'UnityEditor.TestRunner']);

let errors = 0;
const fail = (file, message) => {
  console.log(`::error file=${file}::${message}`);
  errors++;
};

const files = (dir) =>
  readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const path = join(dir, entry.name);
    return entry.isDirectory() ? files(path) : [path];
  });

// Unity imports no entry with these names, so it needs no .meta. Samples~ is one of them, but its content needs one:
// Unity copies Samples~ into the project on import, and the scenes there refer to the GUIDs of its .meta files.
const unityIgnores = (name) => name.endsWith('~') || name.endsWith('.tmp') || name.toLowerCase() === 'cvs';
const isFolder = (path) => statSync(path, { throwIfNoEntry: false })?.isDirectory() ?? false;

// Files and folders. A hidden entry (a name that starts with a dot) is skipped with its content, and so is a folder that
// Unity ignores, except Samples~.
const entries = (dir) =>
  readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    if (entry.name.startsWith('.')) return [];
    const path = join(dir, entry.name);
    if (!entry.isDirectory()) return [path];
    return unityIgnores(entry.name) && entry.name !== 'Samples~' ? [path] : [path, ...entries(path)];
  });

const samples = `${PACKAGE}/Samples~/`;
// Unity does not import Samples~ in the dev project, so it writes no .meta there.
const NO_META = 'no .meta; open the project in Unity once and commit the .meta it writes';
const NO_SAMPLE_META = 'no .meta; import the sample into a project and copy the .meta it writes back, or write it by hand';

// Samples~ is installed under Assets/Samples/, so the install-folder limit does not apply to it.
for (const file of files(PACKAGE)) {
  if (file.endsWith('.meta') || file.startsWith(samples)) continue;
  const length = (INSTALL_PREFIX + relative(PACKAGE, file)).length;
  if (length >= PATH_LIMIT) fail(file, `path is ${length} characters from "${INSTALL_PREFIX}"; the limit is ${PATH_LIMIT - 1}`);
}

for (const file of [...files(PACKAGE), ...files(DEV_TESTS)]) {
  if (file.endsWith('.cs.meta') && !file.startsWith(samples) && !readFileSync(file, 'utf8').includes('\nMonoImporter:')) {
    fail(file, 'no MonoImporter block; open the project in Unity once and commit the .meta it writes');
  }
  if (file.endsWith('.uss.meta') && /^\s+script: \{fileID: 12388,/m.test(readFileSync(file, 'utf8'))) {
    fail(file, 'importer fileID 12388 (ThemeStyleSheetImporter) on a .uss; use 12385 (StyleSheetImporter)');
  }
}

const guidOf = new Map(); // .meta path -> guid
const ownerOf = new Map(); // guid -> .meta path
for (const root of [PACKAGE, DEV_TESTS]) {
  const all = entries(root);
  const present = new Set(all);
  for (const path of all) {
    if (!path.endsWith('.meta')) {
      if (!unityIgnores(basename(path)) && !present.has(`${path}.meta`)) {
        fail(path, path.startsWith(samples) ? NO_SAMPLE_META : NO_META);
      }
      continue;
    }
    if (!present.has(path.slice(0, -'.meta'.length))) {
      fail(path, 'no asset for this .meta; delete the .meta or restore the asset');
    }
    const guid = /^guid: ([0-9a-f]{32})\s*$/m.exec(readFileSync(path, 'utf8'))?.[1];
    if (!guid) {
      fail(path, 'no "guid:" line with 32 hexadecimal digits');
      continue;
    }
    if (ownerOf.has(guid)) fail(path, `guid ${guid} is also the guid of ${ownerOf.get(guid)}`);
    guidOf.set(path, guid);
    ownerOf.set(guid, path);
  }
}

const manifestPath = `${PACKAGE}/package.json`;
for (const sample of JSON.parse(readFileSync(manifestPath, 'utf8')).samples ?? []) {
  if (typeof sample.path !== 'string') {
    fail(manifestPath, `the sample "${sample.displayName}" has no "path"`);
  } else if (!isFolder(join(PACKAGE, sample.path))) {
    fail(manifestPath, `the path "${sample.path}" of the sample "${sample.displayName}" is not a folder of the package`);
  }
}

const asmdefs = [];
for (const path of entries(PACKAGE)) {
  if (!path.endsWith('.asmdef')) continue;
  try {
    asmdefs.push({ path, json: JSON.parse(readFileSync(path, 'utf8')) });
  } catch (error) {
    fail(path, `not valid JSON: ${error.message}`);
  }
}
const assemblyNames = new Set(asmdefs.map(({ json }) => json.name));
const assemblyGuids = new Set(asmdefs.map(({ path }) => guidOf.get(`${path}.meta`)));
const resolves = (reference) =>
  reference.startsWith('GUID:')
    ? assemblyGuids.has(reference.slice('GUID:'.length))
    : assemblyNames.has(reference) || EXTERNAL_ASSEMBLIES.has(reference);

for (const { path, json } of asmdefs) {
  for (const reference of json.references ?? []) {
    if (!resolves(reference)) {
      fail(path, `the reference "${reference}" is no asmdef of the package; fix it, or add the assembly to EXTERNAL_ASSEMBLIES`);
    }
  }
}

if (errors) process.exit(1);
console.log('Package files OK');
