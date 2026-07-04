using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Same fields as DamageDealer, but the companion editor (IMGUIDamageDealerEditor) overrides
    // OnInspectorGUI without CreateInspectorGUI, forcing the entire inspector — and every
    // EnumValues<TValue>/EnumValues<TEnum,TValue> field — through the IMGUI drawer path instead
    // of the UIToolkit one.
    //
    // Use this to compare both rendering paths side by side in the EnumValues demo scene.
    public sealed class IMGUIDamageDealer : MonoBehaviour
    {
        [SerializeField] private EnumValues<float> _damageMultipliers;
        [SerializeField] private EnumValues<DamageType, Color> _damageColors;
        [SerializeField] private EnumValues<float> _speedMultipliersByStatus;
    }
}
