import assert from 'node:assert/strict';
import {test} from 'node:test';
import {collectReceiverBases, dropForeignExtensions, joinParagraphs} from './docfx-markdown.mjs';

test('a paragraph spread over lines becomes one line', () => {
  const source = 'Remarks\n\n<p>\nFirst line\nsecond [`x`](y.md) line.\n</p>\n<p>\nNext.\n</p>\n\n## Methods\n';
  assert.equal(joinParagraphs(source), 'Remarks\n\n<p>First line second [`x`](y.md) line.</p>\n<p>Next.</p>\n\n## Methods\n');
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
test('a list left empty stays a bare header', () => {
  const result = dropForeignExtensions(page('[X](X.md)', 'IFoo', entry('SetMaxLength', 'A_SetMaxLength')), bases);
  assert.match(result, /\n#### Extension Methods\n\n\n## Methods\n$/);
});
test('a page without an extension list is unchanged', () => {
  const source = '# Class X\n\n## Methods\n';
  assert.equal(dropForeignExtensions(source, bases), source);
});
