using UnityEngine;
using Aspid.FastTools.Ids;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Ids
{
    // Step-by-step tour of IId / IdRegistry / [UniqueId]: each [Header("STEP N …")] is one lesson.
    // Open Scenes/IdsTutorial.unity and follow TUTORIAL.md / TUTORIAL_RU.md.
    public sealed class IdsTutorial : MonoBehaviour
    {
        [Header("STEP 1 — Pick an id in the Inspector")]
        [SerializeField]
        [Tooltip("An EnemyId field renders as a name dropdown sourced from IdRegistry_EnemyId, but serializes a stable int.")]
        private EnemyId _step1EnemyId;

        [Header("STEP 2 — The registry behind the dropdown")]
        [SerializeField]
        [Tooltip("The IdRegistry asset that binds EnemyId to its (Id, Name) rows. Open it and add a row — the STEP 1 dropdown updates.")]
        private IdRegistry _step2Registry;

        [Header("STEP 3 — [UniqueId] collision guard")]
        [SerializeField]
        [Tooltip("An EnemyDefinition asset whose _id field carries [UniqueId]. Duplicate the asset to see the collision warning.")]
        private EnemyDefinition _step3Definition;

        [Header("STEP 4 — Runtime lookups")]
        [SerializeField]
        [Tooltip("A name to resolve to an int via TryGetId in the context menu → Log Tutorial Lookups.")]
        private string _step4NameToResolve = "walk_enemy_goblin";

        [SerializeField]
        [Tooltip("The catalog the STEP 1 id is resolved against — the same pattern EnemySpawner uses in the demo scene.")]
        private EnemyDefinition[] _step4Catalog;

        [ContextMenu("Log Tutorial Lookups")]
        private void LogTutorialLookups()
        {
            // STEP 1 + 2 — int → name: the field stores an int; the registry turns it back into a name.
            Debug.Log(_step2Registry.TryGetName(_step1EnemyId.Id, out var name)
                ? $"STEP 1 picked id {_step1EnemyId.Id} → \"{name}\""
                : $"STEP 1 picked id {_step1EnemyId.Id} is not in the registry");

            // STEP 4 — name → int, membership checks and iteration.
            Debug.Log(_step2Registry.TryGetId(_step4NameToResolve, out var id)
                ? $"STEP 4 TryGetId(\"{_step4NameToResolve}\") → {id}"
                : $"STEP 4 TryGetId(\"{_step4NameToResolve}\") → no such name");

            Debug.Log($"STEP 4 Contains({_step1EnemyId.Id}): {_step2Registry.Contains(_step1EnemyId.Id)}; " +
                      $"Contains(999): {_step2Registry.Contains(999)}");

            foreach (var entry in _step2Registry)
                Debug.Log($"STEP 4 registry row: {entry.Key} = \"{entry.Value}\"");

            // STEP 4 — resolve the picked id against a catalog, like EnemySpawner does on Start().
            foreach (var enemy in _step4Catalog)
            {
                if (enemy.Id.Id != _step1EnemyId.Id) continue;

                Debug.Log($"STEP 4 catalog hit: {enemy.DisplayName} — HP: {enemy.MaxHealth}, Speed: {enemy.MoveSpeed}");
                return;
            }

            Debug.LogWarning($"STEP 4 no EnemyDefinition in the catalog for id {_step1EnemyId.Id}");
        }
    }
}
