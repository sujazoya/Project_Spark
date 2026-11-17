#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ProjectSpark.Level94.EditorTools
{
    public static class SparkLevel94ArtValidator
    {
        [MenuItem("Tools/Project Spark/Levels/94/5. Validate Imported Assets And Plans")]
        public static void Validate()
        {
            int modelCount=0,invalid=0;
            foreach(string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                if(!Path.GetFileNameWithoutExtension(path).Contains("_L94_"))continue;
                if(path.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))modelCount++;
            }
            if(modelCount!=48){Debug.LogError("[Level94] Expected 48 modular FBXs; found "+modelCount);invalid++;}
            string[] required={"Component_BOM_27.csv","Copper_Trace_Tasks_W001_W034.csv","Required_Via_Placements_V01_V06.csv","Logical_Pad_Locations.csv","Critical_W035_Feedback.csv","Objective_Certification_Plan_6.csv"};
            foreach(string file in required)
            {
                if(SparkLevel94PBRMaterialEditor.FindPathExact(file)==null){Debug.LogError("[Level94] Missing "+file);invalid++;}
            }
            var root=Selection.activeGameObject;
            if(root!=null)
            {
                int meshes=root.GetComponentsInChildren<MeshFilter>(true).Count(m=>m.sharedMesh!=null);
                Debug.Log("[Level94] Selected visual root contains "+meshes+" mesh filters.");
            }
            if(invalid==0)Debug.Log("[Level94] Imported art/data presence checks passed. This DOES NOT validate PCB electrical gameplay or Unity Play Mode.");
            else Debug.LogError("[Level94] Validation failed with "+invalid+" missing/incorrect asset groups.");
        }
    }
}
#endif
