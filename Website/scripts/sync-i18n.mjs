/**
 * Builds Website/i18n from the translations that live inside the UPM package, so the package stays the
 * single source of truth and contributors never touch Docusaurus' plugin-named folders by hand.
 *
 *   Documentation/<locale>/**              → i18n/<locale>/docusaurus-plugin-content-docs/current/**
 *   Samples~/<Sample>/Documentation/<Name>.md          → tutorials/<Sample>/<Name>.md
 *   Samples~/<Sample>/Documentation/<Name>.<locale>.md → i18n/<locale>/docusaurus-plugin-content-docs-tutorials/current/<Sample>/<Name>.md
 *   CHANGELOG.md / CHANGELOG.<locale>.md   → changelog/index.md / i18n/<locale>/docusaurus-plugin-content-docs-changelog/current/index.md
 *
 * Files are copied, not symlinked: webpack resolves symlinks to their real path, which breaks the
 * relative Markdown links inside a translation. `Website/i18n` is a build artifact and is gitignored.
 * Runs before `start` and `build`.
 *
 * Copies are not tracked by git, so Docusaurus cannot read their history; every copied Markdown page gets
 * `last_update.date` from the source file's last commit instead (the footer's "Last updated" line).
 */
import { execFileSync } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const siteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
const repoDir = path.resolve(siteDir, '..');
const packageDir = path.resolve(siteDir, '../Aspid.FastTools/Packages/tech.aspid.fasttools');
const changelogDir = path.join(siteDir, 'changelog');
const docsDir = path.join(packageDir, 'Documentation');
const samplesDir = path.join(packageDir, 'Samples~');
const tutorialsDir = path.join(siteDir, 'tutorials');
const i18nDir = path.join(siteDir, 'i18n');

const locales = fs
  .readdirSync(docsDir, { withFileTypes: true })
  .filter((entry) => entry.isDirectory() && /^[a-z]{2}(-[A-Za-z]{2,4})?$/.test(entry.name))
  .map((entry) => entry.name);

fs.rmSync(i18nDir, { recursive: true, force: true });
fs.rmSync(changelogDir, { recursive: true, force: true });
fs.rmSync(tutorialsDir, { recursive: true, force: true });

function copy(source, destination) {
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.cpSync(source, destination, { recursive: true, filter: (file) => !file.endsWith('.meta') });
  stampLastUpdate(source, destination);
}

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

/** Adds `last_update.date` to the front matter of `destination` (or of every Markdown file under it). */
function stampLastUpdate(source, destination) {
  if (fs.statSync(source).isDirectory()) {
    for (const entry of fs.readdirSync(source, { withFileTypes: true })) {
      if (entry.name.endsWith('.meta')) continue;
      stampLastUpdate(path.join(source, entry.name), path.join(destination, entry.name));
    }
    return;
  }
  if (!/\.mdx?$/.test(source)) return;
  const date = lastCommitDate(source);
  if (!date) return;
  const body = fs.readFileSync(destination, 'utf8');
  const stamp = `last_update:\n  date: ${date}\n`;
  const frontMatter = /^---\r?\n([\s\S]*?)\r?\n---\r?\n/.exec(body);
  const stamped = frontMatter
    ? `---\n${frontMatter[1]}\n${stamp}---\n${body.slice(frontMatter[0].length)}`
    : `---\n${stamp}---\n\n${body}`;
  fs.writeFileSync(destination, stamped);
}

/**
 * The changelog is served at /changelog. The language-switch line at its top (`> Русская версия: …`)
 * exists for GitHub readers; the site has a locale dropdown, so it is dropped.
 */
// `## [1.0.0] — 2026-01-01` → anchor `#v1-0-0`, so the generated sidebar can link every version in every locale.
const versionHeading = /^## \[([^\]]+)\](.*)$/gm;
const versionAnchor = (version) => `v${version.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`;

function writeChangelog(source, destination) {
  const body = fs
    .readFileSync(source, 'utf8')
    .replace(/^> .*CHANGELOG(?:\.[a-z]{2})?\.md.*\n\n/m, '')
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

// The gallery that opens the Samples section lives in the site, not in the package.
const samplesIndexDir = path.join(siteDir, 'src', 'samples');
copy(path.join(samplesIndexDir, 'index.mdx'), path.join(tutorialsDir, 'index.mdx'));

for (const sample of fs.readdirSync(samplesDir, { withFileTypes: true })) {
  if (!sample.isDirectory()) continue;
  const documentationDir = path.join(samplesDir, sample.name, 'Documentation');
  if (!fs.existsSync(documentationDir)) continue;

  const outputDir = path.join(tutorialsDir, sample.name);
  const images = path.join(documentationDir, 'Images');
  if (fs.existsSync(images)) copy(images, path.join(outputDir, 'Images'));

  for (const file of fs.readdirSync(documentationDir)) {
    if (/^(README|TUTORIAL)\.md$/.test(file)) copy(path.join(documentationDir, file), path.join(outputDir, file));
  }
}

for (const locale of locales) {
  // Site interface translations are maintained separately from package Markdown.
  const interfaceTranslations = path.join(siteDir, 'translations', locale);
  if (fs.existsSync(interfaceTranslations)) copy(interfaceTranslations, path.join(i18nDir, locale));
  const changelog = path.join(repoDir, `CHANGELOG.${locale}.md`);
  if (fs.existsSync(changelog)) {
    writeChangelog(changelog, path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-changelog', 'current', 'index.md'));
  }

  copy(path.join(docsDir, locale), path.join(i18nDir, locale, 'docusaurus-plugin-content-docs', 'current'));
  // Translated main docs reference `../Images/…`; mirror the folder so those file paths resolve in i18n.
  copy(path.join(docsDir, 'Images'), path.join(i18nDir, locale, 'docusaurus-plugin-content-docs', 'Images'));
  copy(path.join(docsDir, 'Images'), path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-tutorials', 'Documentation', 'Images'));
  const samplesIndex = path.join(samplesIndexDir, `index.${locale}.mdx`);
  if (fs.existsSync(samplesIndex)) {
    copy(samplesIndex, path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-tutorials', 'current', 'index.mdx'));
  }

  for (const sample of fs.readdirSync(samplesDir, { withFileTypes: true })) {
    if (!sample.isDirectory()) continue;
    const documentationDir = path.join(samplesDir, sample.name, 'Documentation');
    if (!fs.existsSync(documentationDir)) continue;
    const images = path.join(documentationDir, 'Images');
    if (fs.existsSync(images)) {
      copy(images, path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-tutorials', 'current', sample.name, 'Images'));
    }
    const suffix = `.${locale}.md`;
    for (const file of fs.readdirSync(documentationDir)) {
      if (!file.endsWith(suffix)) continue;
      const target = file.slice(0, -suffix.length) + '.md';
      copy(
        path.join(documentationDir, file),
        path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-tutorials', 'current', sample.name, target),
      );
    }
  }
}

console.log(`[sync-i18n] locales: ${locales.join(', ') || 'none'}`);
