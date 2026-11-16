#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 92 art structure validation only. Does not test electrical/runtime gameplay.</summary>
    public static class SparkLevel92AssetValidatorEditor
    {
        public static readonly string[] PivotNames = new string[]
        {
            "ANIM_AURORA_REMOTE_ORBIT",
            "ANIM_GEOLOGICAL_VECTOR_MAP",
            "ANIM_MISSION_CONTROL_DISPLAY",
            "ANIM_DEPLOYMENT_GANTRY_HOIST",
            "ANIM_TOMOGRAPHY_LAYER_DISPLAY",
            "ANIM_SAMPLING_SERVICE_GANTRY",
            "ANIM_FINAL_MISSION_BEACON",
            "ANIM_LANDER_BODY_LEVEL",
            "ANIM_LANDER_DEPLOYMENT_SLIDE",
            "ANIM_LANDER_AVIONICS_ACCESS",
            "ANIM_LANDER_LEG_A_HIP",
            "ANIM_LANDER_LEG_A_KNEE",
            "ANIM_LANDER_LEG_A_FOOT_ROTATOR",
            "ANIM_ANCHOR_A_MICROSPINE_DEPLOY",
            "ANIM_ANCHOR_A_MICROSPINE_LATCH",
            "ANIM_LANDER_LEG_B_HIP",
            "ANIM_LANDER_LEG_B_KNEE",
            "ANIM_LANDER_LEG_B_FOOT_ROTATOR",
            "ANIM_ANCHOR_B_MICROSPINE_DEPLOY",
            "ANIM_ANCHOR_B_MICROSPINE_LATCH",
            "ANIM_LANDER_LEG_C_HIP",
            "ANIM_LANDER_LEG_C_KNEE",
            "ANIM_LANDER_LEG_C_FOOT_ROTATOR",
            "ANIM_ANCHOR_C_MICROSPINE_DEPLOY",
            "ANIM_ANCHOR_C_MICROSPINE_LATCH",
            "ANIM_LANDER_LEG_D_HIP",
            "ANIM_LANDER_LEG_D_KNEE",
            "ANIM_LANDER_LEG_D_FOOT_ROTATOR",
            "ANIM_ANCHOR_D_MICROSPINE_DEPLOY",
            "ANIM_ANCHOR_D_MICROSPINE_LATCH",
            "ANIM_LANDER_LEG_E_HIP",
            "ANIM_LANDER_LEG_E_KNEE",
            "ANIM_LANDER_LEG_E_FOOT_ROTATOR",
            "ANIM_ANCHOR_E_MICROSPINE_DEPLOY",
            "ANIM_ANCHOR_E_MICROSPINE_LATCH",
            "ANIM_LANDER_LEG_F_HIP",
            "ANIM_LANDER_LEG_F_KNEE",
            "ANIM_LANDER_LEG_F_FOOT_ROTATOR",
            "ANIM_ANCHOR_F_MICROSPINE_DEPLOY",
            "ANIM_ANCHOR_F_MICROSPINE_LATCH",
            "ANIM_ANCHOR_FORCE_DISTRIBUTION",
            "ANIM_ANCHOR_LOAD_PLC",
            "ANIM_LANDER_ATTITUDE_TRIM",
            "ANIM_DRILL_SPINDLE",
            "ANIM_DRILL_SPINDLE_CLAMP",
            "ANIM_CORE_BIT",
            "ANIM_AXIAL_FEED",
            "ANIM_DRIVE_GEARBOX_SHAFT",
            "ANIM_PERCUSSIVE_STRIKER",
            "ANIM_DRILL_THERMAL_PROBE",
            "ANIM_MEASURED_TORQUE_NEEDLE",
            "ANIM_MEASURED_FEED_FORCE_NEEDLE",
            "ANIM_DRILL_DEPTH_ENCODER",
            "ANIM_DRILL_VIBRATION",
            "ANIM_DRIVE_POWER_STATUS",
            "ANIM_DRILL_FEED_STATUS",
            "ANIM_GUIDE_SLEEVE",
            "ANIM_CUTTINGS_GATE",
            "ANIM_JAM_RECOVERY_SLIDER",
            "ANIM_BIT_INSPECTION_CAMERA",
            "ANIM_CORE_RETENTION_SLEEVE",
            "ANIM_THERMAL_FAN",
            "ANIM_DRILL_TELEMETRY",
            "ANIM_RADAR_A_SCAN",
            "ANIM_RADAR_B_SCAN",
            "ANIM_LIDAR_SWEEP",
            "ANIM_POROSITY_PENETRATION",
            "ANIM_GEOPHONE_A_COUPLING",
            "ANIM_GEOPHONE_B_COUPLING",
            "ANIM_LAYER_FUSION_STATUS",
            "ANIM_GEOLOGICAL_COVARIANCE",
            "ANIM_COHESION_LOAD_HEAD",
            "ANIM_RADAR_REFERENCE",
            "ANIM_3D_STRATIGRAPHY",
            "ANIM_SAMPLE_CAPTURE_GRIPPER",
            "ANIM_CARTRIDGE_A",
            "ANIM_CARTRIDGE_B",
            "ANIM_CARTRIDGE_CAROUSEL",
            "ANIM_SEAL_COMPRESSION",
            "ANIM_LEAK_TEST_CHAMBER",
            "ANIM_CORE_MASS_INDICATOR",
            "ANIM_SAMPLE_TRANSFER_ARM",
            "ANIM_TETHER_WINCH_SPOOL",
            "ANIM_SAMPLE_AIRLOCK_HATCH",
            "ANIM_SAMPLING_SAFETY_PLC",
            "ANIM_GEOSCIENCE_SCADA",
            "ANIM_CRITICAL_ANCHOR_W171_PATCH",
            "ANIM_MISSION_CERTIFIED",
        };
        [MenuItem("Tools/Project Spark/Levels/92/Validate Imported HD Art")]
        public static void Validate()
        {
            int found=0;
            for(int i=0;i<SparkLevel92ArtPlacementEditor.ModelNames.Length;i++)
            {
                string path=SparkLevel92ArtPlacementEditor.Folder+i.ToString("00")+"_"+SparkLevel92ArtPlacementEditor.ModelNames[i]+".fbx";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)found++;
                else Debug.LogError("[Level92Validator] Missing "+path);
            }
            Transform root=null;
            if(Selection.activeGameObject!=null)
            {
                root=Selection.activeGameObject.transform;
                if(root.name!=SparkLevel92ArtPlacementEditor.VisualRoot)root=root.Find(SparkLevel92ArtPlacementEditor.VisualRoot);
            }
            if(root==null)
            {
                Debug.LogWarning("[Level92Validator] Select the visual root or a layout containing it. FBX assets imported: "+found+"/98.");
                return;
            }
            var pc=new Dictionary<string,int>(); var sockets=new HashSet<string>();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if(t.name.StartsWith("ANIM_")) {int old;pc.TryGetValue(t.name,out old);pc[t.name]=old+1;}
                if(t.name.StartsWith("SOCKET_P"))sockets.Add(t.name);
            }
            int pv=0;
            foreach(string name in PivotNames)
            {
                int count;
                if(pc.TryGetValue(name,out count) && count==1)pv++;
                else Debug.LogError("[Level92Validator] Pivot "+name+" expected one instance; found "+(pc.ContainsKey(name)?pc[name]:0));
            }
            if(sockets.Count!=266)Debug.LogWarning("[Level92Validator] DECORATIVE socket nodes "+sockets.Count+"/266. Functional SparkTerminals not included.");
            Debug.Log("[Level92Validator] FBX "+found+"/98; pivots "+pv+"/88; decorative sockets "+sockets.Count+"/266; MeshRenderers "+root.GetComponentsInChildren<MeshRenderer>(true).Length+". Unity compilation/Play Mode NOT VERIFIED by generator.");
        }
    }
}
#endif
