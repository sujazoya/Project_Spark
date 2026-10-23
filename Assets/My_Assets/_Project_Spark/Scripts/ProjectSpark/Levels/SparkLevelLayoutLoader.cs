
using System;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevelLayoutLoader : MonoBehaviour
    {
        [Header("Catalog")]
        [SerializeField]
        private SparkLevelLayoutCatalog catalog;

        [Header("Hierarchy")]
        [SerializeField]
        private Transform layoutRoot;

        [Header("Debug")]
        [SerializeField]
        private bool debugLogging;

        private SparkLevelLayout activeLayout;

        public SparkLevelLayout ActiveLayout => activeLayout;

        public SparkLevelSceneBindings ActiveBindings =>
            activeLayout != null
                ? activeLayout.SceneBindings
                : null;

        public event Action<SparkLevelLayout> LayoutLoaded;
        public event Action LayoutUnloaded;

        public bool LoadLevel(SparkLevelDefinition level)
        {
            if (level == null || catalog == null)
                return false;

            SparkLevelLayout prefab = catalog.GetPrefab(level);

            if (prefab == null)
            {
                Debug.LogError(
                    $"[SparkLevelLayoutLoader] Missing prefab for " +
                    $"'{level.LevelId}'.", this);
                return false;
            }

            Transform parent =
                layoutRoot != null ? layoutRoot : transform;

            SparkLevelLayout candidate =
                Instantiate(prefab, parent);          
          
            if (candidate.LevelDefinition != level)
            {
                string expectedName =
                    level != null
                        ? level.name
                        : "NULL";

                string actualName =
                    candidate.LevelDefinition != null
                        ? candidate.LevelDefinition.name
                        : "NULL";

                Debug.LogError(
                    "[SparkLevelLayoutLoader] Level definition mismatch.\n" +
                    $"Requested level: {expectedName}\n" +
                    $"Prefab level: {actualName}\n" +
                    $"Prefab: {prefab.name}\n" +
                    "Assign the SAME SparkLevelDefinition asset " +
                    "to the manager, catalog, and layout prefab.",
                    this);

                DestroyCandidate(candidate);
                return false;
            }


            if (!candidate.Validate(out string error))
            {
                Debug.LogError(
                    "[SparkLevelLayoutLoader] Invalid layout: " +
                    error,
                    this);

                DestroyCandidate(candidate);
                return false;
            }


            SparkLevelLayout previous = activeLayout;

            if (previous != null)
            {
                // Disable immediately, before the next solve.
                previous.gameObject.SetActive(false);
            }

            activeLayout = candidate;

            if (previous != null)
                DestroyCandidate(previous);

            LayoutLoaded?.Invoke(activeLayout);

            if (debugLogging)
            {
                Debug.Log(
                    $"[SparkLevelLayoutLoader] Loaded {level.LevelId}",
                    this);
            }

            return true;
        }
        
#if UNITY_EDITOR
private static void ClearSelectionBeforeDestroy(
    GameObject root)
{
    if (root == null)
        return;

    UnityEngine.Object[] selectedObjects =
        UnityEditor.Selection.objects;

    if (selectedObjects == null ||
        selectedObjects.Length == 0)
    {
        return;
    }

    foreach (UnityEngine.Object selected in selectedObjects)
    {
        GameObject selectedGameObject = null;

        if (selected is GameObject gameObject)
        {
            selectedGameObject = gameObject;
        }
        else if (selected is Component component)
        {
            selectedGameObject = component.gameObject;
        }

        if (selectedGameObject == null)
            continue;

        Transform selectedTransform =
            selectedGameObject.transform;

        if (selectedTransform == root.transform ||
            selectedTransform.IsChildOf(root.transform))
        {
            // Select a persistent object instead of
            // the GameObject about to be destroyed.
            UnityEditor.Selection.activeGameObject =
                root.transform.parent != null
                    ? root.transform.parent.gameObject
                    : null;

            return;
        }
    }
}
#endif


        public void UnloadLevel()
        {
            if (activeLayout == null)
                return;

            SparkLevelLayout previous = activeLayout;
            activeLayout = null;

            previous.gameObject.SetActive(false);
            DestroyCandidate(previous);

            LayoutUnloaded?.Invoke();
        }

       
private static void DestroyCandidate(
    SparkLevelLayout layout)
{
    if (layout == null)
        return;

#if UNITY_EDITOR
    ClearSelectionBeforeDestroy(layout.gameObject);
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

    }
}
