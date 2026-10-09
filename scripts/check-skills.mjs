// Check every SKILL.md of the repository, from the repository root:
// - the frontmatter, with a strict YAML parser, as the skills installer and the agents read it;
// - the `internal` mark: the installer (`npx skills add`) scans about 30 agent folders (.claude/skills, .agents/skills,
//   ...), so any SKILL.md outside skills/ must be internal, or it reaches users;
// - the analyzer codes (AFT0001) that the skills mention: each one must be reported by an analyzer;
// - the namespaces and assemblies (Aspid.FastTools.Types) that the consumer skills mention: each one must exist in the
//   package.
// Needs js-yaml:
//   npm install --no-save js-yaml@4 && node scripts/check-skills.mjs
import { execFileSync } from 'node:child_process';
import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { basename, dirname, extname } from 'node:path';
import yaml from 'js-yaml';

// Consumer skills live in skills/<name>/SKILL.md; every other SKILL.md is a repo skill.
const CONSUMER_ROOT = 'skills';
const SKILL_ROOTS = [CONSUMER_ROOT, '.claude/skills'];
const ANALYZERS = ':(glob)Aspid.FastTools.Analyzers/**/*.cs';
const PACKAGE = 'Aspid.FastTools/Packages/tech.aspid.fasttools';

let errors = 0;
const fail = (file, message) => {
  console.log(`::error file=${file}::${message}`);
  errors++;
};

// Tracked and not-ignored files, so a new file is checked before `git add`.
const files = (...pathspec) =>
  execFileSync('git', ['ls-files', '-z', '--cached', '--others', '--exclude-standard', '--', ...pathspec], { encoding: 'utf8' })
    .split('\0')
    .filter(file => file && existsSync(file));

const skillFiles = files(':(glob)**/SKILL.md');

for (const root of SKILL_ROOTS) {
  if (!existsSync(root)) continue;
  for (const entry of readdirSync(root, { withFileTypes: true }).filter(e => e.isDirectory())) {
    if (!skillFiles.includes(`${root}/${entry.name}/SKILL.md`)) fail(`${root}/${entry.name}`, 'missing SKILL.md');
  }
}

for (const file of skillFiles) {
  const name = basename(dirname(file));
  const internal = dirname(dirname(file)) !== CONSUMER_ROOT;
  const match = readFileSync(file, 'utf8').match(/^---\r?\n([\s\S]*?)\r?\n---\r?\n/);
  if (!match) {
    fail(file, 'no YAML frontmatter');
    continue;
  }
  let data;
  try {
    data = yaml.load(match[1]);
  } catch (e) {
    fail(file, `invalid YAML frontmatter: ${e.reason ?? e.message}`);
    continue;
  }
  // Limits of the Agent Skills spec (agentskills.io); Codex counts the description in bytes and skips a longer skill.
  if (data?.name !== name) fail(file, `name "${data?.name}" must match the folder name "${name}"`);
  if (!/^[a-z0-9]+(-[a-z0-9]+)*$/.test(name) || name.length > 64) fail(file, 'name must be kebab-case, at most 64 characters');
  if (typeof data?.description !== 'string' || !data.description.trim()) fail(file, 'description is missing');
  else if (Buffer.byteLength(data.description) > 1024) fail(file, 'description exceeds 1024 bytes');
  // `npx skills add` skips skills marked internal: a skill outside skills/ must carry the mark, a consumer skill must not.
  if ((data?.metadata?.internal === true) !== internal) {
    fail(file, internal
      ? 'a SKILL.md outside skills/ needs `metadata: internal: true`, or `npx skills add` offers it to users'
      : 'consumer skill must not be internal');
  }
}

// A skill that names a removed analyzer code sends the agent to a rule that does not exist.
const reported = new Set(
  files(ANALYZERS)
    .filter(file => !file.includes('.Tests/'))
    .flatMap(file => [...readFileSync(file, 'utf8').matchAll(/"(AFT\d{4})"/g)].map(m => m[1])));
if (!reported.size)
  fail('Aspid.FastTools.Analyzers', 'no AFT code found: analyzer sources not checked out (sparse checkout?) or moved');

// A skill that names a renamed namespace or assembly makes the agent write a `using` or an asmdef reference that does
// not compile. The set holds every declared name and each of its parents, so `Aspid.FastTools` matches too.
const known = new Set();
const addWithParents = name => {
  const parts = name.split('.');
  for (let length = 1; length <= parts.length; length++) known.add(parts.slice(0, length).join('.'));
};
for (const file of files(`:(glob)${PACKAGE}/**/*.cs`))
  for (const m of readFileSync(file, 'utf8').matchAll(/^\s*namespace\s+(Aspid\.FastTools[\w.]*)/gm)) addWithParents(m[1]);
for (const file of files(`:(glob)${PACKAGE}/**/*.asmdef`))
  for (const m of readFileSync(file, 'utf8').matchAll(/"name"\s*:\s*"(Aspid\.FastTools[\w.]*)"/g)) addWithParents(m[1]);
if (!known.size) fail(PACKAGE, 'no Aspid.FastTools namespace found: package not checked out (sparse checkout?) or moved');

// A mention is `AFT0001` or a range `AFT0001-AFT0009`; every code of a range must exist.
const mentionedIn = text => [...text.matchAll(/AFT(\d{4})(?:`?\s*[-–—]\s*`?AFT(\d{4}))?/g)].flatMap(m => {
  const [from, to] = [Number(m[1]), Number(m[2] ?? m[1])];
  return Array.from({ length: Math.max(to - from + 1, 1) }, (_, i) => `AFT${String(from + i).padStart(4, '0')}`);
});

const documented = new Set();
for (const file of files(...SKILL_ROOTS).filter(file => extname(file) === '.md')) {
  const text = readFileSync(file, 'utf8');
  const consumer = file.startsWith(`${CONSUMER_ROOT}/`);
  const mentioned = new Set(mentionedIn(text));
  if (consumer) mentioned.forEach(code => documented.add(code));
  for (const code of mentioned) if (reported.size && !reported.has(code)) fail(file, `${code} is not reported by any analyzer`);
  // Only a namespace or an assembly: a fully qualified type name is not known here, so write a `using` instead.
  if (consumer)
    for (const name of new Set(text.match(/Aspid\.FastTools(?:\.\w+)*/g)))
      if (known.size && !known.has(name)) fail(file, `${name} is not a namespace or an assembly of the package`);
}
for (const code of [...reported].sort().filter(code => !documented.has(code)))
  console.log(`::warning::${code} is reported by an analyzer but no consumer skill in ${CONSUMER_ROOT}/ mentions it`);

if (errors) process.exit(1);
console.log(`Skills OK: ${skillFiles.length} SKILL.md, ${reported.size} analyzer codes, ${known.size} package names`);
