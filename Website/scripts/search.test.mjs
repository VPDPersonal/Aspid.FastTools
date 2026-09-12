import assert from 'node:assert/strict';
import {test} from 'node:test';
import {prepareIndex, searchEntries} from '../src/components/search.js';
import {plainText} from '../src/plugins/search/index.js';

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
test('empty queries suggest documentation and samples', () => {
  assert.equal(searchEntries(entries, '  ').length, 3);
  assert.ok(searchEntries(entries, '').every((entry) => entry.section !== 'API'));
});
