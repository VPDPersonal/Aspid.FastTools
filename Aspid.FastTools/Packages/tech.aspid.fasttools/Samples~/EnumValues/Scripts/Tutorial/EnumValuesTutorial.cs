using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // Step-by-step tour of EnumValues<TValue> / EnumValues<TEnum, TValue>: each [Header("STEP N …")]
    // is one lesson. Open Scenes/EnumValuesTutorial.unity and follow TUTORIAL.md / TUTORIAL_RU.md.
    public sealed class EnumValuesTutorial : MonoBehaviour
    {
        [Header("STEP 1 — Your first mapping")]
        [SerializeField]
        [Tooltip("Pick the DamageType enum in the header, then add rows with + and set a value per key.")]
        private EnumValues<float> _step1Multipliers;

        [Header("STEP 2 — Populate missing members")]
        [SerializeField]
        [Tooltip("Right-click the field → Populate Missing Enum Members. New rows are seeded with Default Value.")]
        private EnumValues<Color> _step2Colors;

        [Header("STEP 3 — [Flags] keys and lookup rules")]
        [SerializeField]
        [Tooltip("Keys are flag combinations. Exact match wins first; then the first contained entry; then Default Value.")]
        private EnumValues<float> _step3SpeedByStatus;

        [Header("STEP 4 — Read values from code")]
        [SerializeField]
        [Tooltip("Lookup key for STEP 1/2. Feed it to GetValue via the context menu → Log Tutorial Lookups.")]
        private DamageType _step4DamageType = DamageType.Fire;

        [SerializeField]
        [Tooltip("Lookup key for STEP 3. Toggle flags to exercise the exact / contained / default rules.")]
        private StatusEffect _step4Effects = StatusEffect.Burning | StatusEffect.Slowed;

        [Header("STEP 5 — The typed variant")]
        [SerializeField]
        [Tooltip("EnumValues<DamageType, float>: the type picker is disabled — the enum is fixed by the field declaration.")]
        private EnumValues<DamageType, float> _step5TypedMultipliers;

        [ContextMenu("Log Tutorial Lookups")]
        private void LogTutorialLookups()
        {
            Debug.Log($"STEP 1 multiplier for {_step4DamageType}: {_step1Multipliers.GetValue(_step4DamageType)}");
            var color = _step2Colors.GetValue(_step4DamageType);
            var colorHex = ColorUtility.ToHtmlStringRGB(color);
            Debug.Log($"<color=#{colorHex}>STEP 2 color for {_step4DamageType}: {color}</color>");
            Debug.Log($"STEP 3 speed for [{_step4Effects}]: {_step3SpeedByStatus.GetValue(_step4Effects):F2}");

            foreach (var entry in _step3SpeedByStatus)
                Debug.Log($"STEP 4 iteration: {entry.Key} = {entry.Value}");

            Debug.Log($"STEP 5 typed multiplier for {_step4DamageType}: {_step5TypedMultipliers.GetValue(_step4DamageType)}");
        }
    }
}
