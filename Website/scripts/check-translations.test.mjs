import assert from 'node:assert/strict';
import {test} from 'node:test';
import {compare, compareChangelogs, englishPath, needsTranslation, outline, russianPath} from './check-translations.mjs';

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

test('an English path maps to its Russian twin and back', () => {
  const pairs = [
    [`${doc}/03-type-selector.md`, `${ruDoc}/03-type-selector.md`],
    ['Website/tutorials/index.mdx', 'Website/i18n/ru/docusaurus-plugin-content-docs-tutorials/current/index.mdx'],
    ['Pkg/Samples~/Types/Documentation/README.md', 'Pkg/Samples~/Types/Documentation/README.ru.md'],
    ['CHANGELOG.md', 'CHANGELOG.ru.md'],
  ];
  for (const [english, russian] of pairs) {
    assert.equal(russianPath(english), russian);
    assert.equal(englishPath(russian), english);
  }
});
test('docs, tutorials, sample READMEs and the changelog need a Russian twin', () => {
  const sample = 'Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~';
  for (const file of [`${doc}/README.md`, 'Website/tutorials/index.mdx', 'Website/tutorials/Types/README.md', 'CHANGELOG.md',
    `${sample}/README.md`, `${sample}/Types/Documentation/README.md`]) assert.ok(needsTranslation(file), file);
  for (const file of ['README.md', `${sample}/Types/Documentation/README.ru.md`, `${sample}/Types/Fonts/LICENSE-iA-Writer-Quattro.md`,
    'Aspid.FastTools/Packages/tech.aspid.fasttools/README.md', `${ruDoc}/README.md`]) assert.ok(!needsTranslation(file), file);
});

const changelog = '# Changelog\n\n## [Unreleased]\n\n### Added\n\n- One.\n- Two.\n\n### Fixed\n\n- Three.\n\n## [1.0.0] — 2026-01-01\n\n### Added\n\n- Four.\n\n[1.0.0]: https://example.test/1.0.0\n';
const changelogRu = '# Журнал изменений\n\n## [Unreleased]\n\n### Добавлено\n\n- Раз.\n- Два.\n\n### Исправлено\n\n- Три.\n\n## [1.0.0] — 2026-01-01\n\n### Добавлено\n\n- Четыре.\n\n[1.0.0]: https://example.test/1.0.0\n';

test('changelogs with the same releases and entry counts match', () => {
  assert.deepEqual(compareChangelogs(changelog, changelogRu), []);
});
test('an entry added to one changelog only is reported with its release and section', () => {
  const problems = compareChangelogs(changelog, changelogRu.replace('- Три.\n', '- Три.\n- Лишнее.\n'));
  assert.deepEqual(problems, ['[Unreleased] / Fixed entries: 1 in English, 2 in Russian']);
});
test('a different version or date in a release heading is reported', () => {
  assert.equal(compareChangelogs(changelog, changelogRu.replace('2026-01-01', '2026-01-02')).length, 1);
  assert.equal(compareChangelogs(changelog, changelogRu.replace('## [1.0.0]', '## [1.0.1]')).length, 1);
});
test('a release missing from one changelog is reported', () => {
  const problems = compareChangelogs(changelog, changelogRu.replace(/\n## \[1\.0\.0\][\s\S]*?- Четыре\.\n/, ''));
  assert.match(problems[0], /changelog headings: 5 in English, 3 in Russian/);
});
