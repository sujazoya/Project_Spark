using System;
using UnityEngine;

namespace ProjectSpark.Level119
{
    /// <summary>
    /// Pure bounded educational three-phase/FOC demonstration. Not an electrical solver,
    /// functional-safety certification, physical motor model, or objective authority.
    /// Runs only when explicitly requested; never calls LevelGamePlayManager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL119FOCTeachingDemo : MonoBehaviour
    {
        [Serializable]
        public struct Sample
        {
            public float timeSeconds;
            public float phaseAAmps;
            public float phaseBAmps;
            public float phaseCAmps;
            public float electricalAngleRadians;
            public float illustrativeTorqueNm;
        }

        [SerializeField, Range(0f, 20f)] private float commandedCurrentAmps = 4f;
        [SerializeField, Range(0.1f, 200f)] private float electricalFrequencyHz = 25f;
        [SerializeField, Range(0.01f, 10f)] private float nominalTorqueConstantNmPerAmp = 0.15f;
        [SerializeField, Min(6)] private int sampleCount = 30;
        [SerializeField] private Sample[] latestSamples = new Sample[0];
        [SerializeField, TextArea(3, 5)] private string notice =
            "ILLUSTRATIVE FOC ONLY. NOT physical current, PWM switching, measured torque, STO compliance, regenerative energy, approved pins, or certified gameplay.";

        public bool IsAuthoritative => false;
        public Sample[] LatestSamples => latestSamples;

        [ContextMenu("Run non-certifiable three-phase teaching sample")]
        public void RunIllustrativeSample()
        {
            int count = Mathf.Clamp(sampleCount, 6, 180);
            float hz = Mathf.Clamp(electricalFrequencyHz, 0.1f, 200f);
            float current = Mathf.Clamp(commandedCurrentAmps, 0f, 20f);
            float torqueConstant = Mathf.Clamp(nominalTorqueConstantNmPerAmp, .01f, 10f);
            latestSamples = new Sample[count];
            float period = 1f / hz;
            for (int i = 0; i < count; i++)
            {
                float t = (period * i) / (count - 1);
                float angle = 2f * Mathf.PI * hz * t;
                latestSamples[i] = new Sample
                {
                    timeSeconds = t,
                    electricalAngleRadians = angle,
                    phaseAAmps = current * Mathf.Sin(angle),
                    phaseBAmps = current * Mathf.Sin(angle - 2f * Mathf.PI / 3f),
                    phaseCAmps = current * Mathf.Sin(angle + 2f * Mathf.PI / 3f),
                    illustrativeTorqueNm = torqueConstant * current
                };
            }
            Debug.Log("[L119 DEMO] Illustrative 3-phase samples updated; no electrical, thermal, safety or objective state changed.");
        }
    }
}
