using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.Level32.B850I.Editor
{
    public static class SparkL32BoardBuilder
    {
        private const string Root = "Assets/ProjectSpark/Level32/B850ILightning";
        private const float WorkshopWidth = 7.51f;
        [MenuItem("Project Spark/Level 32 B850I/Build Workshop (FBX)")]
        public static void BuildFbx() => Build(0);
        [MenuItem("Project Spark/Level 32 B850I/Build Workshop (OBJ fallback)")]
        public static void BuildObj() => Build(1);
        [MenuItem("Tools/Project Spark/Automatic Level Generator/32 B850I/1. Build FULL Level 32 (Native recommended)")]
        public static void BuildNative() => Build(2);
        [MenuItem("Tools/Project Spark/Automatic Level Generator/32 B850I/2. Validate native sources")]
        public static void ValidateNative() {
            var rows = File.ReadAllLines(Root + "/placements.csv");
            for(int i=1;i<rows.Length;i++) Require(rows[i].Split(',')[0],2);
            Debug.Log("[L32] Validated " + (rows.Length-1) + " imported native assets.");
        }
        private static GameObject Require(string id, int mode)
        {
            string path = Root + (mode == 2 ? "/Models_Native/L32_" : mode == 1 ? "/FallbackOBJ/L32_" : "/Models/L32_") + id + (mode == 2 ? ".spark32b850i" : mode == 1 ? ".obj" : ".fbx");
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!asset) throw new InvalidDataException("Missing imported model: " + path);
            foreach (var mesh in asset.GetComponentsInChildren<MeshFilter>(true))
                if (mesh.sharedMesh && mesh.sharedMesh.vertexCount > 0 && mesh.sharedMesh.GetIndexCount(0) > 0) return asset;
            throw new InvalidDataException("No mesh geometry imported: " + path + ". Try OBJ fallback.");
        }
        private static Bounds BoundsOf(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidDataException("No renderer: " + obj.name);
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }
        private static void Build(int mode)
        {
            string[] rows = File.ReadAllLines(Root + "/placements.csv");
            // Validate all imported assets before creating or saving anything.
            var assets = new GameObject[rows.Length - 1];
            for (int i = 1; i < rows.Length; i++) assets[i-1] = Require(rows[i].Split(',')[0], mode);
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("L32_ASRock_B850I_Lightning");
            try
            {
                float importedWidth = 0;
                for (int i = 1; i < rows.Length; i++)
                {
                    string[] row = rows[i].Split(',');
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(assets[i-1]);
                    go.name = row[0]; go.transform.SetParent(root.transform, false);
                    go.transform.localPosition = Vector3.zero;
                    if(row[0] != "PCB") foreach(var mf in go.GetComponentsInChildren<MeshFilter>(true)) { if(!mf.sharedMesh) continue; var col=mf.gameObject.AddComponent<BoxCollider>(); col.center=mf.sharedMesh.bounds.center; col.size=mf.sharedMesh.bounds.size; }
                    if (row[0] == "PCB")
                    {
                        var b = BoundsOf(go); importedWidth = b.size.x;
                        if (Mathf.Abs(importedWidth-.17f) > .002f || Mathf.Abs(b.size.x/b.size.z-1f) > 0.02f)
                            throw new InvalidDataException("PCB dimensions/axes are invalid: " + b.size);
                    }
                    if (importedWidth <= 0) throw new InvalidDataException("PCB must be first in placement CSV.");
                    float x = float.Parse(row[1], CultureInfo.InvariantCulture);
                    float z = float.Parse(row[2], CultureInfo.InvariantCulture);
                    // FBX right-handed X may be mirrored by Unity's importer; verify rear I/O side visually.
                    go.transform.localPosition = new Vector3(-(x-85f), 0, z-85f) * (importedWidth/170f);
                    if (mode == 1) go.transform.localPosition = new Vector3(x-85f,0,z-85f)*(importedWidth/170f);
                }
                SparkL32MaterialTools.ApplyTo(root, mode == 2);
                string[] targets = {"ATX12V1","ATXPWR1","CPU_AM5","DDR5_A1","DDR5_B1","M2_1","M2_2","PCIE1","BIOS_ROM","CPU_FAN1","B850_CHIPSET","PWM_RAA229620","LAN_USB_C"};
                var diagnostics = new GameObject("DiagnosticTargets_VisualOnly"); diagnostics.transform.SetParent(root.transform,false);
                foreach(string target in targets) { var part=root.transform.Find(target); if(!part) throw new InvalidDataException("Missing diagnostic target: "+target); var anchor=new GameObject(target); anchor.transform.SetParent(diagnostics.transform,false); anchor.transform.position=BoundsOf(part.gameObject).center; }
                root.AddComponent<ProjectSpark.Level32.B850I.SparkB850IVisualisation>();
                root.transform.localScale = Vector3.one * (WorkshopWidth / importedWidth);
                root.transform.position = Vector3.up * .45f;
                string output = Root + "/Generated";
                if (!AssetDatabase.IsValidFolder(output)) AssetDatabase.CreateFolder(Root, "Generated");
                string suffix = mode == 2 ? "_Native" : mode == 1 ? "_OBJ" : "_FBX";
                PrefabUtility.SaveAsPrefabAsset(root, output + "/L32_Workshop" + suffix + ".prefab");
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, output + "/L32_Workshop" + suffix + ".unity");
                Selection.activeGameObject = root;
                Debug.Log("[L32] Created " + assets.Length + " parts; PCB workshop width " + WorkshopWidth + ". Check orientation and visual clearances.");
            }
            catch { UnityEngine.Object.DestroyImmediate(root); throw; }
        }
    }
}
