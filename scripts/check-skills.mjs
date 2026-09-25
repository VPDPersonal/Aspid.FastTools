// Check the SKILL.md frontmatter of the consumer skills (skills/) and the repo skills (.claude/skills/) with a strict
// YAML parser, as the skills installer and the agents read it. Needs js-yaml:
//   npm install --no-save js-yaml@4 && node scripts/check-skills.mjs
import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';
import yaml from 'js-yaml';

const ROOTS = [
  { dir: 'skills', internal: false },
  { dir: '.claude/skills', internal: true },
];

let errors = 0;
const fail = (file, message) => {
  console.log(`::error file=${file}::${message}`);
  errors++;
};

for (const { dir, internal } of ROOTS) {
  if (!existsSync(dir)) continue;
  for (const name of readdirSync(dir, { withFileTypes: true }).filter(e => e.isDirectory()).map(e => e.name)) {
    const file = join(dir, name, 'SKILL.md');
    if (!existsSync(file)) {
      fail(join(dir, name), 'missing SKILL.md');
      continue;
    }
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
    // `npx skills add` skips skills marked internal: repo skills must carry the mark, consumer skills must not.
    if ((data?.metadata?.internal === true) !== internal) {
      fail(file, internal ? 'repo skill needs `metadata: internal: true`' : 'consumer skill must not be internal');
    }
  }
}

if (errors) process.exit(1);
console.log('Skill frontmatter OK');
