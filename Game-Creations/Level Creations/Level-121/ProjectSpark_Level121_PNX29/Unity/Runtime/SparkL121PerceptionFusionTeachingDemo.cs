using UnityEngine;
namespace ProjectSpark.Level121
{
    /// <summary>Pure classroom exercise; measurements NOT from sensors or SparkCircuitSystem.
    /// Explicitly cannot award objectives, change physical fault state or authorize protected motion.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkL121PerceptionFusionTeachingDemo : MonoBehaviour
    {
        [SerializeField, Range(0, 40)] private float camera2DelayMs = 5;
        [SerializeField, Range(0, 40)] private float lidarDelayMs = 7;
        [SerializeField, Range(0, 100)] private float cameraConfidence = 88;
        [SerializeField, Range(0, 100)] private float lidarConfidence = 75;
        public bool IsCertifiable => false;
        public float IllustrativeTimestampSkewMs => Mathf.Abs(camera2DelayMs - lidarDelayMs);
        public float IllustrativeFusionConfidence =>
            Mathf.Clamp01((cameraConfidence * .55f + lidarConfidence * .45f) / 100f);
        public string Warning => "EDUCATIONAL DEMO ONLY: no active camera, LiDAR, IMU, route or safety authority";
    }
}
