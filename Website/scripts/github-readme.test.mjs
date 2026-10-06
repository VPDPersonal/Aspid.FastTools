import assert from 'node:assert/strict';
import {test} from 'node:test';
import {unified} from 'unified';
import remarkParse from 'remark-parse';
import remarkGfm from 'remark-gfm';
import remarkStringify from 'remark-stringify';
import {githubLayout} from './github-readme.mjs';

const processor = unified().use(remarkParse).use(remarkGfm).use(remarkStringify, {fences: true});

function layout(markdown, {language = 'en', files = []} = {}) {
  const tree = processor.parse(markdown);
  githubLayout(tree, {
    language,
    translations: {en: 'README.md', ru: 'README.ru.md'},
    previews: 'docs/images/readme-previews',
    exists: (url) => files.includes(url),
  });
  return processor.stringify(tree);
}

const features = `## Features

### Serialization

#### [EnumValues](Website/docs/08-enum-values.md)

Maps enum keys to <code lang="class-name">T</code> values.

<img src="Images/enum.gif" alt="Populate rows" width="640" />

### Editor & tooling

#### [ProfilerMarkers](Website/docs/09-profiler-markers.md)

Marks a section with one line.

\`\`\`csharp
using var _ = this.Marker();
\`\`\`
`;

test('the other translation is linked under the link row, or on top without one', () => {
  const header = '[![Badge](badge.svg)](https://example.com)\n\nLede.\n\n[Docs](https://example.com/docs) · [Releases](https://example.com/releases)\n\n## Installation';
  assert.match(layout(header), /Releases\]\(https:\/\/example.com\/releases\)\n\n<p><b>English<\/b> · <a href="README.ru.md">Русский<\/a><\/p>\n\n## Installation/);
  assert.match(layout('Text', {language: 'ru'}), /^<p><a href="README.md">English<\/a> · <b>Русский<\/b><\/p>/);
});

test('a feature becomes a card with a themed preview, its summary as HTML', () => {
  const result = layout(features, {files: ['Images/enum-light.gif']});
  assert.match(result, /### Serialization\n\n<div>\n<picture>/);
  assert.match(result, /<source media="\(prefers-color-scheme: light\)" srcset="Images\/enum-light.gif"><img src="Images\/enum.gif" alt="Populate rows" width="56%" align="left"><\/picture>/);
  assert.match(result, /<h4><a href="Website\/docs\/08-enum-values.md">EnumValues<\/a><\/h4>\n<p>Maps enum keys to <code lang="class-name">T<\/code> values.<\/p>/);
  assert.match(result, /<p><a href="Website\/docs\/08-enum-values.md">Read more →<\/a><\/p>\n<br clear="all">\n<\/div>/);
  assert.doesNotMatch(result, /####/);
});

test('a code preview gives way to its recording, the translated one first', () => {
  const files = ['docs/images/readme-previews/profiler-markers.webp', 'docs/images/readme-previews/profiler-markers-ru.webp'];
  assert.match(layout(features, {files}), /<img src="docs\/images\/readme-previews\/profiler-markers.webp" alt="ProfilerMarkers" width="56%" align="left">/);
  assert.match(layout(features, {files, language: 'ru'}), /<img src="docs\/images\/readme-previews\/profiler-markers-ru.webp"/);
  assert.match(layout(features, {files, language: 'ru'}), /Подробнее →/);
});

test('without a recording the code stays a fenced block above the text', () => {
  assert.match(layout(features), /```csharp\nusing var _ = this.Marker\(\);\n```\n\n<div>\n<h4>/);
});

test('a list of linked summaries becomes a row of tiles', () => {
  const result = layout('## Resources\n\n- [API](https://example.com/api) — types and methods.\n- [Changelog](CHANGELOG.md) — changes by version.\n');
  assert.match(result, /<td width="50%" valign="top"><b><a href="https:\/\/example.com\/api">API<\/a><\/b><br>Types and methods.<\/td>/);
  assert.match(result, /<td width="50%" valign="top"><b><a href="CHANGELOG.md">Changelog<\/a><\/b><br>Changes by version.<\/td>/);
});

test('an image without a light sibling and the banner stay as they are', () => {
  const banner = '<picture>\n  <source srcset="a.webp" />\n  <img src="a.png" alt="Banner" />\n</picture>';
  assert.match(layout(`${banner}\n\n![Badge](badge.svg)`, {files: ['a-light.png']}), /<img src="a.png" alt="Banner" \/>\n<\/picture>\n\n!\[Badge\]\(badge.svg\)/);
});
