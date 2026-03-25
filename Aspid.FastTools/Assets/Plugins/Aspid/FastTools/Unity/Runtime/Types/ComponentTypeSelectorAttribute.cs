using System;
using UnityEngine;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    [Conditional(conditionString: "UNITY_EDITOR")]
    [AttributeUsage(validOn: AttributeTargets.Field)]
    public sealed class ComponentTypeSelectorAttribute : PropertyAttribute { }
}