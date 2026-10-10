import assert from 'node:assert/strict';
import path from 'node:path';
import {test} from 'node:test';
import {fileURLToPath} from 'node:url';
import {prepareIndex, searchEntries} from '../src/components/search.js';
import searchPlugin, {plainText} from '../src/plugins/search/index.js';

const entries = prepareIndex([
  {title: 'Types Sample', section: 'Samples', url: '/tutorials/types', text: 'Choose Enemy Type in the Inspector.'},
  {title: 'Serializable Type System', section: 'Docs', url: '/docs/serializable-types', text: 'SerializableType<T> stores a type. Choose Enemy Type in a sample.'},
  {title: 'EnumValues<T>', section: 'API', url: '/api/enum', text: 'Maps enum values to data.'},
  {title: 'Выбор типа', section: 'Docs', url: '/ru/docs/types', text: 'Сохраняет объекты и настраивает поведение.'},
]);

test('exact titles rank first and multiple terms can match content', () => {
  assert.equal(searchEntries(entries, 'Types Sample')[0].url, '/tutorials/types');
  assert.equal(searchEntries(entries, 'enemy inspector')[0].url, '/tutorials/types');
  assert.deepEqual(searchEntries(entries, 'not-a-feature'), []);
});
test('search supports Cyrillic and generic type names', () => {
  assert.equal(searchEntries(entries, 'ОБЪЕКТЫ')[0].url, '/ru/docs/types');
  assert.equal(searchEntries(entries, 'SerializableType<T>')[0].url, '/docs/serializable-types');
  assert.equal(searchEntries(entries, 'EnumValues<T>')[0].url, '/api/enum');
});
test('index strips Markdown markup while preserving searchable code', () => {
  assert.equal(plainText('---\ntitle: Hidden\n---\n# Types\n![Screenshot](image.png)\n[Guide](guide.md)\n```csharp\nSerializableType<T> field;\n```'), 'Types Guide SerializableType<T> field;');
});
test('index decodes entities and drops table rules and MDX machinery', () => {
  const table = '| До — Unity API | После — FastTools |\n| --- | --- |\n| <pre>private static readonly&#10;ProfilerMarker _m;</pre> | <pre>if (x) &#123; y(); &#125;</pre> |';
  assert.equal(plainText(table), 'До — Unity API После — FastTools private static readonly ProfilerMarker _m; if (x) { y(); }');
  assert.equal(plainText('Intro\n\n---\n\nNext &lt;T&gt; &amp; more'), 'Intro Next <T> & more');
  assert.equal(plainText("import SamplesGallery from '@site/src/components/SamplesGallery';\n\n<SamplesGallery />\n\nUse <None> or List<float>."), 'Use <None> or List<float>.');
  assert.equal(plainText('<ol className="enum-lookup-flow"><li><b>Exact</b> match</li></ol>'), 'Exact match');
});
test('index drops the banner picture and the empty links of image badges', () => {
  const banner = [
    '<picture>',
    '  <source media="(prefers-color-scheme: dark)" srcset="banner_dark.webp" />',
    '  <img src="banner.png" alt="Aspid.FastTools" />',
    '</picture>',
    '',
    '[![Unity 6.0+](Images/badge.svg)](https://example.com/unity)',
    '[![MIT License](Images/license.svg)](https://example.com/license)',
    '',
    'Aspid.FastTools is a Unity package.',
    '',
    '[Documentation](https://example.com/docs)',
  ].join('\n');
  assert.equal(plainText(banner), 'Aspid.FastTools is a Unity package. Documentation');
});
test('empty queries suggest documentation and samples', () => {
  assert.equal(searchEntries(entries, '  ').length, 3);
  assert.ok(searchEntries(entries, '').every((entry) => entry.section !== 'API'));
});
test('index URLs follow trailingSlash: false except the locale root', async () => {
  const siteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
  const doc = (permalink) => ({permalink, title: 'Samples', source: '@site/tutorials/index.mdx'});
  const allContent = {'docusaurus-plugin-content-docs': {tutorials: {loadedVersions: [{docs: [
    doc('/Aspid.FastTools/ru/'), doc('/Aspid.FastTools/ru/tutorials/'), doc('/Aspid.FastTools/ru/tutorials/types/'),
    doc('/Aspid.FastTools/ru/docs/enum-values'),
  ]}]}}};
  let index;
  const context = {siteDir, baseUrl: '/Aspid.FastTools/ru/', siteConfig: {trailingSlash: false, baseUrl: '/Aspid.FastTools/'}};
  await searchPlugin(context).allContentLoaded({allContent, actions: {createData: async (name, data) => { index = JSON.parse(data); }}});
  assert.deepEqual(index.map((entry) => entry.url),
    ['/Aspid.FastTools/ru/', '/Aspid.FastTools/ru/tutorials', '/Aspid.FastTools/ru/tutorials/types', '/Aspid.FastTools/ru/docs/enum-values']);
});
