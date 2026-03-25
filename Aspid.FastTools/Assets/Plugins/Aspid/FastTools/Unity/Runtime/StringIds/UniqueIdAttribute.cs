#nullable enable
using System.Diagnostics;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    /// <summary>
    /// Ensures the IdDropdown field's value is unique among all assets of the declaring type.
    /// Apply to a struct field that contains an [IdDropdown] string field.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public sealed class UniqueIdAttribute : PropertyAttribute { }
}
