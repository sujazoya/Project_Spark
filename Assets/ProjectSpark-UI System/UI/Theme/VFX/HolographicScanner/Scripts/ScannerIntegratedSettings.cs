using UnityEngine;

namespace ProjectSpark.Scanner
{
    [CreateAssetMenu(
        fileName = "ScannerIntegratedSettings",
        menuName = "Project Spark/Scanner/Integrated Settings")]
    public sealed class ScannerIntegratedSettings
        : ScriptableObject
    {
        [Header("Timing")]
        [SerializeField, Min(0f)]
        private float acquireToScanDelay = 0.15f;

        [SerializeField, Min(0f)]
        private float scanToAnalyzeDelay = 0.15f;

        [SerializeField, Min(0f)]
        private float analyzeToResultDelay = 0.15f;

        [Header("Effect Intensity")]
        [SerializeField, Range(0f, 2f)]
        private float scanIntensityScale = 1f;

        [SerializeField, Range(0f, 2f)]
        private float analysisIntensityScale = 0.9f;

        [SerializeField, Range(0f, 2f)]
        private float resultIntensityScale = 1f;

        [Header("Safety")]
        [SerializeField]
        private bool hideAllEffectsOnDisable = true;

        public float AcquireToScanDelay =>
            acquireToScanDelay;

        public float ScanToAnalyzeDelay =>
            scanToAnalyzeDelay;

        public float AnalyzeToResultDelay =>
            analyzeToResultDelay;

        public float ScanIntensityScale =>
            scanIntensityScale;

        public float AnalysisIntensityScale =>
            analysisIntensityScale;

        public float ResultIntensityScale =>
            resultIntensityScale;

        public bool HideAllEffectsOnDisable =>
            hideAllEffectsOnDisable;
    }
}