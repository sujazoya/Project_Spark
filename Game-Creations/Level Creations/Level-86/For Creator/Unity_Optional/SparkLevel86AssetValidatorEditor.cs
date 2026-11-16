#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ProjectSpark.EditorTools
{
    /// <summary>Art structure validator. NOT proof of runtime, wiring, or gameplay correctness.</summary>
    public static class SparkLevel86AssetValidatorEditor
    {
        public static readonly string[] Pivots = new string[]
        {
            "ANIM_CARGO_HATCH",
            "ANIM_LINEAR_RAIL_CARRIAGE",
            "ANIM_ROBOT_BASE_ROTATE",
            "ANIM_ROBOT_SHOULDER_PITCH",
            "ANIM_ROBOT_UPPERARM_PITCH",
            "ANIM_ROBOT_ELBOW_PITCH",
            "ANIM_ROBOT_FOREARM_ROLL",
            "ANIM_ROBOT_WRIST_PITCH",
            "ANIM_ROBOT_WRIST_ROLL",
            "ANIM_COUNTERBALANCE_SLIDER",
            "ANIM_SERVO_DRIVE_BANK",
            "ANIM_ENCODER_SCAN",
            "ANIM_MOTOR_TORQUE_GAUGE",
            "ANIM_JOINT_BRAKE_ACTUATOR",
            "ANIM_REACH_ENVELOPE",
            "ANIM_HOMING_PROBE",
            "ANIM_STEREO_LEFT_PAN",
            "ANIM_STEREO_RIGHT_PAN",
            "ANIM_DEPTH_SCANNER",
            "ANIM_ALIGNMENT_LASER",
            "ANIM_WRIST_FORCE_SENSOR",
            "ANIM_COMPLIANT_INSERTION",
            "ANIM_CONTACT_WRENCH_VECTOR",
            "ANIM_COLLISION_SENSOR_SWEEP",
            "ANIM_FLEX_MODAL_RESPONSE",
            "ANIM_ACTIVE_DAMPING",
            "ANIM_GRIPPER_JAW_LEFT",
            "ANIM_GRIPPER_JAW_RIGHT",
            "ANIM_TOOL_CHANGER_ROTARY",
            "ANIM_FASTENING_DRIVER_SPINDLE",
            "ANIM_FASTENER_CASSETTE",
            "ANIM_INSPECTION_PROBE",
            "ANIM_TOOL_DOCK_INDEXER",
            "ANIM_GRIP_CONTACT_SCAN",
            "ANIM_PAYLOAD_GRAPPLE_LOCK",
            "ANIM_FAILED_RADIATOR_RELEASE",
            "ANIM_REPLACEMENT_RADIATOR_LIFT",
            "ANIM_RADIATOR_TRUSS_JOINT",
            "ANIM_THERMAL_PLATE_INTERFACE",
            "ANIM_COOLANT_CONNECTOR_A",
            "ANIM_COOLANT_CONNECTOR_B",
            "ANIM_COOLANT_VALVE",
            "ANIM_COOLANT_PRESSURE_INDICATOR",
            "ANIM_COOLANT_FLOW_INDICATOR",
            "ANIM_RADIATOR_SENSOR_GRID",
            "ANIM_AVIONICS_HEAT_LOAD",
            "ANIM_PUMP_ROTOR",
            "ANIM_LEAK_TEST_DIAGNOSTIC",
            "ANIM_COOLANT_ISOLATION_VALVE",
            "ANIM_FASTENER_MOTOR_BANK",
            "ANIM_THERMAL_TREND",
            "ANIM_DISPOSAL_CRADLE_LATCH",
            "ANIM_PAYLOAD_OSCILLATION_FAULT",
            "ANIM_ENCODER_DRIFT_FAULT",
            "ANIM_GRIPPER_ASYMMETRY_FAULT",
            "ANIM_CONTACT_SAFETY_PLC",
            "ANIM_EMERGENCY_ROBOT_STOP",
            "ANIM_INDEPENDENT_CONTACT_RELAY",
            "ANIM_VISION_FUSION",
            "ANIM_SERVICE_SEQUENCE_PLC",
            "ANIM_ROBOTICS_SCADA",
            "ANIM_W129_TRUSTED_FORCE_FEEDBACK",
            "ANIM_FINAL_SERVICING_CERTIFICATION",
            "ANIM_INSPECTION_CAMERA_GANTRY",
        };
        [MenuItem("Tools/Project Spark/Levels/86/Validate Imported HD Art")]
        public static void Validate()
        {
            int found = 0;
            for (int i = 0; i < SparkLevel86ArtPlacementEditor.ModelNames.Length; i++)
            {
                string path = SparkLevel86ArtPlacementEditor.Folder + i.ToString("00") + "_" +
                    SparkLevel86ArtPlacementEditor.ModelNames[i] + ".fbx";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) found++;
                else Debug.LogError("[Level86Validator] MISSING " + path);
            }
            Transform root = null;
            if (Selection.activeGameObject != null)
            {
                root = Selection.activeGameObject.transform;
                if (root.name != SparkLevel86ArtPlacementEditor.VisualRoot)
                    root = root.Find(SparkLevel86ArtPlacementEditor.VisualRoot);
            }
            if (root == null)
            {
                Debug.LogWarning("[Level86Validator] 3D art root not selected. Checked FBX imports only: " + found + "/82.");
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
                else Debug.LogError("[Level86Validator] Expected exactly one " + p + " but got " + (pivotCounts.ContainsKey(p) ? pivotCounts[p] : 0));
            }
            if (socketSet.Count != 230) Debug.LogWarning("[Level86Validator] Found " + socketSet.Count + "/230 DECORATIVE sockets. None are functional SparkTerminals.");
            Debug.Log("[Level86Validator] FBX: " + found + "/82, pivots: " + properPivots + "/64, decorative socket ids: " + socketSet.Count + "/230, mesh renderers: " + rendererCount + ". Unity gameplay/Play Mode: UNTESTED.");
        }
    }
}
#endif
