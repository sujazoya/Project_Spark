#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art structure validator. NOT proof of runtime, wiring, or gameplay correctness.</summary>
    public static class SparkLevel87AssetValidatorEditor
    {
        public static readonly string[] Pivots = new string[]
        {
            "ANIM_GRAVITY_RING_MAIN_ROTATION",
            "ANIM_GRAVITY_RING_SPOKES",
            "ANIM_HABITAT_SEGMENT_A",
            "ANIM_HABITAT_SEGMENT_B",
            "ANIM_HABITAT_SEGMENT_C",
            "ANIM_HABITAT_SEGMENT_D",
            "ANIM_HABITAT_SEGMENT_E",
            "ANIM_HABITAT_SEGMENT_F",
            "ANIM_HABITAT_PRESSURE_SHELL",
            "ANIM_STATION_HUB_FLYWHEEL",
            "ANIM_BEARING_A_FIELD",
            "ANIM_BEARING_B_FIELD",
            "ANIM_BEARING_C_FIELD",
            "ANIM_BEARING_D_FIELD",
            "ANIM_BEARING_E_FIELD",
            "ANIM_BEARING_F_FIELD",
            "ANIM_BEARING_G_FIELD",
            "ANIM_BEARING_H_FIELD",
            "ANIM_PRIMARY_RADIAL_SENSOR",
            "ANIM_SECONDARY_RADIAL_SENSOR",
            "ANIM_AXIAL_SENSOR",
            "ANIM_MAGNETIC_FIELD_LINES",
            "ANIM_EMERGENCY_TOUCHDOWN_RACE",
            "ANIM_BEARING_AMPLIFIER_COOLING",
            "ANIM_SEGMENTED_RIM_DRIVE",
            "ANIM_ROTOR_MAGNET_ARRAY",
            "ANIM_DRIVE_A_INVERTER",
            "ANIM_DRIVE_B_INVERTER",
            "ANIM_TORQUE_ESTIMATOR",
            "ANIM_SPEED_ENCODER",
            "ANIM_SAFE_SPINUP_PROFILE",
            "ANIM_MOTOR_PRECHARGE_CONTACTOR",
            "ANIM_DRIVE_COOLING_IMPELLER",
            "ANIM_COUNTERTORQUE_FLYWHEEL",
            "ANIM_VIBRATION_PROBE_AXES",
            "ANIM_SPECTRAL_ANALYZER",
            "ANIM_SHAFT_ORBIT_PROBE",
            "ANIM_IMBALANCE_VECTOR",
            "ANIM_ADAPTIVE_DAMPING",
            "ANIM_BALANCE_WEIGHT_A",
            "ANIM_BALANCE_WEIGHT_B",
            "ANIM_MASS_SCAN",
            "ANIM_RESONANCE_SWEEP",
            "ANIM_BEARING_TEMPERATURE",
            "ANIM_VIBRATION_SCADA",
            "ANIM_FAULT_CALIBRATION",
            "ANIM_PRIMARY_POWER_COUPLER",
            "ANIM_BACKUP_POWER_COUPLER",
            "ANIM_ROTARY_OPTICAL_LINK",
            "ANIM_ROTARY_POWER_CONTACTOR",
            "ANIM_HABITAT_LOAD_A",
            "ANIM_HABITAT_EMERGENCY_LIGHT",
            "ANIM_HABITAT_THERMAL_LOAD",
            "ANIM_ROTARY_DATA_QUALITY",
            "ANIM_ROTARY_POWER_MONITOR",
            "ANIM_LOAD_DISTURBANCE_SLIDER",
            "ANIM_ARTIFICIAL_GRAVITY_HOLOGRAM",
            "ANIM_POWER_BUDGET",
            "ANIM_SAFETY_SUPERVISOR",
            "ANIM_TRUSTED_RADIAL_SAFETY",
            "ANIM_INDEPENDENT_OVERSPEED",
            "ANIM_EMERGENCY_STOP",
            "ANIM_GAP_SENSOR_DRIFT_FAULT",
            "ANIM_AMP_OVERHEAT_FAULT",
            "ANIM_MASS_IMBALANCE_FAULT",
            "ANIM_W137_CRITICAL_FEEDBACK",
            "ANIM_GRAVITY_MASTER_SCADA",
            "ANIM_FINAL_CERTIFICATION",
        };
        [MenuItem("Tools/Project Spark/Levels/87/Validate Imported HD Art")]
        public static void Validate()
        {
            int found = 0;
            for (int i = 0; i < SparkLevel87ArtPlacementEditor.ModelNames.Length; i++)
            {
                string path = SparkLevel87ArtPlacementEditor.Folder + i.ToString("00") + "_" +
                    SparkLevel87ArtPlacementEditor.ModelNames[i] + ".fbx";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) found++;
                else Debug.LogError("[Level87Validator] MISSING " + path);
            }
            Transform root = null;
            if (Selection.activeGameObject != null)
            {
                root = Selection.activeGameObject.transform;
                if (root.name != SparkLevel87ArtPlacementEditor.VisualRoot)
                    root = root.Find(SparkLevel87ArtPlacementEditor.VisualRoot);
            }
            if (root == null)
            {
                Debug.LogWarning("[Level87Validator] 3D art root not selected. Checked FBX imports only: " + found + "/86.");
                return;
            }
            var pivotCounts = new Dictionary<string,int>();
            var socketSet = new HashSet<string>();
            int rendererCount = 0;
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("ANIM_"))
                {
                    int old;
                    pivotCounts.TryGetValue(t.name, out old);
                    pivotCounts[t.name] = old + 1;
                }
                if (t.name.StartsWith("SOCKET_P")) socketSet.Add(t.name);
            }
            rendererCount = root.GetComponentsInChildren<MeshRenderer>(true).Length;
            int properPivots = 0;
            foreach (string p in Pivots)
            {
                int count;
                if (pivotCounts.TryGetValue(p, out count) && count == 1) properPivots++;
                else Debug.LogError("[Level87Validator] Expected exactly one " + p + " but got " + (pivotCounts.ContainsKey(p) ? pivotCounts[p] : 0));
            }
            if (socketSet.Count != 236) Debug.LogWarning("[Level87Validator] Found " + socketSet.Count + "/236 DECORATIVE sockets. None are functional SparkTerminals.");
            Debug.Log("[Level87Validator] FBX: " + found + "/86, pivots: " + properPivots + "/68, decorative socket ids: " + socketSet.Count + "/236, mesh renderers: " + rendererCount + ". Unity gameplay/Play Mode: UNTESTED.");
        }
    }
}
#endif
