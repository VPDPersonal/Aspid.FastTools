// @ts-check
import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import venom from './src/prism/venom.js';
import remarkGithubAdmonitionsToDirectives from 'remark-github-admonitions-to-directives';
import remarkCrossInstanceLinks from './src/remark/crossInstanceLinks.js';
import remarkThemedImages from './src/remark/themedImages.js';
import remarkAgentPrompt from './src/remark/agentPrompt.js';
import remarkLiveDiagrams from './src/remark/liveDiagrams.js';
import remarkIntroBanner, {remarkInlineCode, remarkStatusBadges} from './src/remark/introBanner.js';
import {ACCENT_BOOT_SCRIPT} from './src/accents.js';
import {INTRO_BOOT_SCRIPT} from './src/intro.js';

const PACKAGE = '../Aspid.FastTools/Packages/tech.aspid.fasttools';
const LOCALES = ['en', 'ru'];
const REPO = 'https://github.com/VPDPersonal/Aspid.FastTools';
const ASSET_STORE = 'https://assetstore.unity.com/packages/slug/365584';
/** The docs in the working tree describe the package version in the working tree. */
const PACKAGE_VERSION = JSON.parse(readFileSync(new URL(`${PACKAGE}/package.json`, import.meta.url), 'utf8')).version;
/**
 * The UPM branch the install URL points at; each release tags it `<branch>/<version>`. A prerelease publishes to
 * `upm-preview`, a stable version to `upm` (.github/workflows/release.yml).
 */
// The captures scripts/frame-doc-captures.sh frames in the file, with the margin it adds: doc pages pad them on to the
// same inset from their rounded frame as the introduction's cards (MDXComponents/Img).
function readFramedCaptures() {
  const script = readFileSync(new URL('../scripts/frame-doc-captures.sh', import.meta.url), 'utf8');
  const margin = Number(script.match(/^PAD=(\d+)$/m)[1]);
  const rows = script.slice(script.indexOf("done <<'EOF'\n")).split('\n').slice(1);
  const names = rows.slice(0, rows.indexOf('EOF')).map((row) => row.trim()).filter((row) => row && !row.startsWith('#'))
    .map((row) => row.split(/\s+/)[0]);
  return {margin, names};
}

/** The translated sidebar label of the introduction, for a page under `i18n/<locale>/`. */
function introductionLabel(filePath) {
  const locale = filePath.match(/[\\/]i18n[\\/]([^\\/]+)[\\/]/)?.[1];
  if (!locale) return undefined;
  const file = new URL(`./i18n/${locale}/docusaurus-plugin-content-docs/current.json`, import.meta.url);
  return JSON.parse(readFileSync(file, 'utf8'))['sidebar.docs.doc.Introduction']?.message;
}

const UPM_BRANCH = PACKAGE_VERSION.includes('-') ? 'upm-preview' : 'upm';

/** Orders `1.0.0-rc.10` after `1.0.0-rc.9`, and a release after its prereleases. */
function compareVersions(a, b) {
  const parse = (value) => value.split('-');
  const [coreA, preA] = parse(a);
  const [coreB, preB] = parse(b);
  const core = coreA.localeCompare(coreB, 'en', {numeric: true});
  if (core !== 0) return core;
  if (!preA || !preB) return preA ? -1 : preB ? 1 : 0;
  return preA.localeCompare(preB, 'en', {numeric: true});
}

/**
 * The versions the install panel can pin, per UPM branch and newest first: each branch's tags on GitHub, or the local
 * ones when offline. A page pins from the branch its own README URL names, which for a docs snapshot can differ from
 * the working tree's. The working-tree version is always offered on its branch, since its tag may be pushed after
 * the docs are built.
 */
function readPackageVersions() {
  const read = (args) => {
    try {
      return execFileSync('git', args, {encoding: 'utf8', timeout: 15000, stdio: ['ignore', 'pipe', 'ignore']});
    } catch {
      return '';
    }
  };
  const branches = ['upm', 'upm-preview'];
  const patterns = branches.map((branch) => `${branch}/*`);
  const tags = read(['ls-remote', '--tags', '--refs', `${REPO}.git`, ...patterns]) || read(['tag', '-l', ...patterns]);
  const refs = tags.split('\n').map((line) => line.trim().split(/\s/).pop().replace(/^refs\/tags\//, ''));
  return Object.fromEntries(branches.map((branch) => {
    // Early release candidates were tagged on upm; they must not enable the Stable tab.
    const versions = refs.filter((ref) => ref.startsWith(`${branch}/`))
      .map((ref) => ref.slice(branch.length + 1))
      .filter((version) => branch !== 'upm' || !version.split('+')[0].includes('-'));
    if (branch === UPM_BRANCH) versions.push(PACKAGE_VERSION);
    return [branch, [...new Set(versions)].sort(compareVersions).reverse()];
  }));
}

/**
 * Turns a sample folder name into a slug: `SerializeReferences` → `serialize-references`,
 * `01. Counter` → `counter` with position 1. FastTools samples carry no number, so they sort by name.
 */
function samplePrefixParser(filename) {
  const match = filename.match(/^(\d+)\.\s*(.+)$/);
  const name = (match ? match[2] : filename)
    .replace(/([a-z])([A-Z])/g, '$1-$2')
    .replace(/\s+/g, '-')
    .toLowerCase();
  return { filename: name, numberPrefix: match ? Number(match[1]) : undefined };
}

/**
 * Shared options: GitHub-style `> [!NOTE]` alerts become Docusaurus admonitions, and file links
 * between the two plugin instances become site routes.
 */
const markdownOptions = {
  beforeDefaultRemarkPlugins: [remarkGithubAdmonitionsToDirectives, remarkCrossInstanceLinks, remarkAgentPrompt, remarkLiveDiagrams, remarkThemedImages],
  showLastUpdateTime: true,
  // English pages under `Website/<instance>/`, translations under `Website/i18n/<locale>/<plugin>/current/`.
  editUrl: `${REPO}/edit/main/Website/`,
  editLocalizedFiles: true,
};

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Aspid.FastTools',
  tagline: 'Unity tools that cut boilerplate',
  favicon: 'img/favicon.png',
  // Applies the stored accent colour (src/accents.js) and arms the introduction's entrance (src/intro.js) before the first paint.
  headTags: [
    { tagName: 'script', attributes: {}, innerHTML: ACCENT_BOOT_SCRIPT },
    { tagName: 'script', attributes: {}, innerHTML: INTRO_BOOT_SCRIPT },
  ],
  clientModules: ['./src/clientModules/accent.js'],

  url: 'https://vpdpersonal.github.io',
  baseUrl: '/Aspid.FastTools/',
  customFields: {
    assetStore: ASSET_STORE, packageVersion: PACKAGE_VERSION, packageVersions: readPackageVersions(),
    framedCaptures: readFramedCaptures(),
  },
  organizationName: 'VPDPersonal',
  projectName: 'Aspid.FastTools',
  trailingSlash: false,

  onBrokenLinks: 'throw',
  onBrokenAnchors: 'warn',
  markdown: {
    // The shared introduction uses a banner instead of a heading; retain its page metadata.
    async parseFrontMatter({filePath, fileContent, defaultParseFrontMatter}) {
      const result = await defaultParseFrontMatter({filePath, fileContent});
      if (fileContent.includes('/aspid_fasttools_readme_banner.png')) {
        result.frontMatter.title ??= 'Aspid.FastTools';
        // Pagination reuses the untranslated sidebar label of a page without a title; give it the translated one.
        result.frontMatter.pagination_label ??= introductionLabel(filePath);
        result.frontMatter.hide_title = true;
        result.frontMatter.description ??= fileContent.match(/^Aspid\.FastTools (is|—) .*$/m)?.[0];
      }
      return result;
    },
    hooks: { onBrokenMarkdownLinks: 'throw' },
  },

  i18n: {
    defaultLocale: 'en',
    locales: LOCALES,
    localeConfigs: {
      en: { label: 'English' },
      ru: { label: 'Русский' },
    },
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        // English pages in `docs/`, translations in `i18n/<locale>/docusaurus-plugin-content-docs/current/`.
        docs: {
          path: 'docs',
          routeBasePath: 'docs',
          breadcrumbs: false,
          sidebarPath: './sidebars.js',
          versions: { current: { label: PACKAGE_VERSION } },
          ...markdownOptions,
          beforeDefaultRemarkPlugins: [[remarkIntroBanner, {baseUrl: '/Aspid.FastTools/', siteUrl: 'https://vpdpersonal.github.io'}], ...markdownOptions.beforeDefaultRemarkPlugins],
          remarkPlugins: [remarkStatusBadges],
        },
        blog: false,
        theme: { customCss: ['./src/css/custom.css', './src/css/accents.css'] },
      }),
    ],
  ],

  plugins: [
    './src/plugins/search/index.js',
    [
      // One page per sample in `tutorials/<Sample>/README.md`, translations in
      // `i18n/<locale>/docusaurus-plugin-content-docs-tutorials/current/<Sample>/README.md`.
      '@docusaurus/plugin-content-docs',
      /** @type {import('@docusaurus/plugin-content-docs').Options} */
      ({
        id: 'tutorials',
        path: 'tutorials',
        routeBasePath: 'tutorials',
        breadcrumbs: false,
        sidebarPath: './sidebarsTutorials.js',
        include: ['index.mdx', '*/README.md'],
        numberPrefixParser: samplePrefixParser,
        ...markdownOptions,
        beforeDefaultRemarkPlugins: [remarkInlineCode, ...markdownOptions.beforeDefaultRemarkPlugins],
      }),
    ],
    [
      // The root CHANGELOG.md (and CHANGELOG.<locale>.md), copied in by scripts/sync-changelog.mjs.
      '@docusaurus/plugin-content-docs',
      /** @type {import('@docusaurus/plugin-content-docs').Options} */
      ({
        id: 'changelog',
        path: 'changelog',
        routeBasePath: 'changelog',
        breadcrumbs: false,
        sidebarPath: './changelog/sidebars.json',
        showLastUpdateTime: true,
        editUrl: ({ locale }) => `${REPO}/edit/main/CHANGELOG${locale === 'en' ? '' : `.${locale}`}.md`,
        beforeDefaultRemarkPlugins: [remarkGithubAdmonitionsToDirectives],
      }),
    ],
    [
      // API reference generated by DocFX from the XML doc comments (`npm run api`), see scripts/docfx-*.mjs.
      '@docusaurus/plugin-content-docs',
      /** @type {import('@docusaurus/plugin-content-docs').Options} */
      ({
        id: 'api',
        path: 'api',
        routeBasePath: 'api',
        breadcrumbs: false,
        sidebarPath: './sidebarsApi.js',
        showLastUpdateTime: false,
        editUrl: undefined,
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      colorMode: { defaultMode: 'dark', respectPrefersColorScheme: false },
      // Link previews (og:image, twitter:image) for every page.
      image: 'img/social-card.png',
      navbar: {
        title: 'Aspid.FastTools',
        logo: { alt: 'Aspid.FastTools', src: 'img/logo.png', width: 28, height: 28 },
        hideOnScroll: false,
        items: [
          { type: 'docSidebar', sidebarId: 'docs', position: 'left', label: 'Docs' },
          { type: 'docSidebar', docsPluginId: 'tutorials', sidebarId: 'tutorials', position: 'left', label: 'Samples' },
          { type: 'docSidebar', docsPluginId: 'api', sidebarId: 'api', position: 'left', label: 'API' },
          { to: '/changelog', label: 'Changelog', position: 'left' },
          { type: 'localeDropdown', position: 'right', className: 'navbar-locale' },
          { href: REPO, label: 'GitHub', position: 'right', className: 'navbar-github', 'aria-label': 'GitHub' },
        ],
      },
      footer: {
        style: 'dark',
        // One row: the author's contacts. Docs, GitHub, Asset Store and the changelog already live in the sidebar panel.
        links: [
          { label: 'Vladislav Panin', href: 'https://github.com/VPDPersonal' },
          { label: 'LinkedIn', href: 'https://www.linkedin.com/in/vladislav-panin-965048314/' },
          { label: 'X', href: 'https://x.com/VPDInc' },
          { label: 'vpd.aspid@gmail.com', href: 'mailto:vpd.aspid@gmail.com' },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Vladislav Panin. MIT License.`,
      },
      prism: {
        theme: venom.light,
        darkTheme: venom.dark,
        additionalLanguages: ['csharp', 'json', 'bash'],
      },
    }),
};

export default config;
