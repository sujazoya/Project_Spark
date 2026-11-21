using System.Collections;
using UnityEngine;

namespace ProjectSpark.Level114
{
    /// <summary>
    /// Inspector-visible, bounded thermal RC teaching demonstration. NOT the real circuit,
    /// temperature sensor, safety authority, or LevelGamePlayManager objective source.
    /// Emitted numerical values MUST be labeled illustrative by any UI that displays them.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkL114ThermalSandbox : MonoBehaviour
    {
        [Header("Educational model only - no objective certification")]
        [SerializeField, Range(0f, 1f)] private float processorLoad = 0.65f;
        [SerializeField, Range(0f, 1f)] private float vrmLoad = 0.45f;
        [SerializeField, Range(15f, 45f)] private float ambientCelsius = 25f;
        [SerializeField] private bool automaticFanControl = true;
        [SerializeField, Range(0f, 1f)] private float manualFanDuty = 0.60f;
        [SerializeField] private bool timDelaminationFaultF1;
        [SerializeField] private bool fanTachFaultF2;
        [SerializeField] private bool vrmThermalViaFaultF3;
        [SerializeField] private bool w257Connected;
        [SerializeField] private bool demoSafetyLatch = true;

        [Header("Illustrative results - not measurements")]
        [SerializeField] private float cpuJunctionCelsius = 25f;
        [SerializeField] private float vrmHotspotCelsius = 25f;
        [SerializeField] private float heatsinkCelsius = 25f;
        [SerializeField] private float fanDuty;
        [SerializeField] private float actualCoolingFraction;
        [SerializeField] private bool demoThermalThresholdExceeded;
        [SerializeField] private string status = "ILLUSTRATIVE - NOT CERTIFIABLE";
        private const float SampleIntervalSeconds = 0.10f;

        private void OnEnable()
        {
            cpuJunctionCelsius = ambientCelsius;
            vrmHotspotCelsius = ambientCelsius;
            heatsinkCelsius = ambientCelsius;
            demoSafetyLatch = true;
            StartCoroutine(Simulate());
        }

        private void OnDisable() { StopAllCoroutines(); }

        private IEnumerator Simulate()
        {
            WaitForSeconds delay = new WaitForSeconds(SampleIntervalSeconds);
            while (true)
            {
                Step(SampleIntervalSeconds);
                yield return delay;
            }
        }

        private void Step(float dt)
        {
            // Empirical educational three-node network with energy in joules and watts.
            // Do not interpret these as measured hardware temperatures or calibrated limits.
            float cpuPowerWatts = 8f + 112f * Mathf.Clamp01(processorLoad);
            float vrmPowerWatts = 4f + 35f * Mathf.Clamp01(vrmLoad);
            float requestedDuty = automaticFanControl
                ? Mathf.Clamp01(0.18f + Mathf.Max(cpuJunctionCelsius - 45f, vrmHotspotCelsius - 53f) / 65f)
                : Mathf.Clamp01(manualFanDuty);
            fanDuty = requestedDuty;
            actualCoolingFraction = fanTachFaultF2 ? requestedDuty * 0.12f : requestedDuty;

            float cpuToSinkK = 1f / (timDelaminationFaultF1 ? 0.27f : 0.070f);
            float vrmToSinkK = 1f / (vrmThermalViaFaultF3 ? 0.72f : 0.16f);
            float qCpu = cpuToSinkK * (cpuJunctionCelsius - heatsinkCelsius);
            float qVrm = vrmToSinkK * (vrmHotspotCelsius - heatsinkCelsius);
            float qAir = (0.6f + 3.1f * actualCoolingFraction) * (heatsinkCelsius - ambientCelsius);

            cpuJunctionCelsius += dt * (cpuPowerWatts - qCpu) / 26f;
            vrmHotspotCelsius += dt * (vrmPowerWatts - qVrm) / 21f;
            heatsinkCelsius += dt * (qCpu + qVrm - qAir) / 115f;
            cpuJunctionCelsius = Mathf.Clamp(cpuJunctionCelsius, ambientCelsius, 250f);
            vrmHotspotCelsius = Mathf.Clamp(vrmHotspotCelsius, ambientCelsius, 250f);
            heatsinkCelsius = Mathf.Clamp(heatsinkCelsius, ambientCelsius, 250f);

            demoThermalThresholdExceeded = cpuJunctionCelsius >= 105f || vrmHotspotCelsius >= 115f;
            if (!w257Connected || demoThermalThresholdExceeded) demoSafetyLatch = true;
            status = demoSafetyLatch ? "ILLUSTRATIVE: DEMO SAFETY LATCHED" :
                "ILLUSTRATIVE: DEMO SAFE (NOT OBJECTIVE EVIDENCE)";
        }

        [ContextMenu("DEMO ONLY - reset safety latch after connecting W257")]
        private void ResetDemoLatch()
        {
            if (w257Connected && !demoThermalThresholdExceeded &&
                cpuJunctionCelsius < 95f && vrmHotspotCelsius < 100f)
                demoSafetyLatch = false;
        }
    }
}
