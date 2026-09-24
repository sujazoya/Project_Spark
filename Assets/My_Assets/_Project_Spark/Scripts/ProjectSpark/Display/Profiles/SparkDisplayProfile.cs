using UnityEngine;

namespace ProjectSpark.Display
{
    [CreateAssetMenu(fileName = "DisplayProfile", menuName = "Project Spark/Display/Display Profile")]
    public sealed class SparkDisplayProfile : ScriptableObject
    {
        [Header("Channels")]
        public bool showPrimary = true;
        public bool showSecondary;
        public bool showTertiary;
        public bool showUnit = true;
        public bool showMode = true;
        public bool showStatus = true;
        public bool showMinMaxAverage;
        public bool showBar;
        public bool showGraph;
        public bool showNotification = true;

        [Header("Startup")]
        public bool playStartupSequence;
        public bool playShutdownSequence = true;

        [Header("Display Defaults")]
        [Range(0, 6)] public int defaultPrecision = 2;
        public bool useEngineeringPrefixes = true;

        [Header("Layout")]
        public string primaryLabel = string.Empty;
        public string secondaryLabel = string.Empty;
    }
}
