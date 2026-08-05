using UnityEngine;

namespace ProjectSpark.Core.Performance
{
    public sealed class PerformanceManager
        : MonoBehaviour
    {
        [SerializeField]
        private PerformanceSettings
            settings;

        private void Awake()
        {
            Application.targetFrameRate =
                settings.TargetFPS;
        }
    }
}
