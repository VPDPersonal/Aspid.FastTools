/**
 * Builds Website/i18n from the translations that live inside the UPM package, so the package stays the
 * single source of truth and contributors never touch Docusaurus' plugin-named folders by hand.
 *
 *   Documentation/<locale>/**              → i18n/<locale>/docusaurus-plugin-content-docs/current/**
 *   Samples~/<Sample>/<Name>.<locale>.md   → i18n/<locale>/docusaurus-plugin-content-docs-tutorials/current/<Sample>/<Name>.md
 *   CHANGELOG.md / CHANGELOG.<locale>.md   → changelog/index.md / i18n/<locale>/docusaurus-plugin-content-docs-changelog/current/index.md
 *
 * Files are copied, not symlinked: webpack resolves symlinks to their real path, which breaks the
 * relative Markdown links inside a translation. `Website/i18n` is a build artifact and is gitignored.
 * Runs before `start` and `build`.
 */
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const siteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
const repoDir = path.resolve(siteDir, '..');
const packageDir = path.resolve(siteDir, '../Aspid.FastTools/Packages/tech.aspid.fasttools');
const changelogDir = path.join(siteDir, 'changelog');
const docsDir = path.join(packageDir, 'Documentation');
const samplesDir = path.join(packageDir, 'Samples~');
const i18nDir = path.join(siteDir, 'i18n');

const locales = fs
  .readdirSync(docsDir, { withFileTypes: true })
  .filter((entry) => entry.isDirectory() && /^[a-z]{2}(-[A-Za-z]{2,4})?$/.test(entry.name))
  .map((entry) => entry.name);

fs.rmSync(i18nDir, { recursive: true, force: true });
fs.rmSync(changelogDir, { recursive: true, force: true });

function copy(source, destination) {
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.cpSync(source, destination, { recursive: true, filter: (file) => !file.endsWith('.meta') });
}

/**
 * The changelog is served at /changelog. The language-switch line at its top (`> Русская версия: …`)
 * exists for GitHub readers; the site has a locale dropdown, so it is dropped.
 */
function writeChangelog(source, destination) {
  const body = fs.readFileSync(source, 'utf8').replace(/^> .*CHANGELOG(?:\.[a-z]{2})?\.md.*\n\n/m, '');
  fs.mkdirSync(path.dirname(destination), { recursive: true });
  fs.writeFileSync(destination, `---\nslug: /\n---\n\n${body}`);
}

writeChangelog(path.join(repoDir, 'CHANGELOG.md'), path.join(changelogDir, 'index.md'));

for (const locale of locales) {
  const changelog = path.join(repoDir, `CHANGELOG.${locale}.md`);
  if (fs.existsSync(changelog)) {
    writeChangelog(changelog, path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-changelog', 'current', 'index.md'));
  }

  copy(path.join(docsDir, locale), path.join(i18nDir, locale, 'docusaurus-plugin-content-docs', 'current'));
  // Translated pages reference `../Images/…` (docs) and `../../Documentation/Images/…` (tutorials) exactly like
  // the English originals; mirror the folder so the relative paths resolve from the i18n copies too.
  copy(path.join(docsDir, 'Images'), path.join(i18nDir, locale, 'docusaurus-plugin-content-docs', 'Images'));
  copy(path.join(docsDir, 'Images'), path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-tutorials', 'Documentation', 'Images'));

  for (const sample of fs.readdirSync(samplesDir, { withFileTypes: true })) {
    if (!sample.isDirectory()) continue;
    const sampleDir = path.join(samplesDir, sample.name);
    const suffix = `.${locale}.md`;
    for (const file of fs.readdirSync(sampleDir)) {
      if (!file.endsWith(suffix)) continue;
      const target = file.slice(0, -suffix.length) + '.md';
      copy(
        path.join(sampleDir, file),
        path.join(i18nDir, locale, 'docusaurus-plugin-content-docs-tutorials', 'current', sample.name, target),
      );
    }
  }
}

console.log(`[sync-i18n] locales: ${locales.join(', ') || 'none'}`);
