#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level101.EditorTools
{
    public static class SparkLevel101PlanningDataEditor
    {
        private static readonly string Out="Assets/ProjectSpark/Level101/Data/L94_PCBDefinition.asset";
        private static TextAsset Find(string filename)
        {
            foreach(var guid in AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename)))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(Path.GetFileName(path).Equals(filename,StringComparison.OrdinalIgnoreCase) && path.Contains("Level101"))
                    return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }
            return null;
        }
        [MenuItem("Tools/Project Spark/Levels/101/4. Create PCB Planning Data Asset")]
        public static void Create()
        {
            SparkLevel101PBRMaterialEditor.EnsureFolder("Assets/ProjectSpark/Level101/Data");
            var asset=AssetDatabase.LoadAssetAtPath<ProjectSpark.Level101.SparkLevel101PCBDefinition>(Out);
            if(asset==null){asset=ScriptableObject.CreateInstance<ProjectSpark.Level101.SparkLevel101PCBDefinition>();AssetDatabase.CreateAsset(asset,Out);}
            asset.componentBOM=Find("Component_BOM_27.csv");
            asset.copperTraceTasks=Find("Copper_Trace_Tasks_W001_W034.csv");
            asset.viaTasks=Find("Required_Via_Placements_V01_V06.csv");
            asset.logicalPadAnchors=Find("Logical_Pad_Locations.csv");
            asset.factoryNetlist=Find("Factory_Existing_Nets.csv");
            asset.w035CriticalJumper=Find("Critical_W035_Feedback.csv");
            asset.objectiveRequirements=Find("Objective_Certification_Plan_6.csv");
            asset.faultScenarios=Find("Three_Fault_Repair_Scenarios.csv");
            asset.testPlan=Find("Engineering_Acceptance_Tests_56.csv");
            EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();Selection.activeObject=asset;
            Debug.Log("[Level101] Created PCB source-data asset. CSVs describe intended component/trace tasks, not live gameplay connections.");
        }
    }
}
#endif

