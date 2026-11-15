using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Visuals
{
    /// <summary>
    /// Scene-art-only Level 77 pivot adapter. Never computes polishing removal, pad wear,
    /// sensor validity, leakage, critical protection or authoritative objectives.
    /// Set all inputs from one real gameplay simulation, not from UI controls directly.
    /// The pivotLookup are exported as named parent transforms in each modular binary FBX.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SparkLevel77VisualRig : MonoBehaviour
    {
        [Header("Optional authored/Unity-built VFX")]
        [SerializeField] private ParticleSystem bulkSlurryVFX;
        [SerializeField] private ParticleSystem softLandingFluidVFX;
        [SerializeField] private ParticleSystem barrierFluidVFX;
        [SerializeField] private Light criticalAlarmLight;
        [SerializeField] private Light opticalEndpointLight;

        public Transform art;
        public Transform[] pivots;
        private readonly Dictionary<string, Transform> pivotLookup = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Vector3> initialPositions = new Dictionary<string, Vector3>();
        private readonly Dictionary<string, Quaternion> initialRotations = new Dictionary<string, Quaternion>();

        private Vector3 actualPlatenRPM, actualCarrierRPM, actualCarrierLift01;
        private Vector3 actualStageRemoval01;
        private Vector3 metrology01;
        private Vector3 gantryOffset;
        private Vector3 robotAngles;
        private float gripperDrop01, bulkNozzle01, softNozzle01, barrierNozzle01;
        private float dishing01, erosion01, cleaning01, drying01, endpointConfidence01;

        private bool polishingAuthorized, endpointMeasurementValid, criticalFault;

        private void Awake()
        {
            foreach (Transform tr in pivots != null && pivots.Length > 0 ? pivots : GetComponentsInChildren<Transform>(true))
            {
                if (tr == null) continue;
                int offset = tr.name.IndexOf("ANIM_", StringComparison.Ordinal);
                if (offset < 0) continue;
                string key = tr.name.Substring(offset);
                if (pivotLookup.ContainsKey(key))
                {
                    Debug.LogWarning("[Level 77 Visual Rig] Duplicate pivot name: " + tr.name, this);
                    continue;
                }
                pivotLookup.Add(key, tr);
                initialPositions.Add(key, tr.localPosition);
                initialRotations.Add(key, tr.localRotation);
            }
        }

        public void SetActualPlatenRPM(float bulk, float softLanding, float barrierClear)
        { actualPlatenRPM = new Vector3(bulk, softLanding, barrierClear); }

        public void SetActualCarrierRPM(float bulk, float softLanding, float barrierClear)
        { actualCarrierRPM = new Vector3(bulk, softLanding, barrierClear); }

        // Normalized actual physical lift position: 0 engaged, 1 retracted.
        public void SetActualCarrierLift(float bulk01, float soft01, float barrier01)
        { actualCarrierLift01 = new Vector3(Mathf.Clamp01(bulk01), Mathf.Clamp01(soft01), Mathf.Clamp01(barrier01)); }

        public void SetGantryPosition(float actualXMeters, float actualZMeters, float actualGripperDrop01)
        {
            gantryOffset = new Vector3(Mathf.Clamp(actualXMeters, -7f, 7f), 0f,
                Mathf.Clamp(actualZMeters, -1.1f, 1.1f));
            gripperDrop01 = Mathf.Clamp01(actualGripperDrop01);
        }

        public void SetWaferHandlingPose(float actualRobotBaseDegrees, float actualRobotArmDegrees, float actualRobotToolDegrees)
        { robotAngles = new Vector3(actualRobotBaseDegrees, actualRobotArmDegrees, actualRobotToolDegrees); }

        public void SetActualProcessState(float bulkRemoval01, float softLandingClear01, float barrierClear01,
            float actualDishing01, float actualDielectricErosion01, bool actualPolishingAuthorized,
            bool actualEndpointMeasurementValid, bool actualCriticalFault)
        {
            actualStageRemoval01 = new Vector3(Mathf.Clamp01(bulkRemoval01),
                Mathf.Clamp01(softLandingClear01), Mathf.Clamp01(barrierClear01));
            dishing01 = Mathf.Clamp01(actualDishing01);
            erosion01 = Mathf.Clamp01(actualDielectricErosion01);
            polishingAuthorized = actualPolishingAuthorized;
            endpointMeasurementValid = actualEndpointMeasurementValid;
            criticalFault = actualCriticalFault;
        }

        public void SetFluidActuators(float actualBulkNozzle01, float actualSoftNozzle01, float actualBarrierNozzle01)
        {
            bulkNozzle01 = Mathf.Clamp01(actualBulkNozzle01);
            softNozzle01 = Mathf.Clamp01(actualSoftNozzle01);
            barrierNozzle01 = Mathf.Clamp01(actualBarrierNozzle01);
        }

        public void SetCleaningAndInspection(float actualCleaningProgress01, float actualDryingProgress01,
            float actualMetrologyProgress01, float actualOpticalConfidence01)
        {
            cleaning01 = Mathf.Clamp01(actualCleaningProgress01);
            drying01 = Mathf.Clamp01(actualDryingProgress01);
            metrology01 = new Vector3(Mathf.Clamp01(actualMetrologyProgress01), 0, 0);
            endpointConfidence01 = Mathf.Clamp01(actualOpticalConfidence01);
        }

        public void Apply(ProjectSpark.Gameplay.SparkLevel77Plant plant, int station, int destination, float transfer, bool polishing,
            bool optical, bool fault, int cleaning, float scan, bool emergency)
        {
            SetActualPlatenRPM(plant.Rpm[0], plant.Rpm[1], plant.Rpm[2]);
            SetActualCarrierRPM(plant.Carrier[0], plant.Carrier[1], plant.Carrier[2]);
            SetActualCarrierLift(plant.Lift[0], plant.Lift[1], plant.Lift[2]);
            float transit = Mathf.Clamp01(transfer / 1.45f);
            float reach = Mathf.Sin(transit * Mathf.PI);
            SetGantryPosition(Mathf.Lerp((station - 1) * 5.35f, (destination - 1) * 5.35f, transit), 0, reach);
            SetWaferHandlingPose(-32 * reach, 15 * reach, 24 * reach);
            SetActualProcessState((118 - plant.MeanCopper) / 98, 1 - plant.MeanCopper / 20,
                1 - plant.MeanBarrier / 12, plant.WorstDishing / 5, plant.WorstErosion / 4, polishing, optical, fault || emergency);
            SetFluidActuators(station == 0 && polishing ? plant.Flow[0] : 0,
                station == 1 && polishing ? plant.Flow[1] : 0, station == 2 && polishing ? plant.Flow[2] : 0);
            SetCleaningAndInspection(cleaning > 0 ? 1 - plant.Particles : 0,
                plant.Dryer / 490, scan / 1.25f, optical ? plant.Confidence : 0);
        }

        public void BindEffects(ParticleSystem bulk, ParticleSystem soft, ParticleSystem barrier, Light alarm, Light optics)
        {
            bulkSlurryVFX=bulk;
            softLandingFluidVFX=soft;
            barrierFluidVFX=barrier;
            criticalAlarmLight=alarm;
            opticalEndpointLight=optics;
        }

        private void Spin(string pivotName, float rpm, float deltaTime)
        {
            if (pivotLookup.TryGetValue(pivotName, out Transform tr))
                tr.Rotate(Vector3.up, rpm * 6f * deltaTime, Space.Self);
        }
        private void Shift(string pivotName, Vector3 offset)
        {
            if (pivotLookup.TryGetValue(pivotName, out Transform tr))
                tr.localPosition = initialPositions[pivotName] + (art != null ? tr.parent.InverseTransformVector(art.TransformVector(new Vector3(-offset.x,offset.y,-offset.z) * .096f)) : offset);
        }
        private void Rotate(string pivotName, Vector3 degrees)
        {
            if (pivotLookup.TryGetValue(pivotName, out Transform tr))
                tr.localRotation = initialRotations[pivotName] * Quaternion.Euler(degrees);
        }
        private static void RunVFX(ParticleSystem ps, bool active)
        {
            if (ps == null) return;
            if (active && !ps.isPlaying) ps.Play();
            else if (!active && ps.isPlaying)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            // Independent platen, pad and carrier pivot parents are in separate FBXs.
            Spin("ANIM_BULK_PLATEN_ROTATION", actualPlatenRPM.x, dt);
            Spin("ANIM_BULK_PAD_ROTATION", actualPlatenRPM.x, dt);
            Spin("ANIM_BULK_CARRIER_SPIN", actualCarrierRPM.x, dt);
            Spin("ANIM_SOFT_PLATEN_ROTATION", actualPlatenRPM.y, dt);
            Spin("ANIM_SOFT_PAD_ROTATION", actualPlatenRPM.y, dt);
            Spin("ANIM_SOFT_CARRIER_SPIN", actualCarrierRPM.y, dt);
            Spin("ANIM_BARRIER_PLATEN_ROTATION", actualPlatenRPM.z, dt);
            Spin("ANIM_BARRIER_PAD_ROTATION", actualPlatenRPM.z, dt);
            Spin("ANIM_BARRIER_CARRIER_SPIN", actualCarrierRPM.z, dt);

            Shift("ANIM_BULK_CARRIER_LIFT", Vector3.up * (actualCarrierLift01.x * .55f));
            Shift("ANIM_SOFT_CARRIER_LIFT", Vector3.up * (actualCarrierLift01.y * .55f));
            Shift("ANIM_BARRIER_CARRIER_LIFT", Vector3.up * (actualCarrierLift01.z * .55f));
            Shift("ANIM_SOFT_PRESSURE_ACTUATOR", Vector3.up * (actualCarrierLift01.y * .28f));
            Shift("ANIM_BULK_PRESSURE_RING", Vector3.up * (actualCarrierLift01.x * .18f));

            Shift("ANIM_TRANSFER_GANTRY_X", Vector3.right * gantryOffset.x);
            Shift("ANIM_TRANSFER_GANTRY_Z", Vector3.forward * gantryOffset.z);
            Shift("ANIM_TRANSFER_GRIPPER", Vector3.down * (gripperDrop01 * .45f));
            Rotate("ANIM_WAFER_ROBOT_BASE", Vector3.up * robotAngles.x);
            Rotate("ANIM_WAFER_ROBOT_ARM", Vector3.forward * robotAngles.y);
            Rotate("ANIM_WAFER_ROBOT_TOOL", Vector3.up * robotAngles.z);

            Shift("ANIM_BULK_SLURRY_NOZZLE", Vector3.forward * (bulkNozzle01 * .10f));
            Shift("ANIM_SOFT_PROCESS_FLUID_NOZZLE", Vector3.forward * (softNozzle01 * .10f));
            Shift("ANIM_BARRIER_SLURRY_NOZZLE", Vector3.forward * (barrierNozzle01 * .10f));
            Shift("ANIM_BARRIER_SPECTROMETER_HEAD", Vector3.up * (.10f * (1f - endpointConfidence01)));
            Shift("ANIM_DISHING_EROSION_SCANNER", Vector3.right * (metrology01.x * .22f));
            Shift("ANIM_COPPER_RESIDUE_CAMERA", Vector3.right * (metrology01.x * .25f));
            Shift("ANIM_INTERLINE_ISOLATION_PROBE", Vector3.down * (metrology01.x * .14f));
            Shift("ANIM_VIA_KELVIN_PROBE", Vector3.down * (metrology01.x * .14f));
            Spin("ANIM_BRUSH_CLEANER", cleaning01 * 140f, dt);
            Spin("ANIM_SPIN_DRYER", drying01 * 380f, dt);
            Rotate("ANIM_RINSE_VALVE", Vector3.up * (cleaning01 * 70f));

            // VFX must reflect the real controller state; never imply unsafe polishing is running.
            bool enable = polishingAuthorized && !criticalFault;
            RunVFX(bulkSlurryVFX, enable && bulkNozzle01 > .03f && actualPlatenRPM.x > .1f);
            RunVFX(softLandingFluidVFX, enable && softNozzle01 > .03f && actualPlatenRPM.y > .1f);
            RunVFX(barrierFluidVFX, enable && barrierNozzle01 > .03f && actualPlatenRPM.z > .1f);
            if (criticalAlarmLight != null) criticalAlarmLight.enabled = criticalFault;
            if (opticalEndpointLight != null)
            {
                opticalEndpointLight.enabled = endpointMeasurementValid && !criticalFault;
                opticalEndpointLight.intensity = endpointConfidence01 * 2.1f;
                opticalEndpointLight.color = Color.Lerp(new Color(.97f,.30f,.2f),
                    new Color(.54f,.27f,1f), endpointConfidence01);
            }
            // Actual wafer layer visual shaders should be driven separately from the single
            // authoritative wafer material map; these art pivotLookup are not its source of truth.
        }
    }
}



