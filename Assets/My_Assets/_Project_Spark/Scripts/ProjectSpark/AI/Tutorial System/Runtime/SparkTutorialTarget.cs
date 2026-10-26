using UnityEngine;

namespace ProjectSpark.AI.Tutorial
{
    [DisallowMultipleComponent]
    public sealed class SparkTutorialTarget : MonoBehaviour
    {
        [Header("Tutorial Identification")]
        public string key;

        [Header("Focus Targets")]
        public Transform worldAnchor;
        public RectTransform screenRect;

        public Transform WorldAnchor =>
            worldAnchor != null ? worldAnchor : transform;
    }
}