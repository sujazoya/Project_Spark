using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Level 78 ART-ONLY animator for the 46 named parent pivotLookup in the binary FBXs.
    /// DO NOT use this class as a source of vacuum, plasma, electrical, film, safety,
    /// or objective truth. Only feed actual results from the authoritative simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel78VisualRig : MonoBehaviour
    {
        [Header("Optional runtime-created or authored visuals")]
        [SerializeField] private ParticleSystem actualCapPlasmaVFX;
        [SerializeField] private ParticleSystem actualLowKPlasmaVFX;
        [SerializeField] private ParticleSystem actualCureByproductVFX;
        [SerializeField] private Light capPlasmaLight;
        [SerializeField] private Light lowKPlasmaLight;
        [SerializeField] private Light uvCureLight;
        [SerializeField] private Light criticalAlarmLight;
        [SerializeField] private Light leakageSignalHealthyLight;

        public Transform art;
        public Transform[] pivots;
        private readonly Dictionary<string, Transform> pivotLookup = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> originalPositions = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> originalRotations = new Dictionary<string, Quaternion>();
        private Vector3 robotAngles;
        private Vector3 chamberLids01;
        private Vector3 precursorFlow01;
        private float loadLock01, gateClosed01, waferAlignDegrees;
        private float capRF01, lowKRF01, capCoverage01, lowKFilm01, cureProgress01;
        private float actualCureRPM, uvLamp01, shutterOpen01, actualTurboRPM, actualBackingRPM;
        private float stressProgress01, measuredLeakage01, inspectionProgress01, humidity01;
        private bool capPlasmaActuallyOn, lowKPlasmaActuallyOn, cureActuallyOn;
        private bool leakageMeasurementValid, stressSourceAuthorized, criticalFault;

        private void Awake()
        {
            foreach (Transform t in pivots != null && pivots.Length > 0 ? pivots : GetComponentsInChildren<Transform>(true))
            {
                if (t == null) continue;
                int offset = t.name.IndexOf("ANIM_", StringComparison.Ordinal);
                if (offset < 0) continue;
                string key = t.name.Substring(offset);
                if (pivotLookup.ContainsKey(key))
                {
                    Debug.LogWarning("[Level78VisualRig] Duplicate animation parent: " + t.name, this);
                    continue;
                }
                pivotLookup.Add(key, t);
                originalPositions.Add(key, t.localPosition);
                originalRotations.Add(key, t.localRotation);
            }
            if (pivotLookup.Count != 46)
                Debug.LogWarning("[Level78VisualRig] Found " + pivotLookup.Count + "/46 named animation pivotLookup. Use all 64 modular FBXs.", this);
        }

        public void SetActualVacuumRPM(float turboRPM, float backingRPM)
        { actualTurboRPM = Mathf.Max(0f, turboRPM); actualBackingRPM = Mathf.Max(0f, backingRPM); }

        public void SetActualTransferPose(float robotBaseDeg, float robotArmDeg, float robotBladeDeg,
            float actualLoadLockOpen01, float actualGateClosed01, float actualWaferAlignmentDeg)
        {
            robotAngles = new Vector3(robotBaseDeg, robotArmDeg, robotBladeDeg);
            loadLock01 = Mathf.Clamp01(actualLoadLockOpen01);
            gateClosed01 = Mathf.Clamp01(actualGateClosed01);
            waferAlignDegrees = actualWaferAlignmentDeg;
        }

        public void SetActualChamberPositions(float capLidOpen01, float lowKLidOpen01, float cureDoorOpen01)
        { chamberLids01 = new Vector3(Mathf.Clamp01(capLidOpen01), Mathf.Clamp01(lowKLidOpen01), Mathf.Clamp01(cureDoorOpen01)); }

        public void SetActualGasFlows(float capPrecursorFlow01, float lowKPrecursorFlow01, float porogenFlow01)
        { precursorFlow01 = new Vector3(Mathf.Clamp01(capPrecursorFlow01), Mathf.Clamp01(lowKPrecursorFlow01), Mathf.Clamp01(porogenFlow01)); }

        public void SetActualDepositionState(bool actualCapPlasmaActive, bool actualLowKPlasmaActive,
            float actualCapRF01, float actualLowKRF01, float measuredCapCoverage01, float measuredLowKFilmProgress01)
        {
            capPlasmaActuallyOn = actualCapPlasmaActive;
            lowKPlasmaActuallyOn = actualLowKPlasmaActive;
            capRF01 = Mathf.Clamp01(actualCapRF01);
            lowKRF01 = Mathf.Clamp01(actualLowKRF01);
            capCoverage01 = Mathf.Clamp01(measuredCapCoverage01);
            lowKFilm01 = Mathf.Clamp01(measuredLowKFilmProgress01);
        }

        public void SetActualCureState(bool actualCureActive, float actualCurePlatenRPM, float actualLampOutput01,
            float actualShutterOpen01, float measuredCureProgress01)
        {
            cureActuallyOn = actualCureActive;
            actualCureRPM = Mathf.Max(0f, actualCurePlatenRPM);
            uvLamp01 = Mathf.Clamp01(actualLampOutput01);
            shutterOpen01 = Mathf.Clamp01(actualShutterOpen01);
            cureProgress01 = Mathf.Clamp01(measuredCureProgress01);
        }

        public void SetActualReliabilityState(float measuredLeakageCurrentNormalized01, bool leakageSignalIsValid,
            bool actualBiasStressSourceAuthorized, float measuredMoisture01, float actualStressProgress01,
            float actualInspectionProgress01, bool actualCriticalFault)
        {
            measuredLeakage01 = Mathf.Clamp01(measuredLeakageCurrentNormalized01);
            leakageMeasurementValid = leakageSignalIsValid;
            stressSourceAuthorized = actualBiasStressSourceAuthorized;
            humidity01 = Mathf.Clamp01(measuredMoisture01);
            stressProgress01 = Mathf.Clamp01(actualStressProgress01);
            inspectionProgress01 = Mathf.Clamp01(actualInspectionProgress01);
            criticalFault = actualCriticalFault;
        }

        public void BindEffects(Light cap,Light lowK,Light cure,Light alarm)
        {
            capPlasmaLight=cap;
            lowKPlasmaLight=lowK;
            uvCureLight=cure;
            criticalAlarmLight=alarm;
            cap.enabled=lowK.enabled=cure.enabled=alarm.enabled=false;
        }

        public void Apply(ProjectSpark.Gameplay.SparkLevel78Plant plant,ProjectSpark.Gameplay.SparkLevel78Plant.Cycle cycle,
            float transfer,float scan,bool valid,bool permit,bool fault,bool emergency)
        {
            float reach=Mathf.Sin(Mathf.Clamp01(transfer/1.35f)*Mathf.PI);
            SetActualVacuumRPM(plant.Turbo*1600f,plant.Backing*460f);
            SetActualTransferPose(reach*42f,reach*25f,-reach*31f,transfer>0&&transfer<.32f?reach*2:0,reach<.45f?1:0,reach*95f);
            SetActualChamberPositions(0,0,0);
            SetActualGasFlows(plant.CapFlow,plant.MatrixFlow,plant.PorogenFlow);
            bool cap=cycle==ProjectSpark.Gameplay.SparkLevel78Plant.Cycle.Cap&&plant.RF>.10f;
            bool low=cycle==ProjectSpark.Gameplay.SparkLevel78Plant.Cycle.LowK&&plant.RF>.10f;
            bool curing=cycle==ProjectSpark.Gameplay.SparkLevel78Plant.Cycle.Cure&&plant.Lamp>.10f;
            SetActualDepositionState(cap,low,cap?plant.RF:0,low?plant.RF:0,1-plant.Pinhole,plant.MeanFilm/76);
            SetActualCureState(curing,plant.Platen,plant.Lamp,plant.Shutter,plant.CureProgress);
            SetActualReliabilityState(valid?plant.LeakageSignal:0,valid,permit,plant.Humidity,
                Mathf.Clamp01(plant.Exposure/2.9f),scan/1.2f,fault||emergency);
        }

        private void Rotate(string name, Vector3 euler)
        {
            if (pivotLookup.TryGetValue(name, out Transform t))
                t.localRotation = originalRotations[name] * Quaternion.Euler(euler);
        }
        private void Shift(string name, Vector3 localOffset)
        {
            if (pivotLookup.TryGetValue(name, out Transform t))
                t.localPosition = originalPositions[name] + (art != null ? t.parent.InverseTransformVector(art.TransformVector(new Vector3(-localOffset.x,localOffset.y,-localOffset.z)*.096f)) : localOffset);
        }
        private void Spin(string name, float rpm, float dt, Vector3 axis)
        {
            if (pivotLookup.TryGetValue(name, out Transform t))
                t.Rotate(axis, Mathf.Clamp(rpm, -4000f, 4000f) * 6f * dt, Space.Self);
        }
        private static void VFX(ParticleSystem ps, bool shouldPlay)
        {
            if (ps == null) return;
            if (shouldPlay && !ps.isPlaying) ps.Play();
            else if (!shouldPlay && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        private void Update()
        {
            float dt = Time.deltaTime;
            Rotate("ANIM_TRANSFER_ROBOT_BASE", Vector3.up * robotAngles.x);
            Rotate("ANIM_TRANSFER_ROBOT_ARM", Vector3.up * robotAngles.y);
            Rotate("ANIM_TRANSFER_ROBOT_BLADE", Vector3.up * robotAngles.z);
            Rotate("ANIM_WAFER_ALIGNER", Vector3.up * waferAlignDegrees);
            Shift("ANIM_LOADLOCK_DOOR", Vector3.forward * (.35f * loadLock01));
            Shift("ANIM_GATE_VALVE", Vector3.up * (.25f * (1f - gateClosed01)));
            Shift("ANIM_CAP_CHAMBER_LID", Vector3.up * (.55f * chamberLids01.x));
            Shift("ANIM_LOWK_CHAMBER_LID", Vector3.up * (.55f * chamberLids01.y));
            Shift("ANIM_UV_CURE_DOOR", Vector3.up * (.42f * chamberLids01.z));
            Rotate("ANIM_CAP_PRECURSOR_VALVE", Vector3.up * (68f * precursorFlow01.x));
            Rotate("ANIM_LOWK_PRECURSOR_VALVE", Vector3.up * (68f * precursorFlow01.y));
            Rotate("ANIM_LOWK_POROGEN_VALVE", Vector3.up * (68f * precursorFlow01.z));
            Shift("ANIM_CAP_SHOWERHEAD", Vector3.up * (.07f * (capPlasmaActuallyOn ? 1f : 0f)));
            Shift("ANIM_LOWK_SHOWERHEAD", Vector3.up * (.07f * (lowKPlasmaActuallyOn ? 1f : 0f)));
            Shift("ANIM_CAP_HEATED_SUSCEPTOR", Vector3.up * (.05f * capCoverage01));
            Shift("ANIM_LOWK_SUSCEPTOR", Vector3.up * (.05f * lowKFilm01));
            Shift("ANIM_UV_SHUTTER", Vector3.right * (.39f * shutterOpen01));
            Spin("ANIM_CAP_WAFER_ROTATION", capPlasmaActuallyOn ? 8f : 0f, dt, Vector3.up);
            Spin("ANIM_CURE_ROTATING_PLATEN", actualCureRPM, dt, Vector3.up);
            Spin("ANIM_TURBO_ROTOR", actualTurboRPM, dt, Vector3.up);
            Spin("ANIM_BACKING_ROTOR", actualBackingRPM, dt, Vector3.right);
            Shift("ANIM_CAP_THICKNESS_SCAN", Vector3.right * (.12f * capCoverage01));
            Shift("ANIM_LOWK_FILM_SCAN", Vector3.right * (.12f * lowKFilm01));
            Shift("ANIM_FILM_BOW_SCAN", Vector3.right * (.24f * inspectionProgress01));
            Shift("ANIM_ELLIPSOMETRY_HEAD", Vector3.down * (.08f * inspectionProgress01));
            Shift("ANIM_LEAKAGE_TEST_PROBE", Vector3.down * (.14f * stressProgress01));
            Shift("ANIM_BIAS_STRESS_GUARD", Vector3.forward * (.08f * (stressSourceAuthorized ? 1f : 0f)));
            Shift("ANIM_MOISTURE_PROBE", Vector3.down * (.04f * humidity01));
            Shift("ANIM_RC_TIMING_PROBE", Vector3.down * (.10f * inspectionProgress01));
            if (capPlasmaLight != null) { capPlasmaLight.enabled = capPlasmaActuallyOn && !criticalFault; capPlasmaLight.intensity = capRF01 * 4f; }
            if (lowKPlasmaLight != null) { lowKPlasmaLight.enabled = lowKPlasmaActuallyOn && !criticalFault; lowKPlasmaLight.intensity = lowKRF01 * 4f; }
            if (uvCureLight != null) { uvCureLight.enabled = cureActuallyOn && shutterOpen01 > .05f; uvCureLight.intensity = uvLamp01 * 4f; }
            if (criticalAlarmLight != null) criticalAlarmLight.enabled = criticalFault;
            if (leakageSignalHealthyLight != null) { leakageSignalHealthyLight.enabled = leakageMeasurementValid && !criticalFault; leakageSignalHealthyLight.intensity = 1.5f * (1f - measuredLeakage01); }
            VFX(actualCapPlasmaVFX, capPlasmaActuallyOn && !criticalFault);
            VFX(actualLowKPlasmaVFX, lowKPlasmaActuallyOn && !criticalFault);
            VFX(actualCureByproductVFX, cureActuallyOn && shutterOpen01 > .05f && cureProgress01 < 1f);
            // Hologram shader parameters must be bound separately to actual film and reliability data.
        }
    }
}

