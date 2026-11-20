using System;
using UnityEngine;

namespace ProjectSpark.Level111
{
    [Serializable]
    public struct SparkL111LaneSnapshot
    {
        public int lane;
        public float differentialImpedanceOhms;
        public float eyeHeightMillivolts;
        public float jitterPicoseconds;
        public float skewPicoseconds;
        public float returnLossDecibels;
        public float estimatedBitErrorRate;
        public bool tutorialPassing;
    }

    [Serializable]
    public struct SparkL111TutorialSnapshot
    {
        public double simulationSeconds;
        public bool sourceApproved;
        public string classification;
        public SparkL111LaneSnapshot[] lanes;
    }

    /// <summary>
    /// A BOUNDED EDUCATIONAL signal-integrity demo, independent from Project Spark's electrical graph.
    /// It never creates a real SparkTerminal, certifies a route or awards a LevelGamePlayManager objective.
    /// Does not run Update; caller explicitly asks for a deterministic sample.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL111SignalIntegritySandbox : MonoBehaviour
    {
        [SerializeField, Range(2f, 16f)] private float educationalGigabitsPerSecond = 10f;
        [SerializeField, Range(75f, 125f)] private float nominalDifferentialOhms = 100f;
        [SerializeField, Range(0f, 1f)] private float channelLoss = 0.24f;
        [SerializeField, Range(0f, 1f)] private float equalization = 0.47f;
        [SerializeField] private int deterministicSeed = 11119;
        [SerializeField, Tooltip("Demo input only, not an electrical fault authority.")]
        private bool tutorialLane2ViaOpen;
        [SerializeField] private bool tutorialClockJitterFault;
        [SerializeField] private bool tutorialRetimerThermalFault;

        private const int LaneCount = 4;
        private readonly SparkL111LaneSnapshot[] reusableLanes = new SparkL111LaneSnapshot[LaneCount];

        public SparkL111TutorialSnapshot Sample(double simulatedSeconds)
        {
            double boundedTime = Math.Max(0.0, Math.Min(3600.0, simulatedSeconds));
            float unitIntervalPs = 1000f / Mathf.Max(1f, educationalGigabitsPerSecond);
            float baseJitter = 2.5f + (tutorialClockJitterFault ? 20f : 0f);
            for (int lane = 0; lane < LaneCount; ++lane)
            {
                // Synthetic, stable jitter/loss estimates for visual education only.
                float phase = Mathf.Sin((float)boundedTime * 1.71f + lane * 0.47f + deterministicSeed * 0.0001f);
                float jitter = baseJitter + lane * 0.5f + Mathf.Abs(phase) * 1.3f;
                float viaLoss = (lane == 2 && tutorialLane2ViaOpen) ? 0.92f : 0f;
                float thermalLoss = tutorialRetimerThermalFault ? Mathf.Clamp01(((float)boundedTime - 14f) / 24f) * 0.42f : 0f;
                float attenuation = Mathf.Clamp01(channelLoss + viaLoss + thermalLoss - equalization * 0.34f);
                float eye = Mathf.Max(0f, 680f * (1f - attenuation) - (jitter / unitIntervalPs) * 220f);
                float skew = Mathf.Abs(lane - 1.5f) * 4f + (lane == 2 && tutorialLane2ViaOpen ? 66f : 0f);
                float impedance = nominalDifferentialOhms + (lane == 2 && tutorialLane2ViaOpen ? 26f : 0f) + phase * 1.2f;
                float retLoss = 24f - 17f * attenuation - (lane == 2 && tutorialLane2ViaOpen ? 14f : 0f);
                float relativeEye = eye / 680f;
                float berEstimate = Mathf.Pow(10f, -Mathf.Clamp(2f + 12f * relativeEye - jitter * 0.03f, 2f, 14f));
                reusableLanes[lane] = new SparkL111LaneSnapshot
                {
                    lane = lane,
                    differentialImpedanceOhms = impedance,
                    eyeHeightMillivolts = eye,
                    jitterPicoseconds = jitter,
                    skewPicoseconds = skew,
                    returnLossDecibels = retLoss,
                    estimatedBitErrorRate = berEstimate,
                    tutorialPassing = eye >= 310f && jitter < 18f && skew < 25f && impedance > 85f && impedance < 115f
                };
            }
            // Clone so consumers never receive a live mutable array; sampling is not per frame.
            SparkL111LaneSnapshot[] copy = new SparkL111LaneSnapshot[LaneCount];
            Array.Copy(reusableLanes, copy, LaneCount);
            return new SparkL111TutorialSnapshot
            {
                simulationSeconds = boundedTime,
                sourceApproved = false,
                classification = "ILLUSTRATIVE_ONLY_NONCERTIFIABLE",
                lanes = copy
            };
        }
    }
}
