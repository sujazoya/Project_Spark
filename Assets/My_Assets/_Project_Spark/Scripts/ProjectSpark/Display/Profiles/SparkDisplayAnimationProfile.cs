using UnityEngine;

namespace ProjectSpark.Display
{
    [CreateAssetMenu(fileName = "DisplayAnimationProfile", menuName = "Project Spark/Display/Animation Profile")]
    public sealed class SparkDisplayAnimationProfile : ScriptableObject
    {
        [Header("Primary Value")]
        public SparkDisplayValueTransition valueTransition = SparkDisplayValueTransition.Instrument;
        [Min(0.01f)] public float valueSpeed = 10f;
        [Min(0f)] public float instrumentNoise = 0.01f;
        [Min(0f)] public float instrumentDeadband = 0.001f;
        [Min(1f)] public float rollingDuration = 0.12f;

        [Header("Text")]
        public SparkDisplayTextAnimation textEntry = SparkDisplayTextAnimation.Reveal;
        [Min(1f)] public float textSpeed = 20f;
        [Min(0f)] public float flickerFrequency = 12f;

        [Header("Status")]
        public SparkDisplayStatusAnimation statusAnimation = SparkDisplayStatusAnimation.Fade;
        [Min(0.01f)] public float statusSpeed = 8f;

        [Header("Shader")]
        [Range(0f, 2f)] public float glowAmount = 1f;
        [Range(0f, 1f)] public float scanAmount = 0.15f;
        [Range(0f, 1f)] public float flickerAmount = 0.05f;
    }
}
