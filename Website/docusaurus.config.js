// @ts-check
import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import venom from './src/prism/venom.js';
import remarkGithubAdmonitionsToDirectives from 'remark-github-admonitions-to-directives';
import remarkCrossInstanceLinks from './src/remark/crossInstanceLinks.js';
import remarkThemedImages from './src/remark/themedImages.js';
import remarkIntroBanner, {remarkStatusBadges} from './src/remark/introBanner.js';

const PACKAGE = '../Aspid.FastTools/Packages/tech.aspid.fasttools';
const PACKAGE_DIR = PACKAGE.replace(/^\.\.\//, ''); // repository-relative, for "Edit this page" links
const LOCALES = ['en', 'ru'];
// Translations live in `Documentation/<locale>/`; they must not be picked up as English pages.
const TRANSLATION_FOLDERS = LOCALES.filter((locale) => locale !== 'en').map((locale) => `${locale}/**`);
const REPO = 'https://github.com/VPDPersonal/Aspid.FastTools';
const ASSET_STORE = 'https://assetstore.unity.com/packages/slug/365584';
/** The docs in the working tree describe the package version in the working tree. */
const PACKAGE_VERSION = JSON.parse(readFileSync(new URL(`${PACKAGE}/package.json`, import.meta.url), 'utf8')).version;
/** The UPM branch the install URL points at; each release tags it `<branch>/<version>`. */
const UPM_BRANCH = 'upm-preview';

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
 * The versions the install panel can pin, newest first: the UPM branch's tags on GitHub, or the local ones when offline.
 * The working-tree version is always offered, since its tag may be pushed after the docs are built.
 */
function readPackageVersions() {
  const read = (args) => {
    try {
      return execFileSync('git', args, {encoding: 'utf8', timeout: 15000, stdio: ['ignore', 'pipe', 'ignore']});
    } catch {
      return '';
    }
  };
  const prefix = `${UPM_BRANCH}/`;
  const tags = read(['ls-remote', '--tags', '--refs', `${REPO}.git`, `${prefix}*`]) || read(['tag', '-l', `${prefix}*`]);
  const versions = tags.split('\n').map((line) => line.split(prefix)[1]?.trim()).filter(Boolean);
  return [...new Set([PACKAGE_VERSION, ...versions])].sort(compareVersions).reverse();
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
  beforeDefaultRemarkPlugins: [remarkGithubAdmonitionsToDirectives, remarkCrossInstanceLinks, remarkThemedImages],
  showLastUpdateTime: true,
  // Translations live next to the English sources: `Documentation/<locale>/<file>`.
  editUrl: ({ docPath, locale }) =>
    `${REPO}/edit/main/${PACKAGE_DIR}/Documentation/${locale === 'en' ? '' : `${locale}/`}${docPath}`,
};

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Aspid.FastTools',
  tagline: 'Unity tools that cut boilerplate',
  favicon: 'img/favicon.png',

  url: 'https://vpdpersonal.github.io',
  baseUrl: '/Aspid.FastTools/',
  customFields: { assetStore: ASSET_STORE, packageVersion: PACKAGE_VERSION, packageVersions: readPackageVersions() },
  organizationName: 'VPDPersonal',
  projectName: 'Aspid.FastTools',
  trailingSlash: false,

  onBrokenLinks: 'throw',
  onBrokenAnchors: 'warn',
  markdown: {
    // The shared introduction uses a banner instead of a heading; retain its page metadata.
    async parseFrontMatter({filePath, fileContent, defaultParseFrontMatter}) {
      const result = await defaultParseFrontMatter({filePath, fileContent});
      if (fileContent.includes('/aspid_fasttools_readme_banner.gif')) {
        result.frontMatter.title ??= 'Aspid.FastTools';
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
        // Main documentation lives inside the UPM package so it ships to Unity users as-is.
        // Translations sit next to it in `Documentation/<locale>/` and are wired in by scripts/sync-i18n.mjs.
        docs: {
          path: `${PACKAGE}/Documentation`,
          routeBasePath: 'docs',
          breadcrumbs: false,
          sidebarPath: './sidebars.js',
          exclude: ['**/*.meta', ...TRANSLATION_FOLDERS],
          versions: { current: { label: PACKAGE_VERSION } },
          ...markdownOptions,
          beforeDefaultRemarkPlugins: [[remarkIntroBanner, {baseUrl: '/Aspid.FastTools/', siteUrl: 'https://vpdpersonal.github.io'}], ...markdownOptions.beforeDefaultRemarkPlugins],
          remarkPlugins: [remarkStatusBadges],
        },
        blog: false,
        theme: { customCss: './src/css/custom.css' },
      }),
    ],
  ],

  plugins: [
    './src/plugins/search/index.js',
    [
      // Tutorials are generated from each sample's Documentation folder by scripts/sync-i18n.mjs.
      // The generated tree keeps the public routes flat while the package keeps docs and images out of sample roots.
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
        // `<Sample>/README.md` → `Samples~/<Sample>/Documentation/README.md`, translations as `README.<locale>.md`.
        editUrl: ({ docPath, locale }) =>
          `${REPO}/edit/main/${PACKAGE_DIR}/Samples~/${docPath.replace(
            /\/README\.md$/,
            locale === 'en' ? '/Documentation/README.md' : `/Documentation/README.${locale}.md`,
          )}`,
      }),
    ],
    [
      // The root CHANGELOG.md (and CHANGELOG.<locale>.md), copied in by scripts/sync-i18n.mjs.
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
