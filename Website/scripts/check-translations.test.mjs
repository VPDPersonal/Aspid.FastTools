import assert from 'node:assert/strict';
import {test} from 'node:test';
import {compare, englishPath, outline} from './check-translations.mjs';

const en = '# Title\n\n## Usage\n\nSee [types](02-types.md#quick-start) and [below](#usage).\n\n![Demo](Images/demo.gif)\n\n```csharp\n// Pick a type\n[TypeSelector] string _type;\n```\n';
const ru = '# Заголовок\n\n## Использование\n\nСм. [типы](02-types.md#быстрый-старт) и [ниже](#использование).\n\n![Демо](../../../../docs/Images/demo.gif)\n\n```csharp\n// Выбери тип\n[TypeSelector] string _type;\n```\n';
const doc = 'Website/docs';
const ruDoc = 'Website/i18n/ru/docusaurus-plugin-content-docs/current';

test('translated paths map to their English source', () => {
  assert.equal(englishPath(`${ruDoc}/03-type-selector.md`), `${doc}/03-type-selector.md`);
  assert.equal(englishPath('Website/i18n/ru/docusaurus-plugin-content-docs-tutorials/current/Types/README.md'), 'Website/tutorials/Types/README.md');
  assert.equal(englishPath('Pkg/Samples~/Types/Documentation/README.ru.md'), 'Pkg/Samples~/Types/Documentation/README.md');
});
test('prose, comments and anchors may differ', () => {
  assert.deepEqual(compare(outline(en, `${doc}/README.md`), outline(ru, `${ruDoc}/README.md`)), []);
});
test('a changed code line, image or heading level is reported', () => {
  const english = outline(en, `${doc}/README.md`);
  for (const broken of [ru.replace('string _type', 'Type _type'), ru.replace('demo.gif', 'old.gif'), ru.replace('## Исп', '### Исп')]) {
    assert.equal(compare(english, outline(broken, `${ruDoc}/README.md`)).length, 1);
  }
});
