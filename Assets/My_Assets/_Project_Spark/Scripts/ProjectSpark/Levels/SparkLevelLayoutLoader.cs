
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevelLayoutLoader : MonoBehaviour
    {
        [Header("Catalog")]
        [SerializeField]
        private SparkLevelLayoutCatalog catalog;

        [Header("Layout Parent")]
        [SerializeField]
        private Transform layoutRoot;

        private SparkLevelLayout activeLayout;

        public SparkLevelLayout ActiveLayout =>
            activeLayout;

        public SparkLevelSceneBindings ActiveBindings =>
            activeLayout != null
                ? activeLayout.SceneBindings
                : null;

        public bool LoadLevel(
            SparkLevelDefinition level)
        {
            if (level == null || catalog == null)
                return false;

            SparkLevelLayout prefab =
                catalog.GetPrefab(level);

            if (prefab == null)
            {
                Debug.LogError(
                    $"[SparkLevelLayoutLoader] " +
                    $"No layout prefab for '{level.LevelId}'.",
                    this);

                return false;
            }

            UnloadLevel();

            Transform parent =
                layoutRoot != null
                    ? layoutRoot
                    : transform;

            activeLayout =
                Instantiate(prefab, parent);

            if (!activeLayout.Validate(out string error))
            {
                Debug.LogError(
                    $"[SparkLevelLayoutLoader] " +
                    $"Invalid layout: {error}",
                    this);

                UnloadLevel();
                return false;
            }

            return true;
        }

        public void UnloadLevel()
        {
            if (activeLayout == null)
                return;

            Destroy(activeLayout.gameObject);

            activeLayout = null;
        }
    }
}
