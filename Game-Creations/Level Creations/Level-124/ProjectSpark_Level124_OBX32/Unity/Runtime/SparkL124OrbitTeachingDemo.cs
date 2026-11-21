using UnityEngine;

namespace ProjectSpark.Level124
{
    /// <summary>
    /// Non-scoring spacecraft attitude/thermal/power TEACHING EXAMPLE. No relation to the
    /// physical circuit graph, actuator authorization, radiation certification or real flight safety.
    /// Does not run automatically and never calls a gameplay manager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL124OrbitTeachingDemo : MonoBehaviour
    {
        [SerializeField, Range(-1f, 1f)] private float illustrativeRollError;
        [SerializeField, Range(-1f, 1f)] private float illustrativePitchError;
        [SerializeField, Range(-1f, 1f)] private float illustrativeYawError;
        [SerializeField, Range(0f, 1f)] private float illustrativeWheelThreeFault;
        [SerializeField, Range(0f, 1f)] private float illustrativeStarTrackerConfidence = 1f;
        public bool IsCertified => false;
        public bool CanAuthorizeActuators => false;
        public string Status => "ILLUSTRATIVE ONLY: no approved schematic, real star-tracker timing or safety authority.";

        // Caller allocates four floats. Values are normalized art/lesson samples, not torque.
        public bool TryCalculateIllustrativeWheelEfforts(float[] output)
        {
            if (output == null || output.Length < 4) return false;
            float roll = illustrativeRollError * illustrativeStarTrackerConfidence;
            float pitch = illustrativePitchError * illustrativeStarTrackerConfidence;
            float yaw = illustrativeYawError * illustrativeStarTrackerConfidence;
            output[0] = Mathf.Clamp(-roll + yaw, -1f, 1f);
            output[1] = Mathf.Clamp(roll + yaw, -1f, 1f);
            output[2] = Mathf.Clamp(-pitch - yaw, -1f, 1f) * (1f - illustrativeWheelThreeFault);
            output[3] = Mathf.Clamp(pitch - yaw, -1f, 1f);
            return true;
        }
    }
}
