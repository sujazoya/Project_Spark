using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public sealed class SparkLevel98Profile : ScriptableObject
    {
        [Tooltip("Effective impedances for an accelerated fictional training bench.")]
        public float idleOhms = 3.6f, peakOhms = .583f, feedOhms = .054f;
        public float goodEsr = .016f, wornEsr = .48f, bulkFarads = 1.15f, bypassFarads = .09f;
        public float returnBypassOhms = .18f, pulseDuration = .45f, restDuration = .45f;
        public float minimumLoadVolts = 1.60f, maximumLoadVolts = 1.93f, minimumPeakAmps = 2.73f;
        public float maximumIdleError = .09f, maximumHealthyGroundOffset = .055f, maximumKelvinError = .055f;
    }
}
