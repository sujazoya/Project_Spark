#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 89 art structure validation only. Does not test electrical/runtime gameplay.</summary>
    public static class SparkLevel89AssetValidatorEditor
    {
        public static readonly string[] PivotNames = new string[]
        {
            "ANIM_SHELTER_MAIN_SHIELD_DOOR",
            "ANIM_STORM_FORECAST_HOLOGRAM",
            "ANIM_EMERGENCY_EGRESS_HATCH",
            "ANIM_TELESCOPE_A_YAW",
            "ANIM_TELESCOPE_A_ELEVATION",
            "ANIM_TELESCOPE_B_YAW",
            "ANIM_TELESCOPE_B_ELEVATION",
            "ANIM_SPECTROMETER_DETECTOR_SCAN",
            "ANIM_PROTON_ENERGY_SELECTOR",
            "ANIM_HEAVY_ION_CHANNEL_SCAN",
            "ANIM_SUN_TRACKER_AZIMUTH",
            "ANIM_SUN_TRACKER_ELEVATION",
            "ANIM_PARTICLE_COLLIMATOR_APERTURE",
            "ANIM_PRIMARY_PARTICLE_PULSE",
            "ANIM_SECONDARY_PARTICLE_PULSE",
            "ANIM_DETECTOR_PULSE_CONDITIONING",
            "ANIM_SPECTRAL_CALIBRATION_LAMP",
            "ANIM_DETECTOR_THERMAL_SHROUD",
            "ANIM_STORM_FORECAST_CONSOLE",
            "ANIM_STORM_ARRIVAL_TIMELINE",
            "ANIM_EXTERNAL_DOSIMETER_SCAN",
            "ANIM_HABITAT_DOSIMETER_SCAN",
            "ANIM_SHELTER_REFERENCE_DOSE",
            "ANIM_INDEPENDENT_DOSE_MONITOR",
            "ANIM_TRIAXIAL_DOSE_PROBE",
            "ANIM_DOSE_TRANSIENT_RECORDER",
            "ANIM_DOSIMETER_CALIBRATION_FRAME",
            "ANIM_DETECTOR_SATURATION_TEST",
            "ANIM_ACCUMULATED_DOSE_DISPLAY",
            "ANIM_DETECTOR_NETWORK_GATEWAY",
            "ANIM_RADIATION_HEATMAP_HOLOGRAM",
            "ANIM_DETECTOR_COMPARATOR",
            "ANIM_DOSE_LIMIT_EVALUATOR",
            "ANIM_WATERWALL_PORT_A_LEVEL",
            "ANIM_WATERWALL_PORT_B_LEVEL",
            "ANIM_WATERWALL_STARBOARD_A_LEVEL",
            "ANIM_WATERWALL_STARBOARD_B_LEVEL",
            "ANIM_MAIN_WATER_TANK_LEVEL",
            "ANIM_SHIELD_WATER_SENSOR_SCAN",
            "ANIM_WATER_PUMP_A_ROTOR",
            "ANIM_WATER_PUMP_B_ROTOR",
            "ANIM_SHIELD_WATER_DISTRIBUTOR",
            "ANIM_SHIELD_MASS_ALLOCATION",
            "ANIM_AREAL_DENSITY_HEATMAP",
            "ANIM_WATER_RESOURCE_GUARD",
            "ANIM_WATER_WALL_LEAK_MONITOR",
            "ANIM_SHIELD_CASSETTE_TOP",
            "ANIM_SHIELD_CASSETTE_BOTTOM",
            "ANIM_SHIELD_CASSETTE_PORT",
            "ANIM_SHIELD_CASSETTE_STARBOARD",
            "ANIM_SHIELD_CARRIAGE_LOCK",
            "ANIM_SHIELD_ACTUATOR_A",
            "ANIM_SHIELD_ACTUATOR_B",
            "ANIM_SHIELD_ACTUATOR_C",
            "ANIM_SHIELD_ACTUATOR_D",
            "ANIM_LOCKING_PIN_A",
            "ANIM_LOCKING_PIN_B",
            "ANIM_SHIELD_POSITION_FEEDBACK",
            "ANIM_SHIELD_MOTOR_DRIVE",
            "ANIM_SHIELD_JAM_FAULT",
            "ANIM_SHELTER_DOOR_INTERLOCK",
            "ANIM_SHELTER_SHIELD_QUALITY",
            "ANIM_SHIELD_COVERAGE_TWIN",
            "ANIM_SHELTER_AIR_BLOWER",
            "ANIM_OXYGEN_BUFFER_STATUS",
            "ANIM_THERMAL_RECLAIMER_VALVE",
            "ANIM_SHELTER_LIFE_SUPPORT_PLC",
            "ANIM_SIMULATED_SOLAR_STORM_INTENSITY",
            "ANIM_ENERGY_SURGE_FAULT",
            "ANIM_EMERGENCY_ISOLATION_LEVER",
            "ANIM_RADIATION_SAFETY_PLC",
            "ANIM_RADIATION_DEFENSE_SCADA",
            "ANIM_SOLAR_STORM_3D_HOLOGRAM",
            "ANIM_W149_CRITICAL_FEEDBACK_PATCH",
            "ANIM_FINAL_CERTIFICATION",
            "ANIM_SURVEY_ANTENNA_AZIMUTH",
        };
        [MenuItem("Tools/Project Spark/Levels/89/Validate Imported HD Art")]
        public static void Validate()
        {
            int found=0;
            for(int i=0;i<SparkLevel89ArtPlacementEditor.ModelNames.Length;i++)
            {
                string path=SparkLevel89ArtPlacementEditor.Folder+i.ToString("00")+"_"+SparkLevel89ArtPlacementEditor.ModelNames[i]+".fbx";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)found++;
                else Debug.LogError("[Level89Validator] Missing "+path);
            }
            Transform root=null;
            if(Selection.activeGameObject!=null)
            {
                root=Selection.activeGameObject.transform;
                if(root.name!=SparkLevel89ArtPlacementEditor.VisualRoot)root=root.Find(SparkLevel89ArtPlacementEditor.VisualRoot);
            }
            if(root==null)
            {
                Debug.LogWarning("[Level89Validator] Select the visual root or a layout containing it. FBX assets imported: "+found+"/92.");
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
                else Debug.LogError("[Level89Validator] Pivot "+name+" expected one instance; found "+(pc.ContainsKey(name)?pc[name]:0));
            }
            if(sockets.Count!=248)Debug.LogWarning("[Level89Validator] DECORATIVE socket nodes "+sockets.Count+"/248. Functional SparkTerminals not included.");
            Debug.Log("[Level89Validator] FBX "+found+"/92; pivots "+pv+"/76; decorative sockets "+sockets.Count+"/248; MeshRenderers "+root.GetComponentsInChildren<MeshRenderer>(true).Length+". Unity compilation/Play Mode NOT VERIFIED by generator.");
        }
    }
}
#endif
