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

/**
 * The changelog is served at /changelog. The language-switch line at its top (`> Русская версия: …`)
 * exists for GitHub readers; the site has a locale dropdown, so it is dropped. A translation's H1 carries a
 * language suffix for GitHub (`# Changelog (RU)`); on the site it takes the navbar's translated label instead.
 */
// `## [1.0.0] — 2026-01-01` → anchor `#v1-0-0`, so the generated sidebar can link every version in every locale.
const versionHeading = /^## \[([^\]]+)\](.*)$/gm;
const versionAnchor = (version) => `v${version.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`;

function writeChangelog(source, destination, title) {
  const body = fs
    .readFileSync(source, 'utf8')
    .replace(/^> .*CHANGELOG(?:\.[a-z]{2})?\.md.*\n\n/m, '')
    .replace(/^# (.+?)(?: \([A-Z]{2}\))?$/m, (line, heading) => `# ${title ?? heading}`)
    .replace(versionHeading, (line, version) => `${line} {#${versionAnchor(version)}}`);
  const date = lastCommitDate(source);
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.writeFileSync(destination, `---\nslug: /\ndisplayed_sidebar: changelog\n${date ? `last_update:\n  date: ${date}\n` : ''}---\n\n${body}`);
}

// The changelog is a single page; its sidebar lists the versions so the left panel is never empty.
function writeChangelogSidebar(source, destination) {
  const versions = [...fs.readFileSync(source, 'utf8').matchAll(versionHeading)].map(([, version]) => ({
    type: 'link', label: version, href: `/changelog#${versionAnchor(version)}`,
  }));
  const sidebars = { changelog: [{ type: 'category', label: 'Versions', className: 'doc-menu-group', collapsible: false, items: versions }] };
  fs.writeFileSync(destination, `${JSON.stringify(sidebars, null, 2)}\n`);
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
