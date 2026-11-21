using System;
using UnityEngine;

namespace ProjectSpark.Level112
{
    [Serializable]
    public struct SparkL112EMCBandSample
    {
        public float frequencyMHz;
        public float conductedNoiseDbMicroVolts;
        public float radiatedNoiseDbMicroVoltsPerMetre;
        public float commonModeAttenuationDb;
        public float shieldBondMilliohms;
        public bool illustrativeUnderTrainingThreshold;
    }

    [Serializable]
    public struct SparkL112EMCTutorialSnapshot
    {
        public double simulatedSeconds;
        public string classification;
        public bool certified;
        public SparkL112EMCBandSample[] bands;
    }

    /// <summary>
    /// Non-certifiable deterministic teaching waveform. Does NOT access SparkCircuitSystem,
    /// does NOT make electrical contacts and can NEVER award a gameplay objective.
    /// Sample is caller-driven; there is no Update loop.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL112EMCSpectrumSandbox : MonoBehaviour
    {
        [SerializeField, Range(0.0f, 1.0f)] private float illustrativeFilterStrength = 0.76f;
        [SerializeField, Range(0.0f, 1.0f)] private float illustrativeShieldQuality = 0.86f;
        [SerializeField] private int deterministicSeed = 11220;
        [SerializeField, Tooltip("ILLUSTRATIVE ONLY, not an electrical fault authority.")]
        private bool tutorialShieldSeamOpen;
        [SerializeField] private bool tutorialCommonModeChokeSaturation;
        [SerializeField] private bool tutorialReturnViaOpen;

        private static readonly float[] TrainingFrequenciesMHz = { 0.15f, 1f, 10f, 30f, 100f, 300f };

        public SparkL112EMCTutorialSnapshot Sample(double seconds)
        {
            double t = Math.Max(0.0, Math.Min(3600.0, seconds));
            var samples = new SparkL112EMCBandSample[TrainingFrequenciesMHz.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                float f = TrainingFrequenciesMHz[i];
                float phase = Mathf.Sin((float)t * 0.17f + i * 1.83f + deterministicSeed * 0.0001f);
                float filterPenalty = tutorialCommonModeChokeSaturation ? 20f : 0f;
                float viaPenalty = tutorialReturnViaOpen ? 11f + i * 1.2f : 0f;
                float seamPenalty = tutorialShieldSeamOpen ? 23f : 0f;
                float conducted = 62f + 4f * Mathf.Log10(f + 1f) - 22f * illustrativeFilterStrength + filterPenalty + viaPenalty + phase;
                float radiated = 48f + 5f * Mathf.Log10(f + 1f) - 18f * illustrativeShieldQuality + seamPenalty + viaPenalty + phase;
                samples[i] = new SparkL112EMCBandSample
                {
                    frequencyMHz = f,
                    conductedNoiseDbMicroVolts = conducted,
                    radiatedNoiseDbMicroVoltsPerMetre = radiated,
                    commonModeAttenuationDb = 22f * illustrativeFilterStrength - filterPenalty,
                    shieldBondMilliohms = tutorialShieldSeamOpen ? 230f : 15f,
                    illustrativeUnderTrainingThreshold = conducted < 58f && radiated < 46f
                };
            }
            return new SparkL112EMCTutorialSnapshot
            {
                simulatedSeconds = t,
                classification = "SYNTHETIC_DEMO_ONLY_NOT_A_MEASUREMENT",
                certified = false,
                bands = samples
            };
        }
    }
}
