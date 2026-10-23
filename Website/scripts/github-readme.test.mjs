import assert from 'node:assert/strict';
import {test} from 'node:test';
import {unified} from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import remarkStringify from 'remark-stringify';
import {githubLayout} from './github-readme.mjs';

const processor = unified().use(remarkParse).use(remarkGfm).use(remarkStringify, {fences: true});
const site = 'https://site.test/';
const repository = 'https://github.com/owner/repo';

function layout(markdown, files = []) {
  const tree = processor.parse(markdown);
  githubLayout(tree, {site, locales: ['en', 'ru'], repository, previews: 'Website/docs/Images/readme-previews',
    images: 'Website/docs/Images', exists: (url) => files.includes(url)});
  return processor.stringify(tree);
}

const features = `## Features

### Serialization

#### [EnumValues](Website/docs/08-enum-values.md)

Maps enum keys to <code lang="class-name">T</code> values, see [TypeSelector](Website/docs/03-type-selector.md#settings).

<img src="Images/enum.gif" alt="Populate rows" width="640" />

### Editor & tooling

#### [ProfilerMarkers](Website/docs/09-profiler-markers.md)

Marks a section with one line.

\`\`\`csharp
using var _ = this.Marker();
\`\`\`
`;

test('doc pages and the tutorials gallery open on the site', () => {
  const result = layout(`${features}\n## Resources\n\n- [Samples](Website/tutorials/index.mdx) — scenes.\n- [Raw](Website/tutorials/Types/README.md) — kept.\n`);
  assert.match(result, /<h4><a href="https:\/\/site.test\/docs\/enum-values">EnumValues<\/a><\/h4>/);
  assert.match(result, /<a href="https:\/\/site.test\/docs\/type-selector#settings">TypeSelector<\/a>/);
  assert.match(result, /<a href="https:\/\/site.test\/tutorials">Samples<\/a>/);
  assert.match(result, /<a href="Website\/tutorials\/Types\/README.md">Raw<\/a>/);
});

test('the link row lists the site languages for a link to the site', () => {
  const result = layout('[Documentation](https://site.test/docs) · [Releases](https://github.com/owner/repo/releases)');
  assert.equal(result.trim(), 'Documentation: [EN](https://site.test/docs), [RU](https://site.test/ru/docs) · [Releases](https://github.com/owner/repo/releases)');
});

test('the note under the install URL links the install panel on the site', () => {
  const result = layout('```text\nhttps://github.com/owner/repo.git#upm\n```\n\nThe URL installs the latest release.\n');
  assert.match(result, /\n\nThe URL installs the latest release\. To pin a version or switch channels, see \[Installation\]\(https:\/\/site.test\/docs#installation\) in the documentation\.\n$/);
});

test('the install instruction becomes a card with the walk-through and the steps', () => {
  const install = '## Installation\n\nIn **Window → Package Manager**, paste this URL:\n\n```text\nhttps://github.com/owner/repo.git#upm\n```\n\nThe URL installs the latest release.\n';
  const result = layout(install, ['Website/docs/Images/readme-previews/install.webp']);
  assert.match(result, /^## Installation\n\n<table>\n<tr>\n<td width="56%"><img src="Website\/docs\/Images\/readme-previews\/install.webp"/);
  assert.match(result, /<ol>\n<li>Open <b>Window → Package Manager<\/b><\/li>\n.*\n<li>Paste the URL and press <b>Install<\/b><\/li>\n<\/ol>/);
  assert.match(result, /<\/table>\n\n```text\nhttps:\/\/github.com\/owner\/repo.git#upm\n```/);
  assert.match(layout(install), /^## Installation\n\nIn \*\*Window → Package Manager\*\*, paste this URL:/);
});

test('a feature becomes a card with its dark preview only, its summary as HTML', () => {
  const result = layout(features, ['Images/enum-light.gif']);
  assert.match(result, /### Serialization\n\n<table>\n<tr>\n<td width="56%"><img src="Images\/enum.gif" alt="Populate rows" width="100%"><\/td>/);
  assert.match(result, /<p>Maps enum keys to <code lang="class-name">T<\/code> values/);
  assert.match(result, /<p><a href="https:\/\/site.test\/docs\/enum-values">Read more →<\/a><\/p>\n<\/td>\n<\/tr>\n<\/table>/);
  assert.doesNotMatch(result, /####/);
});

test('a code preview gives way to its recording', () => {
  const result = layout(features, ['Website/docs/Images/readme-previews/profiler-markers.webp']);
  assert.match(result, /<img src="Website\/docs\/Images\/readme-previews\/profiler-markers.webp" alt="ProfilerMarkers" width="100%">/);
});

test('without a recording the code stays a fenced block inside the card', () => {
  assert.match(layout(features), /<td width="56%">\n\n```csharp\nusing var _ = this.Marker\(\);\n```\n\n<\/td>\n<td width="44%">/);
});

test('a list of linked summaries becomes a row of tiles', () => {
  const result = layout('## Resources\n\n- [API](https://example.com/api) — types and methods.\n- [Changelog](CHANGELOG.md) — changes by version.\n');
  assert.match(result, /<td width="50%" valign="top"><b><a href="https:\/\/example.com\/api">API<\/a><\/b><br>Types and methods.<\/td>/);
  assert.match(result, /<td width="50%" valign="top"><b><a href="CHANGELOG.md">Changelog<\/a><\/b><br>Changes by version.<\/td>/);
});

test('the help section becomes the call to action, without the licence line', () => {
  const result = layout('## Help\n\nAsk in [Issues](https://github.com/owner/repo/issues).\n\nStar it on [GitHub](https://github.com/owner/repo).\n\n'
    + 'Under the [MIT License](https://github.com/owner/repo/blob/main/LICENSE).\n');
  assert.equal(result, '## Help\n\n<p>Found a bug or have a question? Include your Unity version, package version and steps to reproduce.</p>\n'
    + '<p><a href="https://github.com/owner/repo/issues"><b>Open an issue</b></a> · <a href="https://github.com/owner/repo">Star on GitHub</a></p>\n');
});

test('the donation section becomes a framed card with the button image', () => {
  const store = 'https://assetstore.unity.com/packages/package/1';
  const section = `## Support\n\nSupport it on the [Asset Store](${store}).\n`;
  assert.equal(layout(section, ['Website/docs/Images/support-asset-store.svg']), '## Support\n\n<table>\n<tr>\n<td>\n'
    + `<h3>Enjoying Aspid.FastTools?</h3>\n<p>Support it on the <a href="${store}">Asset Store</a>.</p>\n`
    + `<p><a href="${store}"><img src="Website/docs/Images/support-asset-store.svg" alt="Support on the Asset Store" height="48"></a></p>\n`
    + '</td>\n</tr>\n</table>\n');
  assert.equal(layout(section), section, 'without the button image the section stays as it is');
});

test('images and the banner stay as they are, without a light variant', () => {
  const banner = '<picture>\n  <source srcset="a.webp" />\n  <img src="a.png" alt="Banner" />\n</picture>';
  assert.match(layout(`${banner}\n\n![Badge](badge.svg)`, ['a-light.png']), /<img src="a.png" alt="Banner" \/>\n<\/picture>\n\n!\[Badge\]\(badge.svg\)/);
  assert.doesNotMatch(layout('![Shot](shot.png)', ['shot-light.png']), /picture|light/);
});
