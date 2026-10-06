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

test('the other translation is linked at the top', () => {
  assert.match(layout('Text'), /^<p align="right"><b>English<\/b> · <a href="README.ru.md">Русский<\/a><\/p>/);
  assert.match(layout('Text', {language: 'ru'}), /^<p align="right"><a href="README.md">English<\/a> · <b>Русский<\/b><\/p>/);
});

test('a feature becomes a card with a themed preview, its summary as HTML', () => {
  const result = layout(features, {files: ['Images/enum-light.gif']});
  assert.match(result, /### Serialization\n\n<table>\n<tr>\n<td width="56%"><picture>/);
  assert.match(result, /<source media="\(prefers-color-scheme: light\)" srcset="Images\/enum-light.gif"><img src="Images\/enum.gif" alt="Populate rows" width="100%"><\/picture>/);
  assert.match(result, /<h4><a href="Website\/docs\/08-enum-values.md">EnumValues<\/a><\/h4>\n<p>Maps enum keys to <code lang="class-name">T<\/code> values.<\/p>/);
  assert.match(result, /<p><a href="Website\/docs\/08-enum-values.md">Read more →<\/a><\/p>/);
  assert.doesNotMatch(result, /####/);
});

test('a code preview gives way to its recording, the translated one first', () => {
  const files = ['docs/images/readme-previews/profiler-markers.webp', 'docs/images/readme-previews/profiler-markers-ru.webp'];
  assert.match(layout(features, {files}), /<img src="docs\/images\/readme-previews\/profiler-markers.webp" alt="ProfilerMarkers" width="100%">/);
  assert.match(layout(features, {files, language: 'ru'}), /<img src="docs\/images\/readme-previews\/profiler-markers-ru.webp"/);
  assert.match(layout(features, {files, language: 'ru'}), /Подробнее →/);
});

test('without a recording the code stays a fenced block inside the card', () => {
  assert.match(layout(features), /<td width="56%">\n\n```csharp\nusing var _ = this.Marker\(\);\n```\n\n<\/td>\n<td width="44%">/);
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
