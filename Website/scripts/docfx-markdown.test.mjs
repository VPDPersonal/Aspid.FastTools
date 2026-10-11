import assert from 'node:assert/strict';
import {test} from 'node:test';
import {
  collectReceiverBases, disambiguateLabels, dropForeignExtensions, dropObjectExtensions, dropSection, escapeMdx,
  headingAnchors, joinParagraphs, namespaceCategory, parseToc, preToFence, shortName, titleOf, uidToAnchor, uidToFile,
  unescapeLinkTargets,
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

test('a paragraph spread over lines becomes one line', () => {
  const source = 'Remarks\n\n<p>\nFirst line\nsecond [`x`](y.md) line.\n</p>\n<p>\nNext.\n</p>\n\n## Methods\n';
  assert.equal(joinParagraphs(source), 'Remarks\n\n<p>First line second [`x`](y.md) line.</p>\n<p>Next.</p>\n\n## Methods\n');
});
test('a paragraph with a code fence is left alone', () => {
  const source = '<p>\nText\n\n```csharp\nvar a = 1;\nvar b = 2;\n```\n</p>\n';
  assert.equal(joinParagraphs(source), source);
});
test('a paragraph on one line is left alone', () => {
  const source = '<p>One line.</p>\n<p>Another <code>one</code>.</p>\n';
  assert.equal(joinParagraphs(source), source);
});

const member = (anchor, signature) => `### <a id="${anchor}"></a> Name\n\nSummary.\n\n\`\`\`csharp\n${signature}\n\`\`\`\n\n#### Parameters\n\n`;
const extensions = [
  member('A_SetMaxLength', 'public static TField SetMaxLength<TField, TValue>(this TField element, int value) where TField : TextInputBaseField<TValue>'),
  member('A_SetHighValue', 'public static T SetHighValue<T, TValue>(this T element, TValue value) where T : BaseSlider<TValue> where TValue : IComparable<TValue>'),
  member('A_SetValue', 'public static T SetValue<T, TValue>(this T element, TValue value, bool notify = true) where T : INotifyValueChanged<TValue>'),
  member('A_Both', 'public static T Both<T, TValue>(this T element, TValue value) where T : VisualElement, INotifyValueChanged<TValue>'),
  member('A_AddChild', 'public static T AddChild<T>(this T element, VisualElement child) where T : VisualElement'),
  member('A_Fixed', 'public static T Fixed<T>(this T element, int value) where T : INotifyValueChanged<int>'),
  member('A_Plain', 'public static void Plain(this VisualElement element)'),
  member('A_Marker', 'public static AutoScope Marker<T>(this T instance, int line = -1)'),
  member('A_NotExtension', 'public static T Create<T, TValue>(TValue value) where T : BaseSlider<TValue>'),
].join('');

test('a receiver constrained by a generic type with another type parameter is found', () => {
  const bases = collectReceiverBases(`## Methods\n\n${extensions}`);
  assert.deepEqual(Object.fromEntries(bases), {
    A_SetMaxLength: ['TextInputBaseField'],
    A_SetHighValue: ['BaseSlider'],
    A_SetValue: ['INotifyValueChanged'],
    A_Both: ['INotifyValueChanged'],
  });
});

const bases = new Map([
  ['A_SetMaxLength', ['TextInputBaseField']],
  ['A_SetHighValue', ['BaseSlider']],
  ['A_SetValue', ['INotifyValueChanged']],
]);
const entry = (name, anchor) => `[Ext.${name}\\<T, TValue\\>\\(T\\)](Ext.md#${anchor})`;
const page = (inheritance, implementation, entries) =>
  `# Class X\n\n#### Inheritance\n\n${inheritance}\n\n#### Implements\n\n${implementation}\n\n\n#### Extension Methods\n\n${entries}\n\n## Methods\n`;
const listed = (markdown) => [...markdown.matchAll(/^\[Ext\.(\w+)/gm)].map((m) => m[1]);

test('a type that is not a field loses the methods of fields and sliders', () => {
  const entries = [entry('SetMaxLength', 'A_SetMaxLength'), entry('SetValue', 'A_SetValue'), entry('Marker', 'A_Marker')].join(',\n');
  const result = dropForeignExtensions(page('[object](o.md) ← \n[X](X.md)', 'ISerializationCallbackReceiver', entries), bases);
  assert.deepEqual(listed(result), ['Marker']);
  assert.match(result, /\n#### Extension Methods\n\n\[Ext\.Marker[^\n]*\)\n\n## Methods/, 'the last entry has no trailing comma');
});
test('a field keeps the methods of its base types', () => {
  const entries = [entry('SetMaxLength', 'A_SetMaxLength'), entry('SetHighValue', 'A_SetHighValue'), entry('SetValue', 'A_SetValue')].join(',\n');
  const result = dropForeignExtensions(page('BindableElement ← \nBaseField<Type\\> ← \n[X](X.md)', 'IBindable,\nINotifyValueChanged<Type\\>', entries), bases);
  assert.deepEqual(listed(result), ['SetValue']);
});
test('only the generic type itself counts, not a type with the same ending or a nested one', () => {
  const entries = [entry('SetMaxLength', 'A_SetMaxLength'), entry('SetLabel', 'A_SetLabel')].join(',\n');
  const text = page('BaseField<Type\\>.UxmlSerializedData ← \n[X](X.md)', 'IBindable', entries);
  const result = dropForeignExtensions(text, new Map([...bases, ['A_SetLabel', ['BaseField']]]));
  assert.deepEqual(listed(result), []);
  const field = page('TextInputBaseField<string\\> ← \n[X](X.md)', 'IBindable', entries);
  assert.deepEqual(listed(dropForeignExtensions(field, new Map([...bases, ['A_SetLabel', ['BaseField']]]))), ['SetMaxLength']);
});
test('an ancestor written as a link counts', () => {
  const entries = [entry('SetHighValue', 'A_SetHighValue'), entry('SetValue', 'A_SetValue')].join(',\n');
  const text = page('[BaseSlider<float\\>](https://docs.unity3d.com/BaseSlider.html) ← \n[X](X.md)', '[INotifyValueChanged<float\\>](n.md),\nIBindable', entries);
  assert.deepEqual(listed(dropForeignExtensions(text, bases)), ['SetHighValue', 'SetValue']);
});
test('a list left empty stays a bare header', () => {
  const result = dropForeignExtensions(page('[X](X.md)', 'IFoo', entry('SetMaxLength', 'A_SetMaxLength')), bases);
  assert.match(result, /\n#### Extension Methods\n\n\n## Methods\n$/);
});
test('a page without an extension list is unchanged', () => {
  const source = '# Class X\n\n## Methods\n';
  assert.equal(dropForeignExtensions(source, bases), source);
});
