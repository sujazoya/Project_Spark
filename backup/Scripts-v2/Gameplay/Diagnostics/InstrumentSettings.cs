using UnityEngine;

namespace ProjectSpark.Gameplay.Diagnostics
{
    [CreateAssetMenu(
        menuName="Project Spark/Diagnostics/Instrument Settings")]
    public sealed class InstrumentSettings
        : ScriptableObject
    {
        public bool AutoRange = true;

        public bool Hold;

        public bool RelativeMode;

        public float RefreshRate = 10f;
    }
}
