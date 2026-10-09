// Check the package files that Unity or the Asset Store Validator complain about later:
//  1. Path length: the Validator warns when a path is 140 characters or longer, counted from "Aspid/FastTools/"
//     (the install folder under Assets/) without the .meta files.
//  2. Every .cs.meta of the package and the dev tests has a MonoImporter block, so all of them stay in the format Unity
//     writes for a new file. Samples~ is skipped: it is not imported in the dev project.
//  3. A .uss.meta importer (samples included) is the StyleSheetImporter (12385), not the ThemeStyleSheetImporter (12388);
//     Unity rewrites 12388 in the working tree when it imports the sample.
// package-files.yml runs it. It has no dependencies:
//   node scripts/check-package-files.mjs
import { readdirSync, readFileSync } from 'node:fs';
import { join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

process.chdir(fileURLToPath(new URL('..', import.meta.url)));

const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';
const DEV_TESTS = 'Aspid.FastTools/Assets/DevTests';
const INSTALL_PREFIX = 'Aspid/FastTools/';
const PATH_LIMIT = 140;

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

const samples = `${PACKAGE}/Samples~/`;

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

if (errors) process.exit(1);
console.log('Package files OK');
