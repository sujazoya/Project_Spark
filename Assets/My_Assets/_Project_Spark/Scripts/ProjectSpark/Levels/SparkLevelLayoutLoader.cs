
using System;
using UnityEngine;
using ProjectSpark.Circuit;
using ProjectSpark.Gameplay.Shelf;

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
                    ResolveShelf();

                                    
                    SparkLevelLayout previous = activeLayout;

                    if (previous != null)
                    {
                        ResolveShelf();

                        if (objectShelf == null)
                        {
                            Debug.LogError(
                                "[LayoutLoader] Cannot replace level: " +
                                "SparkObjectShelf is missing.",
                                this);

                            DestroyLayoutSafely(candidate);
                            return false;
                        }

                        // Preserve purchased items before destroying
                        // workstation and storage slots in the old layout.
                        if (!objectShelf.TryPrepareLevelUnload(
                                previous.transform,
                                out string unloadReason))
                        {
                            Debug.LogWarning(
                                "[LayoutLoader] Level replacement blocked: " +
                                unloadReason,
                                this);

                            // This candidate was never activated or registered.
                            // Destroying it leaves the old level untouched.
                            DestroyLayoutSafely(candidate);
                            return false;
                        }

                        // Old electrical connections, shelf registrations
                        // and level-owned objects are cleaned up here.
                        DestroyLayoutSafely(previous);
                    }

                    // Move the validated candidate into
                    // the permanent layout hierarchy.
                    candidate.transform.SetParent(
                        parent,
                        false);

                  
                    activeLayout = candidate;

                    candidate.gameObject.SetActive(prefabWasActive);

                    if (!RegisterLayoutShelfContent(candidate))
                    {
                        Debug.LogError(
                            "[SparkLevelLayoutLoader] Shelf registration failed: " +
                            level.LevelId,
                            this);

                        activeLayout = null;
                        DestroyLayoutSafely(candidate);
                        return false;
                    }

                    LayoutLoaded?.Invoke(candidate);

                    if (debugLogging)
                    {
                        Debug.Log(
                            "[SparkLevelLayoutLoader] Loaded " +
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
        return;

    ResolveShelf();

    if (objectShelf == null)
    {
        Debug.LogError(
            "[LayoutLoader] Cannot unload: " +
            "SparkObjectShelf is missing.",
            this);
        return;
    }

    SparkLevelLayout previous = activeLayout;

    if (!objectShelf.TryPrepareLevelUnload(
            previous.transform,
            out string reason))
    {
        Debug.LogWarning(
            "[LayoutLoader] Unload blocked: " + reason,
            this);
        return;
    }

    activeLayout = null;

    DestroyLayoutSafely(previous);

    LayoutUnloaded?.Invoke();

    if (debugLogging)
    {
        Debug.Log(
            "[LayoutLoader] Active level unloaded safely.",
            this);
    }
}


        // =========================================================
        // SAFE LAYOUT DESTRUCTION
        // =========================================================

       
       

private void DestroyLayoutSafely(SparkLevelLayout layout)
{
    if (layout == null)
        return;

    // 1. Remove electrical connections belonging
    // to the layout before deleting terminals.
    if (circuitSystem != null)
    {
        circuitSystem.RemoveConnectionsForLayout(
            layout.transform);
    }

    // 2. Release level-owned items and slots.
    // This is safe to call for an unregistered candidate:
    // the shelf checks its tracked level ownership.
    UnregisterLayoutShelfContent(layout);

    // 3. Deactivate layout.
    layout.gameObject.SetActive(false);

#if UNITY_EDITOR
    ClearSelectionBeforeDestroy(layout.gameObject);
#endif

    // 4. Destroy GameObject.
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
[Header("Shelf Registration")]
[SerializeField]
private SparkObjectShelf objectShelf;

private void ResolveShelf()
{
    if (objectShelf == null)
        objectShelf = FindFirstObjectByType<SparkObjectShelf>();
}



private bool RegisterLayoutShelfContent(
    SparkLevelLayout layout)
{
    if (layout == null)
        return false;

    ResolveShelf();

    if (objectShelf == null)
    {
        Debug.LogError(
            "[LayoutLoader] SparkObjectShelf is missing.",
            this);

        return false;
    }

    Transform root = layout.transform;

    objectShelf.RegisterRuntimeSlots(root);
    objectShelf.RegisterLevelItems(root);

    if (debugLogging)
    {
        Debug.Log(
            "[LayoutLoader] Registered shelf content: " +
            layout.name,
            this);
    }

    return true;
}


private void UnregisterLayoutShelfContent(SparkLevelLayout layout)
{
    if (layout == null)
        return;

    ResolveShelf();

    if (objectShelf == null)
        return;

    Transform root = layout.transform;

    // Remove items first because their current slots
    // must still be available for releasing occupancy.
    objectShelf.UnregisterLevelItems(root);

    // Remove the level's runtime slots afterward.
    objectShelf.UnregisterRuntimeSlots(root);

    if (debugLogging)
    {
        Debug.Log(
            "[LayoutLoader] Unregistered shelf content: " +
            layout.name,
            this);
    }
}


        // =========================================================
        // UNITY LIFECYCLE
        // =========================================================

        private void OnDestroy()
        {
            if (activeLayout == null)
                return;

            SparkLevelLayout previous = activeLayout;
            activeLayout = null;

            // Application/scene teardown cannot always be blocked.
            // Clean up the level-owned registrations even if
            // purchased-item relocation is no longer possible.
            DestroyLayoutSafely(previous);
        }

    }
}
