import assert from 'node:assert/strict';
import {test} from 'node:test';
import {unityScriptReferenceUrl} from './unity-script-reference.mjs';

const page = (uid) => unityScriptReferenceUrl(uid)?.replace('https://docs.unity3d.com/ScriptReference/', '');

test('types keep namespaces and nesting joined by dots, generic arity becomes _N', () => {
  assert.equal(page('UnityEngine.Object'), 'Object.html');
  assert.equal(page('UnityEngine.UIElements.VisualElement'), 'UIElements.VisualElement.html');
  assert.equal(page('UnityEngine.UIElements.BaseField%601'), 'UIElements.BaseField_1.html');
  assert.equal(page('Unity.Profiling.ProfilerMarker'), 'Unity.Profiling.ProfilerMarker.html');
});
test('properties, fields and constructors are joined by a dash', () => {
  assert.equal(page('UnityEditor.SerializedProperty.arraySize'), 'SerializedProperty-arraySize.html');
  assert.equal(page('UnityEngine.UIElements.IStyle.borderTopLeftRadius'), 'UIElements.IStyle-borderTopLeftRadius.html');
  assert.equal(page('UnityEngine.UIElements.TextInputBaseField%601.textSelection'), 'UIElements.TextInputBaseField_1-textSelection.html');
  assert.equal(page('UnityEngine.Vector3.#ctor(System.Single,System.Single,System.Single)'), 'Vector3-ctor.html');
});
test('methods and enum values are joined by a dot', () => {
  assert.equal(page('UnityEditor.Editor.CreateInspectorGUI'), 'Editor.CreateInspectorGUI.html');
  assert.equal(page('UnityEngine.UIElements.VisualElement.SetEnabled(System.Boolean)'), 'UIElements.VisualElement.SetEnabled.html');
  assert.equal(page('UnityEngine.GameObject.GetComponent%60%601'), 'GameObject.GetComponent.html');
  assert.equal(page('UnityEngine.FontStyle.Bold'), 'FontStyle.Bold.html');
});
test('the link goes to the reference of the given Unity version', () => {
  assert.equal(
    unityScriptReferenceUrl('UnityEngine.UIElements.BaseField%601.label', '6000.4'),
    'https://docs.unity3d.com/6000.4/Documentation/ScriptReference/UIElements.BaseField_1-label.html',
  );
});
test('other APIs are not linked', () => {
  assert.equal(unityScriptReferenceUrl('Unity.Mathematics.float3'), null);
  assert.equal(unityScriptReferenceUrl('System.Object'), null);
});
