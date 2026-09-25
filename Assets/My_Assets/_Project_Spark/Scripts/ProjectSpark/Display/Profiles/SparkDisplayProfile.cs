
using UnityEngine;

namespace ProjectSpark.Display
{
    [CreateAssetMenu(
        fileName = "DisplayProfile",
        menuName = "Project Spark/Display/Display Profile")]
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
        public bool showRange;
        public bool showBar;
        public bool showGraph;
        public bool showNotification = true;
    }
}
