using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A nested reference the package draws itself skips Unity's decorator drawers, so both drawers re-emit its
    // [Tooltip], [Space] and [Header] from here. Read once per field: the IMGUI drawer asks on every event.
    internal sealed class SerializeReferenceDecorators
    {
        private static readonly Dictionary<FieldInfo, SerializeReferenceDecorators> _cache = new();

        private static readonly SerializeReferenceDecorators _none = new(
            tooltip: string.Empty,
            items: Array.Empty<PropertyAttribute>());

        // Empty when the field has no [Tooltip].
        internal string Tooltip { get; }

        // [Space] and [Header] only, in the order Unity draws them.
        internal IReadOnlyList<PropertyAttribute> Items { get; }

        private SerializeReferenceDecorators(string tooltip, IReadOnlyList<PropertyAttribute> items)
        {
            Tooltip = tooltip;
            Items = items;
        }

        internal static SerializeReferenceDecorators For(SerializedProperty child)
        {
            var field = child.GetFieldInfo();
            if (field is null) return _none;

            if (_cache.TryGetValue(field, out var decorators)) return decorators;

            decorators = Read(field);
            _cache[field] = decorators;
            return decorators;
        }

        // [Header] and [Space] allow several per field, so a single-attribute lookup throws AmbiguousMatchException.
        private static SerializeReferenceDecorators Read(FieldInfo field)
        {
            var tooltip = string.Empty;
            var items = new List<PropertyAttribute>();

            foreach (var attribute in OrderedAttributes(field))
            {
                switch (attribute)
                {
                    case TooltipAttribute tooltipAttribute:
                        tooltip = tooltipAttribute.tooltip ?? string.Empty;
                        break;
                    case HeaderAttribute or SpaceAttribute:
                        items.Add(attribute);
                        break;
                }
            }

            return tooltip.Length == 0 && items.Count == 0
                ? _none
                : new SerializeReferenceDecorators(tooltip: tooltip, items: items.ToArray());
        }

        // Unity orders them the same way: by ascending `order`, equal orders as the runtime lists them.
        private static IEnumerable<PropertyAttribute> OrderedAttributes(FieldInfo field) => field
            .GetCustomAttributes<PropertyAttribute>(inherit: true)
            .OrderBy(attribute => attribute.order);
    }
}
