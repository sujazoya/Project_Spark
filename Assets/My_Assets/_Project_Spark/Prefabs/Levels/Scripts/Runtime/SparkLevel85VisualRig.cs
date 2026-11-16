using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// OPTIONAL ART-ONLY Level 85 rig adapter. An authoritative gameplay simulation must
    /// call the setters; this class must NEVER decide mission success, safety or physics.
    /// Coordinates are stage-relative metres and source physical measurements.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel85VisualRig : MonoBehaviour
    {
        [SerializeField] private Transform dynamicServiceSpacecraft;
        [SerializeField] private Light approachWarningLight;
        [SerializeField] private Light dockingQualificationLight;
        [SerializeField] private Light criticalFeedbackFaultLight;
        [SerializeField] private ParticleSystem[] sixThrusterPlumes;
        [SerializeField, Range(0f, 1f)] private float visualLerp = .18f;
        private readonly Dictionary<string, Transform> pivots = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> pivotPositions = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> pivotRotations = new Dictionary<string, Quaternion>();
        private Vector3 shipHome;
        private Quaternion shipRotHome;
        private Vector3 actualShipTranslation;
        private Quaternion actualShipAttitude = Quaternion.identity;
        private float actualRange, actualClosingRate, actualLateralError, actualAttitudeError;
        private bool actualW123Valid, actualSafetyLatched, actualAbortActive, actualDockContact;
        private float capture01, latch01, seal01, powerCoupler01;
        private float lidarScanAngleA, lidarScanAngleB;
        private readonly float[] pulse01 = new float[6];
        private bool actualDockQualified;

        private void Awake()
        {
            if (dynamicServiceSpacecraft == null)
            {
                var t = transform.Find("L85_DYNAMIC_SERVICE_SPACECRAFT");
                if (t != null) dynamicServiceSpacecraft = t;
            }
            if (dynamicServiceSpacecraft != null)
            {
                shipHome = dynamicServiceSpacecraft.localPosition;
                shipRotHome = dynamicServiceSpacecraft.localRotation;
            }
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("ANIM_")) continue;
                if (pivots.ContainsKey(t.name)) { Debug.LogWarning("[Level85VisualRig] Duplicate pivot " + t.name, t); continue; }
                pivots.Add(t.name, t);
                pivotPositions.Add(t.name, t.localPosition);
                pivotRotations.Add(t.name, t.localRotation);
            }
            if (pivots.Count != 60) Debug.LogWarning("[Level85VisualRig] Expected 60 unique animation pivots, found " + pivots.Count, this);
        }

        /// <summary>Absolute actual translation from the designed vehicle start pose (m); rotation relative to rest.</summary>
        public void SetActualServiceSpacecraftPose(Vector3 actualTranslationMetres, Quaternion actualBodyAttitude)
        { actualShipTranslation = actualTranslationMetres; actualShipAttitude = actualBodyAttitude; }

        public void SetActualRelativeNavigation(float measuredRangeMetres, float measuredClosingRateMps,
            float measuredLateralErrorMetres, float measuredAttitudeErrorDegrees, bool trustedClosingRateW123Valid)
        {
            actualRange = Mathf.Max(0, measuredRangeMetres); actualClosingRate = measuredClosingRateMps;
            actualLateralError = measuredLateralErrorMetres; actualAttitudeError = measuredAttitudeErrorDegrees;
            actualW123Valid = trustedClosingRateW123Valid;
        }

        /// <summary>Set the measured physical scanner head angles (degrees), not visual clocks.</summary>
        public void SetActualLiDARScanHeadAngles(float actualAdeg, float actualBdeg)
        { lidarScanAngleA = actualAdeg; lidarScanAngleB = actualBdeg; }

        public void SetActualThrusterPulseLevels(float plusX, float minusX, float plusY, float minusY, float plusZ, float minusZ)
        {
            pulse01[0] = Mathf.Clamp01(plusX); pulse01[1] = Mathf.Clamp01(minusX);
            pulse01[2] = Mathf.Clamp01(plusY); pulse01[3] = Mathf.Clamp01(minusY);
            pulse01[4] = Mathf.Clamp01(plusZ); pulse01[5] = Mathf.Clamp01(minusZ);
        }
        public void SetActualDockingMechanisms(float actualSoftCapture01, float actualSixLatchLockFraction01,
            float actualSealCompression01, float actualElectricalCouplerExtension01, bool physicallyContacting,
            bool actualStructuralPressureAndInterfaceQualified)
        {
            capture01 = Mathf.Clamp01(actualSoftCapture01); latch01 = Mathf.Clamp01(actualSixLatchLockFraction01);
            seal01 = Mathf.Clamp01(actualSealCompression01); powerCoupler01 = Mathf.Clamp01(actualElectricalCouplerExtension01);
            actualDockContact = physicallyContacting; actualDockQualified = actualStructuralPressureAndInterfaceQualified;
        }
        public void SetActualSafetyState(bool actualW123FeedbackValid, bool actualCriticalFaultLatched, bool actualAbortModeActive)
        { actualW123Valid = actualW123FeedbackValid; actualSafetyLatched = actualCriticalFaultLatched; actualAbortActive = actualAbortModeActive; }

        private void MoveAxis(string pivotName, Vector3 axis, float amount)
        {
            if (pivots.TryGetValue(pivotName, out var t))
                t.localPosition = Vector3.Lerp(t.localPosition, pivotPositions[pivotName] + axis * amount, visualLerp);
        }
        private void RotateAxis(string pivotName, Vector3 axis, float degrees)
        {
            if (pivots.TryGetValue(pivotName, out var t))
                t.localRotation = Quaternion.Slerp(t.localRotation, pivotRotations[pivotName] * Quaternion.AngleAxis(degrees, axis), visualLerp);
        }
        private void LateUpdate()
        {
            if (dynamicServiceSpacecraft != null)
            {
                dynamicServiceSpacecraft.localPosition = Vector3.Lerp(dynamicServiceSpacecraft.localPosition, shipHome + actualShipTranslation, visualLerp);
                dynamicServiceSpacecraft.localRotation = Quaternion.Slerp(dynamicServiceSpacecraft.localRotation, shipRotHome * actualShipAttitude, visualLerp);
            }
            MoveAxis("ANIM_SERVICE_DOCK_PROBE", Vector3.right, actualDockContact ? .18f : 0f);
            MoveAxis("ANIM_ACTIVE_SOFT_CAPTURE_RING", Vector3.left, -.15f * capture01);
            MoveAxis("ANIM_DOCKING_LATCH_BANK", Vector3.left, -.18f * latch01);
            MoveAxis("ANIM_PRESSURE_SEAL", Vector3.right, .045f * seal01);
            MoveAxis("ANIM_ELECTRICAL_UMBILICAL", Vector3.right, .18f * powerCoupler01);
            RotateAxis("ANIM_CAPTURE_GUIDE_PETALS", Vector3.right, 9f * capture01);
            RotateAxis("ANIM_LIDAR_A_SCANNER", Vector3.up, lidarScanAngleA);
            RotateAxis("ANIM_LIDAR_B_SCANNER", Vector3.up, lidarScanAngleB);
            if (approachWarningLight != null) approachWarningLight.enabled = actualAbortActive || actualLateralError > .15f || Mathf.Abs(actualClosingRate) > .05f;
            if (dockingQualificationLight != null) dockingQualificationLight.enabled = actualDockQualified;
            if (criticalFeedbackFaultLight != null) criticalFeedbackFaultLight.enabled = actualSafetyLatched || !actualW123Valid;
            for (int i = 0; i < sixThrusterPlumes.Length && i < 6; i++)
            {
                var ps = sixThrusterPlumes[i]; if (ps == null) continue;
                var emission = ps.emission; emission.rateOverTime = 120f * pulse01[i];
                if (pulse01[i] > .02f && !ps.isPlaying) ps.Play();
                else if (pulse01[i] <= .02f && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}
