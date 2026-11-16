#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 91 art structure validation only. Does not test electrical/runtime gameplay.</summary>
    public static class SparkLevel91AssetValidatorEditor
    {
        public static readonly string[] PivotNames = new string[]
        {
            "ANIM_AURORA_ATTITUDE",
            "ANIM_PULSAR_VECTOR_MAP",
            "ANIM_CELESTIAL_REFERENCE_MATRIX",
            "ANIM_MISSION_CONSOLE",
            "ANIM_TELESCOPE_SERVICE_LOCK",
            "ANIM_PULSAR_PHASE_DISPLAY",
            "ANIM_FINAL_MISSION_BEACON",
            "ANIM_XRAY_TELESCOPE_A_AZIMUTH",
            "ANIM_XRAY_TELESCOPE_A_SUNSHADE",
            "ANIM_XRAY_GIMBAL_A_ELEVATION",
            "ANIM_XRAY_GIMBAL_A_ROTATOR",
            "ANIM_XRAY_CONCENTRATOR_A_CALIBRATION",
            "ANIM_PHOTON_DETECTOR_A_TIMING",
            "ANIM_XRAY_TELESCOPE_B_AZIMUTH",
            "ANIM_XRAY_TELESCOPE_B_SUNSHADE",
            "ANIM_XRAY_GIMBAL_B_ELEVATION",
            "ANIM_XRAY_GIMBAL_B_ROTATOR",
            "ANIM_XRAY_CONCENTRATOR_B_CALIBRATION",
            "ANIM_PHOTON_DETECTOR_B_TIMING",
            "ANIM_XRAY_TELESCOPE_C_AZIMUTH",
            "ANIM_XRAY_TELESCOPE_C_SUNSHADE",
            "ANIM_XRAY_GIMBAL_C_ELEVATION",
            "ANIM_XRAY_GIMBAL_C_ROTATOR",
            "ANIM_XRAY_CONCENTRATOR_C_CALIBRATION",
            "ANIM_PHOTON_DETECTOR_C_TIMING",
            "ANIM_XRAY_TELESCOPE_D_AZIMUTH",
            "ANIM_XRAY_TELESCOPE_D_SUNSHADE",
            "ANIM_XRAY_GIMBAL_D_ELEVATION",
            "ANIM_XRAY_GIMBAL_D_ROTATOR",
            "ANIM_XRAY_CONCENTRATOR_D_CALIBRATION",
            "ANIM_PHOTON_DETECTOR_D_TIMING",
            "ANIM_PRIMARY_CLOCK_CESIUM",
            "ANIM_PRIMARY_CLOCK_LOCK",
            "ANIM_REDUNDANT_CLOCK_CESIUM",
            "ANIM_REDUNDANT_CLOCK_LOCK",
            "ANIM_ULTRA_STABLE_OSCILLATOR",
            "ANIM_TIME_DISTRIBUTION",
            "ANIM_PRIMARY_TIME_TAGGER",
            "ANIM_SECONDARY_TIME_TAGGER",
            "ANIM_EVENT_BUFFER_A",
            "ANIM_EVENT_BUFFER_B",
            "ANIM_EVENT_BUFFER_C",
            "ANIM_EVENT_BUFFER_D",
            "ANIM_CLOCK_COMPARISON",
            "ANIM_CLOCK_COOLING_FAN",
            "ANIM_CLOCK_ISOLATOR_Z",
            "ANIM_CLOCK_BIAS_METER",
            "ANIM_REFERENCE_SELECTOR",
            "ANIM_PHASE_SCOPE",
            "ANIM_FREQUENCY_METER",
            "ANIM_CALIBRATION_PULSE",
            "ANIM_EVENTPROCESSORA_LIVE",
            "ANIM_EVENTPROCESSORB_LIVE",
            "ANIM_EVENTPROCESSORC_LIVE",
            "ANIM_EVENTPROCESSORD_LIVE",
            "ANIM_EVENTCOINCIDENCEFILTER_LIVE",
            "ANIM_BACKGROUNDREJECTIONCOMPUTER_LIVE",
            "ANIM_PULSEPROFILEFOLDINGENGINE_LIVE",
            "ANIM_PULSARCATALOGSTORAGEARRAY_LIVE",
            "ANIM_EPHEMERISINTEGRITYVALIDATOR_LIVE",
            "ANIM_CELESTIALTIMEBARYCENTERCOMPUTER_LIVE",
            "ANIM_PULSARCORRELATIONPROCESSOR_LIVE",
            "ANIM_TIMINGRESIDUALANALYZER_LIVE",
            "ANIM_NAVIGATIONMEASUREMENTFUSIONUNIT_LIVE",
            "ANIM_NAVIGATIONCOVARIANCEESTIMATOR_LIVE",
            "ANIM_POSITIONVELOCITYSTATEESTIMATOR_LIVE",
            "ANIM_FOURPULSARGEOMETRYANALYZER_LIVE",
            "ANIM_PULSEAMBIGUITYRESOLVER_LIVE",
            "ANIM_NAVIGATIONUNCERTAINTYHOLOGRAM_LIVE",
            "ANIM_POINTING_CONTROLLER",
            "ANIM_GUIDANCE_COMPUTER",
            "ANIM_COURSE_CORRECTION",
            "ANIM_PROPULSION_COMMAND",
            "ANIM_THRUSTER_LINK",
            "ANIM_FLIGHT_SAFE",
            "ANIM_CRITICAL_TIME_VALIDITY",
            "ANIM_NAV_FAULT_RECORDER",
            "ANIM_CONTROL_POWER",
            "ANIM_NAV_SCADA",
            "ANIM_OBJECTIVE_6_CERTIFIED",
            "ANIM_COSMIC_PARTICLE_FAULT",
            "ANIM_CLOCK_THERMAL_DRIFT",
            "ANIM_PULSAR_EPHEMERIS_FAULT",
            "ANIM_GIMBAL_JAM_TEST",
        };
        [MenuItem("Tools/Project Spark/Levels/90/Validate Imported HD Art")]
        public static void Validate()
        {
            int found=0;
            for(int i=0;i<SparkLevel91ArtPlacementEditor.ModelNames.Length;i++)
            {
                string path=SparkLevel91ArtPlacementEditor.Folder+i.ToString("00")+"_"+SparkLevel91ArtPlacementEditor.ModelNames[i]+".fbx";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)found++;
                else Debug.LogError("[Level91Validator] Missing "+path);
            }
            Transform root=null;
            if(Selection.activeGameObject!=null)
            {
                root=Selection.activeGameObject.transform;
                if(root.name!=SparkLevel91ArtPlacementEditor.VisualRoot)root=root.Find(SparkLevel91ArtPlacementEditor.VisualRoot);
            }
            if(root==null)
            {
                Debug.LogWarning("[Level91Validator] Select the visual root or a layout containing it. FBX assets imported: "+found+"/96.");
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
                else Debug.LogError("[Level91Validator] Pivot "+name+" expected one instance; found "+(pc.ContainsKey(name)?pc[name]:0));
            }
            if(sockets.Count!=260)Debug.LogWarning("[Level91Validator] DECORATIVE socket nodes "+sockets.Count+"/260. Functional SparkTerminals not included.");
            Debug.Log("[Level91Validator] FBX "+found+"/96; pivots "+pv+"/84; decorative sockets "+sockets.Count+"/260; MeshRenderers "+root.GetComponentsInChildren<MeshRenderer>(true).Length+". Unity compilation/Play Mode NOT VERIFIED by generator.");
        }
    }
}
#endif
