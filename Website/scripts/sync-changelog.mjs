/**
 * Builds the changelog pages from the root changelogs, which GitHub and Unity read as they are:
 *
 *   CHANGELOG.md          → changelog/index.md (+ changelog/sidebars.json)
 *   CHANGELOG.<locale>.md → i18n/<locale>/docusaurus-plugin-content-docs-changelog/current/index.md
 *
 * Both outputs are gitignored and rebuilt before `start` and `build`. A copy is not tracked by git, so Docusaurus
 * cannot read its history; each page gets `last_update.date` from the source file's last commit instead.
 */
import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { changelogPage, changelogSidebars } from './changelog-page.mjs';

const siteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
const repoDir = path.resolve(siteDir, '..');
const changelogDir = path.join(siteDir, 'changelog');
const i18nDir = path.join(siteDir, 'i18n');

const locales = fs
  .readdirSync(repoDir)
  .map((file) => file.match(/^CHANGELOG\.([a-z]{2}(?:-[A-Za-z]{2,4})?)\.md$/)?.[1])
  .filter(Boolean);

fs.rmSync(changelogDir, { recursive: true, force: true });

/** ISO date of the last commit touching `file`, or null when git has no history for it (uncommitted). */
function lastCommitDate(file) {
  try {
    const date = execFileSync('git', ['log', '-1', '--format=%cI', '--', file], { cwd: repoDir, stdio: ['ignore', 'pipe', 'ignore'] })
      .toString()
      .trim();
    return date || null;
  } catch {
    return null;
  }
}

function writeChangelog(source, destination, title) {
  const date = lastCommitDate(source);
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.writeFileSync(destination, changelogPage(fs.readFileSync(source, 'utf8'), title, date));
}

function writeChangelogSidebar(source, destination) {
  fs.writeFileSync(destination, `${JSON.stringify(changelogSidebars(fs.readFileSync(source, 'utf8')), null, 2)}\n`);
}

writeChangelog(path.join(repoDir, 'CHANGELOG.md'), path.join(changelogDir, 'index.md'));
writeChangelogSidebar(path.join(repoDir, 'CHANGELOG.md'), path.join(changelogDir, 'sidebars.json'));

for (const locale of locales) {
  const navbar = path.join(i18nDir, locale, 'docusaurus-theme-classic', 'navbar.json');
  const title = fs.existsSync(navbar) ? JSON.parse(fs.readFileSync(navbar, 'utf8'))['item.label.Changelog']?.message : undefined;
  const destination = path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-changelog', 'current');
  fs.rmSync(destination, { recursive: true, force: true });
  writeChangelog(path.join(repoDir, `CHANGELOG.${locale}.md`), path.join(destination, 'index.md'), title);
}

console.log(`[sync-changelog] locales: en${locales.map((locale) => `, ${locale}`).join('')}`);
