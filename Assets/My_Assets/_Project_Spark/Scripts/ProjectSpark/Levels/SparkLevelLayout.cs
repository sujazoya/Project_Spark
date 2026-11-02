using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevelLayout : MonoBehaviour
    {
        [Header("Level Identity")]
        [SerializeField] private SparkLevelDefinition levelDefinition;

        [Header("Scene Bindings")]
        [Tooltip("Authoritative binding belonging to this layout. Used by the level loader/evaluator.")]
        [SerializeField] private SparkLevelSceneBindings sceneBindings;

        [Tooltip("Runtime-discovered additional bindings. Populated automatically on enable, visible in Play Mode; prefab asset is not changed.")]
        [SerializeField] private SparkLevelSceneBindings[] additionalSceneBindings =
            Array.Empty<SparkLevelSceneBindings>();

        [Header("Automatic Discovery")]
        [SerializeField] private bool fetchOnEnable = true;
        [SerializeField] private bool includeInactiveObjects = true;
        [Tooltip("Find scene-wide bindings (for example, the persistent LevelGamePlayManager binding), but not bindings belonging to other level layouts.")]
        [SerializeField] private bool includeSceneBindings = true;
        [SerializeField] private bool logDiscovery;

        private SparkLevelSceneBindings[] resolvedBindings =
            Array.Empty<SparkLevelSceneBindings>();

        // Stores author-assigned references without accidentally treating
        // a previous runtime scan as the new serialized authoring baseline.
        private SparkLevelSceneBindings[] configuredAdditionalBindings;

        public SparkLevelDefinition LevelDefinition => levelDefinition;

        // Compatibility with SparkLevelLayoutLoader.ActiveBindings.
        // Never substitute a persistent scene binding for the level's own binding.
        public SparkLevelSceneBindings SceneBindings
        {
            get
            {
                EnsureBindings();
                return GetPrimaryLayoutBinding();
            }
        }

        public IReadOnlyList<SparkLevelSceneBindings> SceneBindingsArray
        {
            get
            {
                EnsureBindings();
                return resolvedBindings;
            }
        }

        private void Awake()
        {
            CaptureConfiguredBindings();
            // Do not search the wider scene here: layout loader instantiates
            // layouts under an inactive staging parent, before committing them.
            if (!fetchOnEnable)
                RefreshSceneBindings();
        }

        private void OnEnable()
        {
            if (fetchOnEnable)
                RefreshSceneBindings();
        }

        private void CaptureConfiguredBindings()
        {
            if (configuredAdditionalBindings == null)
                configuredAdditionalBindings = additionalSceneBindings != null
                    ? (SparkLevelSceneBindings[])additionalSceneBindings.Clone()
                    : Array.Empty<SparkLevelSceneBindings>();
        }

        [ContextMenu("Refresh Scene Bindings")]
        public void RefreshSceneBindings()
        {
            CaptureConfiguredBindings();

            var collected = new List<SparkLevelSceneBindings>();
            var seen = new HashSet<SparkLevelSceneBindings>();

            // Layout-owned bindings always have priority.
            var owned = GetComponentsInChildren<SparkLevelSceneBindings>(includeInactiveObjects);
            foreach (var binding in owned)
                AddUnique(collected, seen, binding);

            // Retain an explicitly configured primary binding for diagnostics,
            // but only a binding physically in this layout can be authoritative.
            AddUnique(collected, seen, sceneBindings);

            foreach (var binding in configuredAdditionalBindings)
                AddUnique(collected, seen, binding);

            // Scan the actual instantiated scene, not prefab assets or other scenes.
            if (includeSceneBindings && gameObject.scene.IsValid())
            {
                var roots = gameObject.scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    if (root == null) continue;
                    var found = root.GetComponentsInChildren<SparkLevelSceneBindings>(includeInactiveObjects);
                    foreach (var binding in found)
                    {
                        if (binding == null) continue;
                        SparkLevelLayout owner = binding.GetComponentInParent<SparkLevelLayout>(true);
                        if (owner != null && owner != this)
                            continue; // Never import another level's bindings.
                        AddUnique(collected, seen, binding);
                    }
                }
            }

            SparkLevelSceneBindings primary = GetPrimaryFrom(collected);
            var ordered = new List<SparkLevelSceneBindings>();
            if (primary != null) ordered.Add(primary);
            foreach (var binding in collected)
                if (binding != null && binding != primary)
                    ordered.Add(binding);
            resolvedBindings = ordered.ToArray();

            // IMPORTANT: expose automatically found bindings in the actual
            // Inspector array at runtime, instead of only in a private cache.
            // This modifies the instantiated level object, not the prefab asset.
            var extras = new List<SparkLevelSceneBindings>();
            foreach (var binding in resolvedBindings)
                if (binding != primary) extras.Add(binding);
            additionalSceneBindings = extras.ToArray();

            if (logDiscovery)
            {
                Debug.Log($"[SparkLevelLayout] {name}: primary=" +
                          (primary != null ? primary.name : "NONE") +
                          $"; additional={additionalSceneBindings.Length}; total={resolvedBindings.Length}", this);
            }
        }

        private static void AddUnique(
            List<SparkLevelSceneBindings> destination,
            HashSet<SparkLevelSceneBindings> seen,
            SparkLevelSceneBindings binding)
        {
            if (binding != null && seen.Add(binding)) destination.Add(binding);
        }

        private void EnsureBindings()
        {
            if (resolvedBindings == null || resolvedBindings.Length == 0)
                RefreshSceneBindings();
        }

        private SparkLevelSceneBindings GetPrimaryLayoutBinding()
        {
            return GetPrimaryFrom(resolvedBindings);
        }

        private SparkLevelSceneBindings GetPrimaryFrom(
            IList<SparkLevelSceneBindings> candidates)
        {
            if (IsOwnedByThisLayout(sceneBindings)) return sceneBindings;
            if (candidates != null)
                foreach (var candidate in candidates)
                    if (IsOwnedByThisLayout(candidate)) return candidate;
            return null;
        }

        private bool IsOwnedByThisLayout(SparkLevelSceneBindings binding)
        {
            return binding != null &&
                   (binding.transform == transform || binding.transform.IsChildOf(transform));
        }

        public bool Validate(out string error)
        {
            error = string.Empty;
            if (levelDefinition == null)
            {
                error = "Level definition is missing.";
                return false;
            }

            SparkLevelSceneBindings primary = SceneBindings;
            if (primary == null)
            {
                error = "No SparkLevelSceneBindings belongs to this level layout. " +
                        "Scene-wide bindings cannot replace its primary bindings.";
                return false;
            }
            return primary.Validate(out error);
        }
    }
}
