#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using AAAUI.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSpark.SparkV5.Pro.Editor
{
    /// <summary>
    /// Binds the real player's SignalWireBuilder adapter to the v9 director.
    /// Does not edit the original player tool, existing cable prefab, or solver.
    /// </summary>
    public static class SparkAINativeSignalWireSetup
    {
        private const string Menu = "Project Spark/Spark V5 Pro/AAA Laboratory/";

        [MenuItem(Menu + "17 - AUTO Bind REAL Player Wire Tool v10")]
        public static void Bind()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Spark Native v10] Exit Play Mode before binding.");
                return;
            }
            if (!EditorUtility.DisplayDialog("Spark Native Wire v10",
                "Bind the REAL player's SignalWireBuilder adapter to the existing v9 native-wire director?\n\n" +
                "This adds an adapter and sets the director's port reference on Spark V5 prefabs and open scenes. " +
                "It does NOT modify the player's wire tool, cable prefabs, electrical solver or missions.\n\n" +
                "Save and back up your Unity project first.", "Bind native tool", "Cancel"))
                return;

            int prefabs = 0, sceneObjects = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab",
                new[] { "Assets/ProjectSpark/SparkV5" });
            string backup = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "..", "AI", "SparkNativeWire_Backups",
                "NATIVE_v10_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(assetPath)) continue;
                GameObject contents = null;
                try
                {
                    contents = PrefabUtility.LoadPrefabContents(assetPath);
                    SparkNativeWireDirector[] directors =
                        contents.GetComponentsInChildren<SparkNativeWireDirector>(true);
                    if (directors.Length == 0) continue;
                    bool changed = false;
                    foreach (SparkNativeWireDirector director in directors)
                        changed |= BindOne(director.gameObject);
                    if (!changed) continue;
                    string absolute = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                        assetPath.Replace('/', Path.DirectorySeparatorChar));
                    string copy = Path.Combine(backup, assetPath.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(copy));
                    File.Copy(absolute, copy, true);
                    PrefabUtility.SaveAsPrefabAsset(contents, assetPath);
                    prefabs++;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Spark Native v10] Prefab binding failed: " + assetPath + ": " + ex);
                }
                finally
                {
                    if (contents) PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int r = 0; r < roots.Length; r++)
                {
                    SparkNativeWireDirector[] directors =
                        roots[r].GetComponentsInChildren<SparkNativeWireDirector>(true);
                    foreach (SparkNativeWireDirector director in directors)
                    {
                        if (!BindOne(director.gameObject)) continue;
                        sceneObjects++;
                        EditorSceneManager.MarkSceneDirty(scene);
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Spark Native v10] REAL player tool adapter bound. Updated " +
                prefabs + " prefabs and " + sceneObjects + " open-scene objects. " +
                "Modified prefab backups: " + backup + ". Save your open scenes.");
        }

        private static bool BindOne(GameObject target)
        {
            SparkNativeWireDirector director = target.GetComponent<SparkNativeWireDirector>();
            if (!director) return false;
            SparkAINativeSignalWirePort port = target.GetComponent<SparkAINativeSignalWirePort>();
            bool changed = false;
            if (!port)
            {
                port = target.AddComponent<SparkAINativeSignalWirePort>();
                changed = true;
            }
            SerializedObject serialized = new SerializedObject(director);
            SerializedProperty property = serialized.FindProperty("playerWirePort");
            if (property == null)
            {
                Debug.LogError("[Spark Native v10] Director API changed: playerWirePort missing.", target);
                return changed;
            }
            if (property.objectReferenceValue != port)
            {
                property.objectReferenceValue = port;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                changed = true;
            }
            return changed;
        }

        [MenuItem(Menu + "18 - Diagnose REAL Player Wire v10")]
        public static void Diagnose()
        {
            var text = new StringBuilder(1200);
            text.AppendLine("SPARK NATIVE WIRE v10 — DIAGNOSTIC");
            text.AppendLine("This report is inspection only. It does not create an electrical wire.");
            SparkNativeWireDirector[] directors = UnityEngine.Object.FindObjectsByType<SparkNativeWireDirector>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            SignalWireBuilder[] builders = UnityEngine.Object.FindObjectsByType<SignalWireBuilder>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            text.AppendLine("Directors in loaded scenes: " + directors.Length);
            text.AppendLine("Player SignalWireBuilders in loaded scenes: " + builders.Length);
            for (int i = 0; i < builders.Length; i++)
                text.AppendLine("BUILDER " + builders[i].name +
                    " active=" + builders[i].isActiveAndEnabled +
                    " mode=" + builders[i].DrawingMode +
                    " circuit=" + (builders[i].Circuit ? builders[i].Circuit.name : "not yet bound"));
            for (int i = 0; i < directors.Length; i++)
            {
                SparkNativeWireDirector director = directors[i];
                SparkAINativeSignalWirePort port = director.GetComponent<SparkAINativeSignalWirePort>();
                SerializedObject serialized = new SerializedObject(director);
                SerializedProperty selected = serialized.FindProperty("playerWirePort");
                bool bound = port && selected != null && selected.objectReferenceValue == port;
                text.AppendLine("DIRECTOR " + director.name + " nativeV10Assigned=" + bound +
                    " builderResolved=" + (port && port.IsConfigured));
                if (port) text.AppendLine("  adapterDetail=" + port.LastError);
            }
            string root = Directory.GetParent(Application.dataPath).FullName;
            string path = Path.Combine(root, "Logs", "SparkNativeWire", "NativeWireV10_Diagnostic.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text.ToString());
            Debug.Log("[Spark Native v10] Diagnostic written: " + path + "\n" + text);
        }
    }
}
#endif
