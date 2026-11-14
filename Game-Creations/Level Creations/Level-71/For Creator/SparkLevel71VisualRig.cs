using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>OPTIONAL LEVEL 71 ART-ONLY adapter. All public values are supplied by the actual
    /// vacuum, RF, process and machine controller simulation. This script has NO gameplay authority.
    /// Assign *separated moving submeshes* to these transforms after FBX import/rigging.</summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel71VisualRig : MonoBehaviour
    {
        [Header("Moving references - optional, none are auto-rigged by the FBXs")]
        [SerializeField] private Transform turboRotor;
        [SerializeField] private Transform backingRotor;
        [SerializeField] private Transform gateValve;
        [SerializeField] private Transform isolationValve;
        [SerializeField] private Transform throttleValve;
        [SerializeField] private Transform reactorLid;
        [SerializeField] private Transform waferLift;
        [SerializeField] private Transform robotBase;
        [SerializeField] private Transform robotArm;
        [SerializeField] private Transform robotTool;
        [SerializeField] private Transform matchingCapacitorA;
        [SerializeField] private Transform matchingCapacitorB;
        [SerializeField] private Light plasmaLight;
        [SerializeField] private GameObject plasmaVFX;
        private float turboRPM, backingRPM, gateFraction, isolationFraction, throttleFraction;
        private float lidFraction, waferLiftFraction, robotBaseDeg, robotArmDeg, robotToolDeg;
        private float matchingA, matchingB, plasmaIntensity;
        public void SetVacuumRPM(float turbo, float backing)
        {
            turboRPM = Mathf.Max(0f, turbo);
            backingRPM = Mathf.Max(0f, backing);
        }
        public void SetValveOpenFractions(float gate, float isolation, float throttle)
        {
            gateFraction = Mathf.Clamp01(gate);
            isolationFraction = Mathf.Clamp01(isolation);
            throttleFraction = Mathf.Clamp01(throttle);
        }
        public void SetWaferHandlingPose(float lid, float lift, float baseDegrees, float armDegrees, float toolDegrees)
        {
            lidFraction = Mathf.Clamp01(lid); waferLiftFraction = Mathf.Clamp01(lift);
            robotBaseDeg = baseDegrees; robotArmDeg = armDegrees; robotToolDeg = toolDegrees;
        }
        public void SetMatchingPositions(float positionA, float positionB)
        {
            matchingA = Mathf.Clamp01(positionA); matchingB = Mathf.Clamp01(positionB);
        }
        public void SetPlasmaVisualIntensity(float normalizedIntensity)
        {
            plasmaIntensity = Mathf.Clamp01(normalizedIntensity);
        }
        private void Update()
        {
            if (turboRotor != null) turboRotor.Rotate(Vector3.right, turboRPM * 6f * Time.deltaTime, Space.Self);
            if (backingRotor != null) backingRotor.Rotate(Vector3.right, backingRPM * 6f * Time.deltaTime, Space.Self);
            if (gateValve != null) gateValve.localPosition = new Vector3(0f, gateFraction * .55f, 0f);
            if (isolationValve != null) isolationValve.localRotation = Quaternion.Euler(0f, isolationFraction * 90f, 0f);
            if (throttleValve != null) throttleValve.localRotation = Quaternion.Euler(0f, throttleFraction * 80f, 0f);
            if (reactorLid != null) reactorLid.localRotation = Quaternion.Euler(lidFraction * 65f, 0f, 0f);
            if (waferLift != null) waferLift.localPosition = new Vector3(0f, waferLiftFraction * .18f, 0f);
            if (robotBase != null) robotBase.localRotation = Quaternion.Euler(0f, robotBaseDeg, 0f);
            if (robotArm != null) robotArm.localRotation = Quaternion.Euler(0f, 0f, robotArmDeg);
            if (robotTool != null) robotTool.localRotation = Quaternion.Euler(0f, 0f, robotToolDeg);
            if (matchingCapacitorA != null) matchingCapacitorA.localRotation = Quaternion.Euler(0f, matchingA * 270f, 0f);
            if (matchingCapacitorB != null) matchingCapacitorB.localRotation = Quaternion.Euler(0f, matchingB * 270f, 0f);
            if (plasmaVFX != null && plasmaVFX.activeSelf != (plasmaIntensity > .03f)) plasmaVFX.SetActive(plasmaIntensity > .03f);
            if (plasmaLight != null)
            {
                plasmaLight.enabled = plasmaIntensity > .03f;
                plasmaLight.intensity = plasmaIntensity * 5f;
            }
        }
    }
}
