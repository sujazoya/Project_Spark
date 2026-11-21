using System;
using UnityEngine;

namespace ProjectSpark.Level120
{
    /// <summary>Illustrative robot joint motion only. No motor/kinematic authority,
    /// safe-motion certification, pin model or gameplay objective awards.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkL120RobotMotionTeachingDemo : MonoBehaviour
    {
        [Serializable]
        public struct JointSample
        {
            public float timeSeconds;
            public float[] jointDegrees;
            public float[] jointSpeedDegreesPerSecond;
        }

        [SerializeField, Min(8)] private int sampleCount = 90;
        [SerializeField, Range(0.5f, 12f)] private float durationSeconds = 6f;
        [SerializeField] private float[] targetDegrees = { 60f, -35f, 42f, 20f, -55f, 70f };
        [SerializeField] private JointSample[] latestSamples = new JointSample[0];
        [SerializeField, TextArea(2, 4)] private string notice =
            "ILLUSTRATIVE SIX-AXIS TRAJECTORY ONLY: NOT a motor solver, measured position, collision clearance, verified safe motion, or gameplay authority.";

        public bool IsAuthoritative => false;
        public JointSample[] LatestSamples => latestSamples;

        [ContextMenu("Run NON-CERTIFIABLE six-axis trajectory teaching demo")]
        public void GenerateIllustrativeTrajectory()
        {
            int count = Mathf.Clamp(sampleCount, 8, 250);
            float duration = Mathf.Clamp(durationSeconds, .5f, 12f);
            latestSamples = new JointSample[count];
            for (int step = 0; step < count; step++)
            {
                float t = step * duration / (count - 1);
                float u = t / duration;
                // Quintic smoothstep gives zero start/end velocity and acceleration.
                float ease = u * u * u * (10f - 15f * u + 6f * u * u);
                float rate = 30f * u * u * (1f - u) * (1f - u) / duration;
                JointSample sample = new JointSample {
                    timeSeconds = t,
                    jointDegrees = new float[6],
                    jointSpeedDegreesPerSecond = new float[6]
                };
                for (int j = 0; j < 6; j++)
                {
                    float target = targetDegrees != null && j < targetDegrees.Length ? targetDegrees[j] : 0f;
                    sample.jointDegrees[j] = target * ease;
                    sample.jointSpeedDegreesPerSecond[j] = target * rate;
                }
                latestSamples[step] = sample;
            }
            Debug.Log("[L120 DEMO] Illustrated joint curves computed. No axis enabled, no motor power, no certification or objective state changed.");
        }
    }
}
