using UnityEngine;

namespace ProjectSpark.Level123
{
    /// <summary>
    /// Visual teaching calculation only. This is NOT a flight controller, ESC PWM generator,
    /// motor simulation, or certifiable safety system. No Update loop or game authority.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL123FlightTeachingDemo : MonoBehaviour
    {
        [SerializeField, Range(-1f, 1f)] private float illustrativeRoll;
        [SerializeField, Range(-1f, 1f)] private float illustrativePitch;
        [SerializeField, Range(-1f, 1f)] private float illustrativeYaw;
        [SerializeField, Range(0f, 1f)] private float illustrativeCollective = .35f;
        [SerializeField, Range(0f, 1f)] private float faultMotor5Magnitude;
        public bool IsCertified => false;
        public bool CanAuthorizeMotorPower => false;
        public string Status => "ILLUSTRATIVE MIX ONLY. No motor gate, IMU evidence, topology, or safety authority.";

        /// <summary>Writes 8 normalized illustrative mix values into caller-owned array.</summary>
        public bool TryCalculateMix(float[] output)
        {
            if (output == null || output.Length < 8) return false;
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * .25f;
                float rollTerm = Mathf.Sin(a) * illustrativeRoll * .15f;
                float pitchTerm = Mathf.Cos(a) * illustrativePitch * .15f;
                float yawTerm = (i % 2 == 0 ? 1f : -1f) * illustrativeYaw * .07f;
                float value = Mathf.Clamp01(illustrativeCollective + rollTerm + pitchTerm + yawTerm);
                if (i == 4) value *= 1f - faultMotor5Magnitude;
                output[i] = value;
            }
            return true;
        }
    }
}
