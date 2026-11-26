#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectSpark.SparkV5.Pro;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    public static class SparkNativeWireEditor
    {
        private const string MenuRoot = "Project Spark/Spark V5 Pro/AAA Laboratory/";

        [MenuItem(MenuRoot + "14 - Bind Native Wire Controller (Prefabs + Open Scenes)")]
        public static void AutoBind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogError("Exit Play Mode before modifying prefabs/scenes."); return; }
            if (!EditorUtility.DisplayDialog("Spark Native Player Wire Control",
                "Add Native Wire Director and an UNCONFIGURED UnityEvent bridge to existing Spark executive objects? No existing wire-tool scripts or prefabs will be replaced. The bridge requires binding to the real player Wire Tool before use.",
                "Bind controller", "Cancel")) return;
            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.StartsWith("Assets/ProjectSpark/", StringComparison.Ordinal) &&
                    !path.Contains("SparkV5")) continue;
                GameObject root = null;
                try
                {
                    root = PrefabUtility.LoadPrefabContents(path);
                    if (!root || !root.GetComponentInChildren<SparkProExecutive>(true)) continue;
                    SparkProExecutive[] executives = root.GetComponentsInChildren<SparkProExecutive>(true);
                    bool changed = false;
                    for (int j = 0; j < executives.Length; j++)
                    {
                        if (!executives[j].GetComponent<SparkNativeWireDirector>())
                        { executives[j].gameObject.AddComponent<SparkNativeWireDirector>(); changed = true; }
                        if (!executives[j].GetComponent<SparkAINativeWirePort>())
                        { executives[j].gameObject.AddComponent<SparkAINativeWireUnityEventPort>(); changed = true; }
                    }
                    if (changed) { PrefabUtility.SaveAsPrefabAsset(root, path); count++; }
                }
                catch (Exception e) { Debug.LogError("Prefab binding failed: " + path + " " + e.Message); }
                finally { if (root) PrefabUtility.UnloadPrefabContents(root); }
            }
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || !scene.IsValid()) continue;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    SparkProExecutive[] executives = roots[r].GetComponentsInChildren<SparkProExecutive>(true);
                    foreach (SparkProExecutive executive in executives)
                    {
                        bool changed = false;
                        if (!executive.GetComponent<SparkNativeWireDirector>())
                        { Undo.AddComponent<SparkNativeWireDirector>(executive.gameObject); changed = true; }
                        if (!executive.GetComponent<SparkAINativeWirePort>())
                        { Undo.AddComponent<SparkAINativeWireUnityEventPort>(executive.gameObject); changed = true; }
                        if (changed) { EditorSceneManager.MarkSceneDirty(scene); count++; }
                    }
                }
            }
            Debug.Log("[Spark Native Wire] Bound " + count + " prefab/scene object(s). " +
                "Player Wire Tool event callbacks are UNBOUND. Configure the native bridge and save scenes.");
        }

        [MenuItem(MenuRoot + "15 - Audit Actual Player Wire Sources")]
        public static void AuditNativeWire()
        {
            var report = new StringBuilder();
            report.AppendLine("SPARK NATIVE PLAYER WIRE — SOURCE INVENTORY");
            report.AppendLine("Report generated: " + DateTime.UtcNow.ToString("u"));
            report.AppendLine("Native wire = existing physical cable prefab + player's exact wire tool + circuit topology.");
            report.AppendLine("This report DOES NOT prove the tool has been bound or a cable created.");
            report.AppendLine();
            string[] scripts = AssetDatabase.FindAssets("t:MonoScript");
            int scriptCount = 0;
            for (int i = 0; i < scripts.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(scripts[i]);
                string name = Path.GetFileName(path);
                if (!LooksLikeWire(name)) continue;
                report.AppendLine("SCRIPT: " + path);
                scriptCount++;
            }
            report.AppendLine("Matching script names: " + scriptCount);
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            int prefabCount = 0;
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                if (!LooksLikeWire(Path.GetFileName(path))) continue;
                report.AppendLine("PREFAB CANDIDATE: " + path);
                prefabCount++;
            }
            report.AppendLine("Matching prefab names: " + prefabCount);
            report.AppendLine();
            report.AppendLine("Required binding: create wire, attach start, move end, attach end, finalize, cancel, undo.");
            report.AppendLine("Actual callbacks must call the SAME PLAYER WIRE TOOL used by the game.");
            var directors = UnityEngine.Object.FindObjectsByType<SparkNativeWireDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine("Loaded native directors: " + directors.Length);
            foreach (SparkNativeWireDirector d in directors) report.AppendLine("DIRECTOR: " + d.gameObject.name);
            string directory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "SparkNativeWire");
            Directory.CreateDirectory(directory);
            string destination = Path.Combine(directory, "SourceAudit.txt");
            File.WriteAllText(destination, report.ToString());
            Debug.Log("[Spark Native Wire] Player tool source audit saved: " + destination +
                "\nCandidate scripts: " + scriptCount + ", prefabs: " + prefabCount);
            EditorUtility.RevealInFinder(destination);
        }
        private static bool LooksLikeWire(string name)
        {
            return name.IndexOf("wire", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("cable", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("plug", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("jack", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        [MenuItem(MenuRoot + "16 - Verify Native Wire Command Routing")]
        public static void Verify()
        {
            string[] recognized = {
                "Draw wire from SwitchOutput to Diode_Cathode",
                "spark draw real wire from <SwitchOutput> to <LED_Anode>",
                "confirm wire", "cancel wire", "stop wiring", "native wire status", "undo last AI wire"
            };
            foreach (string s in recognized)
                if (!SparkNativeWireDirector.RecognizesCommand(s))
                    throw new InvalidOperationException("Native command not recognized: " + s);
            if (SparkNativeWireDirector.RecognizesCommand("Teach me Level 24"))
                throw new InvalidOperationException("Native wire stole ordinary teaching command.");
            Debug.Log("[Spark Native Wire PRO] PASS routing checks. These do NOT demonstrate physical wiring. " +
                "Run Source Audit, bind the real player Wire Tool, then verify the cable in Play Mode.");
        }
    }
}
#endif
