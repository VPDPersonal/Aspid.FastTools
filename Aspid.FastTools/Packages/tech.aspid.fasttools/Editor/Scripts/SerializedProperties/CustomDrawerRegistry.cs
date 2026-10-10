using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.Rendering;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class CustomDrawerRegistry
    {
        // Unity exposes no public drawer registry; missing internal attribute fields disable this lookup.
        internal static readonly FieldInfo TargetField =
            typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static readonly FieldInfo UseForChildrenField =
            typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);

        // Pipelines is the drawer's [SupportedOnRenderPipeline] list; null means any pipeline.
        private static List<(Type Target, bool UseForChildren, Type[] Pipelines)> _registrations;

        private static readonly Dictionary<(Type, bool, Type), bool> Cache = new();

        private static List<(Type Target, bool UseForChildren, Type[] Pipelines)> Registrations => _registrations ??= Collect();

        // Mirrors Unity's lookup: the type and its base classes, then its interfaces, each also by generic definition.
        // An ancestor's drawer applies with useForChildren or, as Unity treats managed references, without it.
        internal static bool HasDrawerFor(Type type, bool isManagedReference = false) =>
            HasDrawerFor(type, isManagedReference, pipelineType: CurrentPipelineType());

        // pipelineType is the active RenderPipelineAsset type, null in the Built-in pipeline.
        internal static bool HasDrawerFor(Type type, bool isManagedReference, Type pipelineType)
        {
            if (type is null) return false;

            // Drawers only change with a domain reload, which also clears this cache. The render pipeline can change
            // without one, so it is part of the key.
            var key = (type, isManagedReference, pipelineType);
            if (Cache.TryGetValue(key, out var hasDrawer)) return hasDrawer;
            return Cache[key] = Lookup(type, isManagedReference, pipelineType: pipelineType);
        }

        private static bool Lookup(Type type, bool isManagedReference, Type pipelineType)
        {
            for (var current = type; current is not null; current = current.BaseType)
                if (Matches(current, requested: current == type, isManagedReference, pipelineType: pipelineType))
                    return true;

            // An interface itself was checked above, so every interface listed here is an ancestor.
            foreach (var @interface in type.GetInterfaces())
                if (Matches(@interface, requested: false, isManagedReference, pipelineType: pipelineType))
                    return true;

            return false;
        }

        // Unity looks up attribute drawers the same way, managed-reference flag included. It hands a collection only
        // the attributes that apply to it, and an element only the others.
        internal static bool DeclaresDrawnAttribute(
            FieldInfo field,
            bool isManagedReference = false,
            bool isArrayElement = false,
            bool isCollection = false)
        {
            if (field is null) return false;

            var pipelineType = CurrentPipelineType();
            foreach (var attribute in field.GetCustomAttributes<PropertyAttribute>(inherit: true))
            {
                if (attribute.applyToCollection ? isArrayElement : isCollection) continue;
                if (HasDrawerFor(attribute.GetType(), isManagedReference, pipelineType: pipelineType)) return true;
            }

            return false;
        }

        private static bool Matches(Type type, bool requested, bool isManagedReference, Type pipelineType)
        {
            var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

            foreach (var (target, useForChildren, pipelines) in Registrations)
            {
                if (target != type && target != definition) continue;
                if (!SupportsPipeline(pipelines, pipelineType: pipelineType)) continue;
                if (requested || useForChildren || isManagedReference) return true;
            }

            return false;
        }

        // Unity skips a drawer marked for other render pipelines, and every marked drawer in the Built-in pipeline.
        // A base RenderPipelineAsset type in the mark covers its derived types.
        private static bool SupportsPipeline(Type[] pipelines, Type pipelineType) =>
            pipelines is null || Array.Exists(pipelines, pipeline => pipeline.IsAssignableFrom(pipelineType));

        private static Type CurrentPipelineType()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            return pipeline ? pipeline.GetType() : null;
        }

        private static List<(Type Target, bool UseForChildren, Type[] Pipelines)> Collect()
        {
            var result = new List<(Type, bool, Type[])>();
            if (TargetField is null) return result;

            foreach (var drawer in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
            {
                if (!typeof(PropertyDrawer).IsAssignableFrom(drawer)) continue;

                var pipelines = drawer.GetCustomAttribute<SupportedOnRenderPipelineAttribute>(inherit: true)?.renderPipelineTypes;
                foreach (var registration in drawer.GetCustomAttributes<CustomPropertyDrawer>(inherit: true))
                {
                    if (TargetField.GetValue(registration) is not Type target) continue;
                    var useForChildren = UseForChildrenField?.GetValue(registration) is true;

                    result.Add((target, useForChildren, pipelines));
                }
            }

            return result;
        }
    }
}
