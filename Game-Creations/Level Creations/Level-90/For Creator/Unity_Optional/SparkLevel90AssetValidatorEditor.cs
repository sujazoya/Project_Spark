#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 90 art structure validation only. Does not test electrical/runtime gameplay.</summary>
    public static class SparkLevel90AssetValidatorEditor
    {
        public static readonly string[] PivotNames = new string[]
        {
            "ANIM_VEHICLE_CLAMP_RELEASE",
            "ANIM_TRAJECTORY_VECTOR",
            "ANIM_TEST_CHAMBER_BAFFLE",
            "ANIM_INSPECTION_GANTRY_X",
            "ANIM_INSPECTION_GANTRY_Z",
            "ANIM_MISSION_HOLOGRAM",
            "ANIM_DOCKING_RELEASE_RING",
            "ANIM_INTERLOCK_GATE",
            "ANIM_ORBITAL_LIGHTING_SWEEP",
            "ANIM_FINAL_CERTIFICATION",
            "ANIM_SPACECRAFT_INTEGRATION_ROTATION",
            "ANIM_NOSE_SERVICE_HATCH",
            "ANIM_SERVICE_RING",
            "ANIM_PORT_SOLAR_WING",
            "ANIM_PORT_SOLAR_OUTER",
            "ANIM_STARBOARD_SOLAR_WING",
            "ANIM_STARBOARD_SOLAR_OUTER",
            "ANIM_PORT_SOLAR_HINGE",
            "ANIM_STARBOARD_SOLAR_HINGE",
            "ANIM_RADIATOR_PORT_EXTEND",
            "ANIM_RADIATOR_STARBOARD_EXTEND",
            "ANIM_NAVIGATION_HEAD_PAN",
            "ANIM_ENGINE_A_GIMBAL_YAW",
            "ANIM_ENGINE_A_GIMBAL_PITCH",
            "ANIM_ENGINE_A_PLASMA_DISCHARGE",
            "ANIM_ENGINE_B_GIMBAL_YAW",
            "ANIM_ENGINE_B_GIMBAL_PITCH",
            "ANIM_ENGINE_B_PLASMA_DISCHARGE",
            "ANIM_ENGINE_C_GIMBAL_YAW",
            "ANIM_ENGINE_C_GIMBAL_PITCH",
            "ANIM_ENGINE_C_PLASMA_DISCHARGE",
            "ANIM_ENGINE_D_GIMBAL_YAW",
            "ANIM_ENGINE_D_GIMBAL_PITCH",
            "ANIM_ENGINE_D_PLASMA_DISCHARGE",
            "ANIM_DISCHARGE_CUTAWAY_ROTATE",
            "ANIM_ANODE_DISTRIBUTOR_VALVE",
            "ANIM_MAGNET_COIL_A_FIELD",
            "ANIM_MAGNET_COIL_B_FIELD",
            "ANIM_MAGNET_COIL_C_FIELD",
            "ANIM_MAGNET_COIL_D_FIELD",
            "ANIM_CATHODE_A_STATUS",
            "ANIM_CATHODE_B_STATUS",
            "ANIM_CATHODE_C_STATUS",
            "ANIM_CATHODE_D_STATUS",
            "ANIM_GIMBAL_CALIBRATION_YAW",
            "ANIM_THRUST_CALIBRATION_SLIDE",
            "ANIM_XENON_TANK_A_GAUGE",
            "ANIM_XENON_TANK_B_GAUGE",
            "ANIM_ISOLATION_VALVE_A",
            "ANIM_ISOLATION_VALVE_B",
            "ANIM_REGULATOR_A_DIAL",
            "ANIM_REGULATOR_B_DIAL",
            "ANIM_XENON_MANIFOLD_SELECTOR",
            "ANIM_FLOW_A_DIAL",
            "ANIM_FLOW_B_DIAL",
            "ANIM_FLOW_C_DIAL",
            "ANIM_FLOW_D_DIAL",
            "ANIM_PPU_A_OUTPUT_INDICATOR",
            "ANIM_PPU_B_OUTPUT_INDICATOR",
            "ANIM_PPU_C_OUTPUT_INDICATOR",
            "ANIM_PPU_D_OUTPUT_INDICATOR",
            "ANIM_PPU_PRECHARGE_CONTACTOR",
            "ANIM_MAGNET_CURRENT_DIAL",
            "ANIM_IGNITION_SEQUENCER",
            "ANIM_NEUTRALIZER_CURRENT_MONITOR",
            "ANIM_PROPULSION_SAFETY_STATUS",
            "ANIM_THERMAL_PUMP_ROTOR",
            "ANIM_THERMAL_RADIATOR_POSITION",
            "ANIM_SPECTROMETER_SCAN_HEAD",
            "ANIM_FARADAY_PROBE_TRANSLATION",
            "ANIM_THRUST_BALANCE_FLEXURE",
            "ANIM_TRAJECTORY_GUIDANCE_DISPLAY",
            "ANIM_AUTONOMOUS_DEPARTURE_STATUS",
            "ANIM_STAR_TRACKER_CALIBRATION",
            "ANIM_XENON_FAULT_VALVE",
            "ANIM_MAGNET_THERMAL_FAULT",
            "ANIM_VECTOR_MISALIGNMENT_FAULT",
            "ANIM_PROPULSION_SCADA",
            "ANIM_PLASMA_VECTOR_HOLOGRAM",
            "ANIM_W157_CRITICAL_PATCH",
        };
        [MenuItem("Tools/Project Spark/Levels/90/Validate Imported HD Art")]
        public static void Validate()
        {
            int found=0;
            for(int i=0;i<SparkLevel90ArtPlacementEditor.ModelNames.Length;i++)
            {
                string path=SparkLevel90ArtPlacementEditor.Folder+i.ToString("00")+"_"+SparkLevel90ArtPlacementEditor.ModelNames[i]+".fbx";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)found++;
                else Debug.LogError("[Level90Validator] Missing "+path);
            }
            Transform root=null;
            if(Selection.activeGameObject!=null)
            {
                root=Selection.activeGameObject.transform;
                if(root.name!=SparkLevel90ArtPlacementEditor.VisualRoot)root=root.Find(SparkLevel90ArtPlacementEditor.VisualRoot);
            }
            if(root==null)
            {
                Debug.LogWarning("[Level90Validator] Select the visual root or a layout containing it. FBX assets imported: "+found+"/94.");
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
                else Debug.LogError("[Level90Validator] Pivot "+name+" expected one instance; found "+(pc.ContainsKey(name)?pc[name]:0));
            }
            if(sockets.Count!=254)Debug.LogWarning("[Level90Validator] DECORATIVE socket nodes "+sockets.Count+"/254. Functional SparkTerminals not included.");
            Debug.Log("[Level90Validator] FBX "+found+"/94; pivots "+pv+"/80; decorative sockets "+sockets.Count+"/254; MeshRenderers "+root.GetComponentsInChildren<MeshRenderer>(true).Length+". Unity compilation/Play Mode NOT VERIFIED by generator.");
        }
    }
}
#endif
