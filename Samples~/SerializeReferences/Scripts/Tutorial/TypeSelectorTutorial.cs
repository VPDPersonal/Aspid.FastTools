using System;
using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    //  [TypeSelector] for [SerializeReference] — a guided, step-by-step tour.
    //
    //  Read this component top to bottom: each [Header("STEP N …")] is one self-contained lesson,
    //  ordered from the simplest picker to generics, narrowing and required validation. Open the
    //  bundled Scenes/TypeSelectorTutorial.unity (a few steps are pre-filled) and follow along, or
    //  drop this component on an empty GameObject and start from a clean slate.
    //
    //  The companion TUTORIAL.md / TUTORIAL_RU.md walk through the same steps in prose and add the
    //  power-user gestures (copy/paste, templates, drag-drop, repair) and window-level tools that
    //  do not show up as plain fields.
    //
    //  The rule for every step: put [SerializeReference] AND [TypeSelector] on the same field. The
    //  first makes Unity store a polymorphic instance; the second renders the searchable type picker.
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    public sealed class TypeSelectorTutorial : MonoBehaviour
    {
        // STEP 1 — Your first picker.
        // A single interface-typed field. Click the dropdown to pick any IWeapon implementation
        // (Sword, Pistol, Shotgun, Railgun). Type to search; ↑↓ navigate; Space stars a favourite;
        // the first row, <None>, clears the field. The chosen instance's own fields appear inline.
        [Header("STEP 1 — Single polymorphic reference")]
        [SerializeReference] [TypeSelector]
        [Tooltip("Click the dropdown → pick an IWeapon. Type to search, Space favourites, <None> clears.")]
        private IWeapon _step1Single;

        // STEP 2 — Lists and arrays.
        // Every element gets its own independent picker. The list's "+" is replaced: instead of
        // duplicating the last element it opens the picker so you choose the new element's type.
        [Header("STEP 2 — List / array of references")]
        [SerializeReference] [TypeSelector]
        [Tooltip("Press + — the picker opens (it does not duplicate). Each element picks its own type.")]
        private List<IWeapon> _step2List = new();

        // STEP 3 — Abstract bases and interfaces.
        // StatusEffect is abstract, so the picker offers only its concrete subclasses
        // (BurnEffect, FreezeEffect). You can never pick the abstract base — it cannot be instantiated.
        [Header("STEP 3 — Abstract base → only concrete subclasses")]
        [SerializeReference] [TypeSelector]
        [Tooltip("Only BurnEffect / FreezeEffect are listed; the abstract StatusEffect is hidden.")]
        private StatusEffect _step3Abstract;

        // STEP 4 — Narrowing the candidate list.
        // All three fields are declared IWeapon, yet the picker shows different sets: the base type(s)
        // passed to [TypeSelector(...)] act as an extra filter BELOW the declared type. You can narrow
        // to one branch, or pass several base types to union them.
        [Header("STEP 4 — Narrow candidates with [TypeSelector(typeof(...))]")]
        [SerializeReference] [TypeSelector(typeof(IRanged))]
        [Tooltip("typeof(IRanged) → only Pistol, Shotgun, Railgun.")]
        private IWeapon _step4Ranged;

        [SerializeReference] [TypeSelector(typeof(IMelee))]
        [Tooltip("typeof(IMelee) → only Sword.")]
        private IWeapon _step4Melee;

        [SerializeReference] [TypeSelector(typeof(IMelee), typeof(IRanged))]
        [Tooltip("Multiple base types are OR-ed → the whole IWeapon hierarchy again.")]
        private IWeapon _step4MeleeOrRanged;

        // STEP 5 — Nested references (recursion).
        // Pick Railgun here, then expand it: its Charge Effect is itself a [SerializeReference]
        // [TypeSelector] field, so it gets its own picker. The drawer nests to any depth.
        [Header("STEP 5 — Nested reference inside the payload")]
        [SerializeReference] [TypeSelector]
        [Tooltip("Pick Railgun, expand it — its Charge Effect is a picker too (recursive).")]
        private IWeapon _step5Nested;

        // STEP 6 — Generics.
        // Modifier<T> is a concrete OPEN generic. On a non-generic IModifier field the picker offers the
        // closed subclasses (DamageModifier/AmmoModifier/NameModifier) AND "Modifier<T>" itself — picking
        // the latter opens a second page to choose T before the instance is built.
        [Header("STEP 6 — Generic hierarchy (open + closed)")]
        [SerializeReference] [TypeSelector]
        [Tooltip("Offers the 3 subclasses AND open Modifier<T>; picking Modifier<T> asks for T (try string, then float).")]
        private IModifier _step6Open;

        // A closed-generic field type fixes T, so the picker constrains candidates by assignability and
        // builds Modifier<float> directly — no second page. AmmoModifier (int) / NameModifier (string)
        // are excluded; DamageModifier (float) and Modifier<float> remain.
        [SerializeReference] [TypeSelector]
        [Tooltip("T is fixed to float: only DamageModifier and Modifier<float> are offered, no extra page.")]
        private Modifier<float> _step6Closed;

        // STEP 7 — References nested in plain [Serializable] containers.
        // The picker, inline child fields and every repair gesture work at any depth — not just when the
        // field sits directly on the component. Here a polymorphic weapon lives inside a container, and
        // inside each element of a list of containers.
        [Header("STEP 7 — References inside [Serializable] containers")]
        [SerializeField]
        [Tooltip("The weapon lives one level deep, inside this container — still a full picker.")]
        private WeaponSlot _step7Slot = new();

        [SerializeField]
        [Tooltip("Each list element is a container whose weapon is its own picker.")]
        private List<WeaponSlot> _step7Slots = new();

        // STEP 8 — Required references.
        // [SerializeReferenceRequired] flags an unset reference with an inline notice (and feeds the
        // build/CI gate). Leave it empty to see the warning; pick any type to clear it.
        [Header("STEP 8 — Required reference validation")]
        [SerializeReference] [TypeSelector] [SerializeReferenceRequired]
        [Tooltip("Empty → 'Required reference is not set' notice. Pick any IWeapon to satisfy it.")]
        private IWeapon _step8Required;

        // A plain [Serializable] container — NOT a managed reference itself. The [SerializeReference]
        // weapon inside it is still a full hierarchical picker (used by STEP 7).
        [Serializable]
        public sealed class WeaponSlot
        {
            public string label = "Slot";

            [Min(0)] public int priority;

            [SerializeReference] [TypeSelector]
            private IWeapon _weapon;
        }

        // Right-click the component header → "Log Tutorial State" to print every configured step.
        [ContextMenu("Log Tutorial State")]
        private void LogTutorialState()
        {
            Debug.Log($"STEP 1 single: {Describe(_step1Single)}");

            for (var i = 0; i < _step2List.Count; i++)
                Debug.Log($"STEP 2 list[{i}]: {Describe(_step2List[i])}");

            Debug.Log($"STEP 3 abstract: {(_step3Abstract is null ? "none" : _step3Abstract.Describe())}");
            Debug.Log($"STEP 4 ranged: {Describe(_step4Ranged)} | melee: {Describe(_step4Melee)} | either: {Describe(_step4MeleeOrRanged)}");
            Debug.Log($"STEP 5 nested: {Describe(_step5Nested)}");
            Debug.Log($"STEP 6 open: {(_step6Open is null ? "none" : _step6Open.Describe())} | closed: {(_step6Closed is null ? "none" : _step6Closed.Describe())}");
            Debug.Log($"STEP 8 required: {Describe(_step8Required)}");
        }

        private static string Describe(IWeapon weapon) => weapon?.Describe() ?? "none";
    }
}
