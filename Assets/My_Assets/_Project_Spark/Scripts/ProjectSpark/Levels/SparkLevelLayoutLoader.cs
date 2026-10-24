
using System;
using UnityEngine;
using ProjectSpark.Circuit;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevelLayoutLoader : MonoBehaviour
    {
        // =========================================================
        // CONFIGURATION
        // =========================================================

        [Header("Catalog")]
        [SerializeField]
        private SparkLevelLayoutCatalog catalog;

        [Header("Hierarchy")]
        [SerializeField]
        private Transform layoutRoot;

        [Header("Circuit Cleanup")]
        [SerializeField]
        private SparkCircuitSystem circuitSystem;

        [Header("Debug")]
        [SerializeField]
        private bool debugLogging;

        // =========================================================
        // RUNTIME STATE
        // =========================================================

        private SparkLevelLayout activeLayout;

        // =========================================================
        // PUBLIC STATE
        // =========================================================

        public bool HasActiveLayout =>
            activeLayout != null;

        public SparkLevelLayout ActiveLayout =>
            activeLayout;

        public SparkLevelSceneBindings ActiveBindings =>
            activeLayout != null
                ? activeLayout.SceneBindings
                : null;

        // =========================================================
        // EVENTS
        // =========================================================

        public event Action<SparkLevelLayout> LayoutLoaded;

        public event Action LayoutUnloaded;

        // =========================================================
        // VALIDATION
        // =========================================================

        public bool CanLoadLevel(
            SparkLevelDefinition level,
            out string error)
        {
            error = string.Empty;

            if (level == null)
            {
                error = "Level definition is null.";
                return false;
            }

            if (catalog == null)
            {
                error = "Layout catalog is missing.";
                return false;
            }

            if (circuitSystem == null)
            {
                error = "Circuit system is not assigned.";
                return false;
            }

            SparkLevelLayout prefab =
                catalog.GetPrefab(level);

            if (prefab == null)
            {
                error =
                    "No layout prefab registered for '" +
                    level.LevelId + "'.";

                return false;
            }

            if (prefab.LevelDefinition != level)
            {
                error =
                    "Layout prefab '" +
                    prefab.name +
                    "' has a different level definition.";

                return false;
            }

            return true;
        }

        // =========================================================
        // LOAD LEVEL
        // =========================================================

        public bool LoadLevel(
            SparkLevelDefinition level)
        {
            if (!CanLoadLevel(level, out string error))
            {
                Debug.LogError(
                    "[SparkLevelLayoutLoader] " + error,
                    this);

                return false;
            }

            SparkLevelLayout prefab =
                catalog.GetPrefab(level);

            Transform parent =
                layoutRoot != null
                    ? layoutRoot
                    : transform;

            SparkLevelLayout candidate = null;

            // Instantiate an inactive candidate.
            // This prevents its OnEnable() callbacks
            // from running before the old layout is removed.
            //
            // Note: Unity still invokes Awake() during
            // instantiation of an active prefab.
            bool prefabWasActive =
                prefab.gameObject.activeSelf;

            try
            {
                // Temporarily deactivate the source prefab
                // so the instantiated copy starts inactive.
                //
                // Do not modify the source prefab asset.
                // Use an inactive staging parent instead
                // if the prefab is stored as an active asset.

                GameObject stagingRoot =
                    new GameObject(
                        "SparkLayout_Staging");

                stagingRoot.transform.SetParent(
                    parent,
                    false);

                stagingRoot.SetActive(false);

                try
                {
                    candidate = Instantiate(
                        prefab,
                        stagingRoot.transform);

                    candidate.gameObject.SetActive(false);

                    if (!candidate.Validate(
                            out string validationError))
                    {
                        Debug.LogError(
                            "[SparkLevelLayoutLoader] " +
                            "Invalid layout: " +
                            validationError,
                            this);

                        DestroyLayoutSafely(candidate);

                        return false;
                    }

                    SparkLevelLayout previous =
                        activeLayout;

                    // Remove the previous layout while
                    // its terminals are still accessible.
                    if (previous != null)
                    {
                        DestroyLayoutSafely(previous);
                    }

                    // Move the validated candidate into
                    // the permanent layout hierarchy.
                    candidate.transform.SetParent(
                        parent,
                        false);

                    activeLayout = candidate;

                    // Activate the replacement only after
                    // old connections have been removed.
                    candidate.gameObject.SetActive(
                        prefabWasActive);

                    LayoutLoaded?.Invoke(
                        activeLayout);

                    if (debugLogging)
                    {
                        Debug.Log(
                            "[SparkLevelLayoutLoader] " +
                            "Loaded " +
                            level.LevelId,
                            this);
                    }

                    return true;
                }
                finally
                {
                    if (Application.isPlaying)
                    {
                        Destroy(stagingRoot);
                    }
                    else
                    {
                        DestroyImmediate(stagingRoot);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception,
                    this);

                // Do not destroy a successfully committed
                // active layout.
                if (candidate != null &&
                    candidate != activeLayout)
                {
                    DestroyLayoutSafely(candidate);
                }

                return false;
            }
        }

        // =========================================================
        // UNLOAD LEVEL
        // =========================================================

        public void UnloadLevel()
        {
            if (activeLayout == null)
            {
                return;
            }

            SparkLevelLayout previous =
                activeLayout;

            activeLayout = null;

            DestroyLayoutSafely(previous);

            LayoutUnloaded?.Invoke();

            if (debugLogging)
            {
                Debug.Log(
                    "[SparkLevelLayoutLoader] " +
                    "Active layout unloaded.",
                    this);
            }
        }

        // =========================================================
        // SAFE LAYOUT DESTRUCTION
        // =========================================================

        private void DestroyLayoutSafely(
            SparkLevelLayout layout)
        {
            if (layout == null)
            {
                return;
            }

            // Remove registered connections before
            // destroying their terminal components.
            if (circuitSystem != null)
            {
                circuitSystem.RemoveConnectionsForLayout(
                    layout.transform);
            }

            layout.gameObject.SetActive(false);

#if UNITY_EDITOR
            ClearSelectionBeforeDestroy(
                layout.gameObject);
#endif

            if (Application.isPlaying)
            {
                Destroy(layout.gameObject);
            }
            else
            {
                DestroyImmediate(layout.gameObject);
            }
        }

        // =========================================================
        // EDITOR SELECTION SAFETY
        // =========================================================

#if UNITY_EDITOR
        private static void ClearSelectionBeforeDestroy(
            GameObject root)
        {
            if (root == null)
            {
                return;
            }

            UnityEngine.Object[] selectedObjects =
                UnityEditor.Selection.objects;

            if (selectedObjects == null ||
                selectedObjects.Length == 0)
            {
                return;
            }

            foreach (UnityEngine.Object selected
                in selectedObjects)
            {
                GameObject selectedGameObject = null;

                if (selected is GameObject gameObject)
                {
                    selectedGameObject = gameObject;
                }
                else if (selected is Component component)
                {
                    selectedGameObject =
                        component.gameObject;
                }

                if (selectedGameObject == null)
                {
                    continue;
                }

                Transform selectedTransform =
                    selectedGameObject.transform;

                if (selectedTransform == root.transform ||
                    selectedTransform.IsChildOf(
                        root.transform))
                {
                    UnityEditor.Selection.activeGameObject =
                        root.transform.parent != null
                            ? root.transform.parent.gameObject
                            : null;

                    return;
                }
            }
        }
#endif

        // =========================================================
        // UNITY LIFECYCLE
        // =========================================================

        private void OnDestroy()
        {
            UnloadLevel();
        }
    }
}
