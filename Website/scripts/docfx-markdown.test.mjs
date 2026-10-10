import assert from 'node:assert/strict';
import {test} from 'node:test';
import {
  disambiguateLabels, dropObjectExtensions, dropSection, escapeMdx, headingAnchors, namespaceCategory, parseToc, preToFence,
  shortName, titleOf, uidToAnchor, uidToFile, unescapeLinkTargets,
} from './docfx-markdown.mjs';

test('DocFX identifiers become file names, anchors and short names', () => {
  assert.equal(uidToFile('Aspid.FastTools.Types.SerializableType%601'), 'Aspid.FastTools.Types.SerializableType-1');
  assert.equal(uidToAnchor('Aspid.FastTools.Types.SerializableType%601.Get(System.Int32)'), 'Aspid_FastTools_Types_SerializableType_1_Get_System_Int32_');
  assert.equal(shortName('Aspid.FastTools.Types.SerializableType%602'), 'SerializableType<T1, T2>');
  assert.equal(shortName('Aspid.FastTools.Types.SerializableType%601', true), 'Types.SerializableType<T>');
  assert.equal(shortName('Aspid.FastTools.Types.Binder.Bind%60%601(System.Int32)'), 'Bind<T>');
});

test('an example block becomes a fenced code block with its entities decoded', () => {
  assert.equal(preToFence('<pre><code class="lang-csharp">var x = new List&lt;int&gt;();\nif (a &amp;&amp; b) { }\n</code></pre>'),
    '\n```csharp\nvar x = new List<int>();\nif (a && b) { }\n```\n');
  assert.equal(preToFence('<pre><code>plain</code></pre>'), '\n```\nplain\n```\n');
});

test('backslashes in link targets are removed and the rest of the text is kept', () => {
  assert.equal(unescapeLinkTargets('See [a](T.md\\#A\\_b\\-c) and Get\\(int\\).'), 'See [a](T.md#A_b-c) and Get\\(int\\).');
});

test('the inherited members and an empty extension methods heading are dropped', () => {
  const page = '#### Remarks\n\nText.\n\n#### Inherited Members\n\n`ToString`\n\n#### Extension Methods\n\n#### Next\n\nEnd';
  assert.equal(dropSection(page, 'Inherited Members'), '#### Remarks\n\nText.\n\n\n#### Extension Methods\n\n#### Next\n\nEnd');
  assert.equal(dropObjectExtensions(dropSection(page, 'Inherited Members')), '#### Remarks\n\nText.\n\n\n\n#### Next\n\nEnd');
});

test('braces and generic brackets outside code are escaped for MDX', () => {
  assert.equal(escapeMdx('Value {x} of Foo<Bar> and <T'), 'Value \\{x\\} of Foo\\<Bar> and \\<T');
  assert.equal(escapeMdx('`code {x} <T` and ```\n{y}\n``` and <p>html</p> <a id="z"></a> </p>'), '`code {x} <T` and ```\n{y}\n``` and <p>html</p> <a id="z"></a> </p>');
  assert.equal(escapeMdx('already \\{escaped\\} \\<T'), 'already \\{escaped\\} \\<T');
});

test('an inline anchor becomes a heading id and the title loses its escapes', () => {
  const heading = headingAnchors('# <a id="Aspid_Type"></a> Struct Type<T\\>\n\n### <a id="Get"></a> Get\\(int\\)\n');
  assert.equal(heading, '# Struct Type<T\\> {#Aspid_Type}\n\n### Get\\(int\\) {#Get}\n');
  assert.equal(titleOf(heading, 'file.md'), 'Struct Type<T>');
  assert.equal(titleOf('No heading', 'file.md'), 'file.md');
});

const toc = `### YamlMime:TableOfContent
- name: Aspid.FastTools.UIElements
  href: Aspid.FastTools.UIElements.md
  items:
  - name: Classes
  - name: VisualElementExtensions
    href: Aspid.FastTools.UIElements.VisualElementExtensions.md
  - name: Enums
- name: Aspid.FastTools.UIElements.Editors
  href: Aspid.FastTools.UIElements.Editors.md
  items:
  - name: VisualElementExtensions
    href: Aspid.FastTools.UIElements.Editors.VisualElementExtensions.md
`;

test('toc.yml becomes categories that keep the DocFX groups and skip empty ones', () => {
  const [ui, editors] = parseToc(toc).map(namespaceCategory);
  assert.equal(ui.link.id, 'Aspid.FastTools.UIElements');
  assert.deepEqual(ui.items.map(item => [item.type, item.label]), [['category', 'Classes']]);
  assert.deepEqual(ui.items[0].items, [{type: 'doc', id: 'Aspid.FastTools.UIElements.VisualElementExtensions', label: 'VisualElementExtensions'}]);
  assert.deepEqual(editors.items, [{type: 'doc', id: 'Aspid.FastTools.UIElements.Editors.VisualElementExtensions', label: 'VisualElementExtensions'}]);
});

test('two types with one name get the last part of their namespace', () => {
  const sidebar = parseToc(toc).map(namespaceCategory);
  disambiguateLabels(sidebar);
  assert.equal(sidebar[0].items[0].items[0].label, 'VisualElementExtensions (UIElements)');
  assert.equal(sidebar[1].items[0].label, 'VisualElementExtensions (Editors)');
});
