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
  githubLayout(tree, {site, repository, previews: 'docs/images/readme-previews', exists: (url) => files.includes(url)});
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

test('doc pages and the samples overview open on the site', () => {
  const result = layout(`${features}\n## Resources\n\n- [Samples](Packages/x/Samples~/README.md) — scenes.\n- [Raw](Website/tutorials/Types/README.md) — kept.\n`);
  assert.match(result, /<h4><a href="https:\/\/site.test\/docs\/enum-values">EnumValues<\/a><\/h4>/);
  assert.match(result, /<a href="https:\/\/site.test\/docs\/type-selector#settings">TypeSelector<\/a>/);
  assert.match(result, /<a href="https:\/\/site.test\/tutorials">Samples<\/a>/);
  assert.match(result, /<a href="Website\/tutorials\/Types\/README.md">Raw<\/a>/);
});

test('the link row drops the repository link', () => {
  const result = layout('[Docs](https://site.test/docs) · [Source code](https://github.com/owner/repo) · [Releases](https://github.com/owner/repo/releases)');
  assert.equal(result.trim(), '[Docs](https://site.test/docs) · [Releases](https://github.com/owner/repo/releases)');
});

test('the note on pinning a version folds into details with the URL as a block', () => {
  const result = layout('The URL installs the latest preview. To pin a version from [Releases](https://x.test), add its number: `https://github.com/owner/repo.git#upm-preview/1.0.0`.');
  assert.equal(result, 'The URL installs the latest preview.\n\n<details>\n<summary>Pin a version</summary>\n\n'
    + 'To pin a version from [Releases](https://x.test), add its number:\n\n```text\nhttps://github.com/owner/repo.git#upm-preview/1.0.0\n```\n\n</details>\n');
});

test('a feature becomes a card with a themed preview, its summary as HTML', () => {
  const result = layout(features, ['Images/enum-light.gif']);
  assert.match(result, /### Serialization\n\n<table>\n<tr>\n<td width="56%"><picture>/);
  assert.match(result, /<source media="\(prefers-color-scheme: light\)" srcset="Images\/enum-light.gif"><img src="Images\/enum.gif" alt="Populate rows" width="100%"><\/picture>/);
  assert.match(result, /<p>Maps enum keys to <code lang="class-name">T<\/code> values/);
  assert.match(result, /<p><a href="https:\/\/site.test\/docs\/enum-values">Read more →<\/a><\/p>\n<\/td>\n<\/tr>\n<\/table>/);
  assert.doesNotMatch(result, /####/);
});

test('a code preview gives way to its recording', () => {
  const result = layout(features, ['docs/images/readme-previews/profiler-markers.webp']);
  assert.match(result, /<img src="docs\/images\/readme-previews\/profiler-markers.webp" alt="ProfilerMarkers" width="100%">/);
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

test('an image without a light sibling and the banner stay as they are', () => {
  const banner = '<picture>\n  <source srcset="a.webp" />\n  <img src="a.png" alt="Banner" />\n</picture>';
  assert.match(layout(`${banner}\n\n![Badge](badge.svg)`, ['a-light.png']), /<img src="a.png" alt="Banner" \/>\n<\/picture>\n\n!\[Badge\]\(badge.svg\)/);
});
