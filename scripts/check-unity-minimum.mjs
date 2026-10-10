// Check that every hand-written copy of the minimum Unity version agrees with package.json, where it is split into
// "unity" (6000.0) and "unityRelease" (53f1): 6000.0.53f1. A raised minimum that misses one place leaves an agent, the
// review bot or the CI matrix on the old version. ci.yml runs it:
//   node scripts/check-unity-minimum.mjs
import { readFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

process.chdir(fileURLToPath(new URL('..', import.meta.url)));

const PACKAGE_JSON = 'Aspid.FastTools/Packages/tech.aspid.fasttools/package.json';
// Files that name the minimum in prose. Every full Unity version in them (6000.0.53f1) states the minimum, so a
// second mention left on the old value fails too.
const MENTIONS = [
  'AGENTS.md',
  '.github/claude-review.md',
  '.github/ISSUE_TEMPLATE/release_checklist.yml',
  'skills/aspid-visual-element-fluent/SKILL.md',
];
// The CI matrix must test the minimum, not only mention it: the first version of its `unity` list, not any version.
const MATRIX = '.github/workflows/ci.yml';

const { unity, unityRelease } = JSON.parse(readFileSync(PACKAGE_JSON, 'utf8'));
if (!unity) {
  console.log(`::error file=${PACKAGE_JSON}::no "unity" field`);
  process.exit(1);
}

const minimum = unityRelease ? `${unity}.${unityRelease}` : unity;
const escape = (text) => text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

let errors = 0;
const fail = (file, what) => {
  console.log(`::error file=${file}::has no ${what}; ${PACKAGE_JSON} says ${minimum}. Update the file by hand.`);
  errors++;
};

for (const file of MENTIONS) {
  const text = readFileSync(file, 'utf8');
  if (!text.includes(minimum)) fail(file, `mention of the minimum Unity ${minimum}`);
  // Without unityRelease the minimum is a minor version (6000.0), and a full version in the text may be any patch of it.
  if (!unityRelease) continue;
  const versions = text.match(/\b\d{4}\.\d+\.\d+[abfp]\d+\b/g) ?? [];
  for (const version of new Set(versions.filter((version) => version !== minimum))) {
    console.log(`::error file=${file}::names Unity ${version}; ${PACKAGE_JSON} says the minimum is ${minimum}. Update the file by hand.`);
    errors++;
  }
}
if (!new RegExp(`^\\s+unity: \\[\\s*${escape(minimum)}\\s*[,\\]]`, 'm').test(readFileSync(MATRIX, 'utf8'))) {
  fail(MATRIX, `matrix "unity: [${minimum}, …]" with the minimum first`);
}

if (errors) process.exit(1);
console.log(`The minimum Unity ${minimum} is the same in ${MENTIONS.length + 1} files`);
