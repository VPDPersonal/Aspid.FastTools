/**
 * Links a DocFX UID of a Unity API to its page in the Unity Scripting Reference.
 *
 * Unity names a page after the type, relative to `UnityEngine`/`UnityEditor`, with namespaces, nested types, methods
 * and enum values joined by `.` and properties, fields and events by `-`; a generic type's arity is `_N`:
 * `UnityEngine.UIElements.TextInputBaseField%601.textSelection` → `UIElements.TextInputBaseField_1-textSelection`,
 * `UnityEditor.Editor.CreateInspectorGUI` → `Editor.CreateInspectorGUI`, `UnityEngine.FontStyle.Bold` → `FontStyle.Bold`.
 * Members are told apart by case, as Unity names them: properties, fields, events and constructors (`-ctor`)
 * start in lower case.
 *
 * The link goes to the reference of `unityVersion` (`6000.4`), the Editor whose assemblies DocFX read: Unity moves members
 * between versions (`BaseField<T>.label` went to `AbstractBaseField` in 6000.6), so the unversioned latest reference loses
 * pages. Without a version the link goes to the latest reference.
 *
 * Returns `null` for UIDs outside the Scripting Reference.
 */
export function unityScriptReferenceUrl(uid, unityVersion) {
  if (!/^(UnityEngine|UnityEditor|Unity\.Profiling)\./.test(uid)) return null;

  const segments = uid
    .replace(/\(.*$/, '')
    .replace(/(?:%60){2}\d+/g, '') // a generic method's arity is not part of its page
    .replace(/%60(\d+)/g, '_$1')
    .replace(/\.#ctor$/, '.ctor')
    .split('.');
  // `Unity.*` namespaces keep their full name, `UnityEngine` and `UnityEditor` are dropped.
  if (segments[0] !== 'Unity') segments.shift();

  const page = segments.map((segment, i) => (i === 0 ? segment : `${/^[a-z]/.test(segment) ? '-' : '.'}${segment}`)).join('');
  return `https://docs.unity3d.com/${unityVersion ? `${unityVersion}/Documentation/` : ''}ScriptReference/${page}.html`;
}
