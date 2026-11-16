#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectSpark.EditorTools
{
    /// <summary>Level 88 art structure validation only. Does not test electrical/runtime gameplay.</summary>
    public static class SparkLevel88AssetValidatorEditor
    {
        public static readonly string[] PivotNames = new string[]
        {
            "ANIM_HABITAT_HOLOGRAM_RING",
            "ANIM_COMPARTMENT_AIR_QUALITY",
            "ANIM_AIRLOCK_DOOR",
            "ANIM_GAS_FLOW_HEADER",
            "ANIM_ADSORBER_A_CYCLE",
            "ANIM_ADSORBER_B_CYCLE",
            "ANIM_ADSORBER_A_INLET_VALVE",
            "ANIM_ADSORBER_B_INLET_VALVE",
            "ANIM_ADSORBENT_A_COLUMN_SCAN",
            "ANIM_ADSORBENT_B_COLUMN_SCAN",
            "ANIM_ADSORBER_A_HEATER",
            "ANIM_ADSORBER_B_HEATER",
            "ANIM_ADSORBER_SWITCHING_MANIFOLD",
            "ANIM_REGEN_EXHAUST_CONDENSER",
            "ANIM_CO2_CANISTER_VALVE",
            "ANIM_CO2_BREAKTHROUGH_FAULT",
            "ANIM_ADSORBER_CONTROL_STATUS",
            "ANIM_ELECTROLYSIS_BUBBLE_COLUMN",
            "ANIM_O2_CELL_A_SCAN",
            "ANIM_O2_CELL_B_SCAN",
            "ANIM_O2_SEPARATOR",
            "ANIM_BYPRODUCT_ISOLATION",
            "ANIM_O2_BUFFER_LEVEL",
            "ANIM_O2_FLOW_CONTROL",
            "ANIM_O2_PARTIAL_PRESSURE",
            "ANIM_O2_PURITY_MONITOR",
            "ANIM_O2_POWER_CONVERTER",
            "ANIM_O2_COOLING_PUMP",
            "ANIM_WATER_FEED_PUMP",
            "ANIM_O2_DELIVERY_REGULATOR",
            "ANIM_O2_BACKUP_ISOLATION",
            "ANIM_CELL_MONITOR",
            "ANIM_O2_PROCESS_PLC",
            "ANIM_CONDENSER_PROCESS",
            "ANIM_CONDENSER_COOLANT",
            "ANIM_CONDENSATE_SEPARATOR",
            "ANIM_CONDENSATE_TANK_LEVEL",
            "ANIM_RECOVERED_WATER_PUMP",
            "ANIM_WATER_QUALITY",
            "ANIM_FREEZE_FAULT",
            "ANIM_HUMIDITY_PLC",
            "ANIM_HUMIDITY_SENSOR",
            "ANIM_COOLING_VALVE",
            "ANIM_WATER_FILTER_DRAWER",
            "ANIM_WATER_BUDGET",
            "ANIM_PRIMARY_BLOWER_ROTOR",
            "ANIM_BACKUP_BLOWER_ROTOR",
            "ANIM_FAN_A_FREQUENCY",
            "ANIM_FAN_B_FREQUENCY",
            "ANIM_MAIN_DUCT_AIR_FLOW",
            "ANIM_DAMPER_A",
            "ANIM_DAMPER_B",
            "ANIM_DAMPER_C",
            "ANIM_DAMPER_D",
            "ANIM_ZONE_A_PRESSURE",
            "ANIM_ZONE_B_PRESSURE",
            "ANIM_ZONE_C_PRESSURE",
            "ANIM_ZONE_D_PRESSURE",
            "ANIM_TRUSTED_PRESSURE_MONITOR",
            "ANIM_INDEPENDENT_PRESSURE",
            "ANIM_LEAK_ISOLATION",
            "ANIM_PRESSURE_REGULATOR",
            "ANIM_MICROLEAK_FAULT",
            "ANIM_FILTER_EXTRACT",
            "ANIM_FLOW_MEASUREMENT",
            "ANIM_PRESSURE_RELIEF",
            "ANIM_LIFE_SUPPORT_SAFETY",
            "ANIM_MASTER_ECLSS_SCADA",
            "ANIM_ATMOS_FLOW_HOLOGRAM",
            "ANIM_W145_TRUSTED_PATCH",
            "ANIM_EMERGENCY_ISOLATION",
            "ANIM_FINAL_CERTIFICATION",
        };
        [MenuItem("Tools/Project Spark/Levels/88/Validate Imported HD Art")]
        public static void Validate()
        {
            int found=0;
            for(int i=0;i<SparkLevel88ArtPlacementEditor.ModelNames.Length;i++)
            {
                string path=SparkLevel88ArtPlacementEditor.Folder+i.ToString("00")+"_"+SparkLevel88ArtPlacementEditor.ModelNames[i]+".fbx";
                if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null)found++;
                else Debug.LogError("[Level88Validator] Missing "+path);
            }
            Transform root=null;
            if(Selection.activeGameObject!=null)
            {
                root=Selection.activeGameObject.transform;
                if(root.name!=SparkLevel88ArtPlacementEditor.VisualRoot)root=root.Find(SparkLevel88ArtPlacementEditor.VisualRoot);
            }
            if(root==null)
            {
                Debug.LogWarning("[Level88Validator] Select the visual root or a layout containing it. FBX assets imported: "+found+"/90.");
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
                else Debug.LogError("[Level88Validator] Pivot "+name+" expected one instance; found "+(pc.ContainsKey(name)?pc[name]:0));
            }
            if(sockets.Count!=242)Debug.LogWarning("[Level88Validator] DECORATIVE socket nodes "+sockets.Count+"/242. Functional SparkTerminals not included.");
            Debug.Log("[Level88Validator] FBX "+found+"/90; pivots "+pv+"/72; decorative sockets "+sockets.Count+"/242; MeshRenderers "+root.GetComponentsInChildren<MeshRenderer>(true).Length+". Unity compilation/Play Mode NOT VERIFIED by generator.");
        }
    }
}
#endif
