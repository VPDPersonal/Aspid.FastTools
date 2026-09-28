using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // In-memory Fixes whose replaced missing-type entry waits for the save. A script edit right after a Fix reloads
    // the domain before that save, so the list lives in an unsaved singleton that survives the reload.
    internal sealed class SerializeReferencePendingRepairs : ScriptableSingleton<SerializeReferencePendingRepairs>
    {
        [SerializeField] private List<Entry> _entries = new();

        public List<Entry> Entries => _entries;

        [Serializable]
        public sealed class Entry
        {
            public Object target;
            public long repairedId;
            public long referenceId;
            public int undoGroup;
            public bool undone;
        }
    }
}
