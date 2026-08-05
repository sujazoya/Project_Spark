using UnityEngine;

namespace ProjectSpark.Core.Performance
{
    [CreateAssetMenu(
        menuName="Project Spark/Performance Settings")]
    public sealed class PerformanceSettings
        : ScriptableObject
    {
        public int TargetFPS = 120;

        public int MaxBackgroundJobs = 8;

        public float SimulationRate = 60f;

        public bool EnablePooling = true;
    }
}
