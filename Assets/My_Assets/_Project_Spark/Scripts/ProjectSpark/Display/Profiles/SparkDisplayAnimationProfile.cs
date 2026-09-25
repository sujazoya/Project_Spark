using UnityEngine;

namespace ProjectSpark.Display
{
    [CreateAssetMenu(
        fileName = "DisplayAnimationProfile",
        menuName = "Project Spark/Display/Animation Profile")]
    public sealed class SparkDisplayAnimationProfile : ScriptableObject
    {
        [Header("Primary Value")]
        public SparkDisplayValueTransition valueTransition =
            SparkDisplayValueTransition.Instrument;

        [Min(0.01f)]
        public float valueSpeed = 10f;

        [Min(0f)]
        public double instrumentDeadband = 0.001d;

        [Min(0f)]
        public float instrumentNoise = 0.01f;

        [Min(0.01f)]
        public float rollingDuration = 0.12f;

        [Header("Text")]
        public SparkDisplayTextAnimation textEntry =
            SparkDisplayTextAnimation.Reveal;

        [Min(1f)]
        public float textSpeed = 20f;

        [Min(0f)]
        public float flickerFrequency = 12f;

        [Header("Status")]
        public SparkDisplayStatusAnimation statusAnimation =
            SparkDisplayStatusAnimation.Fade;

        [Min(0.01f)]
        public float statusSpeed = 8f;

        [Min(0.01f)]
        public float statusPulseSpeed = 8f;

        [Header("Indicator")]
        [Min(0f)]
        public float indicatorPulseSpeed = 5f;

        [Range(0f, 1f)]
        public float indicatorPulseAmount = 0.15f;

        [Header("Shader")]
        [Range(0f, 2f)]
        public float glowAmount = 1f;

        [Range(0f, 1f)]
        public float scanAmount = 0.15f;

        [Range(0f, 1f)]
        public float flickerAmount = 0.05f;

        private void OnValidate()
        {
            valueSpeed =
                Mathf.Max(
                    0.01f,
                    valueSpeed);

            instrumentDeadband =
                System.Math.Max(
                    0d,
                    instrumentDeadband);

            instrumentNoise =
                Mathf.Max(
                    0f,
                    instrumentNoise);

            rollingDuration =
                Mathf.Max(
                    0.01f,
                    rollingDuration);

            textSpeed =
                Mathf.Max(
                    1f,
                    textSpeed);

            flickerFrequency =
                Mathf.Max(
                    0f,
                    flickerFrequency);

            statusSpeed =
                Mathf.Max(
                    0.01f,
                    statusSpeed);

            statusPulseSpeed =
                Mathf.Max(
                    0.01f,
                    statusPulseSpeed);

            indicatorPulseSpeed =
                Mathf.Max(
                    0f,
                    indicatorPulseSpeed);

            indicatorPulseAmount =
                Mathf.Clamp01(
                    indicatorPulseAmount);

            glowAmount =
                Mathf.Clamp(
                    glowAmount,
                    0f,
                    2f);

            scanAmount =
                Mathf.Clamp01(
                    scanAmount);

            flickerAmount =
                Mathf.Clamp01(
                    flickerAmount);
        }
    }
}
