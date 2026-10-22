
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkLevelLayout : MonoBehaviour
    {
        [Header("Level Identity")]
        [SerializeField]
        private SparkLevelDefinition levelDefinition;

        [Header("Scene Bindings")]
        [SerializeField]
        private SparkLevelSceneBindings sceneBindings;

        public SparkLevelDefinition LevelDefinition =>
            levelDefinition;

        public SparkLevelSceneBindings SceneBindings =>
            sceneBindings;

        private void Awake()
        {
            if (sceneBindings == null)
            {
                sceneBindings =
                    GetComponentInChildren<
                        SparkLevelSceneBindings>(true);
            }
        }

        public bool Validate(out string error)
        {
            error = string.Empty;

            if (levelDefinition == null)
            {
                error = "Level definition is missing.";
                return false;
            }

            if (sceneBindings == null)
            {
                error = "Scene bindings are missing.";
                return false;
            }

            return sceneBindings.Validate(out error);
        }
    }
}
