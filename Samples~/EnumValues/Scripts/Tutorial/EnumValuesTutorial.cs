using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    //  EnumValues<TValue> / EnumValues<TEnum, TValue> — a guided, step-by-step tour.
    //
    //  Read this component top to bottom: each [Header("STEP N …")] is one self-contained lesson,
    //  ordered from an empty field to [Flags] lookup rules, reading values from code and the typed
    //  variant. Open the bundled Scenes/EnumValuesTutorial.unity (later steps are pre-filled) and
    //  follow along, or drop this component on an empty GameObject and start from a clean slate.
    //
    //  The companion TUTORIAL.md / TUTORIAL_RU.md walk through the same steps in prose and add the
    //  details that do not show up as plain fields (Populate Missing Enum Members, key migration
    //  when the enum type changes, the [Flags] matching rules).
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    public sealed class EnumValuesTutorial : MonoBehaviour
    {
        // STEP 1 — Your first mapping.
        // The header's type field is a TypeSelector picker over every enum in the project. Pick
        // DamageType, then press "+" on the list: each row is a key dropdown plus a float value.
        // Duplicate keys are allowed but only the first one wins — keep keys unique.
        [Header("STEP 1 — Your first mapping")]
        [SerializeField]
        [Tooltip("Pick the DamageType enum in the header, then add rows with + and set a value per key.")]
        private EnumValues<float> _step1Multipliers;

        // STEP 2 — Populate Missing Enum Members + the default value.
        // The enum type is pre-set here. Right-click anywhere on the field and choose
        // "Populate Missing Enum Members": one row per missing member is appended, seeded with
        // Default Value. The action is greyed out once every member has a row.
        [Header("STEP 2 — Populate missing members")]
        [SerializeField]
        [Tooltip("Right-click the field → Populate Missing Enum Members. New rows are seeded with Default Value.")]
        private EnumValues<Color> _step2Colors;

        // STEP 3 — [Flags] enums and the lookup rules.
        // StatusEffect is a [Flags] enum, so each key renders as a multi-select flags dropdown and
        // a composite key like "Burning | Slowed" is a regular entry. GetValue resolves in order:
        // exact key match (any position) → first entry whose flags are all contained in the lookup
        // value → Default Value. None (zero) only ever matches a None entry.
        [Header("STEP 3 — [Flags] keys and lookup rules")]
        [SerializeField]
        [Tooltip("Keys are flag combinations. Exact match wins first; then the first contained entry; then Default Value.")]
        private EnumValues<float> _step3SpeedByStatus;

        // STEP 4 — Reading values from code.
        // GetValue takes any value of the configured enum. Set the two lookup fields below and use
        // the component context menu → "Log Tutorial Lookups" (works in Edit Mode, no Play needed).
        // foreach over an EnumValues yields the configured entries; the default value is not part
        // of the iteration.
        [Header("STEP 4 — Read values from code")]
        [SerializeField]
        [Tooltip("Lookup key for STEP 1/2. Feed it to GetValue via the context menu → Log Tutorial Lookups.")]
        private DamageType _step4DamageType = DamageType.Fire;

        [SerializeField]
        [Tooltip("Lookup key for STEP 3. Toggle flags to exercise the exact / contained / default rules.")]
        private StatusEffect _step4Effects = StatusEffect.Burning | StatusEffect.Slowed;

        // STEP 5 — The typed variant: EnumValues<TEnum, TValue>.
        // When the enum type is known at compile time, prefer EnumValues<DamageType, float>: the
        // Inspector shows no type-picker row (rows render typed key dropdowns immediately), and
        // GetValue takes a DamageType instead of a boxed Enum. Lookup rules — including [Flags] —
        // are identical, and the serialized layout matches EnumValues<TValue>, so switching a field
        // between the two variants (same enum) keeps the configured entries.
        [Header("STEP 5 — The typed variant")]
        [SerializeField]
        [Tooltip("EnumValues<DamageType, float>: no type picker — the enum is fixed by the field declaration.")]
        private EnumValues<DamageType, float> _step5TypedMultipliers;

        // Right-click the component header → "Log Tutorial Lookups" to run every lookup at once.
        [ContextMenu("Log Tutorial Lookups")]
        private void LogTutorialLookups()
        {
            Debug.Log($"STEP 1 multiplier for {_step4DamageType}: {_step1Multipliers.GetValue(_step4DamageType)}");
            Debug.Log($"STEP 2 color for {_step4DamageType}: {_step2Colors.GetValue(_step4DamageType)}");
            Debug.Log($"STEP 3 speed for [{_step4Effects}]: {_step3SpeedByStatus.GetValue(_step4Effects):F2}");

            foreach (var entry in _step3SpeedByStatus)
                Debug.Log($"STEP 4 iteration: {entry.Key} = {entry.Value}");

            Debug.Log($"STEP 5 typed multiplier for {_step4DamageType}: {_step5TypedMultipliers.GetValue(_step4DamageType)}");
        }
    }
}
