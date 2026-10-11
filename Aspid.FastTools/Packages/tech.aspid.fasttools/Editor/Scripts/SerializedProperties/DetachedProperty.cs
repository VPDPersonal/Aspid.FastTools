using System;
using UnityEditor;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    // Remembers where a property is and opens a copy of it, as Persistent() does, only while an action runs.
    // A menu or picker callback runs after the inspector's own property may be gone. A callback that never runs
    // (the menu or picker is cancelled) leaves no native snapshot behind.
    internal readonly struct DetachedProperty
    {
        private readonly Object _context;

        public string Path { get; }

        public Object[] Targets { get; }

        public DetachedProperty(SerializedProperty property)
        {
            var source = property.serializedObject;

            _context = source.context;
            Targets = source.targetObjects;
            Path = property.propertyPath;
        }

        // The copy is disposed when the action returns, so the action must not keep it. A target destroyed since
        // the callback was set up, or a path that no longer exists on the targets, skips the action.
        public void Use(Action<SerializedProperty> action)
        {
            foreach (var target in Targets)
            {
                if (target == null) return;
            }

            using var serializedObject = new SerializedObject(Targets, _context);

            var property = serializedObject.FindProperty(Path);
            if (property is not null) action(property);
        }
    }
}
