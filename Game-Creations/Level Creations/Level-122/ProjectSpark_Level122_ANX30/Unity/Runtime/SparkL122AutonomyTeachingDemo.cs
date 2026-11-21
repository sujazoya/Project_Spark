using UnityEngine;

namespace ProjectSpark.Level122
{
    /// <summary>
    /// Non-scoring classroom exercise. NOT a GNSS, radar, IMU, SLAM or safety simulation.
    /// No SparkCircuitSystem or LevelGamePlayManager coupling by design.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL122AutonomyTeachingDemo : MonoBehaviour
    {
        [SerializeField, Range(0f, 8f)] private float illustrativeGnssErrorMetres = 1.2f;
        [SerializeField, Range(0f, 8f)] private float illustrativeRadarErrorMetres = 0.8f;
        [SerializeField, Range(0f, 8f)] private float illustrativeInertialDriftMetres = 0.6f;
        [SerializeField, Range(0f, 1f)] private float spoofSuspicion = 0.15f;
        [SerializeField, Range(0f, 1f)] private float disagreementThreshold = 0.55f;

        public bool IsCertifiable => false;
        public bool CanAuthorizeMotion => false;
        public float IllustrativeConfidence => Mathf.Clamp01(
            1f - .035f * (illustrativeGnssErrorMetres + illustrativeRadarErrorMetres + illustrativeInertialDriftMetres)
               - .55f * spoofSuspicion);
        public bool IllustrativeDisagreement =>
            Mathf.Abs(illustrativeGnssErrorMetres - illustrativeRadarErrorMetres) > disagreementThreshold;
        public string Status => "EDUCATIONAL DEMO ONLY - no approved sensor measurements, pin map, safety graph, or scoring";
    }
}
