#nullable enable
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal sealed class IdRegistryAccessor : IRegistryAccessor
    {
        private readonly IdRegistry _registry;
        private readonly SerializedProperty _idsProp;
        private readonly SerializedProperty _namesProp;

        public Object Target => _registry;
        public SerializedObject SerializedObject { get; }
        public SerializedProperty TargetStructTypeProperty { get; }
        public SerializedProperty NextIdProperty { get; }

        public IdRegistryAccessor(IdRegistry registry)
        {
            _registry = registry;
            SerializedObject = new SerializedObject(registry);
            _idsProp = SerializedObject.FindProperty("_ids");
            _namesProp = SerializedObject.FindProperty("_names");
            TargetStructTypeProperty = SerializedObject.FindProperty("_targetStructType");
            NextIdProperty = SerializedObject.FindProperty("_nextId");
        }

        public int Count => Mathf.Min(_idsProp.arraySize, _namesProp.arraySize);

        public int GetId(int index) =>
            _idsProp.GetArrayElementAtIndex(index).intValue;

        public string GetName(int index) =>
            _namesProp.GetArrayElementAtIndex(index).stringValue;

        public int Add(string name)
        {
            var id = NextIdProperty.intValue;
            NextIdProperty.intValue = id + 1;

            var newIndex = _idsProp.arraySize;
            _idsProp.arraySize = newIndex + 1;
            _namesProp.arraySize = newIndex + 1;
            _idsProp.GetArrayElementAtIndex(newIndex).intValue = id;
            _namesProp.GetArrayElementAtIndex(newIndex).stringValue = name;
            return id;
        }

        public void SetName(int index, string name) =>
            _namesProp.GetArrayElementAtIndex(index).stringValue = name;

        public void RemoveAt(int index)
        {
            _idsProp.DeleteArrayElementAtIndex(index);
            if (index < _namesProp.arraySize)
                _namesProp.DeleteArrayElementAtIndex(index);
        }

        public bool HasStructuralDamage(out string reason)
        {
            if (_idsProp.arraySize == _namesProp.arraySize)
            {
                reason = string.Empty;
                return false;
            }
            reason = $"Length mismatch: _ids has {_idsProp.arraySize} entries, _names has {_namesProp.arraySize}.";
            return true;
        }
    }
}
