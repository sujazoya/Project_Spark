using System;
using UnityEngine;

namespace ProjectSpark.Level113
{
    [Serializable]
    public struct SparkL113IllustrativeSnapshot
    {
        public double simulationSeconds;
        public float primaryInputVolts;
        public float reserveInputVolts;
        public float protectedOutputVolts;
        public float loadCurrentAmps;
        public float clockDisagreementPartsPerMillion;
        public int illustrativeEccSyndromes;
        public bool independentSafetyInhibitIllustrated;
        public string classification;
        public bool electricallyCertified;
    }

    /// <summary>
    /// Caller-sampled synthetic TRAINING example of redundant power and safety behavior.
    /// No sockets, no hardware, no SparkCircuitSystem or LevelGamePlayManager binding.
    /// Does NOT award objectives. Demo faults are not repairs or real circuit faults.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL113ResilienceSandbox : MonoBehaviour
    {
        [SerializeField, Range(2f, 20f)] private float illustrativeLoadAmps = 8f;
        [SerializeField, Range(.004f, .1f)] private float fictionalHoldUpFarads = .04f;
        [SerializeField] private bool demoPrimaryPathOpen;
        [SerializeField] private bool demoEccViaIntermittent;
        [SerializeField] private bool demoClockWatchdogDrift;
        [SerializeField] private bool demoW233Removed;
        [SerializeField] private int deterministicSeed = 11321;
        // Not a Unity Update() loop; sampling is explicit and bounded.
        public SparkL113IllustrativeSnapshot Sample(double simulationSeconds)
        {
            float t = (float)Math.Max(0.0, Math.Min(simulationSeconds, 3600.0));
            float switchWindow = .0018f; // educational assumption, not sourced hardware
            float output = 5.0f;
            if (demoPrimaryPathOpen)
            {
                // Model a finite handoff; a power rail never teleports to zero.
                float transient = Mathf.Min(switchWindow, t);
                float droop = illustrativeLoadAmps * transient / Mathf.Max(.004f, fictionalHoldUpFarads);
                output = 5.0f - droop;
                // Conservative illustrative reserve recovery (no electrical certification).
                if (t > switchWindow)
                    output += droop * (1f - Mathf.Exp(-(t - switchWindow) * 450f));
            }
            float clockPpm = demoClockWatchdogDrift ? 230f : 2f;
            int errors = demoEccViaIntermittent ? (int)(3f + 2f * Mathf.Abs(Mathf.Sin(t * 3.1f + deterministicSeed))) : 0;
            return new SparkL113IllustrativeSnapshot
            {
                simulationSeconds = t,
                primaryInputVolts = demoPrimaryPathOpen ? 0f : 12f,
                reserveInputVolts = 12f,
                protectedOutputVolts = Mathf.Clamp(output, 0f, 5.1f),
                loadCurrentAmps = illustrativeLoadAmps,
                clockDisagreementPartsPerMillion = clockPpm,
                illustrativeEccSyndromes = errors,
                independentSafetyInhibitIllustrated = demoW233Removed || demoClockWatchdogDrift,
                classification = "SYNTHETIC_TRAINING_ONLY_NOT_SOLVER_MEASUREMENT",
                electricallyCertified = false
            };
        }
    }
}
