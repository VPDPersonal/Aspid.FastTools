import path from 'node:path';
import assert from 'node:assert/strict';
import {test} from 'node:test';
import {fileURLToPath} from 'node:url';
import remarkAgentPrompt from '../src/remark/agentPrompt.js';
import remarkCrossInstanceLinks from '../src/remark/crossInstanceLinks.js';
import remarkLiveDiagrams from '../src/remark/liveDiagrams.js';
import remarkThemedImages from '../src/remark/themedImages.js';
import {attribute, element, find, transform} from './remark-tree.mjs';

const site = fileURLToPath(new URL('..', import.meta.url));
const page = (...segments) => path.join(site, ...segments);
const ruDocs = ['i18n', 'ru', 'docusaurus-plugin-content-docs', 'current'];
const ruTutorials = ['i18n', 'ru', 'docusaurus-plugin-content-docs-tutorials', 'current'];
const urls = tree => find(tree, node => node.type === 'link' || node.type === 'definition').map(node => node.url);
const links = (file, ...targets) => urls(transform(remarkCrossInstanceLinks(), targets.map(url => `[x](${url})`).join('\n\n'), file));

test('a tutorial links a doc and a doc links a tutorial as routes', () => {
  assert.deepEqual(links(page('tutorials', 'Types', 'README.md'), '../../docs/03-type-selector.md#settings', '../../docs/README.md'),
    ['/docs/type-selector#settings', '/docs']);
  assert.deepEqual(links(page('docs', '02-serializable-types.md'), '../tutorials/Types/README.md', '../tutorials/SerializeReferences/README.md#repair'),
    ['/tutorials/types', '/tutorials/serialize-references#repair']);
});

test('translated pages link translated targets and get the same routes', () => {
  assert.deepEqual(links(page(...ruDocs, '03-type-selector.md'), '../../docusaurus-plugin-content-docs-tutorials/current/Types/README.md'), ['/tutorials/types']);
  assert.deepEqual(links(page(...ruTutorials, 'Types', 'README.md'), '../../../docusaurus-plugin-content-docs/current/03-type-selector.md'), ['/docs/type-selector']);
});

test('the samples overview in the package goes to the gallery', () => {
  const overview = '../../Aspid.FastTools/Packages/tech.aspid.fasttools/Samples~';
  assert.deepEqual(links(page('docs', '02-serializable-types.md'), `${overview}/README.md`, `${overview}/README.ru.md#types`), ['/tutorials', '/tutorials#types']);
});

test('links inside one instance, external links, anchors and images stay as they are', () => {
  const same = ['03-type-selector.md', 'https://example.test/a.md', '#settings', 'Images/demo.gif', '../tutorials/Types/Images/demo.gif'];
  assert.deepEqual(links(page('docs', '02-serializable-types.md'), ...same), same);
});

test('a link definition is rewritten like a link', () => {
  const tree = transform(remarkCrossInstanceLinks(), '[x][types]\n\n[types]: ../tutorials/Types/README.md\n', page('docs', '02-serializable-types.md'));
  assert.deepEqual(urls(tree), ['/tutorials/types']);
});

test('a text prompt followed by a table becomes one agent prompt', () => {
  const table = '| A | B |\n|---|---|\n| 1 | 2 |\n';
  const [prompt] = find(transform(remarkAgentPrompt(), `\`\`\`text prompt\nAdd a marker.\n\`\`\`\n\n${table}`), element('AgentPrompt'));
  assert.equal(attribute(prompt, 'prompt'), 'Add a marker.');
  assert.equal(prompt.children[0].type, 'table');
  const plain = transform(remarkAgentPrompt(), `\`\`\`text\nAdd a marker.\n\`\`\`\n\n${table}`);
  assert.deepEqual(plain.children.map(node => node.type), ['code', 'table']);
});

test('a static diagram becomes its live component and keeps its caption', () => {
  const tree = transform(remarkLiveDiagrams(), '![Marker tree](Images/profiler-markers-hierarchy.svg)\n\nMarker tree\n\n![Skill](Images/agent-skills-enum-values.svg)\n\n![Other](Images/other.svg)\n');
  assert.deepEqual(tree.children.map(node => node.name ?? node.type), ['ProfilerHierarchy', 'paragraph', 'AgentSession', 'paragraph']);
  assert.equal(attribute(tree.children[0], 'alt'), 'Marker tree');
  assert.equal(tree.children[1].data.hProperties.className, 'doc-media-caption');
  assert.equal(attribute(tree.children[2], 'skill'), 'aspid-enum-values');
});

test('the Styles table gets one row component per call', () => {
  const table = '| Unity | FastTools |\n|---|---|\n| <code lang="csharp">SetPadding(8)</code> | every side |\n| <code lang="csharp">SetPadding(8, 4)</code> | vertical, horizontal |\n';
  const [styles] = find(transform(remarkLiveDiagrams(), table), element('StyleSides'));
  assert.deepEqual(styles.children.map(row => [row.name, attribute(row, 'call')]), [['StyleSidesRow', 'SetPadding(8)'], ['StyleSidesRow', 'SetPadding(8, 4)']]);
  assert.equal(find(transform(remarkLiveDiagrams(), table.replace('SetPadding(8)', 'SetMargin(8)')), element('StyleSides')).length, 0);
});

test('the Project References steps become the panel with one span per step', () => {
  const steps = '1. Open **Tools → Aspid 🐍 → FastTools → Project References**.\n2. Click **Scan Project**.\n';
  const [panel] = find(transform(remarkLiveDiagrams(), steps), element('ProjectReferencesPanel'));
  assert.deepEqual(panel.children.map(step => step.name), ['span', 'span']);
  assert.equal(find(transform(remarkLiveDiagrams(), steps.replace('Project References', 'Welcome')), element('ProjectReferencesPanel')).length, 0);
});

test('an image with a light sibling is shown per theme, and a scene sample keeps its own background', () => {
  const tree = transform(remarkThemedImages(), '![Demo](Images/demo.gif)\n\nDemo\n\n![Other](Images/no-such-image.png)\n', page('tutorials', 'Types', 'README.md'));
  const [dark, light] = find(tree, element('span'));
  assert.equal(attribute(dark, 'className'), 'theme-image--dark sample-scene');
  assert.equal(attribute(light, 'className'), 'theme-image--light sample-scene');
  assert.equal(light.children[0].url, 'Images/demo-light.gif');
  assert.equal(tree.children[1].data.hProperties.className, 'doc-media-caption');
  assert.equal(find(tree, node => node.type === 'image' && node.url === 'Images/no-such-image.png').length, 1);
  assert.equal(find(tree, element('span')).length, 2);
});

test('a sample capture linked from a doc page is scene footage', () => {
  const tree = transform(remarkThemedImages(), '![Demo](../tutorials/Types/Images/demo.gif)\n', page('docs', '02-serializable-types.md'));
  assert.deepEqual(find(tree, element('span')).map(node => attribute(node, 'className')), ['theme-image--dark scene-footage', 'theme-image--light scene-footage']);
});
